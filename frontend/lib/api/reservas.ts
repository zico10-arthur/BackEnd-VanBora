import { apiGet, apiPost } from "./http";
import type { ContatoGerenteResponse, PagarReservaResponse, ReservaResponse } from "./types";

export function criarReserva(body: {
  viagemVanId: string;
  itens: {
    numeroAssento: number;
    nomePassageiro: string;
    emailPassageiro: string;
    telefonePassageiro: string;
    cpfPassageiro: string;
  }[];
}) {
  return apiPost<ReservaResponse>("/api/reservas", body, true);
}

export function pagarReserva(reservaId: string) {
  return apiPost<PagarReservaResponse>(`/api/reservas/${reservaId}/pagar`, {}, true);
}

export function listarMinhasReservas() {
  return apiGet<ReservaResponse[]>("/api/reservas/minhas", true);
}

export function obterReserva(reservaId: string) {
  return apiGet<ReservaResponse>(`/api/reservas/${reservaId}`, true);
}

export function cancelarReserva(reservaId: string) {
  return apiPost<ReservaResponse>(`/api/reservas/${reservaId}/cancelar`, {}, true);
}

export function obterContatoGerente(reservaId: string) {
  return apiGet<ContatoGerenteResponse>(`/api/reservas/${reservaId}/contato-gerente`, true);
}
