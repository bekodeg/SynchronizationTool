using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Demo.Database.Models.publicSchema;
using SynchronizationTool.Demo.Dto;

namespace SynchronizationTool.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DemoContext _context;
        public UsersController(DemoContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers() =>
            Ok(await _context.Users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                Password = u.Password
            }).ToListAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Password = user.Password
            });
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(UserCreateUpdateDto dto)
        {
            if (dto.Id == Guid.Empty) dto.Id = Guid.NewGuid();
            var user = new User
            {
                Id = dto.Id,
                Email = dto.Email,
                Name = dto.Name,
                Password = dto.Password
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Password = user.Password
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UserCreateUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.Email = dto.Email;
            user.Name = dto.Name;
            user.Password = dto.Password;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Получить все дома пользователя
        [HttpGet("{userId}/homes")]
        public async Task<ActionResult<IEnumerable<HomeDto>>> GetUserHomes(Guid userId)
        {
            var user = await _context.Users.Include(u => u.Homes).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();
            return Ok(user.Homes.Select(h => new HomeDto { Id = h.Id, Name = h.Name }));
        }

        // Добавить дом пользователю
        [HttpPost("{userId}/homes/{homeId}")]
        public async Task<IActionResult> AddHomeToUser(Guid userId, Guid homeId)
        {
            var user = await _context.Users.Include(u => u.Homes).FirstOrDefaultAsync(u => u.Id == userId);
            var home = await _context.Homes.FindAsync(homeId);
            if (user == null || home == null) return NotFound();

            if (!user.Homes.Any(h => h.Id == homeId))
                user.Homes.Add(home);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Удалить дом у пользователя
        [HttpDelete("{userId}/homes/{homeId}")]
        public async Task<IActionResult> RemoveHomeFromUser(Guid userId, Guid homeId)
        {
            var user = await _context.Users.Include(u => u.Homes).FirstOrDefaultAsync(u => u.Id == userId);
            var home = user?.Homes.FirstOrDefault(h => h.Id == homeId);
            if (user == null || home == null) return NotFound();

            user.Homes.Remove(home);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}