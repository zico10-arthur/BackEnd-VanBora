/** Textos de produto — evitar jargão interno na interface. */

export const BRAND = {
  name: "VanBora",
  tagline: "Mobilidade para megaeventos",
  heroEyebrow: "VanBora",
} as const;

export const RESERVA_STATUS_LABEL: Record<string, string> = {
  PendentePagamento: "Aguardando pagamento",
  Confirmada: "Confirmada",
  Cancelada: "Cancelada",
  Expirada: "Expirada",
  EmAndamento: "Em viagem",
  Concluida: "Concluída",
};

export function labelReservaStatus(status: string): string {
  return RESERVA_STATUS_LABEL[status] ?? status;
}
