using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Protos;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Database.Models;
using SynchronizationTool.Logic.gRPC;
using SynchronizationTool.Logic.Models;
using SynchronizationTool.Logic.Models.Queries;

namespace SynchronizationTool.Logic.Handlers.Queries
{
    public class ChangeLogQueryHandler(
        ILogger<ChangeLogQueryHandler> logger,
        ISynchronizationToolContext synchronizationToolContext,
        IClientChannelStorage clientChannelStorage
        ) : AbstractQueryHandler<ChangeLogQuery, ChangeBucket>(logger)
    {
        private readonly ISynchronizationToolContext _synchronizationToolContext = synchronizationToolContext;
        private readonly IClientChannelStorage _clientChannelStorage = clientChannelStorage;

        public override async Task<ResponseModel<ChangeBucket>> HandleAsync(ChangeLogQuery request, CancellationToken cancellationToken)
        {
            var client = await _synchronizationToolContext.SynchClients
                .FirstOrDefaultAsync(c => c.Id == request.ClientId, cancellationToken);

            var changeLogsTask = _synchronizationToolContext.ChangeLogs
                .Include(c => c.Changes)
                .Where(cl => cl.ClientId != request.ClientId);
            
            if (client?.LastChangeLogId is not null)
            {
                changeLogsTask = changeLogsTask
                    .Where(cl => cl.DateTime > _synchronizationToolContext.ChangeLogs.Find(client.LastChangeLogId)!.DateTime);
            }

            var changelogs = await changeLogsTask.ToListAsync(cancellationToken);

            var rpcRequest = new ChangeBucket();

            if (!changelogs.Any())
            {
                return new() 
                { 
                    Response = rpcRequest
                };
            }   

            changelogs.ForEach(cl => 
            {
                var changelog = new ChangeAtom
                {
                    Id = cl.Id.ToString(),
                    ClientId = cl.ClientId.ToString(),
                    DateTime = Timestamp.FromDateTime(DateTime.SpecifyKind(cl.DateTime, DateTimeKind.Utc)),
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

            return new()
            {
                Response = rpcRequest
            };
        }
    }
}
