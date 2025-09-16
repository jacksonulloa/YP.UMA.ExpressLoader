using Renci.SshNet;
using System.Text;
using YP.ZReg.Entities.Model;
using YP.ZReg.Services.Interfaces;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Services.Implementations
{
    public class AzureSftp(IDependencyProviderService _dps) : IAzureSftp
    {
        private readonly IDependencyProviderService dps = _dps;
        private SftpClient CreateClient()
        {
            ConnectionInfo conn;
            var user = dps.sft.User;
            var pwd = dps.sft.Pass;
            var server = dps.sft.Server;
            conn = new ConnectionInfo(server, 22, user,
                new PasswordAuthenticationMethod(user, pwd));
            var client = new SftpClient(conn);
            return client;
        }
        public async Task UploadAsync(string remotePath, Stream content, CancellationToken ct = default)
        {
            using var c = CreateClient();
            c.Connect();
            try { await Task.Run(() => c.UploadFile(content, remotePath, true), ct); }
            finally { if (c.IsConnected) c.Disconnect(); }
        }

        public async Task<Stream> DownloadAsync(string remotePath, CancellationToken ct = default)
        {
            using var c = CreateClient();
            c.Connect();
            try
            {
                var ms = new MemoryStream();
                await Task.Run(() => c.DownloadFile(remotePath, ms), ct);
                ms.Position = 0;
                return ms; // caller dispone
            }
            finally { if (c.IsConnected) c.Disconnect(); }
        }

        public async Task<IReadOnlyList<string>> ListAsync(string remoteDir, CancellationToken ct = default)
        {
            using var c = CreateClient();
            c.Connect();
            try
            {
                var entries = await Task.Run(() => c.ListDirectory(remoteDir), ct);
                return entries.Where(e => e.Name is not "." and not "..")
                              .Select(e => e.FullName).ToList();
            }
            finally { if (c.IsConnected) c.Disconnect(); }
        }

        public async Task<IReadOnlyList<string>> ReadAllLinesAsync(
            string remoteFilePath,
            Encoding? encoding = null,
            CancellationToken ct = default)
        {
            using var c = CreateClient();
            c.Connect();
            try
            {
                if (!c.Exists(remoteFilePath))
                    throw new FileNotFoundException($"No existe: {remoteFilePath}");

                using var s = c.OpenRead(remoteFilePath);                 // stream SFTP
                using var r = new StreamReader(s, encoding ?? Encoding.UTF8, true);

                var lines = new List<string>();
                while (!r.EndOfStream)
                {
                    ct.ThrowIfCancellationRequested();
                    var line = await r.ReadLineAsync();
                    if (line is not null) lines.Add(line);
                }
                return lines;
            }
            finally { if (c.IsConnected) c.Disconnect(); }
        }

        public async Task<(List<long>, List<long>)> WriteAllLinesAsync(
            string remoteFilePath,
            List<Transaccion> transacciones,
            Encoding? encoding = null,
            CancellationToken ct = default)
        {
            List<long> listaOk = [];
            List<long> listaError = [];
            using var c = CreateClient();
            c.Connect();
            try
            {
                using var s = c.Create(remoteFilePath); // sobrescribe el archivo
                using var w = new StreamWriter(s, encoding ?? Encoding.UTF8);
                foreach (var transaccion in transacciones)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        string linea = ConvertirTransaccion(transaccion);
                        await w.WriteLineAsync(linea);
                        listaOk.Add(transaccion.id);
                    }
                    catch (Exception ex)
                    {
                        listaError.Add(transaccion.id);
                    }
                }
                await w.FlushAsync(); // asegura que todo se envíe
            }
            finally { if (c.IsConnected) c.Disconnect(); }
            return (listaOk, listaError);
        }
        private string ConvertirTransaccion(Transaccion t)
        {
            var sb = new StringBuilder();

            // 01 Tipo de registro (Constante "DD")
            sb.Append("DD".PadRight(2));
            // 02 Código de identificación del cliente (id_consulta) 14 chars
            sb.Append((t.id_consulta ?? "").PadRight(14));
            // 03 Nombre Cliente 
            sb.Append((t.nombre_cliente ?? "").PadRight(30));
            // 04 Código producto (servicio) 3 chars
            sb.Append((t.servicio ?? "").PadRight(3));
            // 05 Número de documento 16 chars
            sb.Append((t.numero_documento ?? "").PadRight(16));
            // 06 Fecha vencimiento (no está en tu clase) → vacía 8 chars
            sb.Append(t.fecha_hora_transaccion.ToString("ddMMyyyy"));
            // 07 Fecha de pago (fecha_hora_transaccion.Date) DDMMAAAA
            sb.Append(t.fecha_hora_transaccion.ToString("ddMMyyyy"));
            // 08 Hora de pago (fecha_hora_transaccion.TimeOfDay) HHmmss
            sb.Append(t.fecha_hora_transaccion.ToString("HHmmss"));
            // 09 Importe pagado (15, sin punto decimal) → ejemplo: 000000000001550
            sb.Append(((long)(t.importe_pagado * 100)).ToString().PadLeft(15, '0'));
            // 10 Agencia (id_banco) 15 chars
            sb.Append(new string(' ', 15));
            // 11 Dirección Agencia (no está en tu clase) → vacío 40 chars
            sb.Append(new string(' ', 40));
            // 12 Número de operación 12 chars
            sb.Append((t.numero_operacion ?? "").PadRight(12));
            // 13 Canal de pago 2 chars
            sb.Append((t.id_canal_pago ?? "").PadRight(2));
            // 14 Forma de pago 2 chars
            sb.Append((t.id_forma_pago ?? "").PadRight(2));
            // 15 Fecha contable (no la tienes, usamos la misma fecha) DDMMAAAA
            sb.Append(t.fecha_hora_transaccion.ToString("ddMMyyyy"));
            // 16 Banco (libre) 30 chars
            sb.Append((t.id_banco ?? "").PadRight(4));
            sb.Append((t.cuenta_banco ?? "").PadRight(20));
            sb.Append((t.voucher ?? "").PadRight(20));
            return sb.ToString();
        }

        /*
         COMO USAR ReadAllLinesAsync
        var lines = await _sftp.ReadAllLinesAsync("/rootlog/clavesftptest.txt", Encoding.UTF8, ct);
         */
        public async Task ReadLinesStreamAsync(
            string remoteFilePath,
            Func<string, Task> onLine,                 // callback por línea
            Encoding? encoding = null,
            CancellationToken ct = default)
        {
            SftpClient? c = null;
            try
            {
                c = CreateClient();
                c.Connect();

                if (!c.Exists(remoteFilePath))
                    throw new FileNotFoundException($"No existe: {remoteFilePath}");

                using var s = c.OpenRead(remoteFilePath);
                using var r = new StreamReader(s, encoding ?? Encoding.UTF8, true);

                while (!r.EndOfStream)
                {
                    ct.ThrowIfCancellationRequested();
                    var line = await r.ReadLineAsync();
                    if (line is not null) await onLine(line);
                }
            }
            finally
            {
                if (c is { IsConnected: true }) c.Disconnect();
                c?.Dispose();
            }
        }
        public async Task UploadJsonAsync(
            string remotePath,
            string jsonContent,
            Encoding? encoding = null,
            CancellationToken ct = default)
        {
            using var c = CreateClient();
            c.Connect();
            try
            {
                using var ms = new MemoryStream();
                using (var writer = new StreamWriter(ms, encoding ?? Encoding.UTF8, leaveOpen: true))
                {
                    await writer.WriteAsync(jsonContent);
                    await writer.FlushAsync();
                }

                ms.Position = 0;
                await Task.Run(() => c.UploadFile(ms, remotePath, true), ct);
            }
            finally
            {
                if (c.IsConnected) c.Disconnect();
            }
        }
        public async Task MoveFileAsync(string sourcePath, string destinationPath, CancellationToken ct = default)
        {
            using var c = CreateClient();
            c.Connect();
            try
            {
                await Task.Run(() => c.RenameFile(sourcePath, destinationPath), ct);
            }
            finally
            {
                if (c.IsConnected) c.Disconnect();
            }
        }
    }
}
