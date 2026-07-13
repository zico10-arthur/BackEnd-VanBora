---
name: Gerenciamento de Motoristas
status: pendente
---

# Design — Spec 40: Gerenciamento de Motoristas

## Arquitetura

```
app/gerente/motoristas/
├── page.tsx                    # Lista de motoristas (server wrapper)
├── MotoristasListClient.tsx    # Client component — tabela + ações
├── novo/
│   └── page.tsx                # Formulário de cadastro
├── [motoristaId]/
│   └── editar/
│       └── page.tsx            # Formulário de edição
└── components/
    ├── MotoristaForm.tsx       # Formulário compartilhado (criar + editar)
    ├── MotoristaRow.tsx        # Linha da tabela
    └── RemoverMotoristaModal.tsx
```

## Stack

- Next.js 15 (App Router)
- Tailwind CSS (dark theme)
- Componentes: `Header`, `VbButton`
- API: `lib/api/http.ts`

## Fluxo de dados — Lista

```
MotoristasListClient
  ├── useEffect → GET api/auth/motoristas
  ├── state: motoristas[], loading, error
  ├── Renderiza tabela com <MotoristaRow />
  │     ├── Colunas: Nome, CPF (mascarado), Telefone, CNH, Status
  │     ├── Botão "Editar" → router.push(/gerente/motoristas/{id}/editar)
  │     └── Botão "Remover" → abre <RemoverMotoristaModal>
  └── Botão "Novo motorista" → router.push(/gerente/motoristas/novo)
```

## Fluxo de dados — Criar/Editar

```
MotoristaForm
  ├── Props: motorista? (opcional)
  ├── Campos: nome, cpf, telefone, cnh, categoriaCnh, dataNascimento, validadeCnh
  ├── Validação client-side:
  │     nome: min 3 caracteres, regex /^[A-Za-zÀ-ÿ\s'-]+$/
  │     cpf: 11 dígitos (só numérico)
  │     telefone: 10-11 dígitos
  │     cnh: 11 dígitos
  │     dataNascimento: idade >= 18 anos
  │     validadeCnh: data futura
  ├── Submit:
  │     Criar: POST api/auth/motorista/registrar → toast + redirect
  │     Editar: PUT api/auth/motorista/{id} → toast + redirect
  └── Erros específicos:
        409 CPF duplicado → "Este CPF já está cadastrado"
        400 CNH vencida → "CNH não pode estar vencida"
        400 menor de idade → "Motorista deve ter 18+ anos"
```

## Tipos TypeScript

```ts
// lib/api/types.ts — adicionar:
type MotoristaResponse = {
  motoristaId: string;
  nome: string;
  cpf: string;
  telefone: string;
  cnh: string;
  categoriaCnh: string;
  validadeCnh: string;
  status: string;
};

type CriarMotoristaRequest = {
  nome: string;
  cpf: string;
  telefone: string;
  cnh: string;
  categoriaCnh: string;
  dataNascimento: string;
  validadeCnh: string;
};
```

## Máscaras

| Campo | Exibição | Formato |
|-------|----------|---------|
| CPF | `***.456.789-**` | Regex replace: `/(\d{3})(\d{3})(\d{3})(\d{2})/` → `***.$2.$3-**` |
| Telefone | `(21) 9****-1102` | Regex replace |
| CNH | Completo (editável) | Sem máscara na listagem |

## Decisões

1. **Tabela em vez de cards**: Motoristas têm mais campos que vans — tabela facilita escaneamento visual.
2. **CPF mascarado na listagem**: Privacidade — o gerente já sabe o CPF pois foi ele quem cadastrou; a máscara impede vazamento acidental em screenshots.
3. **Categoria CNH como select**: Valores fixos: A, B, AB, C, D, E — sem campo livre para evitar erros.

## Rotas

| Rota | Page | Auth |
|------|------|------|
| `/gerente/motoristas` | Lista | JWT + Gerente |
| `/gerente/motoristas/novo` | Cadastrar | JWT + Gerente |
| `/gerente/motoristas/{id}/editar` | Editar | JWT + Gerente |

## Estados visuais

| Estado | UI |
|--------|-----|
| Loading lista | 5 linhas skeleton |
| Empty | "Nenhum motorista" + CTA |
| Erro CPF duplicado | Borda vermelha + mensagem inline |
| Erro CNH vencida | Borda vermelha + mensagem inline |
| Erro remoção (alocado) | Modal com mensagem |
