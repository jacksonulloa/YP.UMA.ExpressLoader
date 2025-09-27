using Org.BouncyCastle.Ocsp;
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
    public class ApiTransacService(IDependencyProviderService _dps, IDeudaRepository _der,
        ITransaccionRepository _trr, IEmpresaCache _emc) : IApiTransacService
    {
        public readonly IDependencyProviderService dps = _dps;
        public IDeudaRepository der = _der;
        public ITransaccionRepository trr = _trr;
        public IEmpresaCache emc = _emc;
        public async Task<(GetDebtsRes, HttpStatusCode)> GetDebtsAsync(GetDebtsReq request)
        {
            BaseResponseExtension responseBase = new() { CodResp = "00", DesResp = "Conforme" };
            string cliente = string.Empty;
            List<Debt> deudas = [];
            try
            {
                List<Cliente> listaBdSinDepurar = await der.ConsultarDeuda(request.empresa, request.servicio, request.id, "E", default);
                if (listaBdSinDepurar.Count > 0)
                {
                    cliente = listaBdSinDepurar.First().nombre;
                    List<Deuda> listaBdDepurada = listaBdSinDepurar[0].deudas.Where(x=>x.estado.Equals("P")).ToList();
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
            ToolHelper.SetFinalResponse(responseBase);
            var responseApi = dps.mpr.Map<GetDebtsRes>(responseBase);
            responseApi.cliente = cliente;
            responseApi.deudas = deudas;
            return (responseApi, HttpStatusCode.OK);
        }
        public async Task<(ExecPaymentRes, HttpStatusCode)> ExecPaymentAsync(ExecPaymentReq request)
        {            
            BaseResponseExtension responseBase = new() { CodResp = "00", DesResp = "Conforme" };
            string cliente = string.Empty;
            long id = 0;
            try
            {
                Empresa? empresa = emc.empresas.FirstOrDefault(x => x.id_proveedor == request.idEmpresa);
                Servicio? servicio = null;
                if (empresa is null)
                {
                    ToolHelper.SetResponse(responseBase, "99", "No se encontró empresa");
                }
                else if ((servicio = empresa.servicios?.FirstOrDefault(x => x.codigo == request.servicio)) is null)
                {
                    ToolHelper.SetResponse(responseBase, "99", "No se encontró servicio");
                }
                else
                {
                    var transaccion = dps.mpr.Map<Transaccion>(request);
                    transaccion.tipo_transac = "P";
                    transaccion.estado_notificacion = "P";
                    transaccion.tipo_validacion = servicio.tipo_validacion;
                    transaccion.cuenta_banco = servicio.numero_cuenta;
                    (id, cliente) = await trr.InsertarPago(transaccion);
                }
                //if (empresa is null)
                //    ToolHelper.SetResponse(responseBase, "99", "No se encontro empresa");                
                //if (responseBase.CodResp.Equals("00"))
                //{
                //    servicio = empresa.servicios.FirstOrDefault(x=> x.codigo.Equals(request.servicio));
                //    if(servicio is null)
                //    {
                //        ToolHelper.SetResponse(responseBase, "99", "No se encontro servicio");
                //    }
                //}
                //if(responseBase.CodResp.Equals("00"))
                //{
                //    Transaccion transaccion = dps.mpr.Map<Transaccion>(request);
                //    transaccion.tipo_transac = "P";
                //    transaccion.estado_notificacion = "P";
                //    transaccion.tipo_validacion = servicio.tipo_validacion;
                //    (id, cliente) = await trr.InsertarPago(transaccion);
                //}                
            }
            catch (Exception ex)
            {
                ToolHelper.SetErrorResponse(responseBase, ex);
            }
            ToolHelper.SetFinalResponse(responseBase);
            ExecPaymentRes responseApi = dps.mpr.Map<ExecPaymentRes>(responseBase);
            responseApi.cliente = cliente;
            responseApi.operacionErp = id > 0 ? id.ToString() : string.Empty;
            return (responseApi, HttpStatusCode.OK);
        }
        public async Task<(ExecReverseRes, HttpStatusCode)> ExecReverseAsync(ExecReverseReq request)
        {
            BaseResponseExtension responseBase = new() { CodResp = "00", DesResp = "Conforme" };
            string cliente = string.Empty;
            long id = 0;
            try
            {
                Transaccion transaccion = dps.mpr.Map<Transaccion>(request);
                transaccion.tipo_transac = "R";
                transaccion.estado_notificacion = "P";
                (id, cliente) = await trr.AplicarReversa(transaccion);
            }
            catch (Exception ex)
            {
                ToolHelper.SetErrorResponse(responseBase, ex);
            }
            ToolHelper.SetFinalResponse(responseBase);
            var responseApi = dps.mpr.Map<ExecReverseRes>(responseBase);
            responseApi.cliente = cliente;
            responseApi.operacionErp = id > 0 ? id.ToString() : string.Empty;
            return (responseApi, HttpStatusCode.OK);
        }
    }
}
