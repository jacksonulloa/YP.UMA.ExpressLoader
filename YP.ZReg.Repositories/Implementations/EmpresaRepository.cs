using Microsoft.Data.SqlClient;
using YP.ZReg.Entities.Model;
using YP.ZReg.Repositories.Interfaces;

namespace YP.ZReg.Repositories.Implementations
{
    public class EmpresaRepository(IBaseRepository _bre) : IEmpresaRepository
    {
        private readonly IBaseRepository bre = _bre;

        public async Task<List<Empresa>> ListarEmpresasConServicios(int idEmpresa, int empEstado, CancellationToken ct = default)
        {
            var parametros = new List<SqlParameter>
            {
                new("@id_empresa", idEmpresa),
                new("@emp_estado", empEstado)
            };

            var response = await bre.EjecutarConsultaJoinSpAsync<Empresa, Servicio, int>(
                "[Profile].Usp_Listar_Empresas_Por_Empresa_Estado",
                parametros,
                // mapParent
                r => new Empresa
                {
                    id = r.GetInt32(r.GetOrdinal("id_empresa")),
                    id_proveedor = r.GetString(r.GetOrdinal("id_proveedor")),
                    nombre = r.GetString(r.GetOrdinal("nombre_empresa")),
                    ruc = r.GetString(r.GetOrdinal("ruc")),
                    estado = r.GetInt32(r.GetOrdinal("estado_empresa")),
                    servicios = []
                },
                // mapChild
                r => new Servicio
                {
                    id = r.GetInt32(r.GetOrdinal("id_servicio")),
                    id_empresa = r.GetInt32(r.GetOrdinal("id_empresa")),
                    codigo = r.GetString(r.GetOrdinal("codigo")),
                    nombre = r.GetString(r.GetOrdinal("nombre_servicio")),
                    moneda = r.GetInt32(r.GetOrdinal("moneda")),
                    tipo_validacion = r.GetString(r.GetOrdinal("tipo_validacion")),
                    tipo_pago = r.GetString(r.GetOrdinal("tipo_pago")),
                    estado = r.GetInt32(r.GetOrdinal("estado_servicio")),
                    numero_cuenta = r.GetString(r.GetOrdinal("numero_cuenta"))
                },
                // childCollection
                e => e.servicios,
                // keyParent
                e => e.id,
                ct
            );
            return response;
        }
    }
}
