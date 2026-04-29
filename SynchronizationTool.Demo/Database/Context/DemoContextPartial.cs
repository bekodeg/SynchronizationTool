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
        public DbSet<Product> Products { get; set; }

        public DemoContext(
            DbContextOptions<DemoContext> options,
            IMediator mediator,
            IOptions<SynchronisationConfiguration> synchronisationConfiguration)
            : base(options, mediator, synchronisationConfiguration.Value)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired();
            });
        }
    }
}
