"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.ScheduleService = void 0;
const node_cron_1 = __importDefault(require("node-cron"));
const electron_1 = require("electron");
class ScheduleService {
    storageService;
    backupService;
    cronTasks = new Map();
    constructor(storageService, backupService) {
        this.storageService = storageService;
        this.backupService = backupService;
        this.initCronJobs();
    }
    initCronJobs() {
        const schedules = this.storageService.getSchedules();
        for (const schedule of schedules) {
            if (schedule.IsEnabled) {
                this.registerCronJob(schedule);
            }
        }
    }
    registerCronJob(schedule) {
        const id = schedule.Id || schedule.id;
        if (this.cronTasks.has(id)) {
            this.cronTasks.get(id)?.stop();
            this.cronTasks.delete(id);
        }
        if (!node_cron_1.default.validate(schedule.CronExpression || schedule.cronExpression)) {
            return;
        }
        const task = node_cron_1.default.schedule(schedule.CronExpression || schedule.cronExpression, async () => {
            await this.runScheduleBackup(schedule);
        });
        this.cronTasks.set(id, task);
    }
    async runScheduleBackup(schedule) {
        try {
            const result = await this.backupService.executeBackup({
                connectionId: schedule.ConnectionId || schedule.connectionId,
                databaseName: schedule.DatabaseName || schedule.databaseName,
                compress: schedule.Compress === 1 || schedule.compress,
                compressionType: schedule.CompressionType || schedule.compressionType || 'gz'
            }, schedule.Id || schedule.id);
            if (result.Status === 'Success') {
                this.sendWindowsNotification('MyNavicat 定時備份成功', `資料庫 [${schedule.DatabaseName}] 已自動備份完成！`);
                this.cleanUpRetainedBackups(schedule.ConnectionId || schedule.connectionId, schedule.RetainCount || schedule.retainCount || 7);
            }
            else {
                this.sendWindowsNotification('MyNavicat 定時備份失敗', `資料庫 [${schedule.DatabaseName}] 備份失敗: ${result.ErrorMessage || '未知錯誤'}`);
            }
        }
        catch (err) {
            this.sendWindowsNotification('MyNavicat 定時備份失敗', `資料庫 [${schedule.DatabaseName}] 發生例外: ${err.message}`);
        }
    }
    cleanUpRetainedBackups(connectionId, retainCount) {
        const histories = this.storageService.getBackupHistories(connectionId, 'Success');
        if (histories.length > retainCount) {
            const toDelete = histories.slice(retainCount);
            for (const history of toDelete) {
                this.backupService['storageService'].deleteBackupHistory(history.Id);
            }
        }
    }
    sendWindowsNotification(title, body) {
        if (electron_1.Notification.isSupported()) {
            new electron_1.Notification({
                title,
                body,
                silent: false
            }).show();
        }
    }
    toggleSchedule(id, isEnabled) {
        const schedules = this.storageService.getSchedules();
        const schedule = schedules.find(s => s.Id === id);
        if (!schedule)
            return false;
        schedule.IsEnabled = isEnabled ? 1 : 0;
        this.storageService.saveSchedule(schedule);
        if (isEnabled) {
            this.registerCronJob(schedule);
        }
        else {
            if (this.cronTasks.has(id)) {
                this.cronTasks.get(id)?.stop();
                this.cronTasks.delete(id);
            }
        }
        return true;
    }
}
exports.ScheduleService = ScheduleService;
