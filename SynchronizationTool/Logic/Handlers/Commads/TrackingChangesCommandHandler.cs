using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Database.Models;
using SynchronizationTool.Database.Models.Enums;
using SynchronizationTool.Logic.Models;
using SynchronizationTool.Logic.Models.Commads;

namespace SynchronizationTool.Logic.Handlers.Commads
{
    public class TrackingChangesCommandHandler(
        ILogger<TrackingChangesCommandHandler> logger,
        IOptions<SynchronisationConfiguration> config,
        ISynchronizationToolContext synchronizationToolContext
        ) : AbstractCommandHandler<TrackingChangesCommand>(logger)
    {
        private readonly SynchronisationConfiguration _config = config.Value;
        private readonly ISynchronizationToolContext _synchronizationToolContext = synchronizationToolContext;

        public override async Task<ResponseModel> HandleAsync(TrackingChangesCommand request, CancellationToken cancellationToken)
        {
            // Получаем все изменённые сущности отслеживаемых типов
            var changedEntries = request.Context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added
                         || e.State == EntityState.Modified
                         || e.State == EntityState.Deleted)
                .ToList();

            // Для каждой изменённой сущности создаём ChangeLog
            foreach (var entry in changedEntries)
            {
                var entityType = entry.Entity.GetType();

                var syncEntity = await _synchronizationToolContext.SyncEntities
                    .FirstOrDefaultAsync(e => e.Code == entry.Metadata.GetTableName(), cancellationToken);

                if (syncEntity == null)
                {
                    continue;
                }

                var entityIdProperty = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
                if (entityIdProperty == null) continue;

                var entityIdValue = entry.Property(entityIdProperty.Name).CurrentValue;
                if (entityIdValue is not Guid entityId) continue; // предполагаем, что все отслеживаемые сущности имеют Guid Id

                var changeLog = new ChangeLog
                {
                    Id = Guid.NewGuid(),
                    DateTime = DateTime.UtcNow,
                    Type = entry.State switch
                    {
                        EntityState.Added => ChangeType.Insert,
                        EntityState.Modified => ChangeType.Update,
                        EntityState.Deleted => ChangeType.Delete,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    Status = ChangeStatus.Pending,
                    EntityId = syncEntity.Id,
                    RowId = (Guid)entityIdValue,
                    ClientId = _config.ClientId,
                    ClientVersion = _config.CurrentClientVersion,
                    Changes = new List<Change>()
                };

                // Заполняем список изменений (колонка -> новое значение)
                if (entry.State == EntityState.Added)
                {
                    foreach (var property in entry.Properties.Where(p => !p.Metadata.IsPrimaryKey()))
                    {
                        changeLog.Changes.Add(new Change
                        {
                            ColumnName = property.Metadata.Name,
                            Value = property.CurrentValue?.ToString() ?? "NULL",
                            ChangeLogId = changeLog.Id
                        });
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    foreach (var property in entry.Properties.Where(p => p.IsModified && !p.Metadata.IsPrimaryKey()))
                    {
                        changeLog.Changes.Add(new Change
                        {
                            ColumnName = property.Metadata.Name,
                            Value = property.CurrentValue?.ToString() ?? "NULL",
                            ChangeLogId = changeLog.Id
                        });
                    }
                }

                await _synchronizationToolContext.ChangeLogs.AddAsync(changeLog);
            }

            await _synchronizationToolContext.SaveChangesAsync();

            return new();
        }
    }
}