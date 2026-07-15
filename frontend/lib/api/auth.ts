import { apiGet, apiPost, apiPut } from "./http";
import type {
  AlterarSenhaRequest,
  AtualizarSlugRequest,
  AtualizarUsuarioRequest,
  AtualizarUsuarioResponse,
  ConfirmarExclusaoResponse,
  LoginResponse,
  RegistrarGerenteResponse,
  RegistrarPassageiroResponse,
  SolicitarExclusaoResponse,
} from "./types";

export function authLogin(email: string, senha: string) {
  return apiPost<LoginResponse>("/api/auth/login", { email, senha });
}

export function authRegistrarPassageiro(body: {
  nome: string;
  cpf: string;
  email: string;
  telefone: string;
  senha: string;
}) {
  return apiPost<RegistrarPassageiroResponse>("/api/auth/passageiro/registrar", body);
}

export function authRegistrarGerente(body: {
  nome: string;
  cpf: string;
  email: string;
  senha: string;
  slug: string;
  telefone?: string | null;
  chavePix?: string | null;
}) {
  return apiPost<RegistrarGerenteResponse>("/api/auth/gerente/registrar", body);
}

// ── Perfil (US18/19/20/21) ────────────────────────────────────────

export function obterUsuario() {
  return apiGet<AtualizarUsuarioResponse>("/api/auth/usuario", true);
}

export function atualizarUsuario(body: AtualizarUsuarioRequest) {
  return apiPut<AtualizarUsuarioResponse>("/api/auth/usuario", body, true);
}

export function alterarSenha(body: AlterarSenhaRequest) {
  return apiPost<{ mensagem: string }>("/api/auth/alterar-senha", body, true);
}

export function atualizarSlug(body: AtualizarSlugRequest) {
  return apiPut<AtualizarUsuarioResponse>("/api/auth/slug", body, true);
}

export function solicitarExclusao() {
  return apiPost<SolicitarExclusaoResponse>("/api/auth/solicitar-exclusao", {}, true);
}

export function confirmarExclusao(codigo: string) {
  return apiPost<ConfirmarExclusaoResponse>("/api/auth/confirmar-exclusao", { codigo }, true);
}
