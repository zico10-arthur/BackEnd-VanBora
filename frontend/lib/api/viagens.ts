import { apiDelete, apiGet, apiPost, apiPut } from "./http";
import type {
  ViagemPublica,
  ViagemVanDetalhe,
  CriarViagemRequest,
  AtualizarViagemRequest,
  ViagemGerenteResponse,
  AlocarVanRequest,
  AlocarMotoristaRequest,
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

// ── Spec 50 — Alocação de Recursos ──────────────────────────────────

export function alocarVan(
  viagemId: string,
  vanId: string,
): Promise<ViagemGerenteResponse> {
  const body: AlocarVanRequest = { vanId };
  return apiPost<ViagemGerenteResponse>(
    `/api/gerente/viagens/${viagemId}/alocar-van`,
    body,
    true,
  );
}

export function removerVanAlocada(
  viagemId: string,
  viagemVanId: string,
): Promise<ViagemGerenteResponse> {
  return apiDelete<ViagemGerenteResponse>(
    `/api/gerente/viagens/${viagemId}/alocar-van/${viagemVanId}`,
    true,
  );
}

export function alocarMotorista(
  viagemId: string,
  viagemVanId: string,
  body: AlocarMotoristaRequest,
): Promise<ViagemGerenteResponse> {
  return apiPost<ViagemGerenteResponse>(
    `/api/gerente/viagens/${viagemId}/alocar-motorista/${viagemVanId}`,
    body,
    true,
  );
}

export function removerMotoristaAlocado(
  viagemId: string,
  viagemVanId: string,
): Promise<boolean> {
  return apiDelete<boolean>(
    `/api/gerente/viagens/${viagemId}/remover-motorista/${viagemVanId}`,
    true,
  );
}
