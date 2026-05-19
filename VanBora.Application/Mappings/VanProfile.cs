using AutoMapper;
using VanBora.Application.DTOs.Vans;
using VanBora.Domain.Entities;

namespace VanBora.Application.Mappings;

public sealed class VanProfile : Profile
{
    public VanProfile()
    {
        CreateMap<Van, VanResponse>()
            .ForMember(dest => dest.Placa, opt => opt.MapFrom(src => src.Placa.Valor));
    }
}
