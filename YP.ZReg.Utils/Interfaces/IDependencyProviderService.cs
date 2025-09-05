using AutoMapper;
using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Utils.Interfaces
{
    public interface IDependencyProviderService
    {
        Configurations cnf { get; }
        DBConfig dbc { get; }
        SftpConfig sft { get; }
        IMapper mpr { get; }
    }
}