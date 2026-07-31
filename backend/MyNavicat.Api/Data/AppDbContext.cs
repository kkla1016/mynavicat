using Microsoft.EntityFrameworkCore;
using MyNavicat.Api.Models.Entities;

namespace MyNavicat.Api.Data
{
    /// <summary>
    /// 應用程式資料庫內容類別 (SQLite)
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Connection> Connections => Set<Connection>();
        public DbSet<BackupHistory> BackupHistories => Set<BackupHistory>();
        public DbSet<BackupSchedule> BackupSchedules => Set<BackupSchedule>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置 Connection 實體
            modelBuilder.Entity<Connection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DbType).IsRequired().HasMaxLength(50);
            });

            // 配置 BackupHistory 實體
            modelBuilder.Entity<BackupHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Connection)
                      .WithMany()
                      .HasForeignKey(e => e.ConnectionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ParentBackup)
                      .WithMany(e => e.Chunks)
                      .HasForeignKey(e => e.ParentBackupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置 BackupSchedule 實體
            modelBuilder.Entity<BackupSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Connection)
                      .WithMany()
                      .HasForeignKey(e => e.ConnectionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
