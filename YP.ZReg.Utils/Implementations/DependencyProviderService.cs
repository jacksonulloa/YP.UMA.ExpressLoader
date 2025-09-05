using AutoMapper;
using Microsoft.Extensions.Options;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Utils.Implementations
{
    public class DependencyProviderService(IOptions<Configurations> _cnf, IOptions<SftpConfig> _sft,
        IOptions<DBConfig> _dbc, IMapper _mpr) : IDependencyProviderService
    {
        public Configurations cnf { get; } = _cnf.Value;
        public SftpConfig sft { get; } = _sft.Value;
        public DBConfig dbc { get; } = _dbc.Value;
        public IMapper mpr { get; } = _mpr;
    }
}
