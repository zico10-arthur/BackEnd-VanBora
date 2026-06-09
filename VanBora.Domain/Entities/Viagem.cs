using VanBora.Domain.Common;
using VanBora.Domain.Enums;

namespace VanBora.Domain.Entities;

public class Viagem
{
    public Guid Id { get; private set; }
    public Guid GerenteUsuarioId { get; private set; }
    public string NomeEvento { get; private set; }
    public DateTime DataEvento { get; private set; }
    public string LocalEvento { get; private set; }
    public DateTime DataPartida { get; private set; }
    public string LocalPartida { get; private set; }
    public decimal PrecoAssento { get; private set; }
    public bool PossuiIngresso { get; private set; }
    public int QuorumMinimo { get; private set; }
    public StatusViagem Status { get; private set; }
    public DateTime CriadoEm { get; private set; }

    // Navigation properties
    public Usuario GerenteUsuario { get; private set; } = null!;
    private readonly List<ViagemVan> _viagemVans = [];
    public IReadOnlyCollection<ViagemVan> ViagemVans => _viagemVans.AsReadOnly();

#pragma warning disable CS8618 // EF Core constructor
    private Viagem() { }
#pragma warning restore CS8618

    public Viagem(
        Guid gerenteUsuarioId,
        string nomeEvento,
        DateTime dataEvento,
        string localEvento,
        DateTime dataPartida,
        string localPartida,
        decimal precoAssento,
        bool possuiIngresso,
        int quorumMinimo)
    {
        Guard.AgainstEmptyGuid(gerenteUsuarioId, nameof(gerenteUsuarioId));
        Guard.AgainstNullOrWhiteSpace(nomeEvento, nameof(nomeEvento));
        Guard.AgainstNullOrWhiteSpace(localEvento, nameof(localEvento));
        Guard.AgainstNullOrWhiteSpace(localPartida, nameof(localPartida));
        Guard.AgainstNegativeOrZero(precoAssento, nameof(precoAssento));
        Guard.AgainstNegativeOrZero(quorumMinimo, nameof(quorumMinimo));
        Guard.AgainstInvalidState(dataPartida >= dataEvento, "Data de partida deve ser anterior à data do evento.");

        Id = Guid.NewGuid();
        GerenteUsuarioId = gerenteUsuarioId;
        NomeEvento = nomeEvento;
        DataEvento = dataEvento;
        LocalEvento = localEvento;
        DataPartida = dataPartida;
        LocalPartida = localPartida;
        PrecoAssento = precoAssento;
        PossuiIngresso = possuiIngresso;
        QuorumMinimo = quorumMinimo;
        Status = StatusViagem.Agendada;
        CriadoEm = DateTime.UtcNow;
    }

    public void AtualizarDados(
        string nomeEvento,
        DateTime dataEvento,
        string localEvento,
        DateTime dataPartida,
        string localPartida,
        bool possuiIngresso)
    {
        Guard.AgainstNullOrWhiteSpace(nomeEvento, nameof(nomeEvento));
        Guard.AgainstNullOrWhiteSpace(localEvento, nameof(localEvento));
        Guard.AgainstNullOrWhiteSpace(localPartida, nameof(localPartida));
        Guard.AgainstInvalidState(dataPartida >= dataEvento, "Data de partida deve ser anterior à data do evento.");

        NomeEvento = nomeEvento;
        DataEvento = dataEvento;
        LocalEvento = localEvento;
        DataPartida = dataPartida;
        LocalPartida = localPartida;
        PossuiIngresso = possuiIngresso;
    }

    public void Iniciar()
    {
        Guard.AgainstInvalidState(Status != StatusViagem.Agendada, "Apenas viagens agendadas podem ser iniciadas.");

        Status = StatusViagem.EmAndamento;
    }

    public void Concluir()
    {
        Guard.AgainstInvalidState(Status != StatusViagem.EmAndamento, "Apenas viagens em andamento podem ser concluídas.");

        Status = StatusViagem.Concluida;
    }

    public void Cancelar()
    {
        Guard.AgainstInvalidState(Status == StatusViagem.Concluida, "Viagem já concluída não pode ser cancelada.");
        Guard.AgainstInvalidState(Status == StatusViagem.Cancelada, "Viagem já está cancelada.");

        Status = StatusViagem.Cancelada;
    }

    public void AdicionarViagemVan(ViagemVan viagemVan)
    {
        Guard.AgainstNull(viagemVan, nameof(viagemVan));

        _viagemVans.Add(viagemVan);
    }

    public void RemoverViagemVan(ViagemVan viagemVan)
    {
        Guard.AgainstNull(viagemVan, nameof(viagemVan));
        Guard.AgainstInvalidState(Status != StatusViagem.Agendada, "Apenas viagens agendadas podem ter vans removidas.");

        _viagemVans.Remove(viagemVan);
    }

 
}
