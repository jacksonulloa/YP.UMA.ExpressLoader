using AutoMapper;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Net;
using TimeZoneConverter;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Utils.Helpers;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Utils.Implementations
{
    public class BlobLogService(IOptions<BlobConfig> _blc, IMapper _mpr) : IBlobLogService
    {
        private readonly BlobConfig blc = _blc.Value;
        private readonly IMapper mpr = _mpr;
        //private readonly IDependencyProviderService dps = _dps;
        private async Task EnviarLogAzure(BlobTableRecord register)
        {
            var tableClient = new TableClient(blc.ConnectionString, blc.Table);
            await tableClient.CreateIfNotExistsAsync();

            var tzPeru = TZConvert.GetTimeZoneInfo("America/Lima");
            var horaPeru = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzPeru);
            register.FechaHoraLog = horaPeru;

            if (register.FechaHoraLog.Kind != DateTimeKind.Utc)
                register.FechaHoraLog = DateTime.SpecifyKind(register.FechaHoraLog, DateTimeKind.Utc);
            if (register.FechaHoraInicio.Kind != DateTimeKind.Utc)
                register.FechaHoraInicio = DateTime.SpecifyKind(register.FechaHoraInicio, DateTimeKind.Utc);
            if (register.FechaHoraFin.Kind != DateTimeKind.Utc)
                register.FechaHoraFin = DateTime.SpecifyKind(register.FechaHoraFin, DateTimeKind.Utc);


            register.RowKey = $"{register.FechaHoraLog:HHmmssfff}-{Guid.NewGuid().ToString("N")[..6]}-{Guid.NewGuid().ToString("N")[..6]}-{Guid.NewGuid().ToString("N")[..6]}";
            register.PartitionKey = $"{register.FechaHoraLog:yyyyMMdd}";
            Azure.Response responseReg = await tableClient.AddEntityAsync(register);
        }
        public async Task RegistrarLogAsync<TRequest, TResponse>(BlobTableRecord record, TRequest request, TResponse? response, HttpStatusCode statusCode)
        {
            if (!blc.EnableLog.Equals("On")) return;
            var log = mpr.Map<BlobTableRecord>(record);
            log.RowKey = $"{log.FechaHoraLog:HHmmssfff}-{Guid.NewGuid().ToString("N")[..6]}-{Guid.NewGuid().ToString("N")[..6]}-{Guid.NewGuid().ToString("N")[..6]}";
            log.PartitionKey = $"{log.FechaHoraLog:yyyyMMdd}";
            log.Request = request is null ? "" : JsonConvert.SerializeObject(request);
            log.Response = response is null ? "" : JsonConvert.SerializeObject(response);
            log.HttpStatus = statusCode.ToString();
            log.Nivel = record.Nivel;
            log.Duracion = ToolHelper.CalcularDuracionSeconds(log.FechaHoraInicio, log.FechaHoraFin);
            await EnviarLogAzure(log);
        }
    }
}
