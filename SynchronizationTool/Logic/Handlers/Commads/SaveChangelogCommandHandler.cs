using Microsoft.Extensions.Logging;
using Protos;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Database.Models;
using SynchronizationTool.Database.Models.Enums;
using SynchronizationTool.Logic.Models;
using SynchronizationTool.Logic.Models.Commads;

namespace SynchronizationTool.Logic.Handlers.Commads
{
    public class SaveChangelogCommandHandler(
        ILogger<SaveChangelogCommandHandler> logger,
        ISynchronizationToolContext synchronizationToolContext)
        : AbstractCommandHandler<SaveChangelogCommand>(logger)
    {
        private readonly ISynchronizationToolContext _synchronizationToolContext = synchronizationToolContext;

        public override async Task<ResponseModel> HandleAsync(SaveChangelogCommand request, CancellationToken cancellationToken)
        {
            List<ChangeLog> changes = request.RpcBucket.Changes.Select(c => new ChangeLog
            {
                Id = Guid.Parse(c.Id),
                DateTime = c.DateTime.ToDateTime(),
                Type = (ChangeType)c.Type,
                EntityId = Guid.Parse(c.EntityId),
                RowId = Guid.NewGuid(),
                ClientId = Guid.Parse(c.ClientId),
                ClientVersion = c.ClientVersion,
                Changes = c.Changes.Select(ch => new Change
                {
                    Id = Guid.NewGuid(),
                    ColumnName = ch.ColumnName,
                    Value = ch.ColumnCase.HasFlag(ChangeModel.ColumnOneofCase.Value) ? ch.Value : null,
                }).ToList()
            }).ToList();

            await _synchronizationToolContext.ChangeLogs.AddRangeAsync(changes);
            await _synchronizationToolContext.SaveChangesAsync(cancellationToken);

            return new();
        }
    }
}
