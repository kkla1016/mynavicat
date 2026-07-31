using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Services;

namespace MyNavicat.Api.Controllers
{
    [ApiController]
    [Route("api/backup")]
    public class BackupController : ControllerBase
    {
        private readonly IBackupService _backupService;

        public BackupController(IBackupService backupService)
        {
            _backupService = backupService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BackupHistory>>> Backup([FromBody] BackupRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<BackupHistory>.Fail("Invalid model request"));

            try
            {
                var history = await _backupService.ExecuteBackupAsync(request);
                if (history.Status == "Failed")
                {
                    return BadRequest(ApiResponse<BackupHistory>.Fail(history.ErrorMessage ?? "Backup failed"));
                }
                return Ok(ApiResponse<BackupHistory>.Ok(history, "Backup completed successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BackupHistory>.Fail(ex.Message));
            }
        }

        [HttpPost("restore")]
        public async Task<ActionResult<ApiResponse<bool>>> Restore([FromBody] RestoreRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.Fail("Invalid restore request"));

            try
            {
                var success = await _backupService.ExecuteRestoreAsync(request);
                if (!success)
                    return BadRequest(ApiResponse<bool>.Fail("Restore process failed"));

                return Ok(ApiResponse<bool>.Ok(true, "Restore completed successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpPost("restore-file")]
        public async Task<ActionResult<ApiResponse<bool>>> RestoreFromFile(
            [FromForm] int targetConnectionId,
            [FromForm] string targetDatabaseName,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<bool>.Fail("Uploaded file is empty"));

            try
            {
                using var stream = file.OpenReadStream();
                var success = await _backupService.ExecuteRestoreFromFileAsync(targetConnectionId, targetDatabaseName, stream, file.FileName);
                if (!success)
                    return BadRequest(ApiResponse<bool>.Fail("File restore process failed"));

                return Ok(ApiResponse<bool>.Ok(true, "File restore completed successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<PagedResult<BackupHistory>>>> GetHistory(
            [FromQuery] int? connectionId,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _backupService.GetHistoryAsync(connectionId, status, page, pageSize);
            return Ok(ApiResponse<PagedResult<BackupHistory>>.Ok(result));
        }

        [HttpGet("history/{id}")]
        public async Task<ActionResult<ApiResponse<BackupHistory>>> GetHistoryById(int id)
        {
            var history = await _backupService.GetHistoryByIdAsync(id);
            if (history == null)
                return NotFound(ApiResponse<BackupHistory>.Fail("Backup history not found"));

            return Ok(ApiResponse<BackupHistory>.Ok(history));
        }

        [HttpDelete("history/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteHistory(int id)
        {
            var success = await _backupService.DeleteHistoryAsync(id);
            if (!success)
                return NotFound(ApiResponse<bool>.Fail("Backup history not found"));

            return Ok(ApiResponse<bool>.Ok(true, "Backup history deleted successfully"));
        }

        [HttpGet("history/{id}/download")]
        public async Task<IActionResult> DownloadBackupFile(int id)
        {
            try
            {
                var (fileStream, contentType, fileName) = await _backupService.GetBackupFileAsync(id);
                return File(fileStream, contentType, fileName);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
