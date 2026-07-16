import type { SeatItem } from "./types";

const ROWS = ["A", "B", "C", "D", "E"] as const;

/** Converte rótulo do mapa (A1) para número enviado à API (1–20). */
export function seatLabelToNumber(label: string): number {
  const row = label.charAt(0).toUpperCase();
  const pos = parseInt(label.slice(1), 10);
  const rowIndex = ROWS.indexOf(row as (typeof ROWS)[number]);
  if (rowIndex < 0 || pos < 1 || pos > 4) return 0;
  return rowIndex * 4 + pos;
}

export function seatNumberToLabel(num: number): string {
  if (num < 1) return "?";
  const rowIndex = Math.floor((num - 1) / 4);
  const pos = ((num - 1) % 4) + 1;
  return `${ROWS[rowIndex] ?? "?"}${pos}`;
}

/** Mapa 5×4 alinhado ao domínio (até 20 assentos). */
export function buildSeatMap(capacidade: number, ocupados: number[]): SeatItem[] {
  const occupied = new Set(ocupados);
  const seats: SeatItem[] = [];
  const max = Math.min(capacidade, ROWS.length * 4);

  for (let n = 1; n <= max; n++) {
    const label = seatNumberToLabel(n);
    const row = label.charAt(0);
    const position = parseInt(label.slice(1), 10) as 1 | 2 | 3 | 4;
    seats.push({
      id: label,
      row,
      position,
      state: occupied.has(n) ? "reserved" : "available",
    });
  }
  return seats;
}
