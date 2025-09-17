using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Utils.Helpers
{
    public static class JwtHelper
    {
        
        public static async Task<HttpResponseData?> ConfirmarToken<T>(HttpRequestData req, string secretKey, 
            string localType) where T : BaseResponse, new()
        {
            try
            {
                if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
                {
                    return await CrearRespuestaError<T>(req, "Token no proporcionado");
                }
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);
                string token = authHeaders.FirstOrDefault()?.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                var tipoClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "tipo")?.Value;
                if (!string.IsNullOrEmpty(tipoClaim) && tipoClaim != localType)
                {
                    HttpResponseData response = await CrearRespuestaError<T>(req, "Claim invalido");
                    return response;
                }
                if (string.IsNullOrWhiteSpace(token) || !JwtHelper.ValidarJwtToken(token, secretKey))
                {
                    return await CrearRespuestaError<T>(req, "Token invalido o expirado");
                }
            }
            catch (Exception ex)
            {
                return await CrearRespuestaError<T>(req, "Token invalido");
            }
            return null; // Token válido
        }
        public static TResponse? ConfirmarTokenObjeto<TRequest, TResponse>(HttpRequestData req, string secretKey,
            string localType) where TResponse : BaseResponse, new()
        {
            try
            {
                if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
                {
                    return CrearRespuestaError<TResponse>("Token no proporcionado");
                }
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);
                string token = authHeaders.FirstOrDefault()?.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                var tipoClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "tipo")?.Value;
                if (!string.IsNullOrEmpty(tipoClaim) && tipoClaim != localType)
                {
                    TResponse response = CrearRespuestaError<TResponse>("Claim invalido");
                    return response;
                }
                if (string.IsNullOrWhiteSpace(token) || !JwtHelper.ValidarJwtToken(token, secretKey))
                {
                    return CrearRespuestaError<TResponse>("Token invalido o expirado");
                }
            }
            catch (Exception ex)
            {
                return CrearRespuestaError<TResponse>("Token invalido");
            }
            return null; // Token válido
        }
        /// <summary>
        /// Metodo que verifica si un token JWT es válido.
        /// </summary>
        /// <param name="token">Token enviado</param>
        /// <param name="encodedKey">Key en base64 utilizado para validar el token</param>
        /// <returns></returns>
        private static bool ValidarJwtToken(string token, string encodedKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(encodedKey); // desde tu configuración

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // opcional
                    ValidateAudience = false, // opcional
                    ClockSkew = TimeSpan.Zero // sin tolerancia de expiración
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
        private static async Task<HttpResponseData> CrearRespuestaError<T>(HttpRequestData req, string mensaje) where T : BaseResponse, new()
        {
            var res = new T { CodResp = "99", DesResp = mensaje };
            var response = req.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteAsJsonAsync(res);
            return response;
        }
        private static TResponse CrearRespuestaError<TResponse>(string mensaje) where TResponse : BaseResponse, new()
        {
            var response = new TResponse { CodResp = "99", DesResp = mensaje };
            return response;
        }
    }
}
