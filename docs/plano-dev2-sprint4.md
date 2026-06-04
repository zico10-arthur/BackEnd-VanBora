# Plano de Implementação — Dev 2 — Sprint 4

> **Baseado no** [`docs/technical-plan.md`](docs/technical-plan.md)
> **Sprint 4:** Conta, Admin e Motorista
> **Dev 2:** US13 (Admin: Gerenciar Gerentes — 5 SP) + US22 (Admin: Buscar Usuarios — 3 SP) + US23 (Admin: Histórico Reservas — 3 SP)
> **Total:** 11 Story Points

---

## Sumário das Tarefas

| # | US | Descrição | SP | Depends | Status |
|---|----|-----------|----|---------|--------|
| 4.4 | **US13** | Admin: Gerenciar Gerentes | 5 | Sprint 1 (auth) | ✅ Implementado |
| 4.5 | **US22** | Admin: Buscar Usuarios | 3 | Sprint 1 (auth) | ✅ Implementado |
| 4.6 | **US23** | Admin: Histórico Reservas | 3 | Sprint 3 (reservas) | ✅ Implementado |

### Mapa de Sub-tarefas

| Sub | SP | Descrição | US | Status |
|-----|----|-----------|----|--------|
| 4.4.1 | 1.0 | Repositório — novos métodos de query | US13 | ✅ |
| 4.4.2 | 2.5 | DTOs + Service + Mapping (AdminService) | US13 | ✅ |
| 4.4.3 | 1.5 | Controllers + DI + Test | US13 | ✅ |
| 4.5.1 | 1.5 | DTOs + Service (BuscarUsuarios) | US22 | ✅ |
| 4.5.2 | 1.5 | Controller + DI + Test | US22 | ✅ |
| 4.6.1 | 1.5 | Repositório + Service (Histórico Reservas) | US23 | ✅ |
| 4.6.2 | 1.5 | Controllers + DI + Test | US23 | ✅ |

---

## US13 — Admin: Gerenciar Gerentes (5 SP)

### Objetivo Geral

Implementar endpoints administrativos que permitam ao **Admin** listar, buscar, criar e editar gerentes. O Admin pode ajustar `taxaPlataforma`, `gratuito` e `ativo` de cada gerente individualmente.

### Regras de Negócio (Checklist)

- [✅] Apenas usuários com `Tipo = Admin` podem acessar os endpoints
- [✅] `GET /api/admin/gerentes` — Lista todos os gerentes com dados completos
- [✅] `GET /api/admin/gerentes?search=termo` — Filtra por nome via ILike
- [✅] `POST /api/admin/gerentes` — Admin pode criar gerente manualmente
- [✅] `PUT /api/admin/gerentes/{id}` — Atualizar `taxaPlataforma`, `gratuito`, `ativo`
- [✅] A alteração de `taxaPlataforma` só afeta **novas reservas**
- [✅] `GET /api/admin/gerentes/{id}` — Detalhes de um gerente específico

### Endpoints da US13

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/admin/gerentes` | Listar gerentes (com opção `?search=`) |
| `GET` | `/api/admin/gerentes/{id}` | Detalhes de um gerente |
| `POST` | `/api/admin/gerentes` | Criar gerente (Admin) |
| `PUT` | `/api/admin/gerentes/{id}` | Atualizar taxa/0800/ativo do gerente |

---

### Sub-tarefa 4.4.1 — Repositório: novos métodos de query (1 SP)

**Objetivo:** Adicionar métodos no `IUsuarioRepository` e `UsuarioRepository` para consultas específicas do Admin.

#### Arquivos a modificar

| # | Arquivo | Caminho |
|---|---------|---------|
| 1 | `IUsuarioRepository.cs` | `VanBora.Domain/Interfaces/IUsuarioRepository.cs` |
| 2 | `UsuarioRepository.cs` | `VanBora.Infrastructure/Repositories/UsuarioRepository.cs` |

#### Métodos a adicionar na interface

```csharp
// IUsuarioRepository.cs — adicionar:

Task<List<Usuario>> GetGerentesAsync(string? search, CancellationToken cancellationToken = default);
```

> **Nota:** `SearchAsync(string termo)` já existe e faz busca por nome com ILike. O novo método `GetGerentesAsync` estende isso filtrando apenas `Tipo == Gerente` e permitindo search opcional.

#### Implementação no `UsuarioRepository.cs`

```csharp
public async Task<List<Usuario>> GetGerentesAsync(
    string? search,
    CancellationToken cancellationToken = default)
{
    var query = _context.Usuarios
        .Where(u => u.Tipo == TipoUsuario.Gerente);

    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(u =>
            EF.Functions.ILike(u.Nome, $"%{search}%") ||
            (u.Slug != null && EF.Functions.ILike(u.Slug, $"%{search}%")));
    }

    return await query
        .OrderByDescending(u => u.CriadoEm)
        .ToListAsync(cancellationToken);
}
```

---

### Sub-tarefa 4.4.2 — DTOs + Service + Mapping (2.5 SP)

**Objetivo:** Criar os DTOs de Admin, o `AdminService` com os métodos de gestão de gerentes, e o AutoMapper profile.

#### Arquivos a criar

| # | Arquivo | Caminho |
|---|---------|---------|
| 1 | `GerenteAdminResponse.cs` | `VanBora.Application/DTOs/Admin/GerenteAdminResponse.cs` |
| 2 | `AtualizarGerenteAdminRequest.cs` | `VanBora.Application/DTOs/Admin/AtualizarGerenteAdminRequest.cs` |
| 3 | `CriarGerenteAdminRequest.cs` | `VanBora.Application/DTOs/Admin/CriarGerenteAdminRequest.cs` |
| 4 | `IAdminService.cs` | `VanBora.Application/Interfaces/IAdminService.cs` |
| 5 | `AdminService.cs` | `VanBora.Application/Services/AdminService.cs` |
| 6 | `AdminProfile.cs` | `VanBora.Application/Mappings/AdminProfile.cs` |
| 7 | `AtualizarGerenteAdminValidator.cs` | `VanBora.Application/Validators/AtualizarGerenteAdminValidator.cs` |
| 8 | `CriarGerenteAdminValidator.cs` | `VanBora.Application/Validators/CriarGerenteAdminValidator.cs` |

---

#### 1. `GerenteAdminResponse.cs`

```csharp
namespace VanBora.Application.DTOs.Admin;

