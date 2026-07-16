const KEY = "vanbora_pending_reserva";

export type PendingReservaCheckout = {
  reservaId: string;
  eventName: string;
  seatLabel: string;
  valorAPagar: number;
  viagemVanId?: string;
  expiraEm?: string;
};

export function savePendingCheckout(data: PendingReservaCheckout): void {
  if (typeof sessionStorage === "undefined") return;
  sessionStorage.setItem(KEY, JSON.stringify(data));
}

export function readPendingCheckout(): PendingReservaCheckout | null {
  if (typeof sessionStorage === "undefined") return null;
  const raw = sessionStorage.getItem(KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as PendingReservaCheckout;
  } catch {
    return null;
  }
}

export function clearPendingCheckout(): void {
  if (typeof sessionStorage === "undefined") return;
  sessionStorage.removeItem(KEY);
}
