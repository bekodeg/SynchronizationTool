using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Demo.Database.Models.publicSchema;
using SynchronizationTool.Demo.Dto;

namespace SynchronizationTool.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly DemoContext _context;

        public DevicesController(DemoContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetDevices()
        {
            var devices = await _context.Devices
                .Select(d => new DeviceDto
                {
                    Id = d.Id,
                    MqttCode = d.MqttCode,
                    Name = d.Name,
                    DeviceTypeId = d.DeviceTypeId,
                    HomeId = d.HomeId
                })
                .ToListAsync();

            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceDto>> GetDevice(Guid id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            return Ok(new DeviceDto
            {
                Id = device.Id,
                MqttCode = device.MqttCode,
                Name = device.Name,
                DeviceTypeId = device.DeviceTypeId,
                HomeId = device.HomeId
            });
        }

        [HttpPost]
        public async Task<ActionResult<DeviceDto>> CreateDevice(DeviceCreateUpdateDto dto)
        {
            if (dto.Id == Guid.Empty)
                dto.Id = Guid.NewGuid();

            var device = new Device
            {
                Id = dto.Id,
                MqttCode = dto.MqttCode,
                Name = dto.Name,
                DeviceTypeId = dto.DeviceTypeId,
                HomeId = dto.HomeId
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, new DeviceDto
            {
                Id = device.Id,
                MqttCode = device.MqttCode,
                Name = device.Name,
                DeviceTypeId = device.DeviceTypeId,
                HomeId = device.HomeId
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(Guid id, DeviceCreateUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            device.MqttCode = dto.MqttCode;
            device.Name = dto.Name;
            device.DeviceTypeId = dto.DeviceTypeId;
            device.HomeId = dto.HomeId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(Guid id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}