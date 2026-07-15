---
name: Correções na Criação de Viagens
status: auditada
references:
  - spec-20 (Gerenciamento de Viagens)
  - backend: VanBora.Application/DTOs/Viagens/CriarViagemRequest.cs
  - backend: VanBora.Application/DTOs/Viagens/AtualizarViagemRequest.cs
  - backend: VanBora.Application/DTOs/Viagens/ViagemResponse.cs
  - backend: VanBora.Application/Validators/CriarViagemValidator.cs
  - backend: VanBora.Application/Validators/AtualizarViagemValidator.cs
---

# Spec 21 — Correções na Criação de Viagens

## O que é?

Correção da Spec 20 para alinhar o frontend com os DTOs reais do backend. A Spec 20 foi implementada com uma estrutura de origem/destino (6 campos: descrição, cidade, UF para cada ponta) que **não existe** no backend. O backend usa um modelo diferente: um campo `LocalPartida` (string única) e um campo `QuorumMinimo` (int) que estavam ausentes no frontend.

## Problema que resolve para o usuário final

O formulário de criação de viagem atual envia campos que o backend não reconhece (`origemDescricao`, `origemCidade`, `origemEstado`, `destinoDescricao`, `destinoCidade`, `destinoEstado`, `dataChegada`) e **não envia** campos obrigatórios (`localPartida`, `quorumMinimo`). Isso causa erro 400 ao tentar criar uma viagem. Além disso, o formulário de edição envia `precoAssento` e `quorumMinimo` no PUT, mas o backend não aceita esses campos na atualização.

Quanto ao nome do evento: atualmente o gerente digita o nome livremente (campo de texto) — isso está correto e deve permanecer assim. O backend espera exatamente um `NomeEvento` string.

---

## Análise de Gaps

### Backend `CriarViagemRequest` (fonte da verdade)

```csharp
public record CriarViagemRequest(
    string NomeEvento,
    DateTime DataEvento,
    string LocalEvento,
    DateTime DataPartida,
    string LocalPartida,
    decimal PrecoAssento,
    bool PossuiIngresso,
    int QuorumMinimo);
```

### Backend `AtualizarViagemRequest` (fonte da verdade)

```csharp
public record AtualizarViagemRequest(
    string NomeEvento,
    DateTime DataEvento,
    string LocalEvento,
    DateTime DataPartida,
    string LocalPartida,
    bool PossuiIngresso);
```

> ⚠️ `AtualizarViagemRequest` **não** tem `PrecoAssento` nem `QuorumMinimo`.

### Tabela de gaps

| Campo no frontend atual | Existe no backend? | Ação |
|---|---|---|
| `nomeEvento` | ✅ `NomeEvento` | Manter (já é texto livre — correto) |
| `dataEvento` | ✅ `DataEvento` | Manter |
| `localEvento` | ✅ `LocalEvento` | Manter |
| `origemDescricao` | ❌ | Remover |
| `origemCidade` | ❌ | Remover |
| `origemEstado` | ❌ | Remover |
| `destinoDescricao` | ❌ | Remover |
| `destinoCidade` | ❌ | Remover |
| `destinoEstado` | ❌ | Remover |
| `dataSaida` | ⚠️ `DataPartida` | Renomear para `dataPartida` |
| `dataChegada` | ❌ | Remover |
| `precoAssento` | ✅ `PrecoAssento` | Manter |
| `possuiIngresso` | ✅ `PossuiIngresso` | Manter |
| *(ausente)* | ❗ `QuorumMinimo` | Adicionar `quorumMinimo` |
| *(ausente)* | ❗ `LocalPartida` | Adicionar `localPartida` |

---

## Functional Requirements

### FR-001 — Tipo `CriarViagemRequest` alinhado ao backend

- [ ] 8 campos exatos: `nomeEvento`, `dataEvento`, `localEvento`, `dataPartida`, `localPartida`, `precoAssento`, `possuiIngresso`, `quorumMinimo`
- [ ] Campos removidos: `origemDescricao`, `origemCidade`, `origemEstado`, `destinoDescricao`, `destinoCidade`, `destinoEstado`, `dataChegada`
- [ ] `dataSaida` renomeado para `dataPartida`
- [ ] `nomeEvento` **permanece como text input** (já é texto livre no código atual; não há dropdown/seletor de jogos — confirmado pela ausência de referências a "jogo" no código do gerente)

### FR-002 — Tipo `AtualizarViagemRequest` independente

- [ ] 6 campos: `nomeEvento`, `dataEvento`, `localEvento`, `dataPartida`, `localPartida`, `possuiIngresso`
- [ ] Não é alias de `CriarViagemRequest`
- [ ] Sem `precoAssento` e sem `quorumMinimo`

### FR-003 — Tipo `ViagemGerenteResponse` alinhado ao backend

- [ ] Adicionar: `localEvento`, `localPartida`, `quorumMinimo`
- [ ] Remover: `origem`, `destino` (não existem no `ViagemResponse` do backend)

