using YP.ZReg.Entities.Model;

namespace YP.ZReg.Repositories.Interfaces
{
    public interface IDeudaRepository
    {
        Task InsertarDeuda(Deuda deuda);
        Task EliminarDeudaPorHash(Deuda deuda);
        Task EliminarDeudaPorEmpresa(string codigo_empresa);
        Task ActualizarDeudaEnCarga(Deuda deuda);
    }
}