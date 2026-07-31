using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Services;

namespace MyNavicat.Api.Controllers
{
    [ApiController]
    [Route("api/connections")]
    public class ConnectionController : ControllerBase
    {
        private readonly IConnectionService _connectionService;

        public ConnectionController(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ConnectionDto>>>> GetAll()
        {
            var result = await _connectionService.GetAllAsync();
            return Ok(ApiResponse<List<ConnectionDto>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ConnectionDto>>> GetById(int id)
        {
            var result = await _connectionService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<ConnectionDto>.Fail("Connection not found"));

            return Ok(ApiResponse<ConnectionDto>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ConnectionDto>>> Create([FromBody] ConnectionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ConnectionDto>.Fail("Invalid model data"));

            var result = await _connectionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ConnectionDto>.Ok(result, "Connection created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ConnectionDto>>> Update(int id, [FromBody] ConnectionDto dto)
        {
            var result = await _connectionService.UpdateAsync(id, dto);
            if (result == null)
                return NotFound(ApiResponse<ConnectionDto>.Fail("Connection not found"));

            return Ok(ApiResponse<ConnectionDto>.Ok(result, "Connection updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var success = await _connectionService.DeleteAsync(id);
            if (!success)
                return NotFound(ApiResponse<bool>.Fail("Connection not found"));

            return Ok(ApiResponse<bool>.Ok(true, "Connection deleted successfully"));
        }

        [HttpPost("{id}/test")]
        public async Task<ActionResult<ApiResponse<bool>>> TestConnection(int id)
        {
            var success = await _connectionService.TestConnectionAsync(id);
            if (!success)
                return Ok(ApiResponse<bool>.Fail("Connection test failed"));

            return Ok(ApiResponse<bool>.Ok(true, "Connection test successful"));
        }

        [HttpPost("test-dto")]
        public async Task<ActionResult<ApiResponse<bool>>> TestConnectionDto([FromBody] ConnectionDto dto)
        {
            var success = await _connectionService.TestConnectionDtoAsync(dto);
            if (!success)
                return Ok(ApiResponse<bool>.Fail("Connection test failed"));

            return Ok(ApiResponse<bool>.Ok(true, "Connection test successful"));
        }
    }
}
