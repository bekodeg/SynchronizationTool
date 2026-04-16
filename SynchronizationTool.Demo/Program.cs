using MediatR;
using Microsoft.EntityFrameworkCore;
using SynchronizationTool.Configuration;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Database.Models;
using SynchronizationTool.Demo.Database.Context;
using SynchronizationTool.Demo.Database.Models;
using SynchronizationTool.Logic.Handlers.Commads;
using SynchronizationTool.Logic.Models.Commads;
using SynchronizationTool.Logic.Models.Dto;

var builder = WebApplication.CreateBuilder(args);

// 1.Привязка конфигурации
builder.Services.Configure<SynchronisationConfiguration>(
    builder.Configuration.GetSection(nameof(SynchronisationConfiguration)));

// 2. Регистрация DemoContext и DbSynchronizationContext
builder.Services.AddDbContext<DemoContext>((sp, options) =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connString);
});

// Чтобы хендлеры могли получить DbSynchronizationContext, регистрируем базовый тип
builder.Services.AddScoped<DbSynchronizationContext>(sp => sp.GetRequiredService<DemoContext>());

// 3. MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ApplyChangeLogsCommandHandler).Assembly));

// 4. Логирование (по желанию)
builder.Services.AddLogging();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoContext>();
    await db.Database.MigrateAsync(); // применит миграции, если не сделано ранее

    if (!await db.SyncEntities.AnyAsync(e => e.Code == nameof(Product)))
    {
        db.SyncEntities.Add(new Entity
        {
            Id = Guid.NewGuid(),
            Code = nameof(Product)
        });
        await db.SaveChangesAsync();
        Console.WriteLine("Сущность 'Product' зарегистрирована в системе синхронизации.");
    }
}
//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<DemoContext>();
//    var mediatr = scope.ServiceProvider.GetRequiredService<IMediator>();

//    var res = await mediatr.Send(new ApplyChangeLogsCommand()
//    {
//        ChangeLogs = await context.ChangeLogs
//            .Select(cl => new ChangeLogDto
//            {
//                Id = cl.Id,
//                EntityId = cl.RowId,
//                DateTime = cl.DateTime,
//                Type = cl.Type,
//                TableId = cl.EntityId,
//                ClientId = cl.ClientId,
//                ClientVersion = cl.ClientVersion,
//                Changes = cl.Changes.Select(c => new ChangeDto
//                {
//                    ColumnName = c.ColumnName,
//                    Value = c.Value
//                }).ToList(),

//            }).ToListAsync()
//    });

//    Console.WriteLine(res.Message);
//}

app.Run();
