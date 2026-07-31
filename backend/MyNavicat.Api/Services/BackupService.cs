using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyNavicat.Api.Data;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Providers;

namespace MyNavicat.Api.Services
{
    public interface IBackupService
    {
        Task<BackupHistory> ExecuteBackupAsync(BackupRequestDto request, int? scheduleId = null);
        Task<BackupHistory> ExecuteChunkedBackupAsync(BackupRequestDto request, long chunkSizeBytes = 100 * 1024 * 1024);
        Task<bool> ExecuteRestoreAsync(RestoreRequestDto request);
        Task<bool> ExecuteRestoreFromFileAsync(int targetConnectionId, string targetDatabaseName, Stream fileStream, string fileName);
        Task<PagedResult<BackupHistory>> GetHistoryAsync(int? connectionId, string? status, int page = 1, int pageSize = 20);
        Task<BackupHistory?> GetHistoryByIdAsync(int id);
        Task<bool> DeleteHistoryAsync(int id);
        Task<(Stream fileStream, string contentType, string fileName)> GetBackupFileAsync(int id);
    }

    public class BackupService : IBackupService
    {
        private readonly AppDbContext _db;
        private readonly IDbProviderFactory _providerFactory;
        private readonly ILogger<BackupService> _logger;
        private readonly string _backupBaseDir;

        public BackupService(AppDbContext db, IDbProviderFactory providerFactory, ILogger<BackupService> logger)
        {
            _db = db;
            _providerFactory = providerFactory;
            _logger = logger;
            _backupBaseDir = Path.Combine(Directory.GetCurrentDirectory(), "backups");
            if (!Directory.Exists(_backupBaseDir))
            {
                Directory.CreateDirectory(_backupBaseDir);
            }
        }

        public async Task<BackupHistory> ExecuteBackupAsync(BackupRequestDto request, int? scheduleId = null)
        {
            var conn = await _db.Connections.FindAsync(request.ConnectionId);
            if (conn == null) throw new KeyNotFoundException($"Connection {request.ConnectionId} not found.");

            var provider = _providerFactory.GetProvider(conn.DbType);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var tablesJson = request.Tables != null && request.Tables.Count > 0 ? JsonSerializer.Serialize(request.Tables) : null;
            var isPartial = request.Tables != null && request.Tables.Count > 0;

            var rawFileName = $"{conn.Name}_{request.DatabaseName}_{timestamp}.sql";
            var rawFilePath = Path.Combine(_backupBaseDir, rawFileName);

            var history = new BackupHistory
            {
                ConnectionId = conn.Id,
                DatabaseName = request.DatabaseName,
                Tables = tablesJson,
                BackupType = isPartial ? "Partial" : "Full",
                FilePath = rawFilePath,
                IsCompressed = false,
                Status = "InProgress",
                ScheduleId = scheduleId,
                StartedAt = DateTime.UtcNow
            };

            _db.BackupHistories.Add(history);
            await _db.SaveChangesAsync();

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var backupCmd = provider.GetBackupCommand(conn, request.DatabaseName, request.Tables, rawFilePath);
                var (success, errorMsg) = await RunProcessAsync(backupCmd);

                if (!success)
                {
                    history.Status = "Failed";
                    history.ErrorMessage = errorMsg;
                    history.CompletedAt = DateTime.UtcNow;
                    history.DurationSeconds = stopwatch.Elapsed.TotalSeconds;
                    await _db.SaveChangesAsync();
                    return history;
                }

                if (request.Compress && File.Exists(rawFilePath))
                {
                    string compressedFilePath = rawFilePath;
                    if (request.CompressionType.Equals("gz", StringComparison.OrdinalIgnoreCase))
                    {
                        compressedFilePath = CompressFileGz(rawFilePath);
                        history.CompressionType = "gz";
                    }
                    else if (request.CompressionType.Equals("zip", StringComparison.OrdinalIgnoreCase))
                    {
                        compressedFilePath = CompressFileZip(rawFilePath);
                        history.CompressionType = "zip";
                    }

                    if (File.Exists(compressedFilePath) && compressedFilePath != rawFilePath)
                    {
                        File.Delete(rawFilePath);
                        history.FilePath = compressedFilePath;
                        history.IsCompressed = true;
                    }
                }

                var fileInfo = new FileInfo(history.FilePath);
                history.FileSize = fileInfo.Exists ? fileInfo.Length : 0;
                history.Status = "Success";
                history.CompletedAt = DateTime.UtcNow;
                history.DurationSeconds = stopwatch.Elapsed.TotalSeconds;

                await _db.SaveChangesAsync();
                return history;
            }
            catch (Exception ex)
            {
                history.Status = "Failed";
                history.ErrorMessage = ex.Message;
                history.CompletedAt = DateTime.UtcNow;
                history.DurationSeconds = stopwatch.Elapsed.TotalSeconds;
                await _db.SaveChangesAsync();
                return history;
            }
        }

