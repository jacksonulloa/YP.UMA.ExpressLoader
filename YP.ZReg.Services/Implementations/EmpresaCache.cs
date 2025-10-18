using System.Net;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Entities.Model;
using YP.ZReg.Repositories.Interfaces;
using YP.ZReg.Services.Interfaces;
using YP.ZReg.Utils.Extensions;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Services.Implementations
{
    public class EmpresaCache(IEmpresaRepository _emr, IDependencyProviderService _dps) : IEmpresaCache
    {
        public List<Empresa> empresas { get; set; } = [];
        private readonly IEmpresaRepository emr = _emr;
        private readonly IDependencyProviderService dps = _dps;
        public async Task InitializeAsync()
        {
            try
            {
                empresas = await emr.ListarEmpresasConServicios(-1, -1, default);
            }
            catch (Exception ex)
            {
                TaskExtension.ProcesarResultadoAsync<object, BaseResponse>(
                                dps,
                                null,
                                new BaseResponse() { CodResp = "99", DesResp = $"Error => {ex.Message}" },
                                "Initial Load",
                                DateTime.Now,
                                "0",
                                "Info",
                                "99",
                                $"Error => {ex.Message}",
                                HttpStatusCode.Accepted).FireAndForget();
            }
        }
    }
}
