using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YP.app.Loader;
using YP.ZReg.Services.Interfaces;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("configurations.json", optional: false, reloadOnChange: true);
StartUp.ConfigureServices(builder.Services, builder.Configuration);

builder.ConfigureFunctionsWebApplication();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var cache = scope.ServiceProvider.GetRequiredService<IEmpresaCache>();
    await cache.InitializeAsync();
}

app.Run();
