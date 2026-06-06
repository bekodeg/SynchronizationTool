using MediatR;
using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Models;

namespace SynchronizationTool.Database.Context
{
    internal partial class SynchronizationToolContext : DbContext, ISynchronizationToolContext
    {
        private readonly SynchronisationConfiguration _synchronisationConfiguration;

        public SynchronizationToolContext(SynchronisationConfiguration synchronisationConfiguration)
            : base()
        {
            _synchronisationConfiguration = synchronisationConfiguration;
            Database.Migrate();
        }

        public SynchronizationToolContext(DbContextOptions options, IMediator mediator, SynchronisationConfiguration synchronisationConfiguration)
            : base(options)
        {
            _synchronisationConfiguration = synchronisationConfiguration;
            Database.Migrate();
        }

        // Таблицы синхронизации
        public DbSet<Entity> SyncEntities { get; set; }

        public DbSet<ChangeLog> ChangeLogs { get; set; }

        public DbSet<Change> Changes { get; set; }

        public DbSet<SynchClient> SynchClients { get; set; }

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
                log.HasOne(l => l.SynchClient)
                   .WithMany(c => c.ChangeLogs)
                   .HasForeignKey(l => l.ClientId);
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