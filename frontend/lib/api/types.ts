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
