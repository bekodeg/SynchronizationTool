using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Database.Models;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Demo.Database.Models;
using SynchronizationTool.Logic.Models.Commads;
using SynchronizationTool.Logic.Models.Dto;

namespace SynchronizationTool.Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly DemoContext _context;
        private readonly IMediator _mediator;

        public ProductsController(DemoContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/products/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            // Генерация недостающих данных
            EnsureProductDefaults(product);

            // Id генерируется на клиенте или автоматически в БД
            if (product.Id == Guid.Empty)
            {
                product.Id = Guid.NewGuid();
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: api/products/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProduct(Guid id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("Id в URL и теле запроса не совпадают");
            }

            // Генерация недостающих данных при обновлении
            EnsureProductDefaults(product);

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Обновление свойств отслеживаемой сущности
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!ProductExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/products/generate-test-data/{count}
        [HttpPost("generate-test-data/{count:int}")]
        public async Task<ActionResult<int>> GenerateTestData(int count)
        {
            if (count <= 0)
                return BadRequest("Количество должно быть больше нуля");

            var random = new Random();
            var newProducts = new List<Product>();

            for (int i = 0; i < count; i++)
            {
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = GenerateRandomProductName(random),
                    Price = Math.Round((decimal)(random.NextDouble() * 1000 + 10), 2),
                    Stock = random.Next(0, 500)
                };
                newProducts.Add(product);
            }

            await _context.Products.AddRangeAsync(newProducts);
            await _context.SaveChangesAsync();

            return Ok(newProducts.Count);
        }

        [HttpGet("changelogs")]
        public async Task<ActionResult<IEnumerable<ChangeLogDto>>> GetChangeLogs()
        {
            var changeLogs = await _context.ChangeLogs
                .Include(cl => cl.Changes)
                .Include(cl => cl.Entity)
                .OrderByDescending(cl => cl.DateTime)
                .ToListAsync();

            var dtos = changeLogs.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        // POST: api/products/apply-changelogs-from-db
        [HttpPost("apply-changelogs-from-db")]
        public async Task<IActionResult> ApplyChangeLogsFromDatabase()
        {
            var changeLogs = await _context.ChangeLogs
                .Include(cl => cl.Changes)
                .Include(cl => cl.Entity)
                .ToListAsync();

            if (!changeLogs.Any())
                return Ok("No change logs to apply.");

            var dtos = changeLogs.Select(MapToDto).ToList();
            var command = new ApplyChangeLogsCommand { ChangeLogs = dtos };
            await _mediator.Send(command);

            return Ok($"Applied {dtos.Count} change logs from database.");
        }

        // POST: api/products/apply-changelogs-from-body
        [HttpPost("apply-changelogs-from-body")]
        public async Task<IActionResult> ApplyChangeLogsFromBody([FromBody] List<ChangeLogDto> changeLogDtos)
        {
            if (changeLogDtos == null || changeLogDtos.Count == 0)
                return BadRequest("Change logs list is empty.");

            var command = new ApplyChangeLogsCommand { ChangeLogs = changeLogDtos };
            await _mediator.Send(command);

            return Ok($"Applied {changeLogDtos.Count} change logs from request body.");
        }

        // Вспомогательный метод маппинга ChangeLog → ChangeLogDto
        private ChangeLogDto MapToDto(ChangeLog log)
        {
            return new ChangeLogDto
            {
                Id = log.Id,
                DateTime = log.DateTime,
                TableId = log.EntityId,
                EntityId = log.RowId,
                Type = log.Type,
                Changes = log.Changes.Select(c => new ChangeDto
                {
                    ColumnName = c.ColumnName,
                    Value = c.Value
                }).ToList(),
                ClientVersion = log.ClientVersion,
                ClientId = log.ClientId
            };
        }

        // Вспомогательный метод для генерации значений по умолчанию
        private void EnsureProductDefaults(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                product.Name = GenerateRandomProductName(new Random());
            }

            if (product.Price < 0)
            {
                product.Price = 0.01m;
            }

            if (product.Stock < 0)
            {
                product.Stock = 0;
            }
        }

        private string GenerateRandomProductName(Random random)
        {
            var adjectives = new[] { "Amazing", "Premium", "Eco", "Smart", "Ultra", "Classic", "Modern", "Deluxe" };
            var nouns = new[] { "Widget", "Gadget", "Device", "Tool", "Appliance", "Instrument", "Component" };

            return $"{adjectives[random.Next(adjectives.Length)]} {nouns[random.Next(nouns.Length)]} " +
                   $"{random.Next(1000, 9999)}";
        }

        private bool ProductExists(Guid id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}