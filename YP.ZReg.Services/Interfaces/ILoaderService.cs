using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Services.Interfaces
{
    public interface ILoaderService
    {
        Task<BaseResponseExtension> ReadFilesAsync();
    }
}