
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using YP.ZReg.Dtos.Contracts.Request;
using YP.ZReg.Dtos.Contracts.Response;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Services.Interfaces;
using YP.ZReg.Utils.Helpers;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Services.Implementations
{
    public class ApiSecurityService(IDependencyProviderService _dps) : IApiSecurityService
    {
        private readonly IDependencyProviderService dps = _dps;
        public async Task<(GetTokenRes, HttpStatusCode)> GetTokenWithClaim(GetTokenReq request)
        {
            BaseResponseExtension baseResponse = new() { CodResp = "00", DesResp = "Ok", StartExec = DateTime.Now };
            GetTokenRes response = new() { CodResp = "00", DesResp = "Ok" };
            string Token = string.Empty;
            string ExpirationDate = string.Empty;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            try
            {
                //string Key = "4sb4nc-T1-734m-1ng3r14SW";
                //string localType = ToolHelper.DesencriptarString(Key, dps.jwc.Claim);
                //string localProfile = ToolHelper.DesencriptarString(Key, dps.jwc.User);
                //string localPass = ToolHelper.DesencriptarString(Key, dps.jwc.Password);
                string localType = dps.jwc.Claim;
                string localProfile = dps.jwc.User;
                string localPass = dps.jwc.Password;
                if (string.IsNullOrWhiteSpace(dps.jwc.User) || 
                    string.IsNullOrWhiteSpace(dps.jwc.Password) || 
                    string.IsNullOrWhiteSpace(dps.jwc.Claim))
                {
                    ToolHelper.SetResponse(baseResponse, "99", "Credenciales incompletas");
                    statusCode = HttpStatusCode.BadRequest;
                }
                else if (request.UserProfile == localProfile && request.UserPass == localPass && request.UserType == localType)
                {
                    if (!int.TryParse(dps.cnf.JwtAlterConfig.MinutesFactor, out int minutes) ||
                        !int.TryParse(dps.cnf.JwtAlterConfig.HoursFactor, out int hours) ||
                        !int.TryParse(dps.cnf.JwtAlterConfig.DaysFactor, out int days))
                    {
                        ToolHelper.SetResponse(baseResponse, "99", "Error de configuración del tiempo de token");
                        statusCode = HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        int tokenTime = minutes * hours * days;
                        //response.StartExec = DateTime.Now;

                        var symetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(dps.jwc.SecretKey));
                        var credentials = new SigningCredentials(symetricKey, SecurityAlgorithms.HmacSha256);
                        var claims = new List<Claim>
                        {
                            new("tipo", localType) // aquí defines el claim personalizado
                        };

                        var token = new JwtSecurityToken(
                            issuer: dps.cnf.JwtAlterConfig.Issuer,
                            audience: dps.cnf.JwtAlterConfig.Audience,
                            claims: claims,
                            expires: DateTime.UtcNow.AddMinutes(tokenTime),
                            signingCredentials: credentials
                        );

                        Token = new JwtSecurityTokenHandler().WriteToken(token);
                        ExpirationDate = $"{ToolHelper.ObtenerFechaExpiracionToken(Token):dd/MM/yyyy-hh:mm:ss tt}";
                    }
                }
                else
                {
                    ToolHelper.SetResponse(baseResponse, "99", "Credenciales incorrectas, generación denegada");
                    statusCode = HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                ToolHelper.SetErrorResponse(baseResponse, ex);
                statusCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                ToolHelper.SetFinalResponse(baseResponse);
                response = dps.mpr.Map<GetTokenRes>(baseResponse);
                response.Token = Token;
                response.ExpirationDate = ExpirationDate;
            }
            return (response, statusCode);
        }
    }
}
