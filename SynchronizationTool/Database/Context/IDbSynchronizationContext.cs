using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System.Transactions;
using static Grpc.Core.Metadata;

namespace SynchronizationTool.Database.Context
{
    public interface IDbSynchronizationContext
    {
        Type? FindEntityType(string tableName);

        IEntityType? FindEntityType(Type type);

        EntityEntry Entry(object entity);

        ValueTask<object?> FindAsync(Type type, CancellationToken cancellationToken, params object[] keys);

        EntityEntry Remove(object entity);

        ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default);

        EntityEntry Attach(object entity);

        Task<int> SaveChangesWithoutTrackingAsync(CancellationToken cancellationToken = default);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        IEnumerable<IEntityType> GetEntityTypes();
    }
}
