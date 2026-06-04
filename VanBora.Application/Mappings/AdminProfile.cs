using AutoMapper;
using VanBora.Application.DTOs.Admin;
using VanBora.Domain.Entities;

namespace VanBora.Application.Mappings;

public sealed class AdminProfile : Profile
{
    public AdminProfile()
    {
        CreateMap<Usuario, GerenteAdminResponse>()
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.CPF.Valor))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email != null ? src.Email.Valor : null))
            .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src =>
                src.Telefone != null ? src.Telefone.DDD + src.Telefone.Numero : null))
            .ForMember(dest => dest.TotalVans, opt => opt.Ignore())
            .ForMember(dest => dest.TotalViagens, opt => opt.Ignore());

        CreateMap<Usuario, UsuarioAdminResponse>()
            .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.CPF.Valor))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email != null ? src.Email.Valor : null))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(dest => dest.TotalReservas, opt => opt.Ignore());

        CreateMap<Reserva, ReservaHistoricoResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CriadaEm, opt => opt.MapFrom(src => src.CriadoEm))
            .ForMember(dest => dest.Viagem, opt => opt.MapFrom(src => src.ViagemVan.Viagem))
            .ForMember(dest => dest.Itens, opt => opt.MapFrom(src => src.Itens));

        CreateMap<ItemReserva, ItemReservaHistoricoResponse>()
            .ForMember(dest => dest.Assento, opt => opt.MapFrom(src => src.NumeroAssento))
            .ForMember(dest => dest.PassageiroNome, opt => opt.MapFrom(src => src.NomePassageiro))
            .ForMember(dest => dest.PassageiroDocumento, opt => opt.MapFrom(src => src.CPFPassageiro.Valor))
            .ForMember(dest => dest.Valor, opt => opt.MapFrom(src => src.PrecoAssento.Valor));

        CreateMap<Viagem, ViagemResumoResponse>()
            .ForMember(dest => dest.Origem, opt => opt.MapFrom(src => src.LocalPartida))
            .ForMember(dest => dest.Destino, opt => opt.MapFrom(src => src.LocalEvento));
    }
}
