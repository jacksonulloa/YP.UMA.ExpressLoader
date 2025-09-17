using AutoMapper;
using System.Globalization;
using YP.ZReg.Dtos.Contracts.Request;
using YP.ZReg.Dtos.Contracts.Response;
using YP.ZReg.Dtos.Models;
using YP.ZReg.Entities.Generic;
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
                .ForMember(d => d.glosa, opt => opt.MapFrom(s => s.Glosa))
                .ForMember(d => d.fecha_vencimiento, opt => opt.MapFrom(s => TxtProcessor.ConvertToDate(s.FechaVencimiento)))
                .ForMember(d => d.fecha_emision, opt => opt.MapFrom(s => TxtProcessor.ConvertToDate(s.FechaEmision)))
                .ForMember(d => d.saldo, opt => opt.MapFrom(s => 
                    (TxtProcessor.ConvertToDecimal(s.ImporteBruto, 2) +
                    TxtProcessor.ConvertToDecimal(s.Mora, 2) +
                    TxtProcessor.ConvertToDecimal(s.GastoAdministrativo, 2)
                    )))
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
            CreateMap<BaseResponse, GetTokenRes>();
            CreateMap<BaseResponse, GetDebtsRes>();
            CreateMap<BaseResponse, ExecPaymentRes>();
            CreateMap<BaseResponse, ExecReverseRes>();
            CreateMap<Deuda, Debt>()
                .ForMember(d => d.idDeuda, opt => opt.MapFrom(s => s.id))
                .ForMember(d => d.servicio, opt => opt.MapFrom(s => s.servicio))
                .ForMember(d => d.documento, opt => opt.MapFrom(s => s.numero_documento))
                .ForMember(d => d.descripcionDoc, opt => opt.MapFrom(s => s.glosa))
                .ForMember(d => d.fechaVencimiento, opt => opt.MapFrom(s => $"{s.fecha_vencimiento:yyyyMMdd}"))
                .ForMember(d => d.fechaEmision, opt => opt.MapFrom(s => $"{s.fecha_emision:yyyyMMdd}"))
                .ForMember(d => d.deuda, opt => opt.MapFrom(s => s.saldo))
                .ForMember(d => d.pagoMinimo, opt => opt.MapFrom(s => s.importe_minimo))
                .ForMember(d => d.moneda, opt => opt.MapFrom(s => s.moneda));

            CreateMap<ExecPaymentReq, Transaccion>()
                .ForMember(d => d.fecha_hora_transaccion, opt => opt.MapFrom(s => DateTime.ParseExact(
                    s.fechaTxn + s.horaTxn,
                    "ddMMyyyyHHmmss",
                    CultureInfo.InvariantCulture
                )))
                .ForMember(d => d.id_canal_pago, opt => opt.MapFrom(s => s.idCanal))
                .ForMember(d => d.id_forma_pago, opt => opt.MapFrom(s => s.idForma))
                .ForMember(d => d.numero_operacion, opt => opt.MapFrom(s => s.numeroOperacion))
                .ForMember(d => d.id_consulta, opt => opt.MapFrom(s => s.idConsulta))
                .ForMember(d => d.servicio, opt => opt.MapFrom(s => s.servicio))
                .ForMember(d => d.numero_documento, opt => opt.MapFrom(s => s.numeroDocumento))
                .ForMember(d => d.importe_pagado, opt => opt.MapFrom(s => s.importePagado))
                .ForMember(d => d.moneda, opt => opt.MapFrom(s => s.moneda))
                .ForMember(d => d.id_empresa, opt => opt.MapFrom(s => s.idEmpresa))
                .ForMember(d => d.id_banco, opt => opt.MapFrom(s => s.idBanco))
                .ForMember(d => d.voucher, opt => opt.MapFrom(s => s.voucher))
                .ForMember(d => d.id_deuda, opt => opt.MapFrom(s => s.referenciaDeuda));
            CreateMap<ExecReverseReq, Transaccion>()
                .ForMember(d => d.fecha_hora_transaccion, opt => opt.MapFrom(s => DateTime.ParseExact(
                    s.fechaTxn + s.horaTxn,
                    "ddMMyyyyHHmmss",
                    CultureInfo.InvariantCulture
                )))
                .ForMember(d => d.id_banco, opt => opt.MapFrom(s => s.idBanco))
                .ForMember(d => d.servicio, opt => opt.MapFrom(s => s.idServicio))
                .ForMember(d => d.id_consulta, opt => opt.MapFrom(s => s.idConsulta))
                .ForMember(d => d.numero_operacion, opt => opt.MapFrom(s => s.numeroOperacion))
                .ForMember(d => d.numero_documento, opt => opt.MapFrom(s => s.numeroDocumento))
                .ForMember(d => d.id_empresa, opt => opt.MapFrom(s => s.idEmpresa))
                .ForMember(d => d.voucher, opt => opt.MapFrom(s => s.voucher));
            CreateMap<BaseResponseExtension, BlobTableRecord>()
                .ForMember(dest => dest.CodResp, opt => opt.MapFrom(src => src.CodResp))
                .ForMember(dest => dest.DescResp, opt => opt.MapFrom(src => src.DesResp))
                .ForMember(dest => dest.FechaHoraInicio, opt => opt.MapFrom(src => src.StartExec))
                .ForMember(dest => dest.FechaHoraFin, opt => opt.MapFrom(src => src.EndExec));
            CreateMap<BaseResponse, BlobTableRecord>()
                .ForMember(dest => dest.CodResp, opt => opt.MapFrom(src => src.CodResp))
                .ForMember(dest => dest.DescResp, opt => opt.MapFrom(src => src.DesResp));
            CreateMap<BaseResponse, BaseResponseExtension>()
                .ForMember(dest => dest.CodResp, opt => opt.MapFrom(src => src.CodResp))
                .ForMember(dest => dest.DesResp, opt => opt.MapFrom(src => src.DesResp));
            CreateMap<ResumeLoadProcess, ResumeCompactLoadProcess>();
            CreateMap<ResumeGeneratorProcess, BlobTableRecord>()
                .ForMember(dest => dest.Empresa, opt => opt.MapFrom(src => src.idEmpresa))
                .ForMember(dest => dest.DescResp, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.FechaHoraInicio, opt => opt.MapFrom(src => src.inicio))
                .ForMember(dest => dest.FechaHoraFin, opt => opt.MapFrom(src => src.fin));
        }
    }
}
