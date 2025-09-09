using YP.ZReg.Entities.Model;

namespace YP.ZReg.Services.Interfaces
{
    public interface IEmpresaCache
    {
        List<Empresa> empresas { get; set; }

        Task InitializeAsync();
    }
}