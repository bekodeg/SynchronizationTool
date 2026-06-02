using MediatR;
using Microsoft.Extensions.Logging;
using SynchronizationTool.Logic.Models;

namespace SynchronizationTool.Logic.Handlers
{
    public abstract class AbstractQueryHandler<TRequest, TResponce>(ILogger logger) : IRequestHandler<TRequest, ResponseModel<TResponce>> where TRequest : Query<TResponce>
    {
        
        protected readonly ILogger _logger = logger;

        public async Task<ResponseModel<TResponce>> Handle(TRequest request, CancellationToken cancellationToken)
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

        public abstract Task<ResponseModel<TResponce>> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
