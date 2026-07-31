import { contextBridge, ipcRenderer } from 'electron';

const electronAPI = {
  ping: () => ipcRenderer.invoke('app:ping'),

  // 視窗控制
  minimizeWindow: () => ipcRenderer.send('window:minimize'),
  maximizeWindow: () => ipcRenderer.send('window:maximize'),
  closeWindow: () => ipcRenderer.send('window:close'),

  // 連線管理
  getConnections: () => ipcRenderer.invoke('connection:get-all'),
  saveConnection: (connection: any) => ipcRenderer.invoke('connection:save', connection),
  deleteConnection: (id: number) => ipcRenderer.invoke('connection:delete', id),
  testConnection: (connection: any) => ipcRenderer.invoke('connection:test', connection),

  // 瀏覽中繼資料
  getDatabases: (connectionId: number) => ipcRenderer.invoke('browse:get-databases', connectionId),
  getTables: (connectionId: number, database: string) => ipcRenderer.invoke('browse:get-tables', connectionId, database),
  getTableSchema: (connectionId: number, database: string, table: string) => ipcRenderer.invoke('browse:get-schema', connectionId, database, table),
  getTableData: (connectionId: number, database: string, table: string, page: number, pageSize: number) => ipcRenderer.invoke('browse:get-data', connectionId, database, table, page, pageSize),

  // 備份與還原
  executeBackup: (request: any) => ipcRenderer.invoke('backup:execute', request),
  executeRestore: (request: any) => ipcRenderer.invoke('backup:restore', request),
  getBackupHistories: (connectionId?: number, status?: string) => ipcRenderer.invoke('backup:get-histories', connectionId, status),
  deleteBackupHistory: (id: number) => ipcRenderer.invoke('backup:delete-history', id),

  // 排程管理
  getSchedules: () => ipcRenderer.invoke('schedule:get-all'),
  saveSchedule: (schedule: any) => ipcRenderer.invoke('schedule:save', schedule),
  deleteSchedule: (id: number) => ipcRenderer.invoke('schedule:delete', id),
  toggleSchedule: (id: number, isEnabled: boolean) => ipcRenderer.invoke('schedule:toggle', id, isEnabled),
  runScheduleNow: (id: number) => ipcRenderer.invoke('schedule:run-now', id),

  // 工具監控與匯出入
  getToolsStatus: () => ipcRenderer.invoke('tools:get-status'),
  exportTableData: (request: any) => ipcRenderer.invoke('export-import:export', request),
  importTableData: (request: any) => ipcRenderer.invoke('export-import:import', request)
};

contextBridge.exposeInMainWorld('electronAPI', electronAPI);

export type ElectronAPI = typeof electronAPI;
