using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Logic.gRPC;
using SynchronizationTool.Logic.Services;
using System.Runtime.CompilerServices;

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

            configFild = Environment.GetEnvironmentVariable("SYNCH_DATABASE_SCHEMA");
            if (!string.IsNullOrWhiteSpace(configFild))
            {
                synchConfigurationSection[nameof(SynchronisationConfiguration.SynchronisationSchema)] = configFild;
            }

            services.Configure<SynchronisationConfiguration>(synchConfigurationSection);

            services.AddScoped<IDbSynchronizationContext, TSynchContext>();
            services.AddDbContext<ISynchronizationToolContext, SynchronizationToolContext>((sp, options) =>
            {
                var config = sp.GetRequiredService<IOptions<SynchronisationConfiguration>>().Value;

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
                        options.UseSqlServer(connectionString, opt =>
                        {
                            opt.MigrationsHistoryTable("__EFMigrationsHistory", config.SynchronisationSchema);
                        });
                        break;
                    case DatabaseType.Postgres:
                        options.UseNpgsql(connectionString, opt =>
                        {
                            opt.MigrationsHistoryTable("__EFMigrationsHistory", config.SynchronisationSchema);
                        });
                        break;
                }
            });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DbSynchronizationContext).Assembly));

            services.AddSingleton<IClientChannelStorage, ClientChannelStorage>();
            
            // gRPC
            services.AddGrpc();
            services.AddScoped<SynchronisationInterface>();

            // Hosted services
            services.AddHostedService<SynchronizedTablesSeeder>();

            return services;
        }


        public static IEndpointRouteBuilder MapSynchronisation(this IEndpointRouteBuilder app)
        {
            app.MapGrpcService<SynchronisationInterface>();

            return app;
        }
    }
}
