using YP.ZReg.Entities.Model;
using YP.ZReg.Repositories.Interfaces;
using YP.ZReg.Services.Interfaces;

namespace YP.ZReg.Services.Implementations
{
    public class EmpresaCache(IEmpresaRepository _emr) : IEmpresaCache
    {
        public List<Empresa> empresas { get; set; } = [];
        private readonly IEmpresaRepository emr = _emr;
        public async Task InitializeAsync()
        {
            empresas = await emr.ListarEmpresasConServicios(-1, -1, default);
        }
    }
}
