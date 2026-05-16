using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Demo.Database.Models.publicSchema;
using SynchronizationTool.Demo.Dto;

namespace SynchronizationTool.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StateTypesController : ControllerBase
    {
        private readonly DemoContext _context;
        public StateTypesController(DemoContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StateTypeDto>>> GetStateTypes() =>
            Ok(await _context.StateTypes.Select(st => new StateTypeDto
            {
                Id = st.Id,
                Code = st.Code,
                ChangeCommand = st.ChangeCommand,
                DeviceTypeId = st.DeviceTypeId
            }).ToListAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<StateTypeDto>> GetStateType(Guid id)
        {
            var st = await _context.StateTypes.FindAsync(id);
            if (st == null) return NotFound();
            return Ok(new StateTypeDto
            {
                Id = st.Id,
                Code = st.Code,
                ChangeCommand = st.ChangeCommand,
                DeviceTypeId = st.DeviceTypeId
            });
        }

        [HttpPost]
        public async Task<ActionResult<StateTypeDto>> CreateStateType(StateTypeCreateUpdateDto dto)
        {
            if (dto.Id == Guid.Empty) dto.Id = Guid.NewGuid();
            var st = new StateType
            {
                Id = dto.Id,
                Code = dto.Code,
                ChangeCommand = dto.ChangeCommand,
                DeviceTypeId = dto.DeviceTypeId
            };
            _context.StateTypes.Add(st);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetStateType), new { id = st.Id }, new StateTypeDto
            {
                Id = st.Id,
                Code = st.Code,
                ChangeCommand = st.ChangeCommand,
                DeviceTypeId = st.DeviceTypeId
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStateType(Guid id, StateTypeCreateUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var st = await _context.StateTypes.FindAsync(id);
            if (st == null) return NotFound();
            st.Code = dto.Code;
            st.ChangeCommand = dto.ChangeCommand;
            st.DeviceTypeId = dto.DeviceTypeId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStateType(Guid id)
        {
            var st = await _context.StateTypes.FindAsync(id);
            if (st == null) return NotFound();
            _context.StateTypes.Remove(st);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}