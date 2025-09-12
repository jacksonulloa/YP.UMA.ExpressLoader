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
                    new(){ ParameterName = "@p_glosa", DbType = DbType.String , Value = deuda.glosa},
                    new(){ ParameterName = "@p_llave_principal", DbType = DbType.String , Value = deuda.llave_principal},
                    new(){ ParameterName = "@p_dni", DbType = DbType.String , Value = deuda.dni},
                    new(){ ParameterName = "@p_ruc", DbType = DbType.String , Value = deuda.ruc},
                    new(){ ParameterName = "@p_llave_alterna", DbType = DbType.String , Value = deuda.llave_alterna},
                    new(){ ParameterName = "@p_fecha_vencimiento", DbType = DbType.DateTime , Value = deuda.fecha_vencimiento},
                    new(){ ParameterName = "@p_saldo", DbType = DbType.Decimal , Value = deuda.saldo},
                    new(){ ParameterName = "@p_fecha_emision", DbType = DbType.DateTime , Value = deuda.fecha_emision},
                    new(){ ParameterName = "@p_importe_bruto", DbType = DbType.Decimal , Value = deuda.importe_bruto},
                    new(){ ParameterName = "@p_mora", DbType = DbType.Decimal , Value = deuda.mora},
                    new(){ ParameterName = "@p_gasto_administrativo", DbType = DbType.Decimal , Value = deuda.gasto_administrativo},
                    new(){ ParameterName = "@p_importe_minimo", DbType = DbType.Decimal , Value = deuda.importe_minimo},
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
                    new(){ ParameterName = "@p_saldo", DbType = DbType.Decimal , Value = (deuda.importe_bruto + deuda.mora + deuda.gasto_administrativo)},
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="empresa"></param>
        /// <param name="servicio"></param>
        /// <param name="llave"></param>
        /// <param name="estado">P:Pending|C:Complete|E:Everything</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<List<Cliente>> ConsultarDeuda(string empresa, string servicio, string llave, string estado, CancellationToken ct = default)
        {
            List<Cliente> response = [];
            try
            {
                var parametros = new List<SqlParameter>
            {
                new("@empresa", empresa),
                new("@servicio", servicio),
                new("@llave", llave),
                new("@estado", estado)
            };
                response = await bre.EjecutarConsultaJoinSpAsync<Cliente, Deuda, string>(
                                    "[Transac].[Usp_Listar_Deudas_Por_Llave]",
                                    parametros,
                            // mapParent
                            r => new Cliente
                            {
                                nombre = r.GetString(r.GetOrdinal("nombre_cliente"))
                                //deudas = new List<Deuda>()            
                            },
                            // mapChild
                            r => new Deuda
                            {
                                id = r.GetInt64(r.GetOrdinal("id")),
                                numero_documento = r.GetString(r.GetOrdinal("numero_documento")),
                                glosa = r.GetString(r.GetOrdinal("glosa")),
                                llave_principal = r.GetString(r.GetOrdinal("llave_principal")),
                                llave_alterna = r.GetString(r.GetOrdinal("llave_alterna")),
                                dni = r.GetString(r.GetOrdinal("dni")),
                                ruc = r.GetString(r.GetOrdinal("ruc")),
                                fecha_emision = r.GetDateTime(r.GetOrdinal("fecha_emision")),
                                fecha_vencimiento = r.GetDateTime(r.GetOrdinal("fecha_vencimiento")),
                                importe_bruto = r.GetDecimal(r.GetOrdinal("importe_bruto")),
                                mora = r.GetDecimal(r.GetOrdinal("mora")),
                                gasto_administrativo = r.GetDecimal(r.GetOrdinal("gasto_administrativo")),
                                importe_minimo = r.GetDecimal(r.GetOrdinal("importe_minimo")),
                                servicio = r.GetString(r.GetOrdinal("servicio")),
                                moneda = r.GetString(r.GetOrdinal("moneda")),
                                periodo = r.GetString(r.GetOrdinal("periodo")),
                                cuota = r.GetString(r.GetOrdinal("cuota")),
                                anio = r.GetString(r.GetOrdinal("anio")),
                                estado = r.GetString(r.GetOrdinal("estado")),
                                id_empresa = r.GetString(r.GetOrdinal("id_empresa")),
                                saldo = r.GetDecimal(r.GetOrdinal("saldo"))

                            },
                            // childCollection
                            e => e.deudas,
                            // keyParent (clave compuesta empresa+id)
                            //e => e.id,
                            e => e.nombre
                                );
            }
            catch
            {
                throw;
            }
            return response;
        }
    }
}