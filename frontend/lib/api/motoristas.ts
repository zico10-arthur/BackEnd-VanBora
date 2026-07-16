import { apiDelete, apiGet, apiPatch, apiPost, apiPut } from "./http";
import type { CriarMotoristaRequest, MotoristaResponse } from "./types";

export function listarMotoristas() {
  return apiGet<MotoristaResponse[]>("/api/gerente/motoristas", true);
}

export function obterMotorista(id: string) {
  return apiGet<MotoristaResponse>(`/api/gerente/motoristas/${id}`, true);
}

export function criarMotorista(body: CriarMotoristaRequest) {
  return apiPost<MotoristaResponse>("/api/gerente/motoristas", body, true);
}

export function atualizarMotorista(id: string, body: CriarMotoristaRequest) {
  return apiPut<MotoristaResponse>(`/api/gerente/motoristas/${id}`, body, true);
}

export function removerMotorista(id: string) {
  return apiDelete<boolean>(`/api/gerente/motoristas/${id}`, true);
}

export function alternarStatusMotorista(id: string) {
  return apiPatch<MotoristaResponse>(`/api/gerente/motoristas/${id}/status`, undefined, true);
}
