using System.Text;
using YP.ZReg.Entities.Model;

namespace YP.ZReg.Services.Interfaces
{
    public interface IAzureSftp
    {
        Task<Stream> DownloadAsync(string remotePath, CancellationToken ct = default);
        Task<IReadOnlyList<string>> ListAsync(string remoteDir, CancellationToken ct = default);
        Task<IReadOnlyList<string>> ReadAllLinesAsync(string remoteFilePath, Encoding? encoding = null, CancellationToken ct = default);
        Task ReadLinesStreamAsync(string remoteFilePath, Func<string, Task> onLine, Encoding? encoding = null, CancellationToken ct = default);
        Task<(List<long>, List<long>)> WriteAllLinesAsync(
            string remoteFilePath,
            List<Transaccion> transacciones,
            Encoding? encoding = null,
            CancellationToken ct = default);
        Task UploadAsync(string remotePath, Stream content, CancellationToken ct = default);
        Task UploadJsonAsync(string remotePath, string jsonContent, Encoding? encoding = null, CancellationToken ct = default);
        Task MoveFileAsync(string sourcePath, string destinationPath, CancellationToken ct = default);
    }
}