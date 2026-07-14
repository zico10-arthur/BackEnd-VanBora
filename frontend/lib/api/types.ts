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

// ── Spec 20 — Viagens (Gerente) ──────────────────────────────────

export type CriarViagemRequest = {
  nomeEvento: string;
  dataEvento: string;
  localEvento: string;
  origemDescricao: string;
  origemCidade: string;
  origemEstado: string;
  destinoDescricao: string;
  destinoCidade: string;
  destinoEstado: string;
  dataSaida: string;
  dataChegada: string;
  precoAssento: number;
  possuiIngresso: boolean;
};

export type AtualizarViagemRequest = CriarViagemRequest;

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
  dataPartida: string;
  origem: string;
  destino: string;
  precoAssento: number;
  possuiIngresso: boolean;
  status: string;
  vans: ViagemVanGerenteInfo[];
};
