using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Services.Interfaces
{
    public interface IGeneratorService
    {
        Task<BaseResponseExtension> WriteFilesAsync();
    }
}