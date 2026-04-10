using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Models;
using SynchronizationTool.Database.Models.Enums;

namespace SynchronizationTool.Database.Context
{
    public partial class DbSynchronizationContext : DbContext, IDbSynchronizationContext
    {
        private readonly SynchronisationConfiguration _synchronisationConfiguration;

        public DbSynchronizationContext(SynchronisationConfiguration synchronisationConfiguration)
            : base() 
        {
            _synchronisationConfiguration = synchronisationConfiguration;
        }

        public DbSynchronizationContext(DbContextOptions options, SynchronisationConfiguration synchronisationConfiguration) 
            : base(options)
        {
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
            // Получаем все изменённые сущности отслеживаемых типов
            var changedEntries = ChangeTracker.Entries()
                .Where(e 
                    => e.State == EntityState.Added 
                    || e.State == EntityState.Modified 
                    || e.State == EntityState.Deleted)
                .ToList();

            if (!changedEntries.Any())
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            // Для каждой изменённой сущности создаём ChangeLog
            foreach (var entry in changedEntries)
            {
                var entityType = entry.Entity.GetType();

                var syncEntity = await EnsureSyncEntityAsync(entityType, cancellationToken);

                if (syncEntity == null)
                {
                    continue;
                }

                var entityIdProperty = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
                if (entityIdProperty == null) continue;

                var entityIdValue = entry.Property(entityIdProperty.Name).CurrentValue;
                if (entityIdValue is not Guid entityId) continue; // предполагаем, что все отслеживаемые сущности имеют Guid Id

                // Получаем или создаём запись в таблице Entity
                
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
                    ClientId = _synchronisationConfiguration.ClientId,
                    ClientVersion = _synchronisationConfiguration.CurrentClientVersion,
                    Changes = new List<Change>()
                };

                // Заполняем список изменений (колонка -> новое значение)
                if (entry.State == EntityState.Added)
                {
                    foreach (var property in entry.Properties
                        .Where(p => !p.Metadata.IsPrimaryKey()))
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
                    foreach (var property in entry.Properties
                        .Where(p => p.IsModified && !p.Metadata.IsPrimaryKey()))
                    {
                        changeLog.Changes.Add(new Change
                        {
                            ColumnName = property.Metadata.Name,
                            Value = property.CurrentValue?.ToString() ?? "NULL",
                            ChangeLogId = changeLog.Id
                        });
                    }
                }

                ChangeLogs.Add(changeLog);
            }

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            return result;
        }

        private async Task<Entity?> EnsureSyncEntityAsync(Type entityType, CancellationToken cancellationToken)
        {
            return await SyncEntities.FirstOrDefaultAsync(e => e.Code == entityType.Name, cancellationToken);
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
