using Microsoft.EntityFrameworkCore;
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
            var incomingIds = request.RpcBucket.Changes.Select(c => Guid.Parse(c.Id)).ToList();

            var existingIds = await _synchronizationToolContext.ChangeLogs
                .Where(l => incomingIds.Contains(l.Id))
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);

            var newChanges = request.RpcBucket.Changes
                .Where(c => !existingIds.Contains(Guid.Parse(c.Id)))
                .Select(c => new ChangeLog
                {
                    Id = Guid.Parse(c.Id),
                    DateTime = c.DateTime.ToDateTime(),
                    Type = (ChangeType)c.Type,
                    EntityId = Guid.Parse(c.TableId),
                    RowId = Guid.Parse(c.EntityId),
                    ClientId = Guid.Parse(c.ClientId),
                    ClientVersion = c.ClientVersion,
                    Status = ChangeStatus.Pending,
                    Changes = c.Changes.Select(ch => new Change
                    {
                        Id = Guid.NewGuid(),
                        ColumnName = ch.ColumnName,
                        Value = ch.ColumnCase.HasFlag(ChangeModel.ColumnOneofCase.Value) ? ch.Value : null,
                        ChangeLogId = Guid.Parse(c.Id),
                    }).ToList()
                }).ToList();

            if (newChanges.Any())
            {
                await _synchronizationToolContext.ChangeLogs.AddRangeAsync(newChanges, cancellationToken);
                await _synchronizationToolContext.SaveChangesAsync(cancellationToken);
            }

            return new ResponseModel();
        }
    }
}
