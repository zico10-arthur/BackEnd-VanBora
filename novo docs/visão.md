# Documento de Visão — Vanbora

## 1. O que é o produto?

O **Vanbora** é um **marketplace SaaS** que conecta donos de vans a passageiros que precisam de transporte para eventos.

```
🔵 QUEM TEM VAN                    🟢 QUEM PRECISA DE TRANSPORTE
  (Vendedor)          ←──→          (Comprador)
   Oferece assentos               Reserva um assento
   para um evento                  para ir ao evento
```

O Vanbora **não** é dono das vans, **não** contrata motoristas e **não** vende ingressos de eventos. Ele é a **ponte** entre quem oferece e quem precisa de transporte.

---

## 2. O que ele faz?

### Funcionalidades do MVP (V1)

1. **Autenticação** — Registro e login com 3 perfis: Admin, Vendedor e Comprador.
2. **Motoristas** — O Vendedor cadastra e gerencia seus motoristas (nome, CPF, CNH, telefone).
3. **Vans** — O Vendedor cadastra suas vans (modelo, placa, ano, capacidade de 8 a 25 lugares) e aloca um motorista a cada uma.
4. **Eventos** — O Vendedor cria eventos (shows, jogos, festivais, competições, etc.).
5. **Rotas** — O Vendedor define rotas com origem, destino e distância em km.
6. **Viagens** — O Vendedor publica uma viagem: escolhe um evento, uma van, um motorista e uma rota, define horário de saída e chegada. Essa viagem é o "anúncio" visível para os compradores.
7. **Reserva com Pagamento** — O Comprador encontra uma viagem, reserva de 1 a 4 assentos (informando CPF de cada passageiro) e paga pelo gateway de pagamento do Vanbora.
8. **Reembolso** — O Comprador pode cancelar a reserva e pedir reembolso em até 48 horas antes do evento.
9. **Contato com o Vendedor** — Cada viagem exibe o WhatsApp do vendedor para o comprador tirar dúvidas ou comprar o ingresso do evento diretamente.
10. **Dashboard** — Cada perfil vê o que é relevante: Admin vê tudo, Vendedor vê suas viagens e reservas, Comprador vê seu histórico de reservas.

### Funcionalidades Futuras (V2+)

- Avaliação de motoristas e vans.
- Chat interno entre comprador e vendedor.
- Lista de espera para vans lotadas.
- Notificações por push e e-mail.
- Mapa com rotas em tempo real.

---

## 3. O que ele NÃO faz?

| O Vanbora **NÃO**... | Motivo |
|---|---|
| **Vende ingressos de eventos** | O ingresso é responsabilidade do vendedor. O comprador usa o WhatsApp exibido na viagem para combinar a compra do ingresso. |
| **Contrata motoristas** | O motorista é cadastrado e gerido pelo Vendedor. O Vanbora só valida os dados (CPF, CNH, idade mínima). |
| **É dono das vans** | As vans pertencem aos Vendedores. O Vanbora apenas as anuncia. |
| **Garante a viagem** | O Vanbora conecta as partes, mas a execução da viagem (pontualidade, condição da van, etc.) é responsabilidade do Vendedor. |
| **Faz reembolso após 48h do evento** | A política de reembolso cobre apenas cancelamentos com até 48 horas de antecedência do horário de início do evento. |
| **Permite mais de 4 assentos por reserva** | Cada comprador pode reservar no máximo 4 assentos por viagem. |
| **Aceita motorista sem CNH válida ou menor de 18 anos** | Validações obrigatórias no cadastro do motorista. |

---

## 4. Público-Alvo

| Perfil | Quem é | O que faz no sistema |
|---|---|---|
| **Admin** | Equipe Vanbora | Gerencia a plataforma, modera vendedores e conteúdo. |
| **Vendedor** | Dono de van | Cadastra motoristas e vans, cria eventos, publica viagens, acompanha reservas. |
| **Comprador** | Passageiro / fã de eventos | Busca viagens, reserva assentos, paga, solicita reembolso. |

---

## 5. Regras de Negócio Essenciais

