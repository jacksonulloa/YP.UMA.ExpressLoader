using Azure;
using Azure.Data.Tables;

namespace YP.ZReg.Entities.Generic
{
    public class BlobTableRecord : ITableEntity
    {
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;

        // Campos personalizados
        public string Empresa { get; set; } = string.Empty;
        public string Proceso { get; set; } = string.Empty;
        public DateTime FechaHoraLog { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string Nivel { get; set; } = string.Empty; //Info, Error, Debug
        public string CodResp { get; set; } = string.Empty;
        public string DescResp { get; set; } = string.Empty;
        public string Request { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string HttpStatus { get; set; } = string.Empty;
        public double Duracion { get; set; }

        // Obligatorios por ITableEntity
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
