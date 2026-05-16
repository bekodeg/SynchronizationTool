using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Demo.Database.Models.publicSchema;
using SynchronizationTool.Demo.Dto;

namespace SynchronizationTool.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceStatesController : ControllerBase
    {
        private readonly DemoContext _context;
        public DeviceStatesController(DemoContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceStateDto>>> GetDeviceStates()
        {
            var states = await _context.DeviceStates
                .Select(s => new DeviceStateDto
                {
                    Id = s.Id,
                    DeviceId = s.DeviceId,
                    StateTypeId = s.StateTypeId,
                    Value = s.Value
                }).ToListAsync();
            return Ok(states);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceStateDto>> GetDeviceState(Guid id)
        {
            var state = await _context.DeviceStates.FindAsync(id);
            if (state == null) return NotFound();
            return Ok(new DeviceStateDto
            {
                Id = state.Id,
                DeviceId = state.DeviceId,
                StateTypeId = state.StateTypeId,
                Value = state.Value
            });
        }

        [HttpPost]
        public async Task<ActionResult<DeviceStateDto>> CreateDeviceState(DeviceStateCreateUpdateDto dto)
        {
            if (dto.Id == Guid.Empty) dto.Id = Guid.NewGuid();
            var state = new DeviceState
            {
                Id = dto.Id,
                DeviceId = dto.DeviceId,
                StateTypeId = dto.StateTypeId,
                Value = dto.Value
            };
            _context.DeviceStates.Add(state);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDeviceState), new { id = state.Id }, new DeviceStateDto
            {
                Id = state.Id,
                DeviceId = state.DeviceId,
                StateTypeId = state.StateTypeId,
                Value = state.Value
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeviceState(Guid id, DeviceStateCreateUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var state = await _context.DeviceStates.FindAsync(id);
            if (state == null) return NotFound();
            state.DeviceId = dto.DeviceId;
            state.StateTypeId = dto.StateTypeId;
            state.Value = dto.Value;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeviceState(Guid id)
        {
            var state = await _context.DeviceStates.FindAsync(id);
            if (state == null) return NotFound();
            _context.DeviceStates.Remove(state);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}