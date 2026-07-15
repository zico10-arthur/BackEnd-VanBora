export type LoginResponse = {
  usuarioId: string;
  nome: string;
  email: string;
  perfis: string[];
  token: string;
};

export type RegistrarPassageiroResponse = {
  usuarioId: string;
  token: string;
};

export type RegistrarGerenteResponse = {
  usuarioId: string;
  nome: string;
  token: string;
  tipo: string;
};

export type ViagemVanPublica = {
  viagemVanId: string;
  viagemId: string;
  nomeVan: string;
  modeloVan: string;
  placaVan: string;
  capacidadePassageiros: number;
  assentosDisponiveis: number;
};

export type ViagemPublica = {
  viagemId: string;
  nomeEvento: string;
  dataEvento: string;
  localEvento: string;
  dataPartida: string;
  localPartida: string;
  precoAssento: number;
  possuiIngresso: boolean;
  status: string;
  vans: ViagemVanPublica[];
};

export type ViagemVanDetalhe = {
  viagemVanId: string;
  viagemId: string;
  nomeEvento: string;
  dataEvento: string;
  localEvento: string;
  dataPartida: string;
  localPartida: string;
  precoAssento: number;
  possuiIngresso: boolean;
  nomeVan: string;
  modeloVan: string;
  placaVan: string;
  capacidadePassageiros: number;
  assentosOcupados: number[];
};

export type ReservaResponse = {
  id: string;
  viagemVanId: string;
  status: string;
  valorTotal: number;
  taxaPlataforma: number;
  valorAPagar: number;
  codigoPix: string;
  expiraEm: string;
  criadoEm: string;
  pagoEm?: string | null;
  itens: { id: string; numeroAssento: number; precoAssento: number; nomePassageiro: string }[];
};

export type PagarReservaResponse = {
  id: string;
  status: string;
  initPoint: string;
  sandboxInitPoint?: string | null;
  preferenceId: string;
  valorAPagar: number;
  expiraEm: string;
};

// ── Spec 30 — Vans ────────────────────────────────────────────────

export type VanResponse = {
  id: string;
  nome: string;
  placa: string;
  modelo: string;
  capacidade: number;
  ativo: boolean;
  criadoEm: string;
};

export type CriarVanRequest = {
  nome: string;
  placa: string;
  modelo: string;
  capacidade: number;
};

export type AtualizarVanRequest = {
  nome: string;
  placa: string;
};

// ── Spec 40 — Motoristas ────────────────────────────────────────────

export type MotoristaResponse = {
  id: string;
  tipo: string;
  nome: string;
  cpf: string;
  email: string | null;
  telefone: string | null;
  ativo: boolean;
  criadoEm: string;
  dataAtualizacao: string | null;
  cnh: string;
  criadoPorUsuarioId: string;
};

export type CriarMotoristaRequest = {
  nome: string;
  cpf: string;
  telefone: string | null;
  cnh: string;
};

// ── Spec 50 — Alocação de Recursos ──────────────────────────────────

export type AlocarVanRequest = {
  vanId: string; // GUID da van
};

export type AlocarMotoristaRequest = {
  motoristaId: string; // GUID do motorista (Usuario com Tipo=Motorista)
  viagemVanId: string; // GUID do ViagemVan — obtido após alocarVan()
};

export type CriarViagemRequest = {
  nomeEvento: string;
  dataEvento: string;
  localEvento: string;
  dataPartida: string;
  localPartida: string;
  precoAssento: number;
  possuiIngresso: boolean;
  quorumMinimo: number;
};

export type AtualizarViagemRequest = {
  nomeEvento: string;
  dataEvento: string;
  localEvento: string;
  dataPartida: string;
  localPartida: string;
  possuiIngresso: boolean;
};

export type ViagemVanGerenteInfo = {
  viagemVanId: string;
  vanModelo: string;
  vanPlaca: string;
  capacidade: number;
  assentosVendidos: number;
  motoristaNome: string | null;
};

export type ViagemGerenteResponse = {
  viagemId: string;
  nomeEvento: string;
  dataEvento: string;
  localEvento: string;
  dataPartida: string;
  localPartida: string;
  precoAssento: number;
  quorumMinimo: number;
  possuiIngresso: boolean;
  status: string;
  receita: number;
  totalReservas: number;
  vans: ViagemVanGerenteInfo[];
};

