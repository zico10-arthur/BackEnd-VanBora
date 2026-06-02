# VanBora (monorepo)

Backend .NET + frontend Next.js — mobilidade para megaeventos.

## Estrutura

```text
BackEnd-VanBora/
├── Api/                 # API REST
├── VanBora.Application/
├── VanBora.Domain/
├── VanBora.Infrastructure/
├── frontend/            # Next.js 14 (Black Label)
└── docs/
```

## Rodar localmente

**API** (PostgreSQL na porta 5433 conforme `appsettings.json`):

```bash
dotnet ef database update --project VanBora.Infrastructure --startup-project Api
dotnet run --project Api
```

**Frontend:**

```bash
cd frontend
cp .env.example .env.local   # NEXT_PUBLIC_API_URL=http://localhost:5151
npm install
npm run dev
```

Em Development, após cadastrar um gerente, a API pode criar uma viagem demo automaticamente (`DevDataSeeder`).

## Pagamento (Mercado Pago)

Guia completo: **[docs/PAGAMENTO-SETUP.md](docs/PAGAMENTO-SETUP.md)**

Resumo: configure `MP_ACCESS_TOKEN`, URL pública do webhook e `frontend/.env.local`. Não commite credenciais.

Fluxo: reservar → pagar (Checkout Pix) → webhook confirma → **Confirmada** em Minhas reservas.
