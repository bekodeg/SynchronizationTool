using MediatR;
using Microsoft.Extensions.Logging;

namespace SynchronizationTool.Logic.Models.Commads
{
    public class TrackInsertBehavior(ILogger<TrackInsertBehavior> Logger) 
        : IPipelineBehavior<ITrackInsertRsponse, ResponseModel>
    {
        public Task<ResponseModel> Handle(ITrackInsertRsponse request, RequestHandlerDelegate<ResponseModel> next, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
