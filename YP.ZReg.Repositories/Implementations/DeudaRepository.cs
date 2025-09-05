using Microsoft.Data.SqlClient;
using System.Data;
using YP.ZReg.Entities.Model;
using YP.ZReg.Repositories.Interfaces;
using YP.ZReg.Utils.Helpers;

namespace YP.ZReg.Repositories.Implementations
{
    public class DeudaRepository(IBaseRepository _bre) : IDeudaRepository
    {
        private readonly IBaseRepository bre = _bre;
        public async Task InsertarDeuda(Deuda deuda)
        {
            try
            {
                List<SqlParameter> parameters = [
                    new(){ ParameterName = "@p_numero_documento", DbType = DbType.String , Value = deuda.numero_documento},
                    new(){ ParameterName = "@p_llave_principal", DbType = DbType.String , Value = deuda.llave_principal},
                    new(){ ParameterName = "@p_dni", DbType = DbType.String , Value = deuda.dni},
                    new(){ ParameterName = "@p_ruc", DbType = DbType.String , Value = deuda.ruc},
                    new(){ ParameterName = "@p_llave_alterna", DbType = DbType.String , Value = deuda.llave_alterna},
                    new(){ ParameterName = "@p_fecha_vencimiento", DbType = DbType.DateTime , Value = deuda.fecha_vencimiento},
                    new(){ ParameterName = "@p_fecha_emision", DbType = DbType.DateTime , Value = deuda.fecha_emision},
                    new(){ ParameterName = "@p_importe_bruto", DbType = DbType.Decimal , Value = deuda.importe_bruto},
                    new(){ ParameterName = "@p_mora", DbType = DbType.Decimal , Value = deuda.mora},
                    new(){ ParameterName = "@p_gasto_administrativo", DbType = DbType.Decimal , Value = deuda.gasto_administrativo},
                    new(){ ParameterName = "@p_importe_minimo", DbType = DbType.Decimal , Value = deuda.importe_minimo},
                    //new(){ ParameterName = "@p_glosa", DbType = DbType.String , Value = deuda.glosa},
                    new(){ ParameterName = "@p_nombre_cliente", DbType = DbType.String , Value = deuda.nombre_cliente},
                    new(){ ParameterName = "@p_servicio", DbType = DbType.String , Value = deuda.servicio},
                    new(){ ParameterName = "@p_moneda", DbType = DbType.String , Value = deuda.moneda},
                    new(){ ParameterName = "@p_periodo", DbType = DbType.String , Value = deuda.periodo},
                    new(){ ParameterName = "@p_cuota", DbType = DbType.String , Value = deuda.cuota},
                    new(){ ParameterName = "@p_anio", DbType = DbType.String , Value = deuda.anio},
                    new(){ ParameterName = "@p_id_empresa", DbType = DbType.String , Value = deuda.id_empresa},
                    new(){ ParameterName = "@p_estado", DbType = DbType.String , Value = deuda.estado}
                ];
                int filas_afectadas = await bre.EjecutarNonQuerySpAsync("Process.Usp_Insertar_Deuda", parameters);
            }
            catch
            {
                throw;
            }
        }
        public async Task EliminarDeudaPorHash(Deuda deuda)
        {
            try
            {
                string canonica = HashHelper.CalcularLlaveCanonica(deuda.id_empresa, deuda.servicio,
                                    deuda.numero_documento, deuda.dni, deuda.ruc,
                                    deuda.llave_principal, deuda.llave_alterna);
                byte[] hash = HashHelper.CalcularLlaveHash(
                                deuda.id_empresa, deuda.servicio, deuda.numero_documento,
                                deuda.dni, deuda.ruc, deuda.llave_principal, deuda.llave_alterna);
                string var = BitConverter.ToString(hash).Replace("-", "").ToLower();
                List<SqlParameter> parameters = [
                    new("@p_llave_hash", SqlDbType.Binary, 32) { Value = hash}
                ];
                int filas_afectadas = await bre.EjecutarNonQuerySpAsync("Process.Usp_Eliminar_Deuda", parameters);
            }
            catch
            {
                throw;
            }
        }
        public async Task EliminarDeudaPorEmpresa(string codigo_empresa)
        {
            try
            {                
                List<SqlParameter> parameters = [
                    new("@p_empresa", SqlDbType.VarChar) { Value = codigo_empresa}
                ];
                int filas_afectadas = await bre.EjecutarNonQuerySpAsync("Process.Usp_Eliminar_Registros_Por_Empresa", parameters);
            }
            catch
            {
                throw;
            }
        }
        public async Task ActualizarDeudaEnCarga(Deuda deuda)
        {
            try
            {
                string canonica = HashHelper.CalcularLlaveCanonica(deuda.id_empresa, deuda.servicio,
                                    deuda.numero_documento, deuda.dni, deuda.ruc,
                                    deuda.llave_principal, deuda.llave_alterna);
                byte[] hash = HashHelper.CalcularLlaveHash(
                                deuda.id_empresa, deuda.servicio, deuda.numero_documento,
                                deuda.dni, deuda.ruc, deuda.llave_principal, deuda.llave_alterna);
                string var = BitConverter.ToString(hash).Replace("-", "").ToLower();
                List<SqlParameter> parameters = [
                    new("@p_llave_hash", SqlDbType.Binary, 32) { Value = hash},
                    new(){ ParameterName = "@p_fecha_vencimiento", DbType = DbType.DateTime , Value = deuda.fecha_vencimiento},
                    new(){ ParameterName = "@p_fecha_emision", DbType = DbType.DateTime , Value = deuda.fecha_emision},
                    new(){ ParameterName = "@p_importe_bruto", DbType = DbType.Decimal , Value = deuda.importe_bruto},
                    new(){ ParameterName = "@p_mora", DbType = DbType.Decimal , Value = deuda.mora},
                    new(){ ParameterName = "@p_gasto_administrativo", DbType = DbType.Decimal , Value = deuda.gasto_administrativo},
                    new(){ ParameterName = "@p_importe_minimo", DbType = DbType.Decimal , Value = deuda.importe_minimo},
                    new(){ ParameterName = "@p_moneda", DbType = DbType.String , Value = deuda.moneda},
                    new(){ ParameterName = "@p_periodo", DbType = DbType.String , Value = deuda.periodo},
                    new(){ ParameterName = "@p_cuota", DbType = DbType.String , Value = deuda.cuota},
                    new(){ ParameterName = "@p_anio", DbType = DbType.String , Value = deuda.anio}
                ];
                int filas_afectadas = await bre.EjecutarNonQuerySpAsync("Process.Usp_Actualizar_Deuda_En_Carga", parameters);
            }
            catch
            {
                throw;
            }
        }
    }
}