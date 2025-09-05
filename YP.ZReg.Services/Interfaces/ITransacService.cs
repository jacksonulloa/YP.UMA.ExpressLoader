using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Services.Interfaces
{
    public interface ITransacService
    {
        Task<BaseResponseExtension> ReadFiles();
    }
}