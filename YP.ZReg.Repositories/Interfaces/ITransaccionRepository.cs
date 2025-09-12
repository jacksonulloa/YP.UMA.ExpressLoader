using YP.ZReg.Entities.Model;

namespace YP.ZReg.Repositories.Interfaces
{
    public interface ITransaccionRepository
    {
        Task<(long, string)> InsertarPago(Transaccion transaccion);
        Task<(long, string)> AplicarReversa(Transaccion transaccion);
    }
}