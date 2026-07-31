import { exec } from 'child_process';
import { promisify } from 'util';

const execAsync = promisify(exec);

export interface ToolStatus {
  name: string;
  executable: string;
  isAvailable: boolean;
  version: string;
  installGuideUrl: string;
}

export class ToolDetectionService {
  private readonly tools = [
    { name: 'MySQL Dump', executable: 'mysqldump', versionFlag: '--version', guide: 'https://dev.mysql.com/downloads/installer/' },
    { name: 'PostgreSQL Dump', executable: 'pg_dump', versionFlag: '--version', guide: 'https://www.postgresql.org/download/' },
    { name: 'SQL Server Command', executable: 'sqlcmd', versionFlag: '-?', guide: 'https://learn.microsoft.com/en-us/sql/tools/sqlcmd-utility' },
    { name: 'SQLite3 CLI', executable: 'sqlite3', versionFlag: '--version', guide: 'https://www.sqlite.org/download.html' },
    { name: 'Oracle Export', executable: 'exp', versionFlag: 'help=y', guide: 'https://www.oracle.com/database/technologies/instant-client/downloads.html' }
  ];

  public async getToolsStatus(): Promise<ToolStatus[]> {
    const results: ToolStatus[] = [];

    for (const tool of this.tools) {
      try {
        const { stdout, stderr } = await execAsync(`"${tool.executable}" ${tool.versionFlag}`);
        const output = (stdout || stderr).trim();
        const firstLine = output.split('\n')[0];

        results.push({
          name: tool.name,
          executable: tool.executable,
          isAvailable: true,
          version: firstLine.length > 50 ? firstLine.substring(0, 50) + '...' : firstLine,
          installGuideUrl: tool.guide
        });
      } catch {
        results.push({
          name: tool.name,
          executable: tool.executable,
          isAvailable: false,
          version: '未偵測到此 CLI 工具',
          installGuideUrl: tool.guide
        });
      }
    }

    return results;
  }
}
