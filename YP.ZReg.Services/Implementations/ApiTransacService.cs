using System.Net;
using YP.ZReg.Dtos.Contracts.Request;
using YP.ZReg.Dtos.Contracts.Response;
using YP.ZReg.Entities.Generic;
using YP.ZReg.Entities.Model;
using YP.ZReg.Repositories.Interfaces;
using YP.ZReg.Services.Interfaces;
using YP.ZReg.Utils.Helpers;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Services.Implementations
{
    public class ApiTransacService(IDependencyProviderService _dps, IDeudaRepository _der) : IApiTransacService
    {
        public readonly IDependencyProviderService dps = _dps;
        public IDeudaRepository der = _der;
        public async Task<(GetDebtsRes, HttpStatusCode)> GetDebtsAsync(GetDebtsReq request)
        {
            GetDebtsRes responseApi = new() { CodResp = "00", DesResp = "Conforme" };
            BaseResponseExtension responseBase = new() { CodResp = "00", DesResp = "Conforme" };
            string cliente = "";
            List<Debt> deudas = [];
            HttpStatusCode statusCode = HttpStatusCode.OK;
            try
            {
                List<Cliente> listaBdSinDepurar = await der.ConsultarDeuda(request.empresa, request.servicio, request.id, default);
                if (listaBdSinDepurar.Count > 0)
                {
                    cliente = listaBdSinDepurar.First().nombre;
                    List<Deuda> listaBdDepurada = [.. listaBdSinDepurar.First().deudas.Where(x => x.estado.Equals("P"))];
                    deudas = dps.mpr.Map<List<Debt>>(listaBdDepurada);
                    responseBase.CodResp = deudas.Count == 0 ? "22" : responseBase.CodResp;
                    responseBase.DesResp = deudas.Count == 0 ? "Sin deudas" : responseBase.DesResp;
                }
                else
                {
                    responseBase.CodResp = "16";
                    responseBase.DesResp = "Cliente no existe";
                }
            }
            catch (Exception ex)
            {
                ToolHelper.SetErrorResponse(responseBase, ex);
            }
            finally
            {
                ToolHelper.SetFinalResponse(responseBase);
                responseApi = dps.mpr.Map<GetDebtsRes>(responseBase);
                responseApi.cliente = cliente;
                responseApi.deudas = deudas;
            }
            return (responseApi, statusCode);
        }
        public async Task<(ExecPaymentRes, HttpStatusCode)> ExecPaymentAsync(ExecPaymentReq request)
        {
            ExecPaymentRes responseApi = new() { CodResp = "00", DesResp = "Conforme" };
            BaseResponseExtension responseBase = new() { CodResp = "00", DesResp = "Conforme" };
            string cliente = "";
            string operacionErp = "";
            HttpStatusCode statusCode = HttpStatusCode.OK;
            try
            {

            }
            catch (Exception ex)
            {
                ToolHelper.SetErrorResponse(responseBase, ex);
                cliente = "";
                operacionErp = "";
            }
            finally
            {
                ToolHelper.SetFinalResponse(responseBase);
                responseApi = dps.mpr.Map<ExecPaymentRes>(responseBase);
                responseApi.cliente = cliente;
                responseApi.operacionErp = operacionErp;
            }
            return (responseApi, statusCode);
        }
        public async Task<(ExecReverseRes, HttpStatusCode)> ExecReverseAsync(ExecReverseReq request)
        {
            ExecReverseRes responseApi = new() { CodResp = "00", DesResp = "Conforme" };
            BaseResponseExtension responseBase = new() { CodResp = "00", DesResp = "Conforme" };
            string cliente = "";
            string operacionErp = "";
            HttpStatusCode statusCode = HttpStatusCode.OK;
            try
            {
            }
            catch (Exception ex)
            {
                ToolHelper.SetErrorResponse(responseBase, ex);
                cliente = "";
                operacionErp = "";
            }
            finally
            {
                ToolHelper.SetFinalResponse(responseBase);
                responseApi = dps.mpr.Map<ExecReverseRes>(responseBase);
                responseApi.cliente = cliente;
                responseApi.operacionErp = operacionErp;
            }
            return (responseApi, statusCode);
        }
    }
}
