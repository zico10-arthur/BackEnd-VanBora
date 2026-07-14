import { apiGet, apiPost, apiPut } from "./http";
import type {
  ViagemPublica,
  ViagemVanDetalhe,
  CriarViagemRequest,
  AtualizarViagemRequest,
  ViagemGerenteResponse,
} from "./types";

export function listarViagens() {
  return apiGet<ViagemPublica[]>("/api/viagens");
}

export function obterDetalheViagemVan(viagemVanId: string) {
  return apiGet<ViagemVanDetalhe>(`/api/viagens/van/${viagemVanId}`);
}

// ── Spec 20 — Viagens (Gerente) ──────────────────────────────────

export function listarViagensGerente(): Promise<ViagemGerenteResponse[]> {
  return apiGet<ViagemGerenteResponse[]>("/api/gerente/viagens", true);
}

export function obterViagemGerente(id: string): Promise<ViagemGerenteResponse> {
  return apiGet<ViagemGerenteResponse>(`/api/gerente/viagens/${id}`, true);
}

export function criarViagemGerente(
  body: CriarViagemRequest
): Promise<ViagemGerenteResponse> {
  return apiPost<ViagemGerenteResponse>("/api/gerente/viagens", body, true);
}

export function atualizarViagemGerente(
  id: string,
  body: AtualizarViagemRequest
): Promise<ViagemGerenteResponse> {
  return apiPut<ViagemGerenteResponse>(`/api/gerente/viagens/${id}`, body, true);
}

export function cancelarViagemGerente(id: string): Promise<void> {
  return apiPost<void>(`/api/gerente/viagens/${id}/cancelar`, {}, true);
}
