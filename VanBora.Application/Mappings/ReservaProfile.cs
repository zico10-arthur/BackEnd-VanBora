using AutoMapper;
using VanBora.Application.DTOs.Reservas;
using VanBora.Domain.Entities;

namespace VanBora.Application.Mappings;

public sealed class ReservaProfile : Profile
{
    public ReservaProfile()
    {
        CreateMap<Reserva, ReservaResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Itens, opt => opt.MapFrom(src => src.Itens));

        CreateMap<ItemReserva, ItemReservaResponse>()
            .ForMember(dest => dest.PrecoAssento, opt => opt.MapFrom(src => src.PrecoAssento.Valor));
    }
}
