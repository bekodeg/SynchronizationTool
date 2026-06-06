using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Database.Models;

namespace SynchronizationTool.Database.Context
{
    public partial class SynchronizationToolContext : DbContext, ISynchronizationToolContext
    {
        public SynchronizationToolContext()
            : base()
        {
        }
        public SynchronizationToolContext(
            DbContextOptions options, bool migrate = true)
            : base(options)
        {
            if (migrate)
            {
                Database.Migrate();
            }
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
                entity.ToTable("entity");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            modelBuilder.Entity<ChangeLog>(log =>
            {
                log.ToTable("ChangeLog");
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
                change.ToTable("Change");
                change.HasKey(c => c.Id);
                change.Property(c => c.Id);
                change.HasOne(c => c.ChangeLog)
                      .WithMany(l => l.Changes)
                      .HasForeignKey(c => c.ChangeLogId);
            });

            modelBuilder.Entity<SynchClient>(client =>
            {
                client.ToTable("SynchClient");
                client.HasKey(c => c.Id);
                client.Property(c => c.LastChangeLogId);
                client.HasOne(c => c.LastChangeLog)
                      .WithOne();
            });
        }
    }
}