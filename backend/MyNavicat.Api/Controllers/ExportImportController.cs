using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Services;

namespace MyNavicat.Api.Controllers
{
    [ApiController]
    [Route("api/export-import")]
    public class ExportImportController : ControllerBase
    {
        private readonly IExportImportService _exportImportService;

        public ExportImportController(IExportImportService exportImportService)
        {
            _exportImportService = exportImportService;
        }

        [HttpGet("formats")]
        public ActionResult<ApiResponse<List<string>>> GetFormats()
        {
            var formats = new List<string> { "CSV", "JSON", "SQL" };
            return Ok(ApiResponse<List<string>>.Ok(formats));
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportTable([FromBody] ExportRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid export request"));

            try
            {
                var (fileBytes, contentType, fileName) = await _exportImportService.ExportTableDataAsync(request);
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail(ex.Message));
            }
        }

        [HttpPost("import")]
        public async Task<ActionResult<ApiResponse<ImportResultDto>>> ImportTable(
            [FromForm] int connectionId,
            [FromForm] string database,
            [FromForm] string table,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<ImportResultDto>.Fail("Uploaded file is empty"));

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _exportImportService.ImportTableDataAsync(connectionId, database, table, stream, file.FileName);
                return Ok(ApiResponse<ImportResultDto>.Ok(result, "Import operation executed"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ImportResultDto>.Fail(ex.Message));
            }
        }
    }
}
