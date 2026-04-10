using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Database.Context;

namespace SynchronizationTool.Demo.Database.Context
{
    public class DemoContext : DbSynchronizationContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
