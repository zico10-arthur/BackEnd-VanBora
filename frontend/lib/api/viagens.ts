import { apiGet } from "./http";
import type { ViagemPublica, ViagemVanDetalhe } from "./types";

export function listarViagens() {
  return apiGet<ViagemPublica[]>("/api/viagens");
}

export function obterDetalheViagemVan(viagemVanId: string) {
  return apiGet<ViagemVanDetalhe>(`/api/viagens/van/${viagemVanId}`);
}
