using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Services;

namespace MyNavicat.Api.Controllers
{
    [ApiController]
    [Route("api/tools")]
    public class ToolDetectionController : ControllerBase
    {
        private readonly IToolDetectionService _toolDetectionService;

        public ToolDetectionController(IToolDetectionService toolDetectionService)
        {
            _toolDetectionService = toolDetectionService;
        }

        /// <summary>
        /// 取得系統 CLI 工具偵測狀態
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<ApiResponse<List<ToolStatus>>>> GetToolStatus()
        {
            var tools = await _toolDetectionService.DetectToolsAsync();
            return Ok(ApiResponse<List<ToolStatus>>.Ok(tools));
        }
    }
}
