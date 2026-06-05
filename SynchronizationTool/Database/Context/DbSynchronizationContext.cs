using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using SynchronizationTool.Configuration;
using SynchronizationTool.Logic.Models.Commads;
using System.Transactions;

namespace SynchronizationTool.Database.Context
{
    public partial class DbSynchronizationContext : DbContext, IDbSynchronizationContext
    {
        private readonly IMediator _mediator;

        public DbSynchronizationContext(IMediator mediator)
            : base()
        {
            _mediator = mediator;
        }

        public DbSynchronizationContext(DbContextOptions options, IMediator mediator, SynchronisationConfiguration synchronisationConfiguration)
            : base(options)
        {
            _mediator = mediator;
        }

        // Переопределение SaveChanges
        public override int SaveChanges(bool acceptAllChangesOnSuccess) 
            => SaveChangesWithTrackingAsync(acceptAllChangesOnSuccess, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

        public override int SaveChanges()
            => SaveChangesWithTrackingAsync(true, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default) 
            => await SaveChangesWithTrackingAsync(acceptAllChangesOnSuccess, cancellationToken);

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await SaveChangesWithTrackingAsync(true, cancellationToken);
            

        private async Task<int> SaveChangesWithTrackingAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
        {
            await _mediator.Send(new TrackingChangesCommand()
            {
                Context = this
            });

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            return result;
        }

        public async Task<int> SaveChangesWithoutTrackingAsync(CancellationToken cancellationToken)
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }

        public Type? FindEntityType(string tableName) => Model
            .GetEntityTypes()
            .FirstOrDefault(et => et.GetTableName() == tableName)?.ClrType;

        public IEntityType? FindEntityType(Type type) 
            => Model.FindEntityType(type);

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
            => await Database.BeginTransactionAsync(cancellationToken);
    }
}