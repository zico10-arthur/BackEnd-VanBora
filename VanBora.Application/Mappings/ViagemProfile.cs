using AutoMapper;
using VanBora.Application.DTOs.Viagens;
using VanBora.Domain.Entities;

namespace VanBora.Application.Mappings;

public sealed class ViagemProfile : Profile
{
    public ViagemProfile()
    {
        CreateMap<Viagem, ViagemResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Vans, opt => opt.MapFrom(src => src.ViagemVans));

        CreateMap<ViagemVan, ViagemVanResponse>()
            .ForMember(dest => dest.VanId, opt => opt.MapFrom(src => src.VanId))
            .ForMember(dest => dest.NomeVan, opt => opt.MapFrom(src => src.Van.Nome))
            .ForMember(dest => dest.PlacaVan, opt => opt.MapFrom(src => src.Van.Placa.Valor))
            .ForMember(dest => dest.CapacidadeVan, opt => opt.MapFrom(src => src.Van.Capacidade))
            .ForMember(dest => dest.AssentosDisponiveis, opt => opt.MapFrom(src => src.ObterQuantidadeAssentosParaReserva()))
            .ForMember(dest => dest.MotoristaUsuarioId, opt => opt.MapFrom(src => src.MotoristaUsuarioId));
    }
}
