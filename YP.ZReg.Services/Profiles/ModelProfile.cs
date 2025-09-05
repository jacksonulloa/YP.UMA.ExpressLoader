using AutoMapper;
using YP.ZReg.Entities.Model;
using YP.ZReg.Utils.Helpers;

namespace YP.ZReg.Services.Profiles
{
    public class ModelProfile : Profile
    {
        public ModelProfile()
        {
            CreateMap<RowTextRecord, Deuda>()
                .ForMember(d => d.llave_principal, opt => opt.MapFrom(s => s.Llave1))
                .ForMember(d => d.nombre_cliente, opt => opt.MapFrom(s => s.NombreCliente))
                .ForMember(d => d.servicio, opt => opt.MapFrom(s => s.CodigoServicio))
                .ForMember(d => d.numero_documento, opt => opt.MapFrom(s => s.NroDocumento))
                .ForMember(d => d.fecha_vencimiento, opt => opt.MapFrom(s => TxtProcessor.ConvertToDate(s.FechaVencimiento)))
                .ForMember(d => d.fecha_emision, opt => opt.MapFrom(s => TxtProcessor.ConvertToDate(s.FechaEmision)))
                .ForMember(d => d.importe_bruto, opt => opt.MapFrom(s => TxtProcessor.ConvertToDecimal(s.ImporteBruto, 2)))
                .ForMember(d => d.mora, opt => opt.MapFrom(s => TxtProcessor.ConvertToDecimal(s.Mora, 2)))
                .ForMember(d => d.gasto_administrativo, opt => opt.MapFrom(s => TxtProcessor.ConvertToDecimal(s.GastoAdministrativo, 2)))
                .ForMember(d => d.importe_minimo, opt => opt.MapFrom(s => TxtProcessor.ConvertToDecimal(s.ImporteMinimo, 2)))
                .ForMember(d => d.periodo, opt => opt.MapFrom(s => s.Periodo))
                .ForMember(d => d.anio, opt => opt.MapFrom(s => s.Anio))
                .ForMember(d => d.cuota, opt => opt.MapFrom(s => s.Cuota))
                .ForMember(d => d.moneda, opt => opt.MapFrom(s => s.Moneda))
                .ForMember(d => d.dni, opt => opt.MapFrom(s => s.Dni))
                .ForMember(d => d.ruc, opt => opt.MapFrom(s => s.Ruc))
                .ForMember(d => d.llave_alterna, opt => opt.MapFrom(s => s.Llave4))
                .ForMember(d => d.id_empresa, opt => opt.MapFrom(s => s.CodigoEmpresa));
        }
    }
}
