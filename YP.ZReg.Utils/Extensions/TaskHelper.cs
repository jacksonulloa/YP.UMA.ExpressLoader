using Microsoft.Extensions.Logging;

namespace YP.ZReg.Utils.Extensions
{
    public static class TaskHelper
    {
        public static void FireAndForget(this Task task, ILogger logger, string? context = null)
        {
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    logger.LogError(t.Exception,
                        "Error en tarea FireAndForget{Context}",
                        string.IsNullOrEmpty(context) ? "" : $" ({context})");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    // Manejo interno, log, etc.
                    //Console.WriteLine($"Error en log async: {t.Exception}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
