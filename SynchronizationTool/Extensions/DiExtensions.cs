using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Logic.Handlers.Commads;

namespace SynchronizationTool.Extensions
{
    public static class DiExtensions
    {
        public static IServiceCollection AddSynchronisation<Tcontext>(
            this IServiceCollection services, 
            IConfiguration configuration) where Tcontext : DbSynchronizationContext
        {
            services.Configure<SynchronisationConfiguration>(
                configuration.GetSection(nameof(SynchronisationConfiguration)));

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DbSynchronizationContext).Assembly));

            return services;
        }
    }
}