public class GerenteAdminResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Telefone { get; init; }
    public string? Slug { get; init; }
    public decimal? TaxaPlataforma { get; init; }
    public bool? Gratuito { get; init; }
    public bool Ativo { get; init; }
    public int TotalVans { get; set; }
    public int TotalViagens { get; set; }
    public DateTime CriadoEm { get; init; }
}
```

| Campo | Origem |
|-------|--------|
| `Id` | `Usuario.Id` |
| `Nome` | `Usuario.Nome` |
| `Cpf` | `Usuario.CPF.Valor` |
| `Email` | `Usuario.Email?.Valor` |
| `Telefone` | `Usuario.Telefone` → formatado como string (ex: "11999999999") |
| `Slug` | `Usuario.Slug` |
| `TaxaPlataforma` | `Usuario.TaxaPlataforma` |
| `Gratuito` | `Usuario.Gratuito` |
| `Ativo` | `Usuario.Ativo` |
| `TotalVans` | Contagem de `Van.GerenteUsuarioId == usuario.Id` |
| `TotalViagens` | Contagem de `Viagem.GerenteUsuarioId == usuario.Id` |
| `CriadoEm` | `Usuario.CriadoEm` |

> ⚠️ **TotalVans e TotalViagens:** Esses campos requerem navegação ou queries separadas. Alternativas:
> - **Opção A (simples):** Usar os métodos existentes `IVanRepository.GetByGerenteUsuarioIdAsync` e `IViagemRepository.GetByGerenteUsuarioIdAsync` para obter as listas e contar `.Count` — um round-trip extra por gerente.
> - **Opção B (eficiente):** Usar `.Select()` com sub-queries no próprio repositório, mapeando diretamente para o DTO, evitando carregar todas as entidades em memória.
> - **Opção C (híbrido):** Carregar contagens via `GroupBy` em uma única query adicional.
>
> **Recomendação:** Para a Sprint 4, usar **Opção A** (simplicidade). Se a lista de gerentes for grande (centenas), migrar para Opção B com projeção direta.

---

#### 2. `AtualizarGerenteAdminRequest.cs`

```csharp
namespace VanBora.Application.DTOs.Admin;

public record AtualizarGerenteAdminRequest(
    decimal? TaxaPlataforma,
    bool? Gratuito,
    bool? Ativo);
```

| Campo | Tipo | Regras |
|-------|------|--------|
| `TaxaPlataforma` | `decimal?` | Se informado, deve ser >= 0, <= 100 |
| `Gratuito` | `bool?` | Se true, taxa = 0 independente do `TaxaPlataforma` |
| `Ativo` | `bool?` | Se false, gerente é desativado (soft delete) |

> **Nota:** Todos os campos são opcionais — o Admin pode atualizar apenas um ou mais campos por vez. O comportamento é **PATCH-like**: campos `null` não são alterados.

---

#### 3. `CriarGerenteAdminRequest.cs`

```csharp
namespace VanBora.Application.DTOs.Admin;

public record CriarGerenteAdminRequest(
    string Nome,
    string Cpf,
    string Email,
    string Senha,
    string? Telefone,
    string Slug,
    decimal? TaxaPlataforma,
    bool? Gratuito,
    string? ChavePix);
```

> **Nota:** Segue a mesma estrutura de `RegistrarGerenteRequest` do `AuthService`, mas exposta na rota de Admin. Pode-se reutilizar o `AuthService.RegistrarGerente()` internamente ou duplicar a lógica no `AdminService`. **Recomendação:** reutilizar o `AuthService` via injeção (composição, não herança).

---

#### 4. `IAdminService.cs`

```csharp
using VanBora.Application.DTOs.Admin;
using VanBora.Domain.Common;

namespace VanBora.Application.Interfaces;

public interface IAdminService
{
    // US13 — Gestão de Gerentes
    Task<Result<List<GerenteAdminResponse>>> ListarGerentesAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<Result<GerenteAdminResponse>> ObterGerentePorIdAsync(
        Guid gerenteId,
        CancellationToken cancellationToken = default);

    Task<Result<GerenteAdminResponse>> CriarGerenteAsync(
        CriarGerenteAdminRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<GerenteAdminResponse>> AtualizarGerenteAsync(
        Guid gerenteId,
        AtualizarGerenteAdminRequest request,
        CancellationToken cancellationToken = default);

    // US22 — Buscar Usuarios
    Task<Result<List<UsuarioAdminResponse>>> BuscarUsuariosAsync(
        string? search,
        CancellationToken cancellationToken = default);

    // US23 — Histórico de Reservas
    Task<Result<List<ReservaHistoricoResponse>>> ObterHistoricoReservasUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<Result<List<ViagemGerenteHistoricoResponse>>> ObterHistoricoReservasGerenteAsync(
        Guid gerenteId,
        CancellationToken cancellationToken = default);
}
```

---

#### 5. `AdminService.cs`

**Estrutura do construtor:**

```csharp
public class AdminService : IAdminService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IReservaRepository _reservaRepo;
    private readonly IVanRepository _vanRepo;
    private readonly IViagemRepository _viagemRepo;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AtualizarGerenteAdminRequest> _atualizarGerenteValidator;
    private readonly IValidator<CriarGerenteAdminRequest> _criarGerenteValidator;

    public AdminService(
        IUsuarioRepository usuarioRepo,
        IReservaRepository reservaRepo,
        IVanRepository vanRepo,
        IViagemRepository viagemRepo,
        IAuthService authService,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IValidator<AtualizarGerenteAdminRequest> atualizarGerenteValidator,
        IValidator<CriarGerenteAdminRequest> criarGerenteValidator)
    {
        _usuarioRepo = usuarioRepo;
        _reservaRepo = reservaRepo;
        _vanRepo = vanRepo;
        _viagemRepo = viagemRepo;
        _authService = authService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _atualizarGerenteValidator = atualizarGerenteValidator;
        _criarGerenteValidator = criarGerenteValidator;
    }
}
```

> ⚠️ **`IUnitOfWork` duplicado vs repositórios:** `IViagemRepository` e `IVanRepository` expõem `IUnitOfWork UnitOfWork { get; }` (ambos retornam o mesmo `AppDbContext` scoped). O `AdminService` injeta `IUnitOfWork` separadamente para `SaveChangesAsync()`. Na prática, ambos apontam para a mesma instância scoped do `AppDbContext` — não há conflito. Usar o `_unitOfWork` injetado diretamente (em vez de `_viagemRepo.UnitOfWork`) é mais explícito e preferível.

**Método `ListarGerentesAsync`:**

```
1. Buscar gerentes via _usuarioRepo.GetGerentesAsync(search, ct)
2. Para cada gerente, buscar TotalVans e TotalViagens
   - Usar _vanRepo.GetByGerenteUsuarioIdAsync(id, ct) e _viagemRepo.GetByGerenteUsuarioIdAsync(id, ct), depois .Count
