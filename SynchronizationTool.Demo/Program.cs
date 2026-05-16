using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Database.Models;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Demo.Database.Models;
using SynchronizationTool.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("LoggingConfiguration.json");
var nLogConfig = new NLogLoggingConfiguration(builder.Configuration.GetSection("NLog"));

builder.Logging.ClearProviders();
builder.Services.AddLogging(m => m.AddNLog(nLogConfig));

builder.Services.AddSynchronisation<DbSynchronizationContext>(builder.Configuration);

builder.Services.AddDbContext<DbSynchronizationContext, DemoContext>((sp, options) =>
{
    var dbType = Enum.Parse<DatabaseType>(builder.Configuration.GetConnectionString("DatabaseType")!);
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");

    switch (dbType)
    {
        case DatabaseType.SQLite:
            options.UseSqlite(connString);
            break;
        case DatabaseType.MSSQL:
            options.UseSqlServer(connString);
            break;
        case DatabaseType.Postgres:
            options.UseNpgsql(connString);
            break;
    }
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c => c.EnableAnnotations());


var app = builder.Build();


app.UseHttpsRedirection();
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
