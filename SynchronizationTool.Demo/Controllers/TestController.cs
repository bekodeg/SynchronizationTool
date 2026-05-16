using MediatR;
using Microsoft.AspNetCore.Mvc;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Logic.Models.Commads;
using SynchronizationTool.Logic.Models.Dto;

namespace SynchronizationTool.Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly DemoContext _context;
        private readonly IMediator _mediator;

        public TestController(DemoContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
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
    }
}