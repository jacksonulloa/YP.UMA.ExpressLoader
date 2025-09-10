using Microsoft.Azure.Functions.Worker;
using YP.ZReg.Services.Interfaces;

namespace YP.Loader.app
{
    public class FunctionLoader(ICoreService _crs)
    {
        private readonly ICoreService crs = _crs;
        [Function("FunctionLoadFile")]
        //public async Task RunLoader([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        public async Task RunLoader([TimerTrigger("%ReclamosCron%", RunOnStartup = true)] TimerInfo myTimer)
        {
            await crs.ReadFilesAsync();
        }
    }
}
