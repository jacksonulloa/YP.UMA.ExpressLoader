using AutoMapper;
using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Utils.Interfaces
{
    public interface IDependencyProviderService
    {
        Configurations cnf { get; }
        DBConfig dbc { get; }
        SftpConfig sft { get; }
        JwtConfig jwc { get; }
        BlobConfig blc { get; }
        IMapper mpr { get; }
        IBlobLogService bls { get; }
    }
}