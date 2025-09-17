using AutoMapper;
using Microsoft.Extensions.Options;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Utils.Implementations
{
    public class DependencyProviderService(IOptions<Configurations> _cnf, IOptions<SftpConfig> _sft, IOptions<BlobConfig> _blc,
        IOptions<JwtConfig> _jwc, IOptions<DBConfig> _dbc, IMapper _mpr, IBlobLogService _bls) : IDependencyProviderService
    {
        public Configurations cnf { get; } = _cnf.Value;
        public SftpConfig sft { get; } = _sft.Value;
        public JwtConfig jwc { get; } = _jwc.Value;
        public DBConfig dbc { get; } = _dbc.Value;
        public BlobConfig blc { get; } = _blc.Value;        
        public IMapper mpr { get; } = _mpr;
        public IBlobLogService bls { get; } = _bls;
    }
}
