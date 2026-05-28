using FluentValidation;
using VanBora.Application.DTOs.Auth;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.Interfaces;
using VanBora.Domain.ValueObjects;
using AutoMapper;
using VanBora.Application.Interfaces;
namespace VanBora.Application.Services;

public class MotoristaService : IMotoristaService
{
    private readonly IUsuarioRepository _repository;
    private readonly IValidator<RegistrarMotoristaRequest> _validator;

    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;

    public MotoristaService(IUsuarioRepository repository, IValidator<RegistrarMotoristaRequest> validator, IMapper mapper, IUnitOfWork unit0fWork)
    {
        _repository = repository;
        _validator = validator;
        _mapper = mapper;
        _unitOfWork = unit0fWork;   
    }

    public async Task<Result<RegistrarMotoristaResponse>> RegistrarMotorista(Guid gerenteid, RegistrarMotoristaRequest registrarmotorista, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(registrarmotorista, ct);
        if (!validation.IsValid)
        {
            var erros = validation.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToList();

            return Result<RegistrarMotoristaResponse>.Failure(Error.Validation(erros));
        }

        var criador = await _repository.GetByIdAsync(gerenteid, ct);
        

        if (criador is null) return Result<RegistrarMotoristaResponse>.Failure(
            Error.NotFound("Usuario.NaoEncontrado", "O usuário criador não foi encontrado."));

        if (criador.Tipo != TipoUsuario.Gerente) return Result<RegistrarMotoristaResponse>.Failure(
            Error.Unauthorized("Usuario.NaoAutorizado", "O usuário não tem permissão para criar motorista"));

        var cpf = CPF.Criar(registrarmotorista.Cpf);
        if (cpf.IsFailure) return Result<RegistrarMotoristaResponse>.Failure(cpf.Error);

        var telefone = Telefone.Criar(registrarmotorista.Telefone);
        if (telefone.IsFailure) return Result<RegistrarMotoristaResponse>.Failure(telefone.Error);

        var cnh = CNH.Criar(registrarmotorista.Cnh);
        if (cnh.IsFailure) return Result<RegistrarMotoristaResponse>.Failure(cnh.Error);

        var motorista = Usuario.CriarMotorista(
            registrarmotorista.Nome,
            cpf.Value,
            telefone.Value,
            cnh.Value,
            gerenteid
        );  

        await _repository.AddAsync(motorista, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var response = _mapper.Map<RegistrarMotoristaResponse>(motorista);
        return Result<RegistrarMotoristaResponse>.Success(response);



       
    }
}
