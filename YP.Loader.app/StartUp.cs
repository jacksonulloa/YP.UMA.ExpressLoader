using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Entities.Model;
using YP.ZReg.Repositories.Implementations;
using YP.ZReg.Repositories.Interfaces;
using YP.ZReg.Services.Implementations;
using YP.ZReg.Services.Interfaces;
using YP.ZReg.Services.Profiles;
using YP.ZReg.Utils.Implementations;
using YP.ZReg.Utils.Interfaces;

namespace YP.Loader.app
{
    public static class StartUp
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Configurations>(configuration.GetSection("Configurations"));
            services.AddOptions<SftpConfig>()
                .BindConfiguration("SftpConfig");
            services.AddOptions<DBConfig>()
                .BindConfiguration("DbConf");
            services.AddOptions<JwtConfig>()
                .BindConfiguration("JwtConfig");
            ConfigRepositories(services);
            ConfigBusinessServices(services);
            ConfigAutoMapper(services);
            ConfigUtilities(services);
        }
        public static void ConfigRepositories(IServiceCollection ListRepositories)
        {
            //ListRepositories.AddSingleton<CosmosClient>(sp =>
            //{
            //    var opt = sp.GetRequiredService<IOptions<Configurations>>().Value;

            //    var client = new CosmosClient(opt.CosmoDb.endpoint, opt.CosmoDb.keyprimary,
            //        new CosmosClientOptions
            //        {
            //            SerializerOptions = new CosmosSerializationOptions
            //            {
            //                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            //            }
            //        });

            //    // Validación rápida; si la primary fue rotada, cae a secondary (si está configurada)
            //    try
            //    {
            //        client.ReadAccountAsync().GetAwaiter().GetResult();
            //        return client;
            //    }
            //    catch (CosmosException) when (!string.IsNullOrWhiteSpace(opt.CosmoDb.keysecondary))
            //    {
            //        client.Dispose();
            //        return new CosmosClient(opt.CosmoDb.endpoint, opt.CosmoDb.keysecondary!,
            //            new CosmosClientOptions
            //            {
            //                SerializerOptions = new CosmosSerializationOptions
            //                {
            //                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            //                }
            //            });
            //    }
            //});
            //ListRepositories.AddSingleton<ICosmosContainerProvider, CosmosContainerProvider>();
            ListRepositories.AddSingleton<IBaseRepository, BaseRepository>();
            ListRepositories.AddTransient<IDeudaRepository, DeudaRepository>();
            ListRepositories.AddTransient<IEmpresaRepository, EmpresaRepository>();
        }
        public static void ConfigBusinessServices(IServiceCollection ListServices)
        {
            ListServices.AddSingleton<IAzureSftp, AzureSftp>();
            ListServices.AddTransient<ITransacService, TransacService>();
            ListServices.AddTransient<ICoreService, CoreService>();
            ListServices.AddSingleton<IEmpresaCache, EmpresaCache>();
            ListServices.AddSingleton<IApiSecurityService, ApiSecurityService>();
            ListServices.AddSingleton<IApiTransacService, ApiTransacService>();
        }
        public static void ConfigUtilities(IServiceCollection ListUtilities)
        {
            ListUtilities.AddSingleton<IDependencyProviderService, DependencyProviderService>();
            //ListUtilities.AddTransient<IToolsService, ToolsService>();
            //ListUtilities.AddSingleton<IAzureLogService, AzureLogService>();
            //ListUtilities.AddHttpClient<IExecApiService, ExecApiService>();
        }
        public static void ConfigAutoMapper(IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ModelProfile>();
            });
        }
    }
}
