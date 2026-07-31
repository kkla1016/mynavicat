export interface IElectronAPI {
  ping: () => Promise<string>;
  minimizeWindow: () => void;
  maximizeWindow: () => void;
  closeWindow: () => void;

  getConnections: () => Promise<any[]>;
  saveConnection: (connection: any) => Promise<any>;
  deleteConnection: (id: number) => Promise<boolean>;
  testConnection: (connection: any) => Promise<boolean>;

  getDatabases: (connectionId: number) => Promise<string[]>;
  getTables: (connectionId: number, database: string) => Promise<any[]>;
  getTableSchema: (connectionId: number, database: string, table: string) => Promise<any>;
  getTableData: (connectionId: number, database: string, table: string, page: number, pageSize: number) => Promise<any>;

  executeBackup: (request: any) => Promise<any>;
  executeRestore: (request: any) => Promise<boolean>;
  getBackupHistories: (connectionId?: number, status?: string) => Promise<any>;
  deleteBackupHistory: (id: number) => Promise<boolean>;

  getSchedules: () => Promise<any[]>;
  saveSchedule: (schedule: any) => Promise<any>;
  deleteSchedule: (id: number) => Promise<boolean>;
  toggleSchedule: (id: number, isEnabled: boolean) => Promise<boolean>;
  runScheduleNow: (id: number) => Promise<boolean>;

  getToolsStatus: () => Promise<any[]>;
  exportTableData: (request: any) => Promise<any>;
  importTableData: (request: any) => Promise<any>;
}

declare global {
  interface Window {
    electronAPI: IElectronAPI;
  }
}