// ── Spec 60 — Relatório Financeiro ────────────────────────────────

export type PassageiroRelatorio = {
  numeroAssento: number;
  nomePassageiro: string | null;
  telefonePassageiro: string | null;
  statusPagamento: string | null;
  vanPlaca?: string | null;
};

export type RelatorioResponse = {
  viagemId: string;
  nomeEvento: string;
  dataEvento: string;
  origem: string;
  destino: string;
  status: string;
  receitaTotal: number;
  taxaPlataforma: number;
  faturamentoLiquido: number;
  assentosVendidos: number;
  capacidadeTotal: number;
  quorumMinimo: number;
  precoAssento: number;
  breakEvenAtingido: boolean;
  viagemVanId: string | null;
  vansAlocadas: number;
  assentosDisponiveis: number;
  reservasConfirmadas: number;
  passageiros: PassageiroRelatorio[];
};

// ── Spec 10 — Dashboard ───────────────────────────────────────────

export type ViagemResumo = {
  viagemId: string;
  nomeEvento: string;
  dataPartida: string;
  vanModelo: string;
  vanPlaca: string;
  assentosVendidos: number;
  capacidade: number;
  status: string;
  receita: number;
  totalReservas: number;
};

export type DashboardData = {
  viagensAtivas: number;
  totalReservas: number;
  ocupacaoMedia: number;
  receitaTotal: number;
  viagensRecentes: ViagemResumo[];
};

// ── Perfil (US18/19/20/21) ────────────────────────────────────────

export type AtualizarUsuarioRequest = {
  nome: string;
  email: string;
  telefone?: string | null;
  chavePix?: string | null;
  numeroCNH?: string | null;
  categoriaCNH?: string | null;
  dataValidadeCNH?: string | null;
};

export type AtualizarUsuarioResponse = {
  usuarioId: string;
  nome: string;
  email: string;
  telefone?: string | null;
  cpf: string;
  tipo: string;
  slug?: string | null;
  chavePix?: string | null;
  numeroCNH?: string | null;
};

export type AlterarSenhaRequest = {
  senhaAtual: string;
  senhaNova: string;
};

export type AtualizarSlugRequest = {
  slug: string;
};

export type SolicitarExclusaoResponse = {
  mensagem: string;
};

export type ConfirmarExclusaoResponse = {
  mensagem: string;
};

export type ContatoGerenteResponse = {
  telefone: string | null;
  possuiIngresso: boolean;
};

// ── Admin (Spec Admin) ────────────────────────────────────────────

export type UsuarioAdminResponse = {
  usuarioId: string;
  nome: string;
  cpf: string;
  email: string | null;
  tipo: string;
  ativo: boolean;
  totalReservas: number;
  criadoEm: string;
};

export type GerenteAdminResponse = {
  id: string;
  nome: string;
  cpf: string;
  email: string | null;
  telefone: string | null;
  slug: string | null;
  taxaPlataforma: number;
  gratuito: boolean;
  ativo: boolean;
  totalVans: number;
  totalViagens: number;
  criadoEm: string;
};

export type CriarGerenteAdminRequest = {
  nome: string;
  cpf: string;
  email: string;
  senha: string;
  telefone?: string | null;
  slug: string;
  taxaPlataforma?: number | null;
  gratuito?: boolean | null;
  chavePix?: string | null;
};

export type AtualizarGerenteAdminRequest = {
  taxaPlataforma?: number | null;
  gratuito?: boolean | null;
  ativo?: boolean | null;
};

export type ReservaHistoricoItemResponse = {
  assento: number;
  passageiroNome: string;
  passageiroDocumento: string;
  valor: number;
};

export type ReservaHistoricoResponse = {
  id: string;
  status: string;
  valorTotal: number;
  taxaPlataforma: number;
  criadaEm: string;
  viagem: {
    id: string;
    nomeEvento: string;
    origem: string;
    destino: string;
    dataPartida: string;
  };
  itens: ReservaHistoricoItemResponse[];
};

export type ViagemGerenteHistoricoResponse = {
  viagemId: string;
  nomeEvento: string;
  origem: string;
  destino: string;
  dataPartida: string;
  dataEvento: string;
  totalReservas: number;
  totalArrecadado: number;
  taxaPlataforma: number;
  statusViagem: string;
};
