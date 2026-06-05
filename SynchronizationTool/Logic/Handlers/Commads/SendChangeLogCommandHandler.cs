using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Protos;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Logic.Models;
using SynchronizationTool.Logic.Models.Commads;
using static Protos.SynchronisationService;

namespace SynchronizationTool.Logic.Handlers.Commads
{
    public class SendChangeLogCommandHandler(
        ILogger<SendChangeLogCommandHandler> logger,
        ISynchronizationToolContext synchronizationToolContext,
        SynchronisationServiceClient synchronisationService
        ) : AbstractCommandHandler<SendChangeLogCommand>(logger)
    {
        private readonly SynchronisationServiceClient _synchronisationService = synchronisationService;
        private readonly ISynchronizationToolContext _synchronizationToolContext = synchronizationToolContext;

        public override async Task<ResponseModel> HandleAsync(SendChangeLogCommand request, CancellationToken cancellationToken)
        {
            var state = await _synchronizationToolContext.SynchStates
                .Include(s => s.SynchClient)
                .Include(s => s.ChangeLog)
                .FirstOrDefaultAsync(s => s.ClientId == request.ClientId, cancellationToken);

            var changeLogsTask = _synchronizationToolContext.ChangeLogs
                .Where(cl => cl.ClientId != request.ClientId);
            
            if (state is not null)
            {
                changeLogsTask = changeLogsTask
                    .Where(cl => cl.DateTime > state.ChangeLog.DateTime);
            }

            var changelogs = await changeLogsTask.ToListAsync(cancellationToken);

            if (!changelogs.Any())
            {
                return new ResponseModel();
            }

            var rpcRequest = new ChangeBucket();

            changelogs.ForEach(cl => 
            {
                var changelog = new ChangeAtom
                {
                    Id = cl.Id.ToString(),
                    ClientId = cl.ClientId.ToString(),
                    DateTime = Timestamp.FromDateTime(cl.DateTime),
                    Type = (int)cl.Type,
                    EntityId = cl.RowId.ToString(),
                    TableId = cl.EntityId.ToString(),
                    ClientVersion = cl.ClientVersion,
                      
                };

                foreach (var propChange in cl.Changes)
                {
                    var change = new ChangeModel
                    {
                        ColumnName = propChange.ColumnName,
                    };

                    if (propChange.Value is not null)
                    {
                        change.Value = propChange.Value;
                    }
                    else
                    {
                        change.Null = new Empty();
                    }

                    changelog.Changes.Add(change);
                }

                rpcRequest.Changes.Add(changelog);
            });

            await _synchronisationService
                .SendChangeAsync(rpcRequest, cancellationToken: cancellationToken);

            return new();
        }
    }
}
