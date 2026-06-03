import { apiPost } from "./http";
import type { LoginResponse, RegistrarGerenteResponse, RegistrarPassageiroResponse } from "./types";

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
