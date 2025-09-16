using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using TimeZoneConverter;
using YP.ZReg.Entities.Generic;

namespace YP.ZReg.Utils.Helpers
{
    public class ToolHelper
    {
        public static EmpresaPaths CreateEmpresaPaths(string pathCore, string empresaCodigo)
        {
            string root = $"{pathCore}/{empresaCodigo}";
            return new EmpresaPaths
            {
                DeudasRoot = $"{root}/Deudas",
                DeudasPending = $"{root}/Deudas/Pending",
                DeudasComplete = $"{root}/Deudas/Complete",
                DeudasError = $"{root}/Deudas/Error",
                PagosRoot = $"{root}/Pagos",
            };
        }
        public static string CalcularDuracion(DateTime fechaIni, DateTime fechaFin)
        {
            TimeSpan duracion = fechaFin - fechaIni;
            double segundosTotales = (duracion.TotalMilliseconds) / 1000;
            return $"{segundosTotales:F5} seg";
        }
        public static string EncriptarString(string key, string texto)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    streamWriter.Write(texto);
                }
                array = memoryStream.ToArray();
            }
            return Convert.ToBase64String(array);
        }
        public static string DesencriptarString(string key, string textoCifrado)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(textoCifrado);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using MemoryStream memoryStream = new(buffer);
                using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
                using StreamReader streamReader = new(cryptoStream);
                return streamReader.ReadToEnd();
            }
        }
        public static DateTime? ObtenerFechaExpiracionToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
                return null;

            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            return jwtToken?.ValidTo.ToLocalTime(); // o solo .ValidTo si prefieres UTC
        }
        public static void SetErrorResponse(BaseResponse _response, Exception exc)
        {
            _response.CodResp = "99";
            _response.DesResp = $"Error => {exc.Message}";
        }
        public static void SetResponse(BaseResponse _response, string codResp, string desResp)
        {
            _response.CodResp = codResp;
            _response.DesResp = desResp;
        }
        public static void SetFinalResponse(BaseResponseExtension _response)
        {
            //_response.EndExec = DateTime.Now;
            _response.EndExec = GetActualPeruHour();
            _response.Duration = CalcularDuracion(_response.StartExec, _response.EndExec);
        }
        public static DateTime GetActualPeruHour()
        {
            var tzPeru = TZConvert.GetTimeZoneInfo("America/Lima");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzPeru);
        }
    }
}
