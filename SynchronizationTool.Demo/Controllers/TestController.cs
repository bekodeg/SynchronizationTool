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
        public async Task<IActionResult> ApplyChangeLogsFromBody()
        {
            var command = new ApplyChangeLogsCommand();
            await _mediator.Send(command);

            return Ok($"Applied change logs from request body.");
        }
    }
}