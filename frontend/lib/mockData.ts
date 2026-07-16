import type { EventItem, SeatItem } from "./types";

/** Eventos ativos — apenas jogos do Botafogo em casa (Nilton Santos). */
export const MOCK_EVENTS: EventItem[] = [
  {
    id: "bf-2026-04-12",
    name: "Botafogo x Flamengo",
    status: "active",
    subtitle: "Brasileirão · Nilton Santos",
    dateLabel: "12 abr 2026 · 16h",
    coverImage:
      "https://images.unsplash.com/photo-1574629810360-7efbbe195018?auto=format&fit=crop&w=1400&q=85",
  },
  {
    id: "bf-2026-04-23",
    name: "Botafogo x LDU Quito",
    status: "active",
    subtitle: "Sul-Americana · Nilton Santos",
    dateLabel: "23 abr 2026 · 19h",
    coverImage:
      "https://images.unsplash.com/photo-1522778119026-d647f0596c20?auto=format&fit=crop&w=1400&q=85",
  },
  {
    id: "bf-2026-05-03",
    name: "Botafogo x Palmeiras",
    status: "active",
    subtitle: "Brasileirão · Nilton Santos",
    dateLabel: "03 mai 2026 · 18h30",
    coverImage:
      "https://images.unsplash.com/photo-1431324155629-1a6deb48526a?auto=format&fit=crop&w=1400&q=85",
  },
  {
    id: "bf-2026-05-17",
    name: "Botafogo x Grêmio",
    status: "sold_out",
    subtitle: "Brasileirão · Nilton Santos",
    dateLabel: "17 mai 2026 · 16h",
    coverImage:
      "https://images.unsplash.com/photo-1459865264687-595d652de67e?auto=format&fit=crop&w=1400&q=85",
  },
];

const ROWS = ["A", "B", "C", "D", "E"] as const;

/** Assentos ocupados por evento (mock). */
const OCCUPIED_BY_EVENT: Record<string, string[]> = {
  "bf-2026-04-12": ["A1", "A4", "B2", "C3", "D1", "D4", "E2"],
  "bf-2026-04-23": ["A2", "B1", "B4", "C2", "D3", "E1", "E3", "E4"],
  "bf-2026-05-03": ["A3", "B3", "C1", "C4", "D2", "E4"],
};

export function getEventById(id: string): EventItem | undefined {
  return MOCK_EVENTS.find((e) => e.id === id);
}

export function getSeatsForEvent(eventId: string): SeatItem[] {
  const occupied = new Set(OCCUPIED_BY_EVENT[eventId] ?? []);
  const seats: SeatItem[] = [];
  for (const row of ROWS) {
    for (let p = 1; p <= 4; p++) {
      const pos = p as 1 | 2 | 3 | 4;
      const id = `${row}${p}`;
      seats.push({
        id,
        row,
        position: pos,
        state: occupied.has(id) ? "reserved" : "available",
      });
    }
  }
  return seats;
}
