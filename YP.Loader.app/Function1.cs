using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Services.Interfaces;

namespace YP.Loader.app;
public class Function1
{
    ICoreService crs;
    ITransacService trs;
    private readonly ILogger _logger;
    private SftpConfig s;
    private DBConfig d;
    private Configurations c;
    public Function1(ICoreService _crs, ITransacService _trs, ILoggerFactory loggerFactory,IOptions<SftpConfig> _s, IOptions<DBConfig> _d, IOptions<Configurations> _c)
    {
        _logger = loggerFactory.CreateLogger<Function1>();
        s = _s.Value;
        d = _d.Value;
        c = _c.Value;
        trs = _trs;
        crs = _crs;
    }

    [Function("Function1")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        await crs.ReadFilesAsync();
        var str = s.User;
        var strd = d.ConnectionString;
        var l = c.EmpresasConfig;
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}