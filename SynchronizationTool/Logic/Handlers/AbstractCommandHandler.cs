using MediatR;
using Microsoft.Extensions.Logging;
using SynchronizationTool.Logic.Models;

namespace SynchronizationTool.Logic.Handlers
{
    public abstract class AbstractCommandHandler<TRequest>(ILogger logger) : IRequestHandler<TRequest, ResponseModel> where TRequest : Command
    {
        
        protected readonly ILogger _logger = logger;

        public async Task<ResponseModel> Handle(TRequest request, CancellationToken cancellationToken)
        {
            return await HandleAsync(request, cancellationToken);
        }

        public abstract Task<ResponseModel> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
