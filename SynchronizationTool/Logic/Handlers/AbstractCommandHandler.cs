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
            var res = await HandleAsync(request, cancellationToken);

            if (!res.IsError)
            {
                _logger.LogInformation("Хендлер {HandlerName} завершил обработку успешно", GetType().Name);
            }
            else
            {   
                _logger.LogError("Хендлер {HandlerName} завершил обработку с ошибкой: {Message}", GetType().Name, res.Message);
            }
            return res;
        }

        public abstract Task<ResponseModel> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
