import { apiDelete, apiGet, apiPatch, apiPost, apiPut } from "./http";
import type { AtualizarVanRequest, CriarVanRequest, VanResponse } from "./types";

export function listarVans() {
  return apiGet<VanResponse[]>("/api/gerente/vans", true);
}

export function obterVan(id: string) {
  return apiGet<VanResponse>(`/api/gerente/vans/${id}`, true);
}

export function criarVan(body: CriarVanRequest) {
  return apiPost<VanResponse>("/api/gerente/vans", body, true);
}

export function atualizarVan(id: string, body: AtualizarVanRequest) {
  return apiPut<VanResponse>(`/api/gerente/vans/${id}`, body, true);
}

export function removerVan(id: string) {
  return apiDelete<{ id: string }>(`/api/gerente/vans/${id}`, true);
}

export function alternarStatusVan(id: string) {
  return apiPatch<VanResponse>(`/api/gerente/vans/${id}/status`, undefined, true);
}
