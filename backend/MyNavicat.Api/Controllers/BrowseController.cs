using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Services;

namespace MyNavicat.Api.Controllers
{
    [ApiController]
    [Route("api/browse")]
    public class BrowseController : ControllerBase
    {
        private readonly IDatabaseBrowseService _browseService;

        public BrowseController(IDatabaseBrowseService browseService)
        {
            _browseService = browseService;
        }

        [HttpGet("{connectionId}/databases")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetDatabases(int connectionId)
        {
            try
            {
                var result = await _browseService.GetDatabasesAsync(connectionId);
                return Ok(ApiResponse<List<string>>.Ok(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<List<string>>.Fail(ex.Message));
            }
        }

        [HttpGet("{connectionId}/{database}/tables")]
        public async Task<ActionResult<ApiResponse<List<TableInfoDto>>>> GetTables(int connectionId, string database)
        {
            try
            {
                var result = await _browseService.GetTablesAsync(connectionId, database);
                return Ok(ApiResponse<List<TableInfoDto>>.Ok(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<List<TableInfoDto>>.Fail(ex.Message));
            }
        }

        [HttpGet("{connectionId}/{database}/{table}/schema")]
        public async Task<ActionResult<ApiResponse<TableSchemaDto>>> GetTableSchema(int connectionId, string database, string table)
        {
            try
            {
                var result = await _browseService.GetTableSchemaAsync(connectionId, database, table);
                return Ok(ApiResponse<TableSchemaDto>.Ok(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<TableSchemaDto>.Fail(ex.Message));
            }
        }

        [HttpGet("{connectionId}/{database}/{table}/data")]
        public async Task<ActionResult<ApiResponse<PagedResult<Dictionary<string, object?>>>>> GetTableData(
            int connectionId, string database, string table, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var result = await _browseService.GetTableDataAsync(connectionId, database, table, page, pageSize);
                return Ok(ApiResponse<PagedResult<Dictionary<string, object?>>>.Ok(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<PagedResult<Dictionary<string, object?>>>.Fail(ex.Message));
            }
        }

        [HttpGet("{connectionId}/{database}/{table}/count")]
        public async Task<ActionResult<ApiResponse<long>>> GetTableCount(int connectionId, string database, string table)
        {
            try
            {
                var result = await _browseService.GetTableCountAsync(connectionId, database, table);
                return Ok(ApiResponse<long>.Ok(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<long>.Fail(ex.Message));
            }
        }
    }
}
