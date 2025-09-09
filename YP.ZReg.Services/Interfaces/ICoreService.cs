using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Services.Interfaces
{
    public interface ICoreService
    {
        Task<BaseResponseExtension> ReadFilesAsync();
    }
}