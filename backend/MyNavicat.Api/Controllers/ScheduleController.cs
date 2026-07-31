using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Services;

namespace MyNavicat.Api.Controllers
{
    [ApiController]
    [Route("api/schedules")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> GetAll()
        {
            var result = await _scheduleService.GetAllAsync();
            return Ok(ApiResponse<List<ScheduleDto>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ScheduleDto>>> GetById(int id)
        {
            var result = await _scheduleService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<ScheduleDto>.Fail("Schedule not found"));

            return Ok(ApiResponse<ScheduleDto>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ScheduleDto>>> Create([FromBody] ScheduleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ScheduleDto>.Fail("Invalid model data"));

            var result = await _scheduleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ScheduleDto>.Ok(result, "Schedule created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ScheduleDto>>> Update(int id, [FromBody] ScheduleDto dto)
        {
            var result = await _scheduleService.UpdateAsync(id, dto);
            if (result == null)
                return NotFound(ApiResponse<ScheduleDto>.Fail("Schedule not found"));

            return Ok(ApiResponse<ScheduleDto>.Ok(result, "Schedule updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var success = await _scheduleService.DeleteAsync(id);
            if (!success)
                return NotFound(ApiResponse<bool>.Fail("Schedule not found"));

            return Ok(ApiResponse<bool>.Ok(true, "Schedule deleted successfully"));
        }

        [HttpPost("{id}/toggle")]
        public async Task<ActionResult<ApiResponse<bool>>> Toggle(int id)
        {
            var success = await _scheduleService.ToggleAsync(id);
            if (!success)
                return NotFound(ApiResponse<bool>.Fail("Schedule not found"));

            return Ok(ApiResponse<bool>.Ok(true, "Schedule toggled successfully"));
        }

        [HttpPost("{id}/run-now")]
        public async Task<ActionResult<ApiResponse<bool>>> RunNow(int id)
        {
            try
            {
                await _scheduleService.RunNowAsync(id);
                return Ok(ApiResponse<bool>.Ok(true, "Scheduled task triggered immediately"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
