using YP.ZReg.Entities.Model;

namespace YP.ZReg.Repositories.Interfaces
{
    public interface IEmpresaRepository
    {
        Task<List<Empresa>> ListarEmpresasConServicios(int idEmpresa, int empEstado, CancellationToken ct = default);
    }
}