        public async Task<BackupHistory> ExecuteChunkedBackupAsync(BackupRequestDto request, long chunkSizeBytes = 100 * 1024 * 1024)
        {
            var parentHistory = await ExecuteBackupAsync(request);
            if (parentHistory.Status != "Success" || !File.Exists(parentHistory.FilePath))
            {
                return parentHistory;
            }

            var fileInfo = new FileInfo(parentHistory.FilePath);
            if (fileInfo.Length <= chunkSizeBytes)
            {
                return parentHistory; // 小於分片閥值無須分片
            }

            parentHistory.BackupType = "Chunked";
            var chunkFiles = SplitFileIntoChunks(parentHistory.FilePath, chunkSizeBytes);
            parentHistory.TotalChunks = chunkFiles.Count;

            for (int i = 0; i < chunkFiles.Count; i++)
            {
                var chunkPath = chunkFiles[i];
                var chunkInfo = new FileInfo(chunkPath);

                var chunkHistory = new BackupHistory
                {
                    ConnectionId = parentHistory.ConnectionId,
                    ParentBackupId = parentHistory.Id,
                    DatabaseName = parentHistory.DatabaseName,
                    Tables = parentHistory.Tables,
                    BackupType = "Chunked",
                    FilePath = chunkPath,
                    FileSize = chunkInfo.Length,
                    IsCompressed = parentHistory.IsCompressed,
                    CompressionType = parentHistory.CompressionType,
                    ChunkIndex = i,
                    TotalChunks = chunkFiles.Count,
                    Status = "Success",
                    StartedAt = parentHistory.StartedAt,
                    CompletedAt = DateTime.UtcNow
                };

                _db.BackupHistories.Add(chunkHistory);
            }

            // 移除原始未分片巨型檔案
            File.Delete(parentHistory.FilePath);
            parentHistory.FilePath = chunkFiles[0]; // 主記錄指向首個分片

            await _db.SaveChangesAsync();
            return parentHistory;
        }

        public async Task<bool> ExecuteRestoreAsync(RestoreRequestDto request)
        {
            if (!request.BackupHistoryId.HasValue) throw new ArgumentException("BackupHistoryId is required.");

            var history = await _db.BackupHistories.Include(h => h.Chunks).FirstOrDefaultAsync(h => h.Id == request.BackupHistoryId.Value);
            if (history == null) throw new FileNotFoundException("Backup history not found.");

            var targetConn = await _db.Connections.FindAsync(request.TargetConnectionId);
            if (targetConn == null) throw new KeyNotFoundException($"Target Connection {request.TargetConnectionId} not found.");

            string restoreFilePath;
            string? tempMergedPath = null;

            if (history.BackupType == "Chunked" && history.Chunks != null && history.Chunks.Count > 0)
            {
                tempMergedPath = MergeChunks(history.Chunks.OrderBy(c => c.ChunkIndex).Select(c => c.FilePath).ToList());
                restoreFilePath = tempMergedPath;
            }
            else
            {
                restoreFilePath = history.FilePath;
            }

            string? tempUncompressedPath = null;
            try
            {
                if (history.IsCompressed)
                {
                    tempUncompressedPath = DecompressFile(restoreFilePath, history.CompressionType);
                    restoreFilePath = tempUncompressedPath;
                }

                var provider = _providerFactory.GetProvider(targetConn.DbType);
                var restoreCmd = provider.GetRestoreCommand(targetConn, request.TargetDatabaseName, restoreFilePath);
                var (success, _) = await RunProcessAsync(restoreCmd);
                return success;
            }
            finally
            {
                if (tempMergedPath != null && File.Exists(tempMergedPath)) File.Delete(tempMergedPath);
                if (tempUncompressedPath != null && File.Exists(tempUncompressedPath)) File.Delete(tempUncompressedPath);
            }
        }

