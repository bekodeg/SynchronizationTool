using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // ѕорт, на котором будет работать gRPC (например, 5000)
    options.ListenAnyIP(8080, listenOptions => {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps("/app/certs/server.pfx", "YourPassword123");
    }); 
});

builder.Configuration.AddJsonFile("LoggingConfiguration.json");
var nLogConfig = new NLogLoggingConfiguration(builder.Configuration.GetSection("NLog"));

builder.Logging.ClearProviders();
builder.Services.AddLogging(m => m.AddNLog(nLogConfig));

builder.Services.AddSynchronisation<DemoContext>(builder.Configuration);

builder.Services.AddDbContext<DemoContext>((sp, options) =>
{
    var dbTypeStr = Environment.GetEnvironmentVariable("DATABASE_TYPE");

    if (string.IsNullOrEmpty(dbTypeStr))
    {
        dbTypeStr = builder.Configuration.GetConnectionString("DatabaseType");
    }

    var dbType = Enum.Parse<DatabaseType>(dbTypeStr!);

    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    }   

    switch (dbType)
    {
        case DatabaseType.SQLite:
            options.UseSqlite(connectionString);
            break;
        case DatabaseType.MSSQL:
            options.UseSqlServer(connectionString);
            break;
        case DatabaseType.Postgres:
            options.UseNpgsql(connectionString);
            break;
    }
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c => c.EnableAnnotations());


var app = builder.Build();


//app.UseHttpsRedirection();
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.MapSynchronisation();

app.Run();
