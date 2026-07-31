"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const electron_1 = require("electron");
const electronAPI = {
    ping: () => electron_1.ipcRenderer.invoke('app:ping'),
    // 視窗控制
    minimizeWindow: () => electron_1.ipcRenderer.send('window:minimize'),
    maximizeWindow: () => electron_1.ipcRenderer.send('window:maximize'),
    closeWindow: () => electron_1.ipcRenderer.send('window:close'),
    // 連線管理
    getConnections: () => electron_1.ipcRenderer.invoke('connection:get-all'),
    saveConnection: (connection) => electron_1.ipcRenderer.invoke('connection:save', connection),
    deleteConnection: (id) => electron_1.ipcRenderer.invoke('connection:delete', id),
    testConnection: (connection) => electron_1.ipcRenderer.invoke('connection:test', connection),
    // 瀏覽中繼資料
    getDatabases: (connectionId) => electron_1.ipcRenderer.invoke('browse:get-databases', connectionId),
    getTables: (connectionId, database) => electron_1.ipcRenderer.invoke('browse:get-tables', connectionId, database),
    getTableSchema: (connectionId, database, table) => electron_1.ipcRenderer.invoke('browse:get-schema', connectionId, database, table),
    getTableData: (connectionId, database, table, page, pageSize) => electron_1.ipcRenderer.invoke('browse:get-data', connectionId, database, table, page, pageSize),
    // 備份與還原
    executeBackup: (request) => electron_1.ipcRenderer.invoke('backup:execute', request),
    executeRestore: (request) => electron_1.ipcRenderer.invoke('backup:restore', request),
    getBackupHistories: (connectionId, status) => electron_1.ipcRenderer.invoke('backup:get-histories', connectionId, status),
    deleteBackupHistory: (id) => electron_1.ipcRenderer.invoke('backup:delete-history', id),
    // 排程管理
    getSchedules: () => electron_1.ipcRenderer.invoke('schedule:get-all'),
    saveSchedule: (schedule) => electron_1.ipcRenderer.invoke('schedule:save', schedule),
    deleteSchedule: (id) => electron_1.ipcRenderer.invoke('schedule:delete', id),
    toggleSchedule: (id, isEnabled) => electron_1.ipcRenderer.invoke('schedule:toggle', id, isEnabled),
    runScheduleNow: (id) => electron_1.ipcRenderer.invoke('schedule:run-now', id),
    // 工具監控與匯出入
    getToolsStatus: () => electron_1.ipcRenderer.invoke('tools:get-status'),
    exportTableData: (request) => electron_1.ipcRenderer.invoke('export-import:export', request),
    importTableData: (request) => electron_1.ipcRenderer.invoke('export-import:import', request)
};
electron_1.contextBridge.exposeInMainWorld('electronAPI', electronAPI);
