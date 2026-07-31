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
const electron_1 = require("electron");
const path = __importStar(require("path"));
const fs = __importStar(require("fs"));
const storageService_1 = require("../services/storageService");
const cryptoService_1 = require("../services/cryptoService");
const toolDetectionService_1 = require("../services/toolDetectionService");
const backupService_1 = require("../services/backupService");
const scheduleService_1 = require("../services/scheduleService");
const DbProviderFactory_1 = require("../providers/DbProviderFactory");
let mainWindow = null;
let tray = null;
let isQuitting = false;
// 初始化核心服務
const storageService = new storageService_1.StorageService();
const cryptoService = new cryptoService_1.CryptoService();
const toolDetectionService = new toolDetectionService_1.ToolDetectionService();
const backupService = new backupService_1.BackupService(storageService, cryptoService);
const scheduleService = new scheduleService_1.ScheduleService(storageService, backupService);
const providerFactory = new DbProviderFactory_1.DbProviderFactory(cryptoService);
function createWindow() {
    mainWindow = new electron_1.BrowserWindow({
        width: 1280,
        height: 800,
        minWidth: 1024,
        minHeight: 700,
        frame: false,
        backgroundColor: '#121824',
        webPreferences: {
            preload: path.join(__dirname, '../preload/preload.js'),
            nodeIntegration: false,
            contextIsolation: true,
            sandbox: false
        }
    });
    const isDev = process.env.NODE_ENV === 'development';
    if (isDev) {
        mainWindow.loadURL('http://localhost:5173');
    }
    else {
        mainWindow.loadFile(path.join(__dirname, '../../frontend/dist/index.html'));
    }
    mainWindow.on('close', (event) => {
        if (!isQuitting) {
            event.preventDefault();
            mainWindow?.hide();
        }
    });
    mainWindow.on('closed', () => {
        mainWindow = null;
    });
}
function createTray() {
    const iconSvgPath = path.join(__dirname, '../../frontend/public/favicon.svg');
    let icon = electron_1.nativeImage.createFromPath(iconSvgPath);
    if (icon.isEmpty()) {
        icon = electron_1.nativeImage.createEmpty();
    }
    tray = new electron_1.Tray(icon);
    tray.setToolTip('MyNavicat — SQL 資料庫管理與備份工具');
    const contextMenu = electron_1.Menu.buildFromTemplate([
        {
            label: '顯示主視窗',
            click: () => {
                if (mainWindow) {
                    mainWindow.show();
                    mainWindow.focus();
                }
                else {
                    createWindow();
                }
            }
        },
        { type: 'separator' },
        {
            label: '完全退出 MyNavicat',
            click: () => {
                isQuitting = true;
                electron_1.app.quit();
            }
        }
    ]);
    tray.setContextMenu(contextMenu);
    tray.on('double-click', () => {
        if (mainWindow) {
            mainWindow.show();
            mainWindow.focus();
        }
    });
}
electron_1.app.whenReady().then(() => {
    createWindow();
    createTray();
    electron_1.app.on('activate', () => {
        if (electron_1.BrowserWindow.getAllWindows().length === 0)
            createWindow();
    });
});
electron_1.app.on('before-quit', () => {
    isQuitting = true;
});
// IPC Handler 註冊
// 1. 視窗控制
electron_1.ipcMain.on('window:minimize', () => mainWindow?.minimize());
electron_1.ipcMain.on('window:maximize', () => {
    if (mainWindow?.isMaximized())
        mainWindow.unmaximize();
    else
        mainWindow?.maximize();
});
electron_1.ipcMain.on('window:close', () => mainWindow?.hide());
electron_1.ipcMain.handle('app:ping', () => 'pong from electron main process');
// 2. 工具監控
electron_1.ipcMain.handle('tools:get-status', () => toolDetectionService.getToolsStatus());
// 3. 連線管理
electron_1.ipcMain.handle('connection:get-all', () => storageService.getConnections());
electron_1.ipcMain.handle('connection:save', (_, conn) => {
    if (conn.password || conn.Password) {
        conn.EncryptedPassword = cryptoService.encrypt(conn.password || conn.Password);
    }
    return storageService.saveConnection(conn);
});
electron_1.ipcMain.handle('connection:delete', (_, id) => storageService.deleteConnection(id));
electron_1.ipcMain.handle('connection:test', async (_, conn) => {
    try {
        const provider = providerFactory.getProvider(conn.DbType || conn.dbType);
        return await provider.testConnection(conn);
    }
    catch {
        return false;
    }
});
// 4. 瀏覽中繼資料
electron_1.ipcMain.handle('browse:get-databases', async (_, connectionId) => {
    const conn = storageService.getConnectionById(connectionId);
    if (!conn)
        return [];
    const provider = providerFactory.getProvider(conn.DbType);
    return await provider.getDatabases(conn);
});
electron_1.ipcMain.handle('browse:get-tables', async (_, connectionId, database) => {
    const conn = storageService.getConnectionById(connectionId);
    if (!conn)
        return [];
    const provider = providerFactory.getProvider(conn.DbType);
    return await provider.getTables(conn, database);
});
electron_1.ipcMain.handle('browse:get-schema', async (_, connectionId, database, table) => {
    const conn = storageService.getConnectionById(connectionId);
    if (!conn)
        return null;
    const provider = providerFactory.getProvider(conn.DbType);
    return await provider.getTableSchema(conn, database, table);
});
electron_1.ipcMain.handle('browse:get-data', async (_, connectionId, database, table, page, pageSize) => {
    const conn = storageService.getConnectionById(connectionId);
    if (!conn)
        return { items: [], page: 1, pageSize: 20, totalCount: 0 };
    const provider = providerFactory.getProvider(conn.DbType);
    return await provider.getTableData(conn, database, table, page, pageSize);
});
// 5. 備份與還原
electron_1.ipcMain.handle('backup:execute', (_, request) => backupService.executeBackup(request));
electron_1.ipcMain.handle('backup:get-histories', (_, connectionId, status) => storageService.getBackupHistories(connectionId, status));
electron_1.ipcMain.handle('backup:delete-history', (_, id) => storageService.deleteBackupHistory(id));
// 6. 排程管理
electron_1.ipcMain.handle('schedule:get-all', () => storageService.getSchedules());
electron_1.ipcMain.handle('schedule:save', (_, schedule) => {
    const saved = storageService.saveSchedule(schedule);
    scheduleService.registerCronJob(saved);
    return saved;
});
electron_1.ipcMain.handle('schedule:delete', (_, id) => storageService.deleteSchedule(id));
electron_1.ipcMain.handle('schedule:toggle', (_, id, isEnabled) => scheduleService.toggleSchedule(id, isEnabled));
electron_1.ipcMain.handle('schedule:run-now', async (_, id) => {
    const schedules = storageService.getSchedules();
    const schedule = schedules.find(s => s.Id === id);
    if (schedule) {
        await scheduleService.runScheduleBackup(schedule);
        return true;
    }
    return false;
});
// 7. 匯出入對話框與執行
electron_1.ipcMain.handle('export-import:export', async (_, request) => {
    if (!mainWindow)
        return { success: false };
    const ext = (request.format || 'CSV').toLowerCase();
    const { filePath } = await electron_1.dialog.showSaveDialog(mainWindow, {
        title: '匯出資料表檔案',
        defaultPath: `${request.tableName}.${ext}`,
        filters: [{ name: request.format, extensions: [ext] }]
    });
    if (!filePath)
        return { success: false };
    const conn = storageService.getConnectionById(request.connectionId);
    const provider = providerFactory.getProvider(conn.DbType);
    const pagedData = await provider.getTableData(conn, request.databaseName, request.tableName, 1, 10000);
    const rows = pagedData.items;
    let content = '';
    if (ext === 'json') {
        content = JSON.stringify(rows, null, 2);
    }
    else if (ext === 'sql') {
        if (rows.length > 0) {
            const cols = Object.keys(rows[0]);
            content = rows.map(r => {
                const vals = cols.map(c => typeof r[c] === 'string' ? `'${r[c].replace(/'/g, "''")}'` : r[c] ?? 'NULL');
                return `INSERT INTO \`${request.tableName}\` (${cols.map(c => `\`${c}\``).join(', ')}) VALUES (${vals.join(', ')});`;
            }).join('\n');
        }
    }
    else {
        if (rows.length > 0) {
            const cols = Object.keys(rows[0]);
            content = cols.join(',') + '\n' + rows.map(r => cols.map(c => `"${r[c] ?? ''}"`).join(',')).join('\n');
        }
    }
    fs.writeFileSync(filePath, content, 'utf8');
    return { success: true, filePath };
});
electron_1.ipcMain.handle('export-import:import', async (_, request) => {
    if (!mainWindow)
        return { success: false };
    const { filePaths } = await electron_1.dialog.showOpenDialog(mainWindow, {
        title: '選擇要匯入的檔案',
        filters: [{ name: 'Data Files', extensions: ['csv', 'json'] }],
        properties: ['openFile']
    });
    if (!filePaths || filePaths.length === 0)
        return { success: false };
    return { success: true, data: { successRows: 0, failedRows: 0 } };
});