### FR-004 — ViagemForm: campo `localPartida` (texto livre)

- [ ] Um único campo `localPartida` substitui os 6 campos de origem/destino
- [ ] Validação: obrigatório, máx 300 caracteres (igual backend)
- [ ] Placeholder: "Ex: Rodoviária Central, Rio de Janeiro - RJ"

### FR-005 — ViagemForm: campo `quorumMinimo` (número)

- [ ] Campo numérico `quorumMinimo` no formulário de criação
- [ ] **Não exibir** no formulário de edição (backend não aceita no PUT)
- [ ] Validação: obrigatório, inteiro > 0, step=1, min=1

### FR-006 — ViagemForm: remover `dataChegada`

- [ ] Campo `dataChegada` removido do formulário (não existe no backend)

### FR-007 — ViagemForm: renomear `dataSaida` → `dataPartida`

- [ ] Estado e campo renomeados de `dataSaida` para `dataPartida`
- [ ] Validação mantida: obrigatório, data futura

### FR-008 — ViagemForm: validação `dataPartida < dataEvento`

- [ ] Nova validação: `dataPartida` deve ser anterior a `dataEvento` (igual backend `CriarViagemValidator` e `AtualizarViagemValidator`)

### FR-009 — ViagemForm: novas validações de max-length

- [ ] `nomeEvento`: validar máximo de 200 caracteres (antes não tinha limite; backend: `MaximumLength(200)`)
- [ ] `localEvento`: validar máximo de 300 caracteres (antes não tinha limite; backend: `MaximumLength(300)`)

### FR-010 — `editar/page.tsx`: enviar apenas campos do `AtualizarViagemRequest`

- [ ] No submit de edição, extrair apenas os 6 campos do `AtualizarViagemRequest`
- [ ] Não enviar `precoAssento` nem `quorumMinimo`

---

## Non-Functional Requirements

### NFR-001 — Consistência com backend
- [ ] Todos os campos enviados pelo frontend correspondem 1:1 aos campos esperados pelo backend
- [ ] Nenhum campo extra é enviado

### NFR-002 — Validação client-side espelha servidor
- [ ] Regras de validação frontend equivalentes às do FluentValidation no backend
- [ ] Mensagens de erro equivalentes

---

## Edge Cases

| ID | Cenário | Comportamento esperado |
|----|---------|----------------------|
| EC-01 | `quorumMinimo = 0` | Erro: "Quórum mínimo deve ser maior que zero." |
| EC-02 | `quorumMinimo` negativo | Erro: "Quórum mínimo deve ser maior que zero." |
| EC-03 | `quorumMinimo` não inteiro | `type="number" step="1"` — navegador impede. Se bypass, backend rejeita. |
| EC-04 | `localPartida` vazio | Erro: "Informe o local de partida." |
| EC-05 | `localPartida` > 300 caracteres | Erro: "Local de partida deve ter no máximo 300 caracteres." |
| EC-06 | `dataPartida >= dataEvento` | Erro: "Data de partida deve ser anterior à data do evento." |
| EC-07 | Editar viagem com status ≠ "Agendada" | Botão "Editar" desabilitado (já tratado pela Spec 20). Sem mudança. |
| EC-08 | Campos `precoAssento`/`quorumMinimo` no modo edição | Não são exibidos no formulário de edição |
| EC-09 | Backend retorna `ViagemResponse` legado sem `localPartida` | Campo tratado como opcional com fallback `""` |
| EC-10 | Backend retorna `ViagemResponse` legado sem `localEvento` | Campo tratado como opcional com fallback `""` |
| EC-11 | Formulário de edição com campos ocultos (`precoAssento`, `quorumMinimo`) | Estados são inicializados de `viagem` mas inputs não são renderizados. `validate()` checa os valores inicializados (passam, pois vieram de dados válidos). `editar/page.tsx` extrai apenas os 6 campos permitidos antes do PUT. |

---

## Endpoints afetados

| Método | Rota | Mudança |
|--------|------|---------|
| POST | `/api/gerente/viagens` | Body corrigido (8 campos) |
| PUT | `/api/gerente/viagens/{id}` | Body corrigido (6 campos, sem preço/quórum) |
| GET | `/api/gerente/viagens` | Tipo de resposta corrigido (`localPartida`, `localEvento`, `quorumMinimo`) |

---

## Dependências

- **Spec 20 — Gerenciamento de Viagens** (auditada) — esta spec corrige a implementação
- **Backend DTOs** (`CriarViagemRequest.cs`, `AtualizarViagemRequest.cs`, `ViagemResponse.cs`) — já existentes, são a fonte da verdade

## Fora do escopo

- Alterar o backend (os DTOs são a fonte da verdade, não serão modificados)
- Modificar Spec 10, 30, 40, 50, 60
- Adicionar novas features além das correções listadas
