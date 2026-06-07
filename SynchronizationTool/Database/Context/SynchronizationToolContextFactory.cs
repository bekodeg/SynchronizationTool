using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Context;

public class SynchronizationToolContextFactory
    : IDesignTimeDbContextFactory<SynchronizationToolContext>
{
    public SynchronizationToolContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SynchronizationToolContext>();
        optionsBuilder.UseNpgsql("Server=192.168.0.14;Port=5432;Database=mydb;User Id=postgres;Password=postgres");

        var config = new SynchronisationConfiguration();

        return new SynchronizationToolContext(optionsBuilder.Options, new OptionsWrapper<SynchronisationConfiguration>(config));
    }
}