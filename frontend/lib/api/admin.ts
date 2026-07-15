import { apiGet, apiPost, apiPut } from "./http";
import type {
  AtualizarGerenteAdminRequest,
  CriarGerenteAdminRequest,
  GerenteAdminResponse,
  ReservaHistoricoResponse,
  UsuarioAdminResponse,
  ViagemGerenteHistoricoResponse,
} from "./types";

// ── Admin — Usuários ──────────────────────────────────────────────

export function listarUsuariosAdmin(search?: string) {
  const qs = search ? `?search=${encodeURIComponent(search)}` : "";
  return apiGet<UsuarioAdminResponse[]>(`/api/admin/Usuarios${qs}`, true);
}

export function obterHistoricoReservasUsuario(usuarioId: string) {
  return apiGet<ReservaHistoricoResponse[]>(`/api/admin/Usuarios/${usuarioId}/reservas`, true);
}

// ── Admin — Gerentes ──────────────────────────────────────────────

export function listarGerentesAdmin(search?: string) {
  const qs = search ? `?search=${encodeURIComponent(search)}` : "";
  return apiGet<GerenteAdminResponse[]>(`/api/admin/Gerentes${qs}`, true);
}

export function obterGerenteAdmin(id: string) {
  return apiGet<GerenteAdminResponse>(`/api/admin/Gerentes/${id}`, true);
}

export function criarGerenteAdmin(body: CriarGerenteAdminRequest) {
  return apiPost<GerenteAdminResponse>("/api/admin/Gerentes", body, true);
}

export function atualizarGerenteAdmin(id: string, body: AtualizarGerenteAdminRequest) {
  return apiPut<GerenteAdminResponse>(`/api/admin/Gerentes/${id}`, body, true);
}

export function obterHistoricoViagensGerente(id: string) {
  return apiGet<ViagemGerenteHistoricoResponse[]>(`/api/admin/Gerentes/${id}/reservas`, true);
}