3. Mapear para List<GerenteAdminResponse> via AutoMapper (preenchendo counts manualmente)
4. Retornar Success
```

**Método `ObterGerentePorIdAsync`:**

```
1. Buscar usuario por ID
2. Se não encontrado ou Tipo != Gerente → Error.NotFound("GERENTE_NAO_ENCONTRADO")
3. Obter contagens de vans e viagens (TotalVans, TotalViagens)
4. Mapear para GerenteAdminResponse via AutoMapper e preencher counts manualmente
5. Retornar Success
```

**Método `CriarGerenteAsync`:**

```
1. Validar request com CriarGerenteAdminValidator
2. Converter CriarGerenteAdminRequest → RegistrarGerenteRequest:
   - Mapear campo a campo (Nome, Cpf, Email, Senha, Telefone, Slug, ChavePix)
   - ⚠️ RegistrarGerenteRequest NÃO tem TaxaPlataforma/Gratuito — a AuthService define internamente (0800 ou 5%)
3. Chamar _authService.RegistrarGerente(registrarRequest, ct)
4. Se falha, propagar o erro
5. Se sucesso E o Admin especificou TaxaPlataforma ou Gratuito customizado:
   - Obter o ID do gerente recém-criado: registrarResult.Value.UsuarioId
   - Buscar via _usuarioRepo.GetByIdAsync(usuarioId, ct)
   - Chamar usuario.AtualizarParametrosGerente(request.TaxaPlataforma, request.Gratuito)
   - _usuarioRepo.Update(usuario) + _unitOfWork.SaveChangesAsync()
6. Buscar contagens de vans e viagens para o gerente e mapear para GerenteAdminResponse:
   - Se step 5 foi executado: já temos o usuario carregado → só buscar counts e mapear
   - Se step 5 foi pulado: buscar via _usuarioRepo.GetByIdAsync(registrarResult.Value.UsuarioId, ct) → counts → mapear
   Retornar Success
```

**Método `AtualizarGerenteAsync`:**

```
1. Validar request com AtualizarGerenteAdminValidator
2. Buscar usuario por ID
3. Se não encontrado ou Tipo != Gerente → Error.NotFound("GERENTE_NAO_ENCONTRADO")
4. Se todos os campos do request forem null, retornar o estado atual sem alterar:
   - Buscar TotalVans e TotalViagens, mapear para GerenteAdminResponse, retornar Success
5. Aplicar atualizações via método de domínio (apenas campos informados):
   - usuario.AtualizarParametrosGerente(request.TaxaPlataforma, request.Gratuito)
   - Se request.Ativo.HasValue → request.Ativo.Value ? usuario.Ativar() : usuario.Desativar()
6. _usuarioRepo.Update(usuario) + _unitOfWork.SaveChangesAsync()
7. Buscar contagens de vans e viagens, mapear para GerenteAdminResponse (preenchendo counts) e retornar Success
```

> ⚠️ **Adicionar método de domínio em `Usuario.cs`:** O `Usuario` não tem um método público para atualizar `TaxaPlataforma` e `Gratuito`. Será necessário adicionar:
>
> ```csharp
> public void AtualizarParametrosGerente(decimal? taxaPlataforma, bool? gratuito)
> {
>     Guard.AgainstInvalidState(Tipo == TipoUsuario.Gerente, "Apenas Gerentes possuem taxa.");
>     if (taxaPlataforma.HasValue)
>     {
>         Guard.AgainstNegative(taxaPlataforma.Value, nameof(taxaPlataforma));
>         TaxaPlataforma = taxaPlataforma.Value;
>     }
>     if (gratuito.HasValue)
>         Gratuito = gratuito.Value;
>     DataAtualizacao = DateTime.UtcNow;
> }
> ```

**Erros da US13:**

| Erro | Error Code | HTTP Status |
|------|-----------|-------------|
| Gerente não encontrado | `GERENTE_NAO_ENCONTRADO` | 404 |
| Request inválido | `*` (do validator) | 400 |
| Slug duplicado | `SLUG_DUPLICADO` | 409 |
| Email duplicado | `EMAIL_DUPLICADO` | 409 |
| CPF duplicado | `CPF_DUPLICADO` | 409 |

---

#### 6. `AdminProfile.cs`

```csharp
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
            .ForMember(dest => dest.TotalVans, opt => opt.Ignore())   // Preenchido manualmente
            .ForMember(dest => dest.TotalViagens, opt => opt.Ignore()); // Preenchido manualmente

        CreateMap<Usuario, UsuarioAdminResponse>()  // US22
            .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.CPF.Valor))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email != null ? src.Email.Valor : null))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(dest => dest.TotalReservas, opt => opt.Ignore());

        CreateMap<Reserva, ReservaHistoricoResponse>()  // US23
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
```

---

#### 7. `AtualizarGerenteAdminValidator.cs`

```csharp
using FluentValidation;
using VanBora.Application.DTOs.Admin;

namespace VanBora.Application.Validators;

public class AtualizarGerenteAdminValidator : AbstractValidator<AtualizarGerenteAdminRequest>
{
    public AtualizarGerenteAdminValidator()
    {
        RuleFor(x => x.TaxaPlataforma)
            .InclusiveBetween(0, 100)
            .WithMessage("Taxa da plataforma deve estar entre 0 e 100.")
            .When(x => x.TaxaPlataforma.HasValue);

        // Ativo e Gratuito são bool? — não precisam de validação
    }
}
```

#### 8. `CriarGerenteAdminValidator.cs`

```csharp
using FluentValidation;
using VanBora.Application.DTOs.Admin;

namespace VanBora.Application.Validators;