1. Um **Vendedor** pode ter várias **Vans** e vários **Motoristas**.
2. Uma **Van** tem um **Motorista** alocado pelo Vendedor.
3. Um **Evento** é criado por um **Vendedor** e pode ter várias **Viagens** associadas.
4. Uma **Viagem** conecta: Evento + Van + Motorista + Rota. Define origem, destino, horário de saída e chegada.
5. O **Comprador** reserva de **1 a 4 assentos** por viagem. Cada assento exige o **CPF do passageiro**. Um mesmo CPF não pode aparecer duas vezes na mesma viagem.
6. Quando a van **lota**, novas reservas são bloqueadas automaticamente.
7. O **pagamento** da reserva é processado dentro do Vanbora, via gateway próprio.
8. O **reembolso** é permitido em até **48 horas antes do horário de início do evento**.
9. O **Admin** pode desativar qualquer Vendedor, Van, Evento ou Viagem que viole as regras da plataforma.

---

## 6. Stack Tecnológica

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core 9 (Web API) |
| Banco de Dados | PostgreSQL |
| ORM | Entity Framework Core 9 |
| Autenticação | JWT (JSON Web Tokens) |
| Documentação | Swagger / OpenAPI |
| Linguagem | C# (.NET 9) |

---

## 7. Regra de Governança

> **Toda spec criada para este projeto deve estar citada neste documento e explicar por que entrega valor para o usuário final.**

O `visão.md` é a bússola do projeto. Nenhuma spec vive isolada — cada uma existe para servir a um propósito que o usuário final (Admin, Vendedor ou Comprador) percebe e valoriza. Ao criar uma spec, atualize este documento respondendo:

1. **O que a spec faz?** (breve descrição de 1 linha)
2. **Qual valor entrega ao usuário final?** (por que isso melhora a vida do Admin, Vendedor ou Comprador?)

---

## 8. Specs do Projeto

| Spec | Descrição | Problema que resolve para o usuário final | Status |
|---|---|---|---|
| Spec 10 — Dashboard do Gerente | Portal inicial pós-login com cards de indicadores e próximas viagens | O gerente não tem nenhuma tela após o login — fica perdido sem saber quantas viagens ativas, reservas ou receita tem | 🟢 auditada |
| Spec 20 — Gerenciamento de Viagens | Criar, listar, editar e cancelar viagens (evento + origem/destino + preço) | Sem isso o gerente não publica anúncios — é o core do marketplace. Substitui o mock atual de jogos do Botafogo por tela real integrada | 🟢 auditada |
| Spec 30 — Gerenciamento de Vans | Listar, cadastrar, editar e remover vans da frota (placa, modelo, ano, capacidade) | O gerente precisa cadastrar seus veículos antes de poder alocá-los a viagens | 🟢 auditada |
| Spec 40 — Gerenciamento de Motoristas | Listar, cadastrar, editar e remover motoristas (CPF, CNH, idade, validade) | O gerente precisa registrar quem vai dirigir — com validação de CNH válida e idade mínima de 18 anos | 🟢 auditada |
| Spec 50 — Alocação de Recursos à Viagem | Alocar/remover van e motorista a uma viagem específica | Uma viagem só aparece para compradores quando tem van + motorista alocados. Esta spec fecha o ciclo de publicação | 🟢 auditada |
| Spec 60 — Relatório Financeiro da Viagem | Indicadores de receita, ocupação, break-even e lista de embarque por viagem | O gerente precisa saber se a viagem deu lucro, quantos assentos vendeu e quem são os passageiros | 🟢 auditada |
| Spec 70 — Serviço de Email | Envio real de emails via SMTP: boas-vindas no cadastro, redefinição de senha, confirmação de reserva, confirmação de reembolso e código de exclusão de conta | O usuário final recebe emails de verdade na caixa de entrada — antes tudo era descartado num log. Isso dá segurança (comprovante de reserva), confiança (boas-vindas) e autonomia (recuperar senha sem depender de suporte). O vendedor também ganha: o comprador recebe o comprovante sem que ele precise fazer nada | 🟢 auditada |
