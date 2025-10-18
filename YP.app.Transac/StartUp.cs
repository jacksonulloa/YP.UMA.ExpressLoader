using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Repositories.Implementations;
using YP.ZReg.Repositories.Interfaces;
using YP.ZReg.Services.Implementations;
using YP.ZReg.Services.Interfaces;
using YP.ZReg.Services.Profiles;
using YP.ZReg.Utils.Implementations;
using YP.ZReg.Utils.Interfaces;

namespace YP.app.Transac
{
    public static class StartUp
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Configurations>(configuration.GetSection("Configurations"));
            services.AddOptions<DBConfig>()
                .BindConfiguration("DbConf");
            services.AddOptions<JwtConfig>()
                .BindConfiguration("JwtConfig");
            services.AddOptions<BlobConfig>()
                .BindConfiguration("BlobConfig");
            ConfigRepositories(services);
            ConfigBusinessServices(services);
            ConfigAutoMapper(services);
            ConfigUtilities(services);
        }
        public static void ConfigRepositories(IServiceCollection ListRepositories)
        {
            ListRepositories.AddSingleton<IBaseRepository, BaseRepository>();
            ListRepositories.AddTransient<IDeudaRepository, DeudaRepository>();
            ListRepositories.AddTransient<IEmpresaRepository, EmpresaRepository>();
            ListRepositories.AddTransient<ITransaccionRepository, TransaccionRepository>();
        }
        public static void ConfigBusinessServices(IServiceCollection ListServices)
        {
            ListServices.AddTransient<ITransacService, TransacService>();
            ListServices.AddTransient<IAzureSftp, AzureSftp>();
            ListServices.AddSingleton<IEmpresaCache, EmpresaCache>();
            ListServices.AddSingleton<IApiSecurityService, ApiSecurityService>();
            ListServices.AddSingleton<IApiTransacService, ApiTransacService>();
        }
        public static void ConfigUtilities(IServiceCollection ListUtilities)
        {
            ListUtilities.AddSingleton<IDependencyProviderService, DependencyProviderService>();
            ListUtilities.AddTransient<IBlobLogService, BlobLogService>();
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
