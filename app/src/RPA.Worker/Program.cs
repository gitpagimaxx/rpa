using Rpa.Application.UseCases.CollectUsdBrl;
using Rpa.Infrastructure;
using Rpa.Infrastructure.Persistence;
using Rpa.Worker;
using Rpa.Worker.Options;
using Serilog;

// Inicializa SQLite ANTES de qualquer uso
SQLitePCL.Batteries.Init();

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection(WorkerOptions.SectionName));

builder.Services.AddInfrastructure();
builder.Services.AddScoped<CollectUsdBrlHandler>();

builder.Services.AddHostedService<UsdBrlCollectorWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync(CancellationToken.None);
}

await host.RunAsync();