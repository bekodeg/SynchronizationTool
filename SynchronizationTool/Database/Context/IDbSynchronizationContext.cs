using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Database.Models;

namespace SynchronizationTool.Database.Context
{
    public interface IDbSynchronizationContext
    {
        Task<int> SaveChangesWithoutTrackingAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken);

        DbSet<Entity> SyncEntities { get; }
        DbSet<ChangeLog> ChangeLogs { get; }
        DbSet<Change> Changes { get; }
    }
}
