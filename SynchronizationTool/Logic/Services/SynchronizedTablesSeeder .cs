using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SynchronizationTool.Database;
using SynchronizationTool.Database.Context;
using System.Reflection;

namespace SynchronizationTool.Logic.Services
{
    internal class SynchronizedTablesSeeder(
        ILogger<SynchronizedTablesSeeder> logger,
        ISynchronizationToolContext synchronizationToolContext,
        IDbSynchronizationContext dbSynchronizationContext) 
        : IHostedService
    {
        private readonly ILogger<SynchronizedTablesSeeder> _logger = logger;
        private readonly ISynchronizationToolContext _synchronizationToolContext = synchronizationToolContext;
        private readonly IDbSynchronizationContext _dbSynchronizationContext = dbSynchronizationContext;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var entityTypes = _dbSynchronizationContext
                .GetEntityTypes()
                .ToArray();

            foreach (var entityType in entityTypes)
            {
                try 
                {
                    var clrType = entityType.ClrType;
                    // Проверяем наличие атрибута [Synchronisated]
                    var atr = clrType.GetCustomAttribute<SynchronisatedAttribute>();
                    if (atr != null)
                    {
                        string tableName = entityType.GetTableName()!;

                        if (_synchronizationToolContext.SyncEntities.Any(e => e.Code == tableName))
                        {
                            continue;
                        }

                        await _synchronizationToolContext.SyncEntities.AddAsync(new()
                        {
                            Id = Guid.Parse(atr.Id),
                            Code = tableName,
                        });
                        
                        _logger.LogInformation("Added/verified table {Table}", tableName);
                    }
                }
                finally
                {
                }
            }
            await _synchronizationToolContext.SaveChangesAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
