using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyNavicat.Api.Data;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Providers;

namespace MyNavicat.Api.Services
{
    public interface IConnectionService
    {
        Task<List<ConnectionDto>> GetAllAsync();
        Task<ConnectionDto?> GetByIdAsync(int id);
        Task<ConnectionDto> CreateAsync(ConnectionDto dto);
        Task<ConnectionDto?> UpdateAsync(int id, ConnectionDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> TestConnectionAsync(int id);
        Task<bool> TestConnectionDtoAsync(ConnectionDto dto);
    }

    /// <summary>
    /// 資料庫連線管理服務實作
    /// </summary>
    public class ConnectionService : IConnectionService
    {
        private readonly AppDbContext _db;
        private readonly ICryptoService _crypto;
        private readonly IDbProviderFactory _providerFactory;

        public ConnectionService(AppDbContext db, ICryptoService crypto, IDbProviderFactory providerFactory)
        {
            _db = db;
            _crypto = crypto;
            _providerFactory = providerFactory;
        }

        public async Task<List<ConnectionDto>> GetAllAsync()
        {
            var connections = await _db.Connections.AsNoTracking().ToListAsync();
            return connections.Select(MapToDto).ToList();
        }

        public async Task<ConnectionDto?> GetByIdAsync(int id)
        {
            var conn = await _db.Connections.FindAsync(id);
            return conn == null ? null : MapToDto(conn);
        }

        public async Task<ConnectionDto> CreateAsync(ConnectionDto dto)
        {
            var entity = new Connection
            {
                Name = dto.Name,
                DbType = dto.DbType,
                Host = dto.Host,
                Port = dto.Port,
                DatabaseName = dto.DatabaseName,
                Username = dto.Username,
                EncryptedPassword = _crypto.Encrypt(dto.Password),
                SshHost = dto.SshHost,
                SshPort = dto.SshPort,
                SshUsername = dto.SshUsername,
                SshKeyPath = dto.SshKeyPath,
                ExtraParams = dto.ExtraParams,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Connections.Add(entity);
            await _db.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<ConnectionDto?> UpdateAsync(int id, ConnectionDto dto)
        {
            var entity = await _db.Connections.FindAsync(id);
            if (entity == null) return null;

            entity.Name = dto.Name;
            entity.DbType = dto.DbType;
            entity.Host = dto.Host;
            entity.Port = dto.Port;
            entity.DatabaseName = dto.DatabaseName;
            entity.Username = dto.Username;

            if (!string.IsNullOrEmpty(dto.Password) && dto.Password != "********")
            {
                entity.EncryptedPassword = _crypto.Encrypt(dto.Password);
            }

            entity.SshHost = dto.SshHost;
            entity.SshPort = dto.SshPort;
            entity.SshUsername = dto.SshUsername;
            entity.SshKeyPath = dto.SshKeyPath;
            entity.ExtraParams = dto.ExtraParams;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return MapToDto(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Connections.FindAsync(id);
            if (entity == null) return false;

            _db.Connections.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TestConnectionAsync(int id)
        {
            var entity = await _db.Connections.FindAsync(id);
            if (entity == null) return false;

            var provider = _providerFactory.GetProvider(entity.DbType);
            return await provider.TestConnectionAsync(entity);
        }

        public async Task<bool> TestConnectionDtoAsync(ConnectionDto dto)
        {
            var tempEntity = new Connection
            {
                Host = dto.Host,
                Port = dto.Port,
                Username = dto.Username,
                EncryptedPassword = _crypto.Encrypt(dto.Password),
                DatabaseName = dto.DatabaseName,
                DbType = dto.DbType
            };

            var provider = _providerFactory.GetProvider(dto.DbType);
            return await provider.TestConnectionAsync(tempEntity);
        }

        private ConnectionDto MapToDto(Connection entity)
        {
            return new ConnectionDto
            {
                Id = entity.Id,
                Name = entity.Name,
                DbType = entity.DbType,
                Host = entity.Host,
                Port = entity.Port,
                DatabaseName = entity.DatabaseName,
                Username = entity.Username,
                Password = "********", // 讀取時隱藏真實密碼
                SshHost = entity.SshHost,
                SshPort = entity.SshPort,
                SshUsername = entity.SshUsername,
                SshKeyPath = entity.SshKeyPath,
                ExtraParams = entity.ExtraParams,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
