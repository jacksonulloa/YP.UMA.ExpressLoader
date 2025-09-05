using Renci.SshNet;
using System.Text;
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
