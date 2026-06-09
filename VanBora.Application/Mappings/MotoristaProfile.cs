using AutoMapper;
using VanBora.Application.DTOs.Auth;
using VanBora.Domain.Entities;

namespace VanBora.Application.Mappings;

public sealed class MotoristaProfile : Profile
{
    public MotoristaProfile()
    {
        CreateMap<Usuario, RegistrarMotoristaResponse>()
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.CPF.Valor))
            .ForMember(dest => dest.Cnh, opt => opt.MapFrom(src => src.CNH != null ? src.CNH.Valor : string.Empty))
            .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src =>
                src.Telefone != null ? src.Telefone.ValorCompleto : null))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
    }
}