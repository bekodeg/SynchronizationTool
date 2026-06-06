using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Logic.gRPC;
using Microsoft.EntityFrameworkCore;

namespace SynchronizationTool.Extensions
{
    public static class DiExtensions
    {
        public static IServiceCollection AddSynchronisation<TSynchContext>(
            this IServiceCollection services, 
            IConfiguration configuration) where TSynchContext : class, IDbSynchronizationContext
        {
            var synchConfigurationSection = configuration.GetSection(nameof(SynchronisationConfiguration));

            string? configFild;
            
            configFild = Environment.GetEnvironmentVariable("SYNCH_CLIENT_ID");
            if (!string.IsNullOrWhiteSpace(configFild)){
                synchConfigurationSection[nameof(SynchronisationConfiguration.ClientId)] = configFild;
            }
            configFild = Environment.GetEnvironmentVariable("SYNCH_CLIENT_VERSION");
            if (!string.IsNullOrWhiteSpace(configFild)){
                synchConfigurationSection[nameof(SynchronisationConfiguration.CurrentClientVersion)] = configFild;
            }

            services.Configure<SynchronisationConfiguration>(synchConfigurationSection);

            services.AddScoped<IDbSynchronizationContext, TSynchContext>();
            services.AddDbContext<ISynchronizationToolContext, SynchronizationToolContext>((sp, options) =>
            {
                var dbTypeStr = Environment.GetEnvironmentVariable("SYNCH_DATABASE_TYPE");

                if (string.IsNullOrEmpty(dbTypeStr))
                {
                    dbTypeStr = configuration.GetConnectionString("SynchDatabaseType");
                }

                var dbType = Enum.Parse<DatabaseType>(dbTypeStr!);

                var connectionString = Environment.GetEnvironmentVariable("SYNCH_CONNECTION_STRING");

                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = configuration.GetConnectionString("SynchConnection");
                }

                switch (dbType)
                {
                    case DatabaseType.SQLite:
                        options.UseSqlite(connectionString);
                        break;
                    case DatabaseType.MSSQL:
                        options.UseSqlServer(connectionString);
                        break;
                    case DatabaseType.Postgres:
                        options.UseNpgsql(connectionString);
                        break;
                }
            });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DbSynchronizationContext).Assembly));

            services.AddSingleton<IClientChannelStorage, ClientChannelStorage>();

            return services;
        }
    }
}
