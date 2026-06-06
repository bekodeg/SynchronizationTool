using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SynchronizationTool.Database.Context;

public class SynchronizationToolContextFactory
    : IDesignTimeDbContextFactory<SynchronizationToolContext>
{
    public SynchronizationToolContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SynchronizationToolContext>();
        optionsBuilder.UseSqlite("DataSource=:memory:");

        // ПЕРЕДАЁМ настроенные опции в конструктор с параметром DbContextOptions
        return new SynchronizationToolContext(optionsBuilder.Options);
    }
}