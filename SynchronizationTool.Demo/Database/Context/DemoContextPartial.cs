using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Demo.Database.Models;

namespace SynchronizationTool.Demo.Database.Context
{
    public partial class DemoContext : DbSynchronizationContext
    {
        public DemoContext(
            DbContextOptions<DemoContext> options,
            IMediator mediator,
            IOptions<SynchronisationConfiguration> synchronisationConfiguration)
            : base(options, mediator, synchronisationConfiguration.Value)
        {
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
