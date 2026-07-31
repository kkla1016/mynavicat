import { app, BrowserWindow, ipcMain, Tray, Menu, nativeImage, dialog } from 'electron';
import * as path from 'path';
import * as fs from 'fs';
import { StorageService } from '../services/storageService';
import { CryptoService } from '../services/cryptoService';
import { ToolDetectionService } from '../services/toolDetectionService';
import { BackupService } from '../services/backupService';
import { ScheduleService } from '../services/scheduleService';
import { DbProviderFactory } from '../providers/DbProviderFactory';

let mainWindow: BrowserWindow | null = null;
let tray: Tray | null = null;
let isQuitting = false;

// 初始化核心服務
const storageService = new StorageService();
const cryptoService = new CryptoService();
const toolDetectionService = new ToolDetectionService();
const backupService = new BackupService(storageService, cryptoService);
const scheduleService = new ScheduleService(storageService, backupService);
const providerFactory = new DbProviderFactory(cryptoService);

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1280,
    height: 800,
    minWidth: 1024,
    minHeight: 700,
    frame: false,
    backgroundColor: '#121824',
    show: false, // 等待內容載入完畢再顯示
    webPreferences: {
      preload: path.join(__dirname, '../preload/preload.js'),
      nodeIntegration: false,
      contextIsolation: true,
      sandbox: false
    }
  });

  const distIndexPath = path.join(__dirname, '../../frontend/dist/index.html');
  const isDev = process.env.NODE_ENV === 'development';

  if (isDev) {
    mainWindow.loadURL('http://localhost:5173').catch(() => {
      if (fs.existsSync(distIndexPath)) {
        mainWindow?.loadFile(distIndexPath);
      }
    });
  } else {
    if (fs.existsSync(distIndexPath)) {
      mainWindow.loadFile(distIndexPath);
    } else {
      mainWindow.loadURL('http://localhost:5173');
    }
  }

  mainWindow.once('ready-to-show', () => {
    mainWindow?.show();
  });

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
  let icon = nativeImage.createFromPath(iconSvgPath);
  if (icon.isEmpty()) {
    icon = nativeImage.createEmpty();
  }

  tray = new Tray(icon);
  tray.setToolTip('MyNavicat — SQL 資料庫管理與備份工具');

  const contextMenu = Menu.buildFromTemplate([
    {
      label: '顯示主視窗',
      click: () => {
        if (mainWindow) {
          mainWindow.show();
          mainWindow.focus();
        } else {
          createWindow();
        }
      }
    },
    { type: 'separator' },
    {
      label: '完全退出 MyNavicat',
      click: () => {
        isQuitting = true;
        app.quit();
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

app.whenReady().then(() => {
  createWindow();
  createTray();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});

app.on('before-quit', () => {
  isQuitting = true;
});

// IPC Handler 註冊

// 1. 視窗控制
ipcMain.on('window:minimize', () => mainWindow?.minimize());
ipcMain.on('window:maximize', () => {
  if (mainWindow?.isMaximized()) mainWindow.unmaximize();
  else mainWindow?.maximize();
});
ipcMain.on('window:close', () => mainWindow?.hide());
ipcMain.handle('app:ping', () => 'pong from electron main process');

// 2. 工具監控
ipcMain.handle('tools:get-status', () => toolDetectionService.getToolsStatus());

// 3. 連線管理
ipcMain.handle('connection:get-all', () => storageService.getConnections());
ipcMain.handle('connection:save', (_, conn) => {
  if (conn.password || conn.Password) {
    conn.EncryptedPassword = cryptoService.encrypt(conn.password || conn.Password);
  }
  return storageService.saveConnection(conn);
});
ipcMain.handle('connection:delete', (_, id) => storageService.deleteConnection(id));
ipcMain.handle('connection:test', async (_, conn) => {
  try {
    const provider = providerFactory.getProvider(conn.DbType || conn.dbType);
    return await provider.testConnection(conn);
  } catch {
    return false;
  }
});

// 4. 瀏覽中繼資料
ipcMain.handle('browse:get-databases', async (_, connectionId) => {
  const conn = storageService.getConnectionById(connectionId);
  if (!conn) return [];
  const provider = providerFactory.getProvider(conn.DbType);
  return await provider.getDatabases(conn);
});

ipcMain.handle('browse:get-tables', async (_, connectionId, database) => {
  const conn = storageService.getConnectionById(connectionId);
  if (!conn) return [];
  const provider = providerFactory.getProvider(conn.DbType);
  return await provider.getTables(conn, database);
});

ipcMain.handle('browse:get-schema', async (_, connectionId, database, table) => {
  const conn = storageService.getConnectionById(connectionId);
  if (!conn) return null;
  const provider = providerFactory.getProvider(conn.DbType);
  return await provider.getTableSchema(conn, database, table);
});

ipcMain.handle('browse:get-data', async (_, connectionId, database, table, page, pageSize) => {
  const conn = storageService.getConnectionById(connectionId);
  if (!conn) return { items: [], page: 1, pageSize: 20, totalCount: 0 };
  const provider = providerFactory.getProvider(conn.DbType);
  return await provider.getTableData(conn, database, table, page, pageSize);
});

// 5. 備份與還原
ipcMain.handle('backup:execute', (_, request) => backupService.executeBackup(request));
ipcMain.handle('backup:get-histories', (_, connectionId, status) => storageService.getBackupHistories(connectionId, status));
ipcMain.handle('backup:delete-history', (_, id) => storageService.deleteBackupHistory(id));

// 6. 排程管理
ipcMain.handle('schedule:get-all', () => storageService.getSchedules());
ipcMain.handle('schedule:save', (_, schedule) => {
  const saved = storageService.saveSchedule(schedule);
  scheduleService.registerCronJob(saved);
  return saved;
});
ipcMain.handle('schedule:delete', (_, id) => storageService.deleteSchedule(id));
ipcMain.handle('schedule:toggle', (_, id, isEnabled) => scheduleService.toggleSchedule(id, isEnabled));
ipcMain.handle('schedule:run-now', async (_, id) => {
  const schedules = storageService.getSchedules();
  const schedule = schedules.find(s => s.Id === id);
  if (schedule) {
    await scheduleService.runScheduleBackup(schedule);
    return true;
  }
  return false;
});

// 7. 匯出入對話框與執行
ipcMain.handle('export-import:export', async (_, request) => {
  if (!mainWindow) return { success: false };
  const ext = (request.format || 'CSV').toLowerCase();
  const { filePath } = await dialog.showSaveDialog(mainWindow, {
    title: '匯出資料表檔案',
    defaultPath: `${request.tableName}.${ext}`,
    filters: [{ name: request.format, extensions: [ext] }]
  });

  if (!filePath) return { success: false };

  const conn = storageService.getConnectionById(request.connectionId);
  const provider = providerFactory.getProvider(conn.DbType);
  const pagedData = await provider.getTableData(conn, request.databaseName, request.tableName, 1, 10000);
  const rows = pagedData.items;

  let content = '';
  if (ext === 'json') {
    content = JSON.stringify(rows, null, 2);
  } else if (ext === 'sql') {
    if (rows.length > 0) {
      const cols = Object.keys(rows[0]);
      content = rows.map(r => {
        const vals = cols.map(c => typeof r[c] === 'string' ? `'${r[c].replace(/'/g, "''")}'` : r[c] ?? 'NULL');
        return `INSERT INTO \`${request.tableName}\` (${cols.map(c => `\`${c}\``).join(', ')}) VALUES (${vals.join(', ')});`;
      }).join('\n');
    }
  } else {
    if (rows.length > 0) {
      const cols = Object.keys(rows[0]);
      content = cols.join(',') + '\n' + rows.map(r => cols.map(c => `"${r[c] ?? ''}"`).join(',')).join('\n');
    }
  }

  fs.writeFileSync(filePath, content, 'utf8');
  return { success: true, filePath };
});

ipcMain.handle('export-import:import', async (_, request) => {
  if (!mainWindow) return { success: false };
  const { filePaths } = await dialog.showOpenDialog(mainWindow, {
    title: '選擇要匯入的檔案',
    filters: [{ name: 'Data Files', extensions: ['csv', 'json'] }],
    properties: ['openFile']
  });

  if (!filePaths || filePaths.length === 0) return { success: false };
  return { success: true, data: { successRows: 0, failedRows: 0 } };
});
