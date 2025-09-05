using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YP.Loader.app;
using YP.ZReg.Services.Implementations;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("configurations.json", optional: false, reloadOnChange: true);
StartUp.ConfigureServices(builder.Services, builder.Configuration);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var cache = scope.ServiceProvider.GetRequiredService<EmpresaCache>();
    await cache.InitializeAsync();
}

app.Run();
