using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Demo.Database.Models.publicSchema;
using SynchronizationTool.Demo.Dto;

namespace SynchronizationTool.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomesController : ControllerBase
    {
        private readonly DemoContext _context;
        public HomesController(DemoContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HomeDto>>> GetHomes() =>
            Ok(await _context.Homes.Select(h => new HomeDto { Id = h.Id, Name = h.Name }).ToListAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<HomeDto>> GetHome(Guid id)
        {
            var home = await _context.Homes.FindAsync(id);
            if (home == null) return NotFound();
            return Ok(new HomeDto { Id = home.Id, Name = home.Name });
        }

        [HttpPost]
        public async Task<ActionResult<HomeDto>> CreateHome(HomeCreateUpdateDto dto)
        {
            if (dto.Id == Guid.Empty) dto.Id = Guid.NewGuid();
            var home = new Home { Id = dto.Id, Name = dto.Name };
            _context.Homes.Add(home);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetHome), new { id = home.Id }, new HomeDto { Id = home.Id, Name = home.Name });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHome(Guid id, HomeCreateUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var home = await _context.Homes.FindAsync(id);
            if (home == null) return NotFound();
            home.Name = dto.Name;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHome(Guid id)
        {
            var home = await _context.Homes.FindAsync(id);
            if (home == null) return NotFound();
            _context.Homes.Remove(home);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Добавить пользователя в дом
        [HttpPost("{homeId}/users/{userId}")]
        public async Task<IActionResult> AddUserToHome(Guid homeId, Guid userId)
        {
            var home = await _context.Homes.Include(h => h.Users).FirstOrDefaultAsync(h => h.Id == homeId);
            var user = await _context.Users.FindAsync(userId);
            if (home == null || user == null) return NotFound();

            if (!home.Users.Any(u => u.Id == userId))
                home.Users.Add(user);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Удалить пользователя из дома
        [HttpDelete("{homeId}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromHome(Guid homeId, Guid userId)
        {
            var home = await _context.Homes.Include(h => h.Users).FirstOrDefaultAsync(h => h.Id == homeId);
            var user = home?.Users.FirstOrDefault(u => u.Id == userId);
            if (home == null || user == null) return NotFound();

            home.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}