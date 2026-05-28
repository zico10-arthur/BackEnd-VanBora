using AutoMapper;
using VanBora.Domain.Entities;
using VanBora.Application.DTOs.Auth;
namespace VanBora.Application.Mappings;

public sealed class MotoristaProfile : Profile
{
    public MotoristaProfile()
    {
         CreateMap<Usuario, RegistrarMotoristaResponse>();
    }
}