public class CriarGerenteAdminValidator : AbstractValidator<CriarGerenteAdminRequest>
{
    public CriarGerenteAdminValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório.")
            .Matches(@"^\d{11}$").WithMessage("CPF deve ter 11 dígitos.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.TaxaPlataforma)
            .InclusiveBetween(0, 100)
            .WithMessage("Taxa da plataforma deve estar entre 0 e 100.")
            .When(x => x.TaxaPlataforma.HasValue);
    }
}
```

---

### Sub-tarefa 4.4.3 — Controllers + DI + Test (1.5 SP)

**Objetivo:** Criar o `GerentesController` na pasta Admin, registrar dependências e testar via Swagger.

#### Arquivos a criar

| # | Arquivo | Caminho |
|---|---------|---------|
| 1 | `GerentesController.cs` | `Api/Controllers/Admin/GerentesController.cs` |

#### Arquivos a modificar

| # | Arquivo | Caminho | Alteração |
|---|---------|---------|-----------|
| 1 | `Program.cs` | `Api/Program.cs` | Registrar `IAdminService → AdminService` e validators |
| 2 | `Usuario.cs` | `VanBora.Domain/Entities/Usuario.cs` | Adicionar método `AtualizarParametrosGerente(decimal?, bool?)` |
| 3 | `ServiceCollectionExtensions.cs` | `VanBora.Infrastructure/Extensions/ServiceCollectionExtensions.cs` | ✅ `IVanRepository`, `IViagemRepository` e `IReservaRepository` já registrados (Sprint 2/3) |

---

#### 1. `GerentesController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.DTOs.Admin;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Enums;

namespace Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/[controller]")]
public class GerentesController : ControllerBase
{
    private readonly IAdminService _adminService;

