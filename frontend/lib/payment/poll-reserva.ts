import { obterReserva } from "@/lib/api/reservas";
import type { ReservaResponse } from "@/lib/api/types";

const TERMINAL = new Set(["Confirmada", "Cancelada", "Expirada"]);

function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

export type PollReservaResult =
  | { kind: "confirmed"; reserva: ReservaResponse }
  | { kind: "terminal"; reserva: ReservaResponse }
  | { kind: "timeout"; last: ReservaResponse | null };

/**
 * Aguarda confirmação via webhook (polling leve no retorno do checkout).
 */
export async function pollReservaStatus(
  reservaId: string,
  options?: { maxAttempts?: number; intervalMs?: number },
): Promise<PollReservaResult> {
  const maxAttempts = options?.maxAttempts ?? 20;
  const intervalMs = options?.intervalMs ?? 2000;

  let last: ReservaResponse | null = null;

  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    last = await obterReserva(reservaId);

    if (last.status === "Confirmada") {
      return { kind: "confirmed", reserva: last };
    }

    if (TERMINAL.has(last.status)) {
      return { kind: "terminal", reserva: last };
    }

    if (attempt < maxAttempts - 1) {
      await sleep(intervalMs);
    }
  }

  return { kind: "timeout", last };
}
