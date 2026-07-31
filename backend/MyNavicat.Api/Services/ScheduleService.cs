using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyNavicat.Api.Data;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;

namespace MyNavicat.Api.Services
{
    public interface IScheduleService
    {
        Task<List<ScheduleDto>> GetAllAsync();
        Task<ScheduleDto?> GetByIdAsync(int id);
        Task<ScheduleDto> CreateAsync(ScheduleDto dto);
        Task<ScheduleDto?> UpdateAsync(int id, ScheduleDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleAsync(int id);
        Task RunNowAsync(int id);
        Task ExecuteScheduledBackupAsync(int scheduleId);
    }

    public class ScheduleService : IScheduleService
    {
        private readonly AppDbContext _db;
        private readonly IRecurringJobManager _jobManager;
        private readonly IBackupService _backupService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(
            AppDbContext db,
            IRecurringJobManager jobManager,
            IBackupService backupService,
            INotificationService notificationService,
            ILogger<ScheduleService> logger)
        {
            _db = db;
            _jobManager = jobManager;
            _backupService = backupService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<List<ScheduleDto>> GetAllAsync()
        {
            var schedules = await _db.BackupSchedules
                                    .Include(s => s.Connection)
                                    .AsNoTracking()
                                    .ToListAsync();

            return schedules.Select(MapToDto).ToList();
        }

        public async Task<ScheduleDto?> GetByIdAsync(int id)
        {
            var schedule = await _db.BackupSchedules
                                    .Include(s => s.Connection)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(s => s.Id == id);
            return schedule == null ? null : MapToDto(schedule);
        }

        public async Task<ScheduleDto> CreateAsync(ScheduleDto dto)
        {
            var entity = new BackupSchedule
            {
                ConnectionId = dto.ConnectionId,
                DatabaseName = dto.DatabaseName,
                Tables = dto.Tables,
                CronExpression = dto.CronExpression,
                Description = dto.Description,
                IsEnabled = dto.IsEnabled,
                CompressBackup = dto.CompressBackup,
                CompressionType = dto.CompressionType,
                RetainCount = dto.RetainCount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.BackupSchedules.Add(entity);
            await _db.SaveChangesAsync();

            var jobId = $"schedule-{entity.Id}";
            entity.HangfireJobId = jobId;

            if (entity.IsEnabled)
            {
                try
                {
                    _jobManager.AddOrUpdate(jobId, () => ExecuteScheduledBackupAsync(entity.Id), entity.CronExpression);
                }
                catch { }
            }

            await _db.SaveChangesAsync();
            return MapToDto(entity);
        }

        public async Task<ScheduleDto?> UpdateAsync(int id, ScheduleDto dto)
        {
            var entity = await _db.BackupSchedules.FindAsync(id);
            if (entity == null) return null;

            entity.ConnectionId = dto.ConnectionId;
            entity.DatabaseName = dto.DatabaseName;
            entity.Tables = dto.Tables;
            entity.CronExpression = dto.CronExpression;
            entity.Description = dto.Description;
            entity.IsEnabled = dto.IsEnabled;
            entity.CompressBackup = dto.CompressBackup;
            entity.CompressionType = dto.CompressionType;
            entity.RetainCount = dto.RetainCount;
            entity.UpdatedAt = DateTime.UtcNow;

            var jobId = $"schedule-{entity.Id}";
            entity.HangfireJobId = jobId;

            if (entity.IsEnabled)
            {
                try { _jobManager.AddOrUpdate(jobId, () => ExecuteScheduledBackupAsync(entity.Id), entity.CronExpression); } catch { }
            }
            else
            {
                try { _jobManager.RemoveIfExists(jobId); } catch { }
            }

            await _db.SaveChangesAsync();
            return MapToDto(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.BackupSchedules.FindAsync(id);
            if (entity == null) return false;

            if (!string.IsNullOrEmpty(entity.HangfireJobId))
            {
                try { _jobManager.RemoveIfExists(entity.HangfireJobId); } catch { }
            }

            _db.BackupSchedules.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleAsync(int id)
        {
            var entity = await _db.BackupSchedules.FindAsync(id);
            if (entity == null) return false;

            entity.IsEnabled = !entity.IsEnabled;
            entity.UpdatedAt = DateTime.UtcNow;

            var jobId = $"schedule-{entity.Id}";
            if (entity.IsEnabled)
            {
                try { _jobManager.AddOrUpdate(jobId, () => ExecuteScheduledBackupAsync(entity.Id), entity.CronExpression); } catch { }
            }
            else
            {
                try { _jobManager.RemoveIfExists(jobId); } catch { }
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task RunNowAsync(int id)
        {
            var entity = await _db.BackupSchedules.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException("Schedule not found.");

            try
            {
                BackgroundJob.Enqueue(() => ExecuteScheduledBackupAsync(id));
            }
            catch { }
            await Task.CompletedTask;
        }

        public async Task ExecuteScheduledBackupAsync(int scheduleId)
        {
            var schedule = await _db.BackupSchedules.FindAsync(scheduleId);
            if (schedule == null) return;

            List<string>? tables = null;
            if (!string.IsNullOrEmpty(schedule.Tables))
            {
                try { tables = JsonSerializer.Deserialize<List<string>>(schedule.Tables); } catch { }
            }

            var request = new BackupRequestDto
            {
                ConnectionId = schedule.ConnectionId,
                DatabaseName = schedule.DatabaseName,
                Tables = tables,
                Compress = schedule.CompressBackup,
                CompressionType = schedule.CompressionType
            };

            try
            {
                var history = await _backupService.ExecuteBackupAsync(request, schedule.Id);

                schedule.LastRunAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                if (history.Status == "Success")
                {
                    await _notificationService.SendBackupCompletedAsync(scheduleId, history);

                    if (schedule.RetainCount > 0)
                    {
                        await CleanupOldBackupsAsync(schedule.Id, schedule.RetainCount);
                    }
                }
                else
                {
                    await _notificationService.SendBackupFailedAsync(scheduleId, history.ErrorMessage ?? "Unknown backup error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled backup failed for schedule ID {Id}", scheduleId);
                await _notificationService.SendBackupFailedAsync(scheduleId, ex.Message);
            }
        }

        private async Task CleanupOldBackupsAsync(int scheduleId, int retainCount)
        {
            var histories = await _db.BackupHistories
                                    .Where(h => h.ScheduleId == scheduleId && h.Status == "Success")
                                    .OrderByDescending(h => h.StartedAt)
                                    .ToListAsync();

            if (histories.Count > retainCount)
            {
                var toDelete = histories.Skip(retainCount).ToList();
                foreach (var oldHistory in toDelete)
                {
                    await _backupService.DeleteHistoryAsync(oldHistory.Id);
                }
            }
        }

        private ScheduleDto MapToDto(BackupSchedule entity)
        {
            return new ScheduleDto
            {
                Id = entity.Id,
                ConnectionId = entity.ConnectionId,
                DatabaseName = entity.DatabaseName,
                Tables = entity.Tables,
                CronExpression = entity.CronExpression,
                Description = entity.Description,
                IsEnabled = entity.IsEnabled,
                CompressBackup = entity.CompressBackup,
                CompressionType = entity.CompressionType,
                RetainCount = entity.RetainCount,
                HangfireJobId = entity.HangfireJobId,
                LastRunAt = entity.LastRunAt,
                NextRunAt = entity.NextRunAt,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                ConnectionName = entity.Connection?.Name
            };
        }
    }
}