        public async Task<bool> ExecuteRestoreFromFileAsync(int targetConnectionId, string targetDatabaseName, Stream fileStream, string fileName)
        {
            var targetConn = await _db.Connections.FindAsync(targetConnectionId);
            if (targetConn == null) throw new KeyNotFoundException($"Target Connection {targetConnectionId} not found.");

            var tempUploadPath = Path.Combine(_backupBaseDir, $"upload_{Guid.NewGuid()}_{fileName}");
            using (var fs = new FileStream(tempUploadPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fs);
            }

            string restoreSqlPath = tempUploadPath;
            string? tempUncompressedPath = null;

            try
            {
                if (fileName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
                {
                    tempUncompressedPath = DecompressFile(tempUploadPath, "gz");
                    restoreSqlPath = tempUncompressedPath;
                }
                else if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    tempUncompressedPath = DecompressFile(tempUploadPath, "zip");
                    restoreSqlPath = tempUncompressedPath;
                }

                var provider = _providerFactory.GetProvider(targetConn.DbType);
                var restoreCmd = provider.GetRestoreCommand(targetConn, targetDatabaseName, restoreSqlPath);
                var (success, _) = await RunProcessAsync(restoreCmd);
                return success;
            }
            finally
            {
                if (File.Exists(tempUploadPath)) File.Delete(tempUploadPath);
                if (tempUncompressedPath != null && File.Exists(tempUncompressedPath)) File.Delete(tempUncompressedPath);
            }
        }

