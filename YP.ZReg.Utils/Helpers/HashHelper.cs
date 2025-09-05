using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace YP.ZReg.Utils.Helpers
{
    public static class HashHelper
    {
        static HashHelper()
        {
            // Registrar el proveedor de páginas de código (necesario en .NET Core+)
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        private static string? NormalizarNullIfEmpty(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null; // igual que NULLIF(...,'') en SQL
            return valor.Trim().ToUpperInvariant();
        }

        private static string Normalizar(string? valor)
        {
            //if (valor == null) return string.Empty;
            //return valor.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(valor))
                return "";
            return valor.Trim().ToUpper(new CultureInfo("es-ES"));
        }

        public static string CalcularLlaveCanonica(
            string? idEmpresa,
            string? servicio,
            string? numeroDocumento,
            string? dni,
            string? ruc,
            string? llavePrincipal,
            string? llaveAlterna)
        {
            // La lógica de CONCAT_WS: ignora NULLs
            var partes = new[]
            {
            Normalizar(idEmpresa),
            Normalizar(servicio),
            Normalizar(numeroDocumento),
            NormalizarNullIfEmpty(dni),
            NormalizarNullIfEmpty(ruc),
            NormalizarNullIfEmpty(llavePrincipal),
            NormalizarNullIfEmpty(llaveAlterna)
        };

            // Une con '|', omitiendo nulls
            return string.Join("|", partes.Where(p => p != null));
        }

        public static byte[] CalcularLlaveHash(
            string? idEmpresa,
            string? servicio,
            string? numeroDocumento,
            string? dni,
            string? ruc,
            string? llavePrincipal,
            string? llaveAlterna)
        {
            string canonica = CalcularLlaveCanonica(
                idEmpresa, servicio, numeroDocumento,
                dni, ruc, llavePrincipal, llaveAlterna);

            //using var sha256 = SHA256.Create();
            //return sha256.ComputeHash(Encoding.UTF8.GetBytes(canonica));
            Encoding encoding = Encoding.GetEncoding(1252);
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(encoding.GetBytes(canonica));
        }
    }
}
