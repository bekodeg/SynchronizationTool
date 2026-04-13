using MediatR;
using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Models;
using SynchronizationTool.Logic.Models.Commads;

namespace SynchronizationTool.Database.Context
{
    public partial class DbSynchronizationContext : DbContext, IDbSynchronizationContext
    {
        private readonly IMediator _mediator;
        private readonly SynchronisationConfiguration _synchronisationConfiguration;

        public DbSynchronizationContext(IMediator mediator, SynchronisationConfiguration synchronisationConfiguration)
            : base()
        {
            _mediator = mediator;
            _synchronisationConfiguration = synchronisationConfiguration;
        }

        public DbSynchronizationContext(DbContextOptions options, IMediator mediator, SynchronisationConfiguration synchronisationConfiguration)
            : base(options)
        {
            _mediator = mediator;
            _synchronisationConfiguration = synchronisationConfiguration;
        }

        // Таблицы синхронизации
        public DbSet<Entity> SyncEntities { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }
        public DbSet<Change> Changes { get; set; }


        // Переопределение SaveChanges
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return SaveChangesWithTrackingAsync(acceptAllChangesOnSuccess, CancellationToken.None).GetAwaiter().GetResult();
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return await SaveChangesWithTrackingAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private async Task<int> SaveChangesWithTrackingAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
        {
            await _mediator.Send(new TrackingChangesCommand()
            {
                Context = this
            });

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация таблиц синхронизации
            modelBuilder.Entity<Entity>(entity =>
            {
                entity.ToTable("entity", _synchronisationConfiguration.SynchSchema);
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            modelBuilder.Entity<ChangeLog>(log =>
            {
                log.ToTable("ChangeLog", _synchronisationConfiguration.SynchSchema);
                log.HasKey(l => l.Id);
                log.HasOne(l => l.Entity)
                   .WithMany(e => e.ChangeLogs)
                   .HasForeignKey(l => l.EntityId);
            });

            modelBuilder.Entity<Change>(change =>
            {
                change.ToTable("Change", _synchronisationConfiguration.SynchSchema);
                change.HasKey(c => c.Id);
                change.Property(c => c.Id);
                change.HasOne(c => c.ChangeLog)
                      .WithMany(l => l.Changes)
                      .HasForeignKey(c => c.ChangeLogId);
            });
        }
    }
}