        public async Task<PagedResult<BackupHistory>> GetHistoryAsync(int? connectionId, string? status, int page = 1, int pageSize = 20)
        {
            var query = _db.BackupHistories.Include(h => h.Connection).AsNoTracking().Where(h => h.ParentBackupId == null).AsQueryable();

            if (connectionId.HasValue) query = query.Where(h => h.ConnectionId == connectionId.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(h => h.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

            long totalCount = await query.LongCountAsync();
            var items = await query.OrderByDescending(h => h.StartedAt)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new PagedResult<BackupHistory>(items, page, pageSize, totalCount);
        }

        public async Task<BackupHistory?> GetHistoryByIdAsync(int id)
        {
            return await _db.BackupHistories.Include(h => h.Connection).Include(h => h.Chunks).FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<bool> DeleteHistoryAsync(int id)
        {
            var history = await _db.BackupHistories.Include(h => h.Chunks).FirstOrDefaultAsync(h => h.Id == id);
            if (history == null) return false;

            if (history.Chunks != null && history.Chunks.Count > 0)
            {
                foreach (var chunk in history.Chunks)
                {
                    if (File.Exists(chunk.FilePath)) try { File.Delete(chunk.FilePath); } catch { }
                }
            }

            if (File.Exists(history.FilePath)) try { File.Delete(history.FilePath); } catch { }

            _db.BackupHistories.Remove(history);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(Stream fileStream, string contentType, string fileName)> GetBackupFileAsync(int id)
        {
            var history = await _db.BackupHistories.FindAsync(id);
            if (history == null || !File.Exists(history.FilePath)) throw new FileNotFoundException("File not found.");

            var fileName = Path.GetFileName(history.FilePath);
            var contentType = fileName.EndsWith(".gz") ? "application/gzip" :
                              fileName.EndsWith(".zip") ? "application/zip" : "application/sql";

            var stream = new FileStream(history.FilePath, FileMode.Open, FileAccess.Read);
            return (stream, contentType, fileName);
        }

        private List<string> SplitFileIntoChunks(string filePath, long chunkSizeBytes)
        {
            var chunkFiles = new List<string>();
            byte[] buffer = new byte[64 * 1024];

            using var input = File.OpenRead(filePath);
            int index = 0;
            while (input.Position < input.Length)
            {
                var chunkPath = $"{filePath}.part{index + 1}";
                using (var output = File.Create(chunkPath))
                {
                    long bytesRemaining = chunkSizeBytes;
                    while (bytesRemaining > 0)
                    {
                        int read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, bytesRemaining));
                        if (read == 0) break;
                        output.Write(buffer, 0, read);
                        bytesRemaining -= read;
                    }
                }
                chunkFiles.Add(chunkPath);
                index++;
            }

            return chunkFiles;
        }

        private string MergeChunks(List<string> chunkFilePaths)
        {
            var mergedPath = Path.Combine(_backupBaseDir, $"merged_{Guid.NewGuid()}.tmp");
            using var output = File.Create(mergedPath);
            byte[] buffer = new byte[64 * 1024];

            foreach (var chunkPath in chunkFilePaths)
            {
                if (File.Exists(chunkPath))
                {
                    using var input = File.OpenRead(chunkPath);
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                    }
                }
            }

            return mergedPath;
        }

        private async Task<(bool success, string errorMsg)> RunProcessAsync(BackupCommand cmd)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = cmd.Executable,
                    Arguments = cmd.Arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                foreach (var kvp in cmd.EnvironmentVariables) psi.EnvironmentVariables[kvp.Key] = kvp.Value;

                using var process = new Process { StartInfo = psi };
                process.Start();

                string errorText = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                bool isSuccess = process.ExitCode == 0;
                return (isSuccess, isSuccess ? string.Empty : errorText);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private string CompressFileGz(string sourceFilePath)
        {
            string targetPath = sourceFilePath + ".gz";
            using (var sourceStream = File.OpenRead(sourceFilePath))
            using (var targetStream = File.Create(targetPath))
            using (var compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
            {
                sourceStream.CopyTo(compressionStream);
            }
            return targetPath;
        }

        private string CompressFileZip(string sourceFilePath)
        {
            string targetPath = sourceFilePath + ".zip";
            using (var archive = ZipFile.Open(targetPath, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(sourceFilePath, Path.GetFileName(sourceFilePath));
            }
            return targetPath;
        }

        private string DecompressFile(string compressedFilePath, string? type)
        {
            string tempSqlPath = Path.Combine(_backupBaseDir, $"decompressed_{Guid.NewGuid()}.sql");

            if (type?.Equals("gz", StringComparison.OrdinalIgnoreCase) == true || compressedFilePath.EndsWith(".gz"))
            {
                using var sourceStream = File.OpenRead(compressedFilePath);
                using var decompressedStream = new GZipStream(sourceStream, CompressionMode.Decompress);
                using var targetStream = File.Create(tempSqlPath);
                decompressedStream.CopyTo(targetStream);
            }
            else if (type?.Equals("zip", StringComparison.OrdinalIgnoreCase) == true || compressedFilePath.EndsWith(".zip"))
            {
                using var archive = ZipFile.OpenRead(compressedFilePath);
                var entry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)) ?? archive.Entries.First();
                entry.ExtractToFile(tempSqlPath, overwrite: true);
            }
            else
            {
                File.Copy(compressedFilePath, tempSqlPath, overwrite: true);
            }

            return tempSqlPath;
        }
    }
}
