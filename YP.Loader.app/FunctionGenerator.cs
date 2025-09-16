using Microsoft.Azure.Functions.Worker;
using YP.ZReg.Services.Interfaces;

namespace YP.Loader.app
{
    public class FunctionGenerator(IGeneratorService _ges)
    {
        private readonly IGeneratorService ges = _ges;
        [Function("FunctionGenerateFile")]
        public async Task RunLoader([TimerTrigger("%GeneratorCron%", RunOnStartup = true)] TimerInfo myTimer)
        {
            await ges.WriteFilesAsync();
        }
    }
}
