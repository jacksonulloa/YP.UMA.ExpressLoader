using YP.ZReg.Entities.Model;

namespace YP.ZReg.Repositories.Interfaces
{
    public interface ITransaccionRepository
    {
        Task<(long, string)> InsertarPago(Transaccion transaccion);
        Task<(long, string)> AplicarReversa(Transaccion transaccion);
        Task<List<Transaccion>> ConsultarDeuda(string empresa, string estado, CancellationToken ct = default);
        Task ActualizarEstadoTransacciones(string empresa, string ids, string estado);
    }
}