using Microsoft.Data.SqlClient;
using System.Data;
using YP.ZReg.Entities.Model;
using YP.ZReg.Repositories.Interfaces;

namespace YP.ZReg.Repositories.Implementations
{
    public class TransaccionRepository(IBaseRepository _bre) : ITransaccionRepository
    {
        private readonly IBaseRepository bre = _bre;
        public async Task<(long, string)> InsertarPago(Transaccion transaccion)
        {
            long idGen = 0;
            string nombreCliente = "";
            try
            {
                //bre.SetParameter("@p_importe_pagado", SqlDbType.Decimal, transaccion.importe_pagado, precision: 12, scale: 2);
                var paramIdTransaccion = bre.SetParameter("@p_id_transaccion", SqlDbType.BigInt, null, direction: ParameterDirection.Output);
                var paramNombreCliente = bre.SetParameter("@p_nombre_cliente", SqlDbType.VarChar, null, size: 30, direction: ParameterDirection.Output);
                List<SqlParameter> parameters = [
                    new(){ ParameterName = "@p_fecha_hora_transaccion", DbType = DbType.DateTime , Value = transaccion.fecha_hora_transaccion},
                    new(){ ParameterName = "@p_id_canal_pago", DbType = DbType.String , Value = transaccion.id_canal_pago},
                    new(){ ParameterName = "@p_id_forma_pago", DbType = DbType.String , Value = transaccion.id_forma_pago},
                    new(){ ParameterName = "@p_numero_operacion", DbType = DbType.String , Value = transaccion.numero_operacion},
                    new(){ ParameterName = "@p_id_consulta", DbType = DbType.String , Value = transaccion.id_consulta},
                    new(){ ParameterName = "@p_servicio", DbType = DbType.String , Value = transaccion.servicio},
                    new(){ ParameterName = "@p_numero_documento", DbType = DbType.String , Value = transaccion.numero_documento},
                    new(){ ParameterName = "@p_importe_pagado", DbType = DbType.Decimal , Value = transaccion.importe_pagado},
                    new(){ ParameterName = "@p_moneda", DbType = DbType.String , Value = transaccion.moneda},
                    new(){ ParameterName = "@p_id_empresa", DbType = DbType.String , Value = transaccion.id_empresa},
                    new(){ ParameterName = "@p_tipo_transac", DbType = DbType.String , Value = transaccion.tipo_transac},
                    new(){ ParameterName = "@p_id_banco", DbType = DbType.String , Value = transaccion.id_banco},
                    new(){ ParameterName = "@p_voucher", DbType = DbType.String , Value = transaccion.voucher},
                    new(){ ParameterName = "@p_cuenta_banco", DbType = DbType.String , Value = transaccion.cuenta_banco},
                    new(){ ParameterName = "@p_id_deuda", DbType = DbType.Int64 , Value = transaccion.id_deuda},
                    new(){ ParameterName = "@p_estado_notificacion", DbType = DbType.String , Value = transaccion.estado_notificacion},
                    paramIdTransaccion,
                    paramNombreCliente
                ];
                int filas_afectadas = await bre.EjecutarNonQuerySpAsync("[Transac].[Usp_Insertar_Pago]", parameters);
                idGen = (long)(paramIdTransaccion.Value ?? 0);
                //nombreCliente = (string)(paramNombreCliente.Value ?? "");
                nombreCliente = Convert.ToString(paramNombreCliente.Value) ?? string.Empty;
            }
            catch
            {
                throw;
            }
            return (idGen, nombreCliente);
        }
        public async Task<(long, string)> AplicarReversa(Transaccion transaccion)
        {
            long idGen = 0;
            string nombreCliente = "";
            try
            {
                var paramIdTransaccion = bre.SetParameter("@p_id_transaccion", SqlDbType.BigInt, null, 12, direction: ParameterDirection.Output);
                var paramNombreCliente = bre.SetParameter("@p_nombre_cliente", SqlDbType.VarChar, null, 30, direction: ParameterDirection.Output);
                List<SqlParameter> parameters = [
                    new(){ ParameterName = "@p_fecha_hora_transaccion", DbType = DbType.DateTime , Value = transaccion.fecha_hora_transaccion},
                    new(){ ParameterName = "@p_id_banco", DbType = DbType.String , Value = transaccion.id_banco},
                    new(){ ParameterName = "@p_id_consulta", DbType = DbType.String , Value = transaccion.id_consulta},
                    new(){ ParameterName = "@p_numero_operacion", DbType = DbType.String , Value = transaccion.numero_operacion},
                    new(){ ParameterName = "@p_numero_documento", DbType = DbType.String , Value = transaccion.numero_documento},
                    new(){ ParameterName = "@p_id_empresa", DbType = DbType.String , Value = transaccion.id_empresa},
                    new(){ ParameterName = "@p_servicio", DbType = DbType.String , Value = transaccion.servicio},
                    new(){ ParameterName = "@p_tipo_transac", DbType = DbType.String , Value = transaccion.tipo_transac},
                    new(){ ParameterName = "@p_voucher", DbType = DbType.String , Value = transaccion.voucher},
                    new(){ ParameterName = "@p_estado_notificacion", DbType = DbType.String , Value = transaccion.estado_notificacion},
                    paramIdTransaccion,
                    paramNombreCliente
                ];
                int filas_afectadas = await bre.EjecutarNonQuerySpAsync("[Transac].[Usp_Insertar_Reversa]", parameters);
                idGen = (long)(paramIdTransaccion.Value ?? 0);
                //nombreCliente = (string)(paramNombreCliente.Value ?? "");
                nombreCliente = Convert.ToString(paramNombreCliente.Value) ?? string.Empty;
            }
            catch
            {
                throw;
            }
            return (idGen, nombreCliente);
        }
    }
}
