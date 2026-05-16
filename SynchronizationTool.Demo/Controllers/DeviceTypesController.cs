using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Demo.Database.Models.publicSchema;
using SynchronizationTool.Demo.Dto;

namespace SynchronizationTool.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceTypesController : ControllerBase
    {
        private readonly DemoContext _context;
        public DeviceTypesController(DemoContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceTypeDto>>> GetDeviceTypes() =>
            Ok(await _context.DeviceTypes.Select(t => new DeviceTypeDto { Id = t.Id, Name = t.Name }).ToListAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceTypeDto>> GetDeviceType(Guid id)
        {
            var type = await _context.DeviceTypes.FindAsync(id);
            if (type == null) return NotFound();
            return Ok(new DeviceTypeDto { Id = type.Id, Name = type.Name });
        }

        [HttpPost]
        public async Task<ActionResult<DeviceTypeDto>> CreateDeviceType(DeviceTypeCreateUpdateDto dto)
        {
            if (dto.Id == Guid.Empty) dto.Id = Guid.NewGuid();
            var type = new DeviceType { Id = dto.Id, Name = dto.Name };
            _context.DeviceTypes.Add(type);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDeviceType), new { id = type.Id }, new DeviceTypeDto { Id = type.Id, Name = type.Name });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeviceType(Guid id, DeviceTypeCreateUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var type = await _context.DeviceTypes.FindAsync(id);
            if (type == null) return NotFound();
            type.Name = dto.Name;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeviceType(Guid id)
        {
            var type = await _context.DeviceTypes.FindAsync(id);
            if (type == null) return NotFound();
            _context.DeviceTypes.Remove(type);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}