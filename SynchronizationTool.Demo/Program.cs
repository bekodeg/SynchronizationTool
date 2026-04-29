using MediatR;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Database.Models;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Demo.Database.Models;
using SynchronizationTool.Logic.Handlers.Commads;
using SynchronizationTool.Logic.Models.Commads;
using SynchronizationTool.Logic.Models.Dto;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("LoggingConfiguration.json");
var nLogConfig = new NLogLoggingConfiguration(builder.Configuration.GetSection("NLog"));

builder.Logging.ClearProviders();
builder.Services.AddLogging(m => m.AddNLog(nLogConfig));

// 1.Привязка конфигурации
builder.Services.Configure<SynchronisationConfiguration>(
    builder.Configuration.GetSection(nameof(SynchronisationConfiguration)));

// 2. Регистрация DemoContext и DbSynchronizationContext
builder.Services.AddDbContext<DemoContext>((sp, options) =>
{
    var dbType = Enum.Parse<DatabaseType>(builder.Configuration.GetConnectionString("DatabaseType")!);
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (dbType == DatabaseType.SQLite)
    {
        options.UseSqlite(connString);
    }
    else if (dbType == DatabaseType.MSSQL)
    {
        options.UseSqlServer(connString);
    }
    else if (dbType == DatabaseType.Postgres)
    {
        options.UseNpgsql(connString);
    }
});

// Чтобы хендлеры могли получить DbSynchronizationContext, регистрируем базовый тип
builder.Services.AddScoped<DbSynchronizationContext>(sp => sp.GetRequiredService<DemoContext>());

// 3. MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ApplyChangeLogsCommandHandler).Assembly));

// 4. Логирование (по желанию)
builder.Services.AddLogging();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c => c.EnableAnnotations());


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoContext>();
    await db.Database.MigrateAsync(); // применит миграции, если не сделано ранее

    var productCode = db.Model.GetEntityTypes().First(et => et.ClrType == typeof(Product)).GetTableName()!;

    if (!await db.SyncEntities.AnyAsync(e => e.Code == productCode))
    {
        db.SyncEntities.Add(new Entity
        {
            Id = Guid.NewGuid(),
            Code = productCode,
        });
        await db.SaveChangesAsync();
        Console.WriteLine("Сущность 'Product' зарегистрирована в системе синхронизации.");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