    public GerentesController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GerenteAdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarGerentes(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.ListarGerentesAsync(search, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GerenteAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterGerentePorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.ObterGerentePorIdAsync(id, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(GerenteAdminResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CriarGerente(
        [FromBody] CriarGerenteAdminRequest request,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.CriarGerenteAsync(request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Created(string.Empty, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GerenteAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AtualizarGerente(
        Guid id,
        [FromBody] AtualizarGerenteAdminRequest request,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.AtualizarGerenteAsync(id, request, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    private IActionResult? ValidarAdmin()
    {
        var tipoClaim = User.FindFirst("tipos")?.Value;
        if (tipoClaim != TipoUsuario.Admin.ToString())
            return new ObjectResult(Error.Forbidden("ACESSO_NEGADO", "Acesso restrito a administradores."));
        return null;
    }
}
```

> ⚠️ **Verificação de Admin:** O claim `"tipos"` contém o `TipoUsuario` como string. O método `ValidarAdmin()` retorna `IActionResult?`: `null` se for Admin, ou `ObjectResult` com `Error.Forbidden` caso contrário. O `ResultFilter` converte automaticamente para HTTP 403.

---

#### 2. Registro de DI em `Program.cs`

Adicionar o import no topo do arquivo:
```csharp
using VanBora.Application.DTOs.Admin;
```

E registrar os serviços:
```csharp
// Admin
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IValidator<AtualizarGerenteAdminRequest>, AtualizarGerenteAdminValidator>();
builder.Services.AddScoped<IValidator<CriarGerenteAdminRequest>, CriarGerenteAdminValidator>();
```

> **Nota:** `IVanRepository` e `IViagemRepository` já estão registrados em `ServiceCollectionExtensions.cs` (Sprint 2). O **AutoMapper** também não requer registro adicional: `AddAutoMapper(typeof(VanProfile).Assembly)` no `Program.cs:48` já descobre automaticamente o `AdminProfile` (mesmo assembly `VanBora.Application`).

---

#### 3. Teste via Swagger

1. Executar a aplicação
2. Fazer login como Admin
3. Testar `GET /api/admin/gerentes` — deve retornar `200 OK` com lista
4. Testar `GET /api/admin/gerentes?search=Transportadora` — filtro funcional
5. Testar `GET /api/admin/gerentes/{id}` — detalhes do gerente
6. Testar `PUT /api/admin/gerentes/{id}` com `{ "taxaPlataforma": 3.0 }` — taxa alterada
7. Testar `POST /api/admin/gerentes` — criar novo gerente
8. Testar acesso como não-Admin — deve retornar `403 Forbidden`

---

## US22 — Admin: Buscar Usuarios (3 SP)

### Objetivo Geral

Implementar endpoint para que o Admin possa pesquisar qualquer usuário do sistema por nome, CPF ou tipo.

### Regras de Negócio (Checklist)

- [✅] Apenas Admin pode acessar
- [✅] `GET /api/admin/usuarios` — Lista todos os usuarios
- [✅] `GET /api/admin/usuarios?search=termo` — Busca por nome (ILike) ou CPF (exato, somente dígitos)
- [✅] A resposta inclui `Tipo` (string), dados pessoais, status e total de reservas
- [✅] Ordenação: mais recentes primeiro (`CriadoEm DESC`)

### Endpoints da US22

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/admin/usuarios` | Listar todos os usuarios (com opção `?search=`) |

---

### Sub-tarefa 4.5.1 — DTOs + Service (1.5 SP)

**Objetivo:** Criar o DTO de resposta e adicionar o método `BuscarUsuariosAsync` no `AdminService`.

#### Arquivos a criar

| # | Arquivo | Caminho |
|---|---------|---------|
| 1 | `UsuarioAdminResponse.cs` | `VanBora.Application/DTOs/Admin/UsuarioAdminResponse.cs` |

#### Arquivos a modificar

| # | Arquivo | Caminho | Alteração |
|---|---------|---------|-----------|
| 1 | `IUsuarioRepository.cs` | `VanBora.Domain/Interfaces/IUsuarioRepository.cs` | Adicionar `SearchAllAsync` |
| 2 | `UsuarioRepository.cs` | `VanBora.Infrastructure/Repositories/UsuarioRepository.cs` | Implementar `SearchAllAsync` |
| 3 | `AdminService.cs` | `VanBora.Application/Services/AdminService.cs` | Adicionar `BuscarUsuariosAsync` |

---

#### 1. `UsuarioAdminResponse.cs`

```csharp
namespace VanBora.Application.DTOs.Admin;

public class UsuarioAdminResponse
{
    public Guid UsuarioId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public int TotalReservas { get; init; }
    public DateTime CriadoEm { get; init; }
}
```

---

#### 2. Novo método no repositório

**Interface `IUsuarioRepository.cs`:**

```csharp
Task<List<Usuario>> SearchAllAsync(string? search, CancellationToken cancellationToken = default);
```

> O método existente `SearchAsync` busca apenas por nome. O novo `SearchAllAsync` também busca por CPF (correspondência exata nos dígitos).

**Implementação `UsuarioRepository.cs`:**

```csharp
public async Task<List<Usuario>> SearchAllAsync(
    string? search,
    CancellationToken cancellationToken = default)
{
    var query = _context.Usuarios.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        // Se o termo for composto apenas por dígitos, busca por CPF exato
        if (search.All(char.IsDigit) && search.Length == 11)
        {
            query = query.Where(u =>
                u.CPF.Valor == search ||
                EF.Functions.ILike(u.Nome, $"%{search}%"));
        }
        else
        {
            query = query.Where(u =>
                EF.Functions.ILike(u.Nome, $"%{search}%"));
        }
    }

    return await query
        .OrderByDescending(u => u.CriadoEm)
        .ToListAsync(cancellationToken);
}
```

---

#### 3. Método `BuscarUsuariosAsync` no `AdminService`

```csharp
public async Task<Result<List<UsuarioAdminResponse>>> BuscarUsuariosAsync(
    string? search,
    CancellationToken cancellationToken)
{
    var usuarios = await _usuarioRepo.SearchAllAsync(search, cancellationToken);

    var response = new List<UsuarioAdminResponse>();
    foreach (var usuario in usuarios)
    {
        var totalReservas = await _reservaRepo.GetCountByUsuarioIdAsync(usuario.Id, cancellationToken);
        var dto = _mapper.Map<UsuarioAdminResponse>(usuario);
        dto.TotalReservas = totalReservas;
        response.Add(dto);
    }

    return Result<List<UsuarioAdminResponse>>.Success(response);
}
```

> ⚠️ **Novo método no `IReservaRepository`:** Será necessário adicionar `GetCountByUsuarioIdAsync` para obter o total de reservas de um usuário sem carregar todas as entidades.

```csharp
// IReservaRepository.cs — adicionar:
Task<int> GetCountByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);

// ReservaRepository.cs — implementar:
public async Task<int> GetCountByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
{
    return await _context.Reservas
        .CountAsync(r => r.UsuarioId == usuarioId, cancellationToken);
}
```

---

### Sub-tarefa 4.5.2 — Controller + DI + Test (1.5 SP)

**Objetivo:** Criar o `UsuariosController` na pasta Admin e testar via Swagger.

#### Arquivos a criar

| # | Arquivo | Caminho |
|---|---------|---------|
| 1 | `UsuariosController.cs` | `Api/Controllers/Admin/UsuariosController.cs` |

---

#### 1. `UsuariosController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanBora.Application.DTOs.Admin;
using VanBora.Application.Interfaces;
using VanBora.Domain.Common;
using VanBora.Domain.Enums;

namespace Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IAdminService _adminService;

    public UsuariosController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioAdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BuscarUsuarios(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.BuscarUsuariosAsync(search, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    // US23 — endpoint adicionado na Sub-tarefa 4.6.2
    [HttpGet("{id:guid}/reservas")]
    [ProducesResponseType(typeof(List<ReservaHistoricoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterHistoricoReservas(
        Guid id,
        CancellationToken cancellationToken)
    {
        var authError = ValidarAdmin();
        if (authError is not null) return authError;
        var result = await _adminService.ObterHistoricoReservasUsuarioAsync(id, cancellationToken);

        if (result.IsFailure)
            return new ObjectResult(result);

        return Ok(result.Value);
    }

    private IActionResult? ValidarAdmin()
    {
        var tipoClaim = User.FindFirst("tipos")?.Value;
        if (tipoClaim != TipoUsuario.Admin.ToString())
            return new ObjectResult(Error.Forbidden("ACESSO_NEGADO", "Acesso restrito a administradores."));
        return null;
    }
}
```

---

#### 2. Teste via Swagger

1. Fazer login como Admin
2. Testar `GET /api/admin/usuarios` — lista completa
3. Testar `GET /api/admin/usuarios?search=João` — filtro por nome
4. Testar `GET /api/admin/usuarios?search=12345678909` — filtro por CPF
5. Testar acesso como não-Admin — deve retornar `403 Forbidden`

---

## US23 — Admin: Histórico Reservas (3 SP)

### Objetivo Geral

Permitir que o Admin visualize o histórico de reservas de qualquer usuário e o histórico de viagens/reservas de qualquer gerente.

### Regras de Negócio (Checklist)

- [✅] Apenas Admin pode acessar
- [✅] `GET /api/admin/usuarios/{id}/reservas` — Histórico de reservas do usuário
- [✅] `GET /api/admin/gerentes/{id}/reservas` — Histórico de viagens do gerente com total arrecadado e taxa
- [✅] Reservas ordenadas da mais recente para a mais antiga

### Endpoints da US23

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/admin/usuarios/{id}/reservas` | Histórico de reservas de um usuário |
| `GET` | `/api/admin/gerentes/{id}/reservas` | Histórico de viagens de um gerente |

---

### Sub-tarefa 4.6.1 — Repositório + Service (1.5 SP)

**Objetivo:** Adicionar métodos de query para histórico e implementar a lógica no `AdminService`.

#### Arquivos a criar

| # | Arquivo | Caminho |
|---|---------|---------|
| 1 | `ReservaHistoricoResponse.cs` | `VanBora.Application/DTOs/Admin/ReservaHistoricoResponse.cs` |

#### Arquivos a modificar

| # | Arquivo | Caminho | Alteração |
|---|---------|---------|-----------|
| 1 | `IReservaRepository.cs` | `VanBora.Domain/Interfaces/IReservaRepository.cs` | `GetByUsuarioIdAsync` já existe com Includes completos — não requer alteração |
| 2 | `ReservaRepository.cs` | `VanBora.Infrastructure/Repositories/ReservaRepository.cs` | Nenhuma alteração necessária — o método existente já inclui Itens, ViagemVan, ViagemVan.Viagem |
| 3 | `AdminService.cs` | `VanBora.Application/Services/AdminService.cs` | Adicionar 2 métodos de histórico |

---

#### 1. `ReservaHistoricoResponse.cs`

```csharp
namespace VanBora.Application.DTOs.Admin;

public class ReservaHistoricoResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal ValorTotal { get; init; }
    public decimal TaxaPlataforma { get; init; }
    public DateTime CriadaEm { get; init; }
    public ViagemResumoResponse Viagem { get; init; } = null!;
    public List<ItemReservaHistoricoResponse> Itens { get; init; } = [];
}

public class ViagemResumoResponse
{
    public Guid Id { get; init; }
    public string NomeEvento { get; init; } = string.Empty;
    public string Origem { get; init; } = string.Empty;
    public string Destino { get; init; } = string.Empty;
    public DateTime DataPartida { get; init; }
}

public class ItemReservaHistoricoResponse
{
    public int Assento { get; init; }
    public string PassageiroNome { get; init; } = string.Empty;
    public string PassageiroDocumento { get; init; } = string.Empty;
    public decimal Valor { get; init; }
}
```

---

#### 2. `ViagemGerenteHistoricoResponse.cs`

```csharp
namespace VanBora.Application.DTOs.Admin;

public class ViagemGerenteHistoricoResponse
{
    public Guid ViagemId { get; init; }
    public string NomeEvento { get; init; } = string.Empty;
    public string Origem { get; init; } = string.Empty;
    public string Destino { get; init; } = string.Empty;
    public DateTime DataPartida { get; init; }
    public DateTime DataEvento { get; init; }
    public int TotalReservas { get; init; }
    public decimal TotalArrecadado { get; init; }
    public decimal TaxaPlataforma { get; init; }
    public string StatusViagem { get; init; } = string.Empty;
}
```

---

#### 3. Método de repositório já existente

> ✅ **O `GetByUsuarioIdAsync` do `ReservaRepository` já retorna as reservas com todos os Includes necessários** (Itens, ViagemVan → Van, ViagemVan → Viagem). Ele foi implementado na Sprint 3 (Sub-tarefa 3.2.1) e **não requer alteração**. O `AdminService` simplesmente chama o método existente.

**Implementação existente (`ReservaRepository.cs:31-44`):**

```csharp
public async Task<List<Reserva>> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
{
    return await _context.Reservas
        .AsNoTracking()
        .AsSplitQuery()
        .Where(r => r.UsuarioId == usuarioId)
        .Include(r => r.Itens)
        .Include(r => r.ViagemVan)
            .ThenInclude(vv => vv.Van)
        .Include(r => r.ViagemVan)
            .ThenInclude(vv => vv.Viagem)
        .OrderByDescending(r => r.CriadoEm)
        .ToListAsync(cancellationToken);
}
```

---

#### 4. Métodos no `AdminService`

**`ObterHistoricoReservasUsuarioAsync`:**

```csharp
public async Task<Result<List<ReservaHistoricoResponse>>> ObterHistoricoReservasUsuarioAsync(
    Guid usuarioId,
    CancellationToken cancellationToken)
{
    var usuario = await _usuarioRepo.GetByIdAsync(usuarioId, cancellationToken);
    if (usuario is null)
        return Result<List<ReservaHistoricoResponse>>.Failure(
            Error.NotFound("USUARIO_NAO_ENCONTRADO", "Usuário não encontrado."));

    var reservas = await _reservaRepo.GetByUsuarioIdAsync(usuarioId, cancellationToken);
    var response = _mapper.Map<List<ReservaHistoricoResponse>>(reservas);
    return Result<List<ReservaHistoricoResponse>>.Success(response);
}
```

**`ObterHistoricoReservasGerenteAsync`:**

```csharp
public async Task<Result<List<ViagemGerenteHistoricoResponse>>> ObterHistoricoReservasGerenteAsync(
    Guid gerenteId,
    CancellationToken cancellationToken)
{
    var gerente = await _usuarioRepo.GetByIdAsync(gerenteId, cancellationToken);
    if (gerente is null || gerente.Tipo != TipoUsuario.Gerente)
        return Result<List<ViagemGerenteHistoricoResponse>>.Failure(
            Error.NotFound("GERENTE_NAO_ENCONTRADO", "Gerente não encontrado."));

    // Buscar todas as viagens do gerente
    var viagens = await _viagemRepo.GetByGerenteUsuarioIdAsync(gerenteId, cancellationToken);

    var response = new List<ViagemGerenteHistoricoResponse>();
    foreach (var viagem in viagens)
    {
        var reservas = await _reservaRepo.GetByViagemIdAsync(viagem.Id, cancellationToken);
        var reservasConfirmadas = reservas.Where(r => r.Status == StatusReserva.Confirmada).ToList();

        response.Add(new ViagemGerenteHistoricoResponse
        {
            ViagemId = viagem.Id,
            NomeEvento = viagem.NomeEvento,
            Origem = viagem.LocalPartida,
            Destino = viagem.LocalEvento,
            DataPartida = viagem.DataPartida,
            DataEvento = viagem.DataEvento,
            TotalReservas = reservas.Count,
            TotalArrecadado = reservasConfirmadas.Sum(r => r.ValorTotal),
            TaxaPlataforma = reservasConfirmadas.Sum(r => r.TaxaPlataforma),
            StatusViagem = viagem.Status.ToString()
        });
    }

    return Result<List<ViagemGerenteHistoricoResponse>>.Success(response);
}
```

> ⚠️ **Dependência:** O método `GetByGerenteUsuarioIdAsync` já existe em `IViagemRepository` (criado na Sprint 2). Certifique-se de que ele esteja registrado no DI.

---

### Sub-tarefa 4.6.2 — Controllers + DI + Test (1.5 SP)

**Objetivo:** Adicionar os endpoints de histórico nos controllers Admin.

#### Arquivos a modificar

| # | Arquivo | Caminho | Alteração |
|---|---------|---------|-----------|
| 1 | `UsuariosController.cs` | `Api/Controllers/Admin/UsuariosController.cs` | Adicionar endpoint `GET /{id}/reservas` |
| 2 | `GerentesController.cs` | `Api/Controllers/Admin/GerentesController.cs` | Adicionar endpoint `GET /{id}/reservas` |

---

#### 1. Endpoint no `UsuariosController.cs`

Já incluído no código da Sub-tarefa 4.5.2 (método `ObterHistoricoReservas`).

#### 2. Endpoint adicional no `GerentesController.cs`

```csharp
[HttpGet("{id:guid}/reservas")]
[ProducesResponseType(typeof(List<ViagemGerenteHistoricoResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<IActionResult> ObterHistoricoReservasGerente(
    Guid id,
    CancellationToken cancellationToken)
{
    var authError = ValidarAdmin();
    if (authError is not null) return authError;
    var result = await _adminService.ObterHistoricoReservasGerenteAsync(id, cancellationToken);

    if (result.IsFailure)
        return new ObjectResult(result);

    return Ok(result.Value);
}
```

---

#### 3. Teste via Swagger

1. Fazer login como Admin
2. Testar `GET /api/admin/usuarios/{id}/reservas` — histórico de reservas de um passageiro
3. Testar `GET /api/admin/gerentes/{id}/reservas` — histórico de viagens de um gerente
4. Testar com ID inexistente — deve retornar `404 Not Found`

---

## Ordem de Implementação Sugerida

Para minimizar bloqueios e permitir testes incrementais:

```
========== [ SUB-TAREFA 4.4.1 — Repositório ] ==========
Step  1: Domain — Adicionar GetGerentesAsync em IUsuarioRepository
Step  2: Infrastructure — Implementar GetGerentesAsync em UsuarioRepository

========== [ SUB-TAREFA 4.4.2 — DTOs + Service + Mapping ] ==========
Step  3: Domain — Adicionar AtualizarParametrosGerente em Usuario.cs
Step  4: Application — Criar GerenteAdminResponse.cs
Step  5: Application — Criar AtualizarGerenteAdminRequest.cs
Step  6: Application — Criar CriarGerenteAdminRequest.cs
Step  7: Application — Criar AtualizarGerenteAdminValidator.cs
Step  8: Application — Criar CriarGerenteAdminValidator.cs
Step  9: Application — Criar AdminProfile.cs
Step 10: Application — Criar IAdminService.cs (métodos da US13)
Step 11: Application — Criar AdminService.cs (métodos da US13)

========== [ SUB-TAREFA 4.4.3 — Controllers + DI + Test ] ==========
Step 12: API — Criar GerentesController.cs
Step 13: *** REGISTRAR DI ***
         - Program.cs → AddScoped<IAdminService, AdminService>
         - Program.cs → AddScoped validators da US13
Step 14: Testar endpoints de gerentes via Swagger

========== [ SUB-TAREFA 4.5.1 — DTOs + Service (US22) ] ==========
Step 15: Domain — Adicionar SearchAllAsync em IUsuarioRepository
Step 16: Infrastructure — Implementar SearchAllAsync em UsuarioRepository
Step 17: Domain — Adicionar GetCountByUsuarioIdAsync em IReservaRepository
Step 18: Infrastructure — Implementar GetCountByUsuarioIdAsync em ReservaRepository
Step 19: Application — Criar UsuarioAdminResponse.cs
Step 20: Application — Adicionar BuscarUsuariosAsync no AdminService

========== [ SUB-TAREFA 4.5.2 — Controller + Test (US22) ] ==========
Step 21: API — Criar UsuariosController.cs (GET /api/admin/usuarios)
Step 22: Testar busca de usuários via Swagger

========== [ SUB-TAREFA 4.6.1 — Repositório + Service (US23) ] ==========
Step 23: ✅ Nenhum novo método necessário em IReservaRepository — GetByUsuarioIdAsync já existe com Includes completos
Step 24: ✅ ReservaRepository — nenhuma alteração
Step 25: Verificar se GetByGerenteUsuarioIdAsync existe em IViagemRepository (já existe desde a Sprint 2)
Step 26: Application — Criar ReservaHistoricoResponse.cs
Step 27: Application — Criar ViagemGerenteHistoricoResponse.cs
Step 28: Application — Adicionar métodos de histórico no AdminService

========== [ SUB-TAREFA 4.6.2 — Controllers + Test (US23) ] ==========
Step 29: API — Adicionar GET /{id}/reservas em UsuariosController
Step 30: API — Adicionar GET /{id}/reservas em GerentesController
Step 31: Testar endpoints de histórico via Swagger
```

---

## Dependências com Outros Devs

| Dependência | Dev | Descrição |
|-------------|-----|-----------|
| `UsuarioRepository`, `ReservaRepository` | **Dev 1** | Criados na Sprint 1 e Sprint 3 — Dev 2 apenas adiciona novos métodos |
| `AuthService.RegistrarGerente()` | **Dev 3** | Reutilizado no `AdminService.CriarGerenteAsync()` |
| Entidades `Usuario`, `Reserva`, `Viagem` | **Dev 1** | Criadas na Sprint 1 — Dev 2 adiciona método `AtualizarParametrosGerente` em `Usuario.cs` |
| Tabelas `reservas`, `viagens` populadas | **Dev 1 / Dev 3** | Depende de dados de reservas e viagens para testar US23 |
| `POST /api/auth/gerente/registrar` | **Dev 3** | Já implementado na Sprint 1, reutilizado pelo Admin |

---

## Notas Técnicas

### Verificação de Admin nos Controllers

O JWT contém o claim `"tipos"` com o valor do `TipoUsuario` (ex: `"Admin"`, `"Gerente"`, `"Passageiro"`). O método `ValidarAdmin()` retorna `IActionResult?` (null = sucesso, `ObjectResult` com `Error.Forbidden` = acesso negado). O `ResultFilter` converte o `Error.Forbidden` para HTTP 403 automaticamente:

```csharp
private IActionResult? ValidarAdmin()
{
    var tipoClaim = User.FindFirst("tipos")?.Value;
    if (tipoClaim != TipoUsuario.Admin.ToString())
        return new ObjectResult(Error.Forbidden("ACESSO_NEGADO", "Acesso restrito a administradores."));
    return null;
}
```

**Uso nos action methods:**

```csharp
var authError = ValidarAdmin();
if (authError is not null) return authError;
// ... continuar com a lógica do endpoint
```

> **Por que não usar `throw`:** O `ExceptionMiddleware` captura exceções não tratadas e retorna 500. `UnauthorizedAccessException` não é uma exceção de autorização do ASP.NET Core — lançá-la resultaria em 500, não 403. O padrão usado é `Result<object>.Failure(Error.Forbidden(...))` que implementa `IAppResult` e é interceptado pelo `ResultFilter`, convertendo para HTTP 403.

### Performance das Queries de Contagem

A US13 (`TotalVans`, `TotalViagens`) faz N+1 queries por enquanto (uma por gerente na lista). Em produção com muitos gerentes, considerar:

```csharp
// Alternativa com GroupBy (uma única query):
var contagemVans = await _context.Vans
    .Where(v => gerenteIds.Contains(v.GerenteUsuarioId))
    .GroupBy(v => v.GerenteUsuarioId)
    .Select(g => new { GerenteId = g.Key, Count = g.Count() })
    .ToDictionaryAsync(x => x.GerenteId, x => x.Count);
```

> Para a Sprint 4, a abordagem simples (uma query por gerente) é aceitável. Migrar para a abordagem eficiente se o número de gerentes crescer.

### Navegação do AutoMapper com Value Objects

O `AdminProfile` precisa lidar com Value Objects nos mapeamentos:
- `CPF.Valor` → `string Cpf`
- `Email?.Valor` → `string? Email` (nullable)
- `Telefone.DDD + Telefone.Numero` → `string? Telefone` (formatação manual ou via `IMemberValueResolver`)

Use `.MapFrom()` com expressões lambda para Value Objects. Se preferir, crie um `ValueResolver` customizado para telefone.

### Método `AtualizarParametrosGerente` no Domínio

Deve ser adicionado em `Usuario.cs`:

```csharp
public void AtualizarParametrosGerente(decimal? taxaPlataforma, bool? gratuito)
{
    Guard.AgainstInvalidState(Tipo == TipoUsuario.Gerente, "Apenas Gerentes possuem taxa.");
    if (taxaPlataforma.HasValue)
    {
        Guard.AgainstNegative(taxaPlataforma.Value, nameof(taxaPlataforma));
        TaxaPlataforma = taxaPlataforma.Value;
    }
    if (gratuito.HasValue)
        Gratuito = gratuito.Value;
    DataAtualizacao = DateTime.UtcNow;
}
```

### Segurança: não expor `SenhaHash` ou `CodigoExclusao`

Os DTOs de Admin (`GerenteAdminResponse`, `UsuarioAdminResponse`) NÃO incluem `SenhaHash` ou `CodigoExclusao`. O Admin gerencia metadados (taxa, ativo, gratuito), não acessa senhas. O AutoMapper naturalmente ignora campos não mapeados.

### Índices de Banco

Os índices existentes já cobrem as queries do Admin:
- `ix_usuarios_cpf` — busca por CPF
- `ix_usuarios_email` — busca por email (filtro IS NOT NULL)
- `ix_usuarios_slug` — busca por slug (filtro IS NOT NULL)

Para o `ILike` em nome (busca textual), considere um índice GIN com `pg_trgm` no PostgreSQL se a performance for degradada com muitos registros:
```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE INDEX ix_usuarios_nome_trgm ON usuarios USING GIN (nome gin_trgm_ops);
```

> Isso é opcional e pode ser adiado para a Sprint 5 ou produção.

---

## Resumo de Arquivos

### Arquivos a criar (13 novos)

| # | Arquivo | Caminho |
|---|---------|---------|
| 1 | `GerenteAdminResponse.cs` | `VanBora.Application/DTOs/Admin/GerenteAdminResponse.cs` |
| 2 | `AtualizarGerenteAdminRequest.cs` | `VanBora.Application/DTOs/Admin/AtualizarGerenteAdminRequest.cs` |
| 3 | `CriarGerenteAdminRequest.cs` | `VanBora.Application/DTOs/Admin/CriarGerenteAdminRequest.cs` |
| 4 | `UsuarioAdminResponse.cs` | `VanBora.Application/DTOs/Admin/UsuarioAdminResponse.cs` |
| 5 | `ReservaHistoricoResponse.cs` | `VanBora.Application/DTOs/Admin/ReservaHistoricoResponse.cs` |
| 6 | `ViagemGerenteHistoricoResponse.cs` | `VanBora.Application/DTOs/Admin/ViagemGerenteHistoricoResponse.cs` |
| 7 | `IAdminService.cs` | `VanBora.Application/Interfaces/IAdminService.cs` |
| 8 | `AdminService.cs` | `VanBora.Application/Services/AdminService.cs` |
| 9 | `AdminProfile.cs` | `VanBora.Application/Mappings/AdminProfile.cs` |
| 10 | `AtualizarGerenteAdminValidator.cs` | `VanBora.Application/Validators/AtualizarGerenteAdminValidator.cs` |
| 11 | `CriarGerenteAdminValidator.cs` | `VanBora.Application/Validators/CriarGerenteAdminValidator.cs` |
| 12 | `GerentesController.cs` | `Api/Controllers/Admin/GerentesController.cs` |
| 13 | `UsuariosController.cs` | `Api/Controllers/Admin/UsuariosController.cs` |

> **Migration:** Nenhuma migration nova necessária — todas as tabelas (`usuarios`, `reservas`, `itens_reserva`, `viagens`, `vans`) já existem no banco.

### Arquivos a modificar (7 existentes)

| # | Arquivo | Caminho | Alteração |
|---|---------|---------|-----------|
| 1 | `IUsuarioRepository.cs` | `VanBora.Domain/Interfaces/` | + `GetGerentesAsync`, `SearchAllAsync` |
| 2 | `UsuarioRepository.cs` | `VanBora.Infrastructure/Repositories/` | Implementar novos métodos |
| 3 | `IReservaRepository.cs` | `VanBora.Domain/Interfaces/` | + `GetCountByUsuarioIdAsync` (apenas) |
| 4 | `ReservaRepository.cs` | `VanBora.Infrastructure/Repositories/` | Implementar `GetCountByUsuarioIdAsync` (apenas) |
| 5 | `Usuario.cs` | `VanBora.Domain/Entities/` | + `AtualizarParametrosGerente(decimal?, bool?)` |
| 6 | `IViagemRepository.cs` | `VanBora.Domain/Interfaces/` | Já possui `GetByGerenteUsuarioIdAsync` (Sprint 2) — não requer alteração |
| 7 | `Program.cs` | `Api/` | Registrar `IAdminService` + validators |
