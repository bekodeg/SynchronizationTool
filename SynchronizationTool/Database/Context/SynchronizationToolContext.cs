using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Models;

namespace SynchronizationTool.Database.Context
{
    public partial class SynchronizationToolContext : DbContext, ISynchronizationToolContext
    {
        private readonly SynchronisationConfiguration _configuration;
        
        public SynchronizationToolContext(IOptions<SynchronisationConfiguration> configuration)
            : base()
        {
            _configuration = configuration.Value;
        }

        public SynchronizationToolContext(
            DbContextOptions<SynchronizationToolContext> options, 
            IOptions<SynchronisationConfiguration> configuration,
            bool migrate = true)
            : base(options)
        {
            _configuration = configuration.Value;
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

            modelBuilder.HasDefaultSchema(_configuration.SynchronisationSchema);
          
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
            });
        }
    }
}