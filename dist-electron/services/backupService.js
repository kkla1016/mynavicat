"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.BackupService = void 0;
const path = __importStar(require("path"));
const fs = __importStar(require("fs"));
const zlib = __importStar(require("zlib"));
const child_process_1 = require("child_process");
const MySqlProvider_1 = require("../providers/MySqlProvider");
class BackupService {
    storageService;
    cryptoService;
    backupBaseDir;
    constructor(storageService, cryptoService) {
        this.storageService = storageService;
        this.cryptoService = cryptoService;
        const userDataPath = path.join(process.env.APPDATA || process.env.HOME || '.', 'MyNavicat');
        this.backupBaseDir = path.join(userDataPath, 'backups');
        if (!fs.existsSync(this.backupBaseDir)) {
            fs.mkdirSync(this.backupBaseDir, { recursive: true });
        }
    }
    async executeBackup(request, scheduleId) {
        const conn = this.storageService.getConnectionById(request.connectionId || request.ConnectionId);
        if (!conn)
            throw new Error(`Connection ${request.connectionId} not found.`);
        const mysqlProvider = new MySqlProvider_1.MySqlProvider(this.cryptoService);
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
        const databaseName = request.databaseName || request.DatabaseName;
        const isPartial = request.tables && request.tables.length > 0;
        const rawFileName = `${conn.Name}_${databaseName}_${timestamp}.sql`;
        const rawFilePath = path.join(this.backupBaseDir, rawFileName);
        let history = this.storageService.saveBackupHistory({
            ConnectionId: conn.Id,
            DatabaseName: databaseName,
            Tables: request.tables ? JSON.stringify(request.tables) : null,
            BackupType: isPartial ? 'Partial' : 'Full',
            FilePath: rawFilePath,
            IsCompressed: false,
            Status: 'InProgress',
            ScheduleId: scheduleId || null,
            StartedAt: new Date().toISOString()
        });
        const startTime = Date.now();
        try {
            const backupCmd = mysqlProvider.getBackupCommand(conn, databaseName, request.tables, rawFilePath);
            const [success, errorMsg] = await this.runProcessAsync(backupCmd);
            if (!success) {
                history.Status = 'Failed';
                history.ErrorMessage = errorMsg;
                history.CompletedAt = new Date().toISOString();
                history.DurationSeconds = (Date.now() - startTime) / 1000;
                return this.storageService.saveBackupHistory(history);
            }
            if (request.compress && fs.existsSync(rawFilePath)) {
                let compressedPath = rawFilePath;
                if ((request.compressionType || 'gz').toLowerCase() === 'gz') {
                    compressedPath = await this.compressFileGz(rawFilePath);
                    history.CompressionType = 'gz';
                }
                if (fs.existsSync(compressedPath) && compressedPath !== rawFilePath) {
                    fs.unlinkSync(rawFilePath);
                    history.FilePath = compressedPath;
                    history.IsCompressed = true;
                }
            }
            const stats = fs.statSync(history.FilePath);
            history.FileSize = stats.size;
            history.Status = 'Success';
            history.CompletedAt = new Date().toISOString();
            history.DurationSeconds = (Date.now() - startTime) / 1000;
            return this.storageService.saveBackupHistory(history);
        }
        catch (ex) {
            history.Status = 'Failed';
            history.ErrorMessage = ex.message;
            history.CompletedAt = new Date().toISOString();
            history.DurationSeconds = (Date.now() - startTime) / 1000;
            return this.storageService.saveBackupHistory(history);
        }
    }
    async executeChunkedBackup(request, chunkSizeMb = 100) {
        const parentHistory = await this.executeBackup(request);
        if (parentHistory.Status !== 'Success' || !fs.existsSync(parentHistory.FilePath)) {
            return parentHistory;
        }
        const chunkSizeBytes = chunkSizeMb * 1024 * 1024;
        const stats = fs.statSync(parentHistory.FilePath);
        if (stats.size <= chunkSizeBytes) {
            return parentHistory;
        }
        parentHistory.BackupType = 'Chunked';
        const chunkFiles = this.splitFileIntoChunks(parentHistory.FilePath, chunkSizeBytes);
        parentHistory.TotalChunks = chunkFiles.length;
        for (let i = 0; i < chunkFiles.length; i++) {
            const chunkPath = chunkFiles[i];
            const chunkStats = fs.statSync(chunkPath);
            this.storageService.saveBackupHistory({
                ConnectionId: parentHistory.ConnectionId,
                ParentBackupId: parentHistory.Id,
                DatabaseName: parentHistory.DatabaseName,
                Tables: parentHistory.Tables,
                BackupType: 'Chunked',
                FilePath: chunkPath,
                FileSize: chunkStats.size,
                IsCompressed: parentHistory.IsCompressed,
                CompressionType: parentHistory.CompressionType,
                ChunkIndex: i,
                TotalChunks: chunkFiles.length,
                Status: 'Success',
                StartedAt: parentHistory.StartedAt,
                CompletedAt: new Date().toISOString()
            });
        }
        fs.unlinkSync(parentHistory.FilePath);
        parentHistory.FilePath = chunkFiles[0];
        return this.storageService.saveBackupHistory(parentHistory);
    }
    splitFileIntoChunks(filePath, chunkSizeBytes) {
        const chunkFiles = [];
        const buffer = Buffer.alloc(64 * 1024);
        const fd = fs.openSync(filePath, 'r');
        let chunkIndex = 0;
        let bytesRead = 0;
        while (true) {
            const chunkPath = `${filePath}.part${chunkIndex + 1}`;
            const outFd = fs.openSync(chunkPath, 'w');
            let currentChunkSize = 0;
            while (currentChunkSize < chunkSizeBytes) {
                bytesRead = fs.readSync(fd, buffer, 0, Math.min(buffer.length, chunkSizeBytes - currentChunkSize), null);
                if (bytesRead === 0)
                    break;
                fs.writeSync(outFd, buffer, 0, bytesRead);
                currentChunkSize += bytesRead;
            }
            fs.closeSync(outFd);
            if (currentChunkSize > 0) {
                chunkFiles.push(chunkPath);
                chunkIndex++;
            }
            if (bytesRead === 0)
                break;
        }
        fs.closeSync(fd);
        return chunkFiles;
    }
    runProcessAsync(cmd) {
        return new Promise((resolve) => {
            const parts = cmd.arguments.match(/(?:[^\s"]+|"[^"]*")+/g) || [];
            const proc = (0, child_process_1.spawn)(cmd.executable, parts, { shell: true });
            let stderr = '';
            proc.stderr.on('data', (data) => { stderr += data.toString(); });
            proc.on('close', (code) => {
                resolve([code === 0, stderr]);
            });
            proc.on('error', (err) => {
                resolve([false, err.message]);
            });
        });
    }
    compressFileGz(sourcePath) {
        return new Promise((resolve, reject) => {
            const targetPath = sourcePath + '.gz';
            const readStream = fs.createReadStream(sourcePath);
            const writeStream = fs.createWriteStream(targetPath);
            const gzip = zlib.createGzip();
            readStream.pipe(gzip).pipe(writeStream).on('finish', () => {
                resolve(targetPath);
            }).on('error', (err) => reject(err));
        });
    }
}
exports.BackupService = BackupService;
