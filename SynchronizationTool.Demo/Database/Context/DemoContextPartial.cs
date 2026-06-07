using MediatR;
using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Database.Context;

namespace SynchronizationTool.Demo.Database.Context
{
    public partial class DemoContext : DbSynchronizationContext
    {
        public DemoContext(
            DbContextOptions<DemoContext> options,
            IMediator mediator)
            : base(options, mediator)
        {
           // Database.Migrate();
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
