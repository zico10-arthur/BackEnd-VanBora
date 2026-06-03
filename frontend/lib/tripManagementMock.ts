/** Estatísticas operacionais mock (demo) — alinhadas ao enunciado. */
export const DEMO_OPERATION = {
  capacity: 15,
  quorumMin: 10,
  soldCount: 4,
  unitPrice: 60,
} as const;

export interface PassengerRow {
  seat: number;
  name: string;
  phoneMasked: string;
  pixStatus: "Pago" | "Pendente";
}

const DEMO_PASSENGERS: PassengerRow[] = [
  { seat: 1, name: "Maria Silva", phoneMasked: "(21) 9****-1102", pixStatus: "Pago" },
  { seat: 2, name: "João Pereira", phoneMasked: "(21) 9****-8841", pixStatus: "Pago" },
  { seat: 3, name: "Ana Costa", phoneMasked: "(11) 9****-2290", pixStatus: "Pago" },
  { seat: 4, name: "Pedro Santos", phoneMasked: "(21) 9****-5567", pixStatus: "Pago" },
];

export function buildPublicTripUrl(tripId: string): string {
  const slug = tripId.replace(/[^a-zA-Z0-9]+/g, "-").toLowerCase();
  return `https://vanbora.com/v/bfr-${slug.slice(0, 18)}-xyz`;
}

export function getTripManagementStats() {
  const { capacity, quorumMin, soldCount, unitPrice } = DEMO_OPERATION;
  const revenueConfirmed = soldCount * unitPrice;
  const missingForQuorum = Math.max(0, quorumMin - soldCount);
  const quorumProgress = quorumMin > 0 ? Math.min(100, (soldCount / quorumMin) * 100) : 0;
  const capacityProgress = capacity > 0 ? Math.min(100, (soldCount / capacity) * 100) : 0;
  return {
    capacity,
    quorumMin,
    soldCount,
    unitPrice,
    revenueConfirmed,
    missingForQuorum,
    quorumProgress,
    capacityProgress,
  };
}

export function getPassengerRows(): { seat: number; passenger: PassengerRow | null }[] {
  const bySeat = new Map(DEMO_PASSENGERS.map((p) => [p.seat, p]));
  return Array.from({ length: DEMO_OPERATION.capacity }, (_, i) => {
    const seat = i + 1;
    return { seat, passenger: bySeat.get(seat) ?? null };
  });
}
