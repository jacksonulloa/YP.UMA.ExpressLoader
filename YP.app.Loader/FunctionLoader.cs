using Microsoft.Azure.Functions.Worker;
using YP.ZReg.Services.Interfaces;

namespace YP.app.Loader
{
    public class FunctionLoader(ILoaderService _los)
    {
        private readonly ILoaderService los = _los;
        [Function("FunctionLoadFile")]
        public async Task RunLoader([TimerTrigger("%LoaderCron%", RunOnStartup = true)] TimerInfo myTimer)
        {
            try
            {
                await los.ReadFilesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
    }
}
