using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SynchronizationTool.Configuration;

namespace SynchronizationTool.Extensions
{
    public static class DiExtensions
    {
        public static IServiceCollection AddSynchronisation(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureOptions<SynchronisationConfiguration>();


            return services;
        }
    }
}
