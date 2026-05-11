using VanBora.Domain.Common;
using VanBora.Domain.ValueObjects;

namespace VanBora.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public CPF CPF { get; private set; }
    public Email? Email { get; private set; }
    public string? SenhaHash { get; private set; }
    public Telefone? Telefone { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private readonly List<Perfil> _perfis = [];
    public IReadOnlyCollection<Perfil> Perfis => _perfis.AsReadOnly();

#pragma warning disable CS8618 // EF Core constructor
    private Usuario() { }
#pragma warning restore CS8618

    public Usuario(string nome, CPF cpf, Email? email, string? senhaHash, Telefone? telefone)
    {
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome));
        Guard.AgainstNull(cpf, nameof(cpf));

        Id = Guid.NewGuid();
        Nome = nome;
        CPF = cpf;
        Email = email;
        SenhaHash = senhaHash;
        Telefone = telefone;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void AtualizarDados(string nome, Email? email, Telefone? telefone)
    {
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome));

        Nome = nome;
        Email = email;
        Telefone = telefone;
    }

    public void DefinirSenha(string senhaHash)
    {
        Guard.AgainstNullOrWhiteSpace(senhaHash, nameof(senhaHash));

        SenhaHash = senhaHash;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void Desativar()
    {
        Ativo = false;
    }

    public void AdicionarPerfil(Perfil perfil)
    {
        Guard.AgainstNull(perfil, nameof(perfil));

        Guard.AgainstInvalidState(
            !_perfis.Any(p => p.Tipo == perfil.Tipo),
            $"O usuário já possui um perfil do tipo {perfil.Tipo}.");

        _perfis.Add(perfil);
    }
}
