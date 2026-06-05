using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Database.Models;

namespace SynchronizationTool.Database.Context
{
    public interface ISynchronizationToolContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        DbSet<Entity> SyncEntities { get; }
        DbSet<ChangeLog> ChangeLogs { get; }
        DbSet<Change> Changes { get; }
        DbSet<SynchState> SynchStates { get; }
        DbSet<SynchClient> SynchClients { get; }
    }
}
