# Lições Aprendidas

## 1. Jamais usar `if` / `else`

### Motivo

`if/else` aninhados reduzem legibilidade, aumentam complexidade cognitiva e dificultam manutenção. Prefira:

- **Guard clauses** (`if` com early `return`) para validações e casos de borda
- **Switch expressions / pattern matching** para decisões com múltiplos branches
- **Polimorfismo** (Strategy, State pattern) para lógica complexa

### ❌ Ruim (evitar)

```csharp
if (condicao)
{
    // ...
}
else if (outraCondicao)
{
    // ...
}
else
{
    // ...
}
```

### ✅ Bom (preferir)

#### Guard clauses

```csharp
if (erroValidacao is not null) return erroValidacao;
if (slugDuplicado) return Error.Conflict(...);
if (cpfNaoExiste) return CriarNovo();
return FazerUpgrade();
```

#### Switch expression

```csharp
return passageiro.Tipo switch
{
    TipoUsuario.Passageiro => FazerUpgrade(),
    TipoUsuario.Gerente => Error.Conflict(...),
    _ => Error.Conflict(...)
};
```

## 2. Validação FluentValidation com lista de erros

### Motivo

O `Error.Validation(IReadOnlyList<Error>)` deve ser usado para repassar **todos** os erros do FluentValidation de uma vez, em vez de apenas o primeiro. Isso permite que o frontend exiba todos os erros de validação simultaneamente.

### ❌ Evitar (apenas primeiro erro)

```csharp
var primeiroErro = validation.Errors.First();
return Result<ViagemResponse>.Failure(
    Error.Validation(primeiroErro.PropertyName, primeiroErro.ErrorMessage));
```

### ✅ Preferir (lista completa)

```csharp
var erros = validation.Errors
    .Select(e => new Error(e.PropertyName, e.ErrorMessage))
    .ToList();

return Result<ViagemResponse>.Failure(Error.Validation(erros));
```

### Como o ResultFilter trata

O `ResultFilter` no middleware deve serializar o `Error.Errors` (lista) quando presente, permitindo que o frontend receba algo como:

```json
{
  "errors": [
    { "code": "NomeEvento", "message": "Nome do evento é obrigatório." },
    { "code": "PrecoAssento", "message": "Preço do assento deve ser maior que zero." }
  ]
}
```

## 3. Carregar propriedades de navegação em métodos de busca

### Motivo

Ao buscar uma entidade para operações de alteração ou remoção, é obrigatório carregar as propriedades de navegação relevantes via `.Include()` / `.ThenInclude()`. Caso contrário, o EF Core retornará `null` para essas coleções, impossibilitando operações como remover um item de uma lista ou acessar dados relacionados.

### ❌ Evitar (buscar sem Includes)

```csharp
// Cenário: Remover Van alocada
// Se GetByIdAsync não incluir ViagemVans, viagem.ViagemVans será null
var viagem = await _viagemRepo.GetByIdAsync(viagemId);
var viagemVan = viagem.ViagemVans.FirstOrDefault(vv => vv.Id == viagemVanId); // ❌ NullReferenceException
```

### ✅ Preferir (sempre carregar navegações)

```csharp
// No repositório:
public async Task<Viagem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
{
    return await _context.Viagens
        .Include(v => v.ViagemVans)        // Carrega vans alocadas
        .ThenInclude(vv => vv.Van)          // Carrega dados da van
        .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
}

// No service:
var viagem = await _viagemRepo.GetByIdAsync(viagemId);
var viagemVan = viagem.ViagemVans.FirstOrDefault(vv => vv.Id == viagemVanId); // ✅ Funciona
```

### Importante

- **O domínio não deve conhecer outras camadas** — ele não pode chamar métodos de repositórios, services, ou qualquer infraestrutura. Ele só opera em memória com os objetos que recebe como parâmetro.
- O método de domínio apenas recebe o objeto já carregado e opera em memória (ex.: `_viagemVans.Remove(viagemVan)`). Não cabe a ele fazer buscas no banco.
- A busca e o `Include` ficam no repositório (infraestrutura)
- O service coordena: busca → validação → domínio → persistência

### ❌ Ruim (domínio fazendo busca)

```csharp
// ❌ O domínio não deve percorrer coleções carregadas do banco
// Isso acopla o domínio ao formato de persistência
public ViagemVan RemoverViagemVan(Guid viagemVanId)
{
    // Busca em coleção carregada do banco — o domínio não deve fazer isso
    var viagemVan = _viagemVans.FirstOrDefault(vv => vv.Id == viagemVanId);
    Guard.AgainstInvalidState(viagemVan is null, "Van não encontrada.");
    _viagemVans.Remove(viagemVan!);
    return viagemVan;
}
```

### ✅ Bom (service faz a busca, domínio só opera)

```csharp
// No service: busca e valida existência
var viagemVan = viagem.ViagemVans.FirstOrDefault(vv => vv.Id == viagemVanId);
if (viagemVan is null)
    return Result<bool>.Failure(Error.NotFound(...));

// No domínio: apenas remove da coleção em memória
public void RemoverViagemVan(ViagemVan viagemVan)
{
    Guard.AgainstNull(viagemVan, nameof(viagemVan));
    Guard.AgainstInvalidState(Status != StatusViagem.Agendada, "Apenas viagens agendadas podem ter vans removidas.");
    _viagemVans.Remove(viagemVan);
}
```
