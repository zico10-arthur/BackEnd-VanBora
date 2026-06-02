"use client";

import type { SeatItem } from "@/lib/types";

interface SeatMapProps {
  seats: SeatItem[];
  selectedId: string | null;
  onSelect: (seat: SeatItem) => void;
}

/** Agrupa assentos por fileira: esquerda [1,2], direita [3,4]. */
function groupByRow(seats: SeatItem[]) {
  const map = new Map<string, SeatItem[]>();
  for (const s of seats) {
    const list = map.get(s.row) ?? [];
    list.push(s);
    map.set(s.row, list);
  }
  return Array.from(map.entries()).sort(([a], [b]) => a.localeCompare(b));
}

export function SeatMap({ seats, selectedId, onSelect }: SeatMapProps) {
  const rows = groupByRow(seats);

  return (
    <div className="mx-auto w-full max-w-md">
      <div
        className="relative mb-4 overflow-hidden rounded-xl border border-transparent bg-gradient-to-br from-[#fff6d4]/25 via-[#f0a500]/15 to-[#2a1a00]/40 p-px shadow-[0_0_24px_rgba(0,0,0,0.35)]"
        aria-hidden
      >
        <div className="rounded-[11px] bg-[#0D0D0D]/90 px-3 py-2.5 text-center text-[11px] font-bold uppercase tracking-[0.28em] text-[#f0a500]/80">
          Frente da van
        </div>
      </div>

      <div className="flex flex-col gap-3 sm:gap-4">
        {rows.map(([row, rowSeats]) => {
          const left = rowSeats.filter((s) => s.position <= 2).sort((a, b) => a.position - b.position);
          const right = rowSeats.filter((s) => s.position > 2).sort((a, b) => a.position - b.position);
          return (
            <div
              key={row}
              className="grid grid-cols-[1fr_1fr_minmax(0.625rem,1rem)_1fr_1fr] items-stretch gap-2 sm:gap-3"
            >
              {left.map((seat) => (
                <SeatButton
                  key={seat.id}
                  seat={seat}
                  selected={selectedId === seat.id}
                  onSelect={onSelect}
                />
              ))}
              <div
                className="vb-seat-dead flex items-center justify-center rounded-md border border-white/[0.07] opacity-90"
                aria-hidden
              />
              {right.map((seat) => (
                <SeatButton
                  key={seat.id}
                  seat={seat}
                  selected={selectedId === seat.id}
                  onSelect={onSelect}
                />
              ))}
            </div>
          );
        })}
      </div>

      <ul className="mt-6 flex flex-wrap justify-center gap-4 text-xs text-neutral-500">
        <li className="flex items-center gap-2">
          <span className="h-4 w-4 rounded border border-neutral-400/40 bg-neutral-100 shadow-inner" />
          Livre
        </li>
        <li className="flex items-center gap-2">
          <span className="vb-seat-dead h-4 w-4 rounded border border-white/5" />
          Ocupado
        </li>
        <li className="flex items-center gap-2">
          <span className="vb-seat-neon h-4 w-4 rounded bg-[#F0A500]" />
          Selecionado
        </li>
      </ul>
    </div>
  );
}

function SeatButton({
  seat,
  selected,
  onSelect,
}: {
  seat: SeatItem;
  selected: boolean;
  onSelect: (seat: SeatItem) => void;
}) {
  const occupied = seat.state === "occupied";

  const base =
    "relative flex min-h-[44px] flex-col items-center justify-center rounded-lg text-sm font-bold transition-[border-color,box-shadow,background-color,transform] duration-200 ease-out focus:outline-none focus-visible:ring-2 focus-visible:ring-[#F0A500] focus-visible:ring-offset-2 focus-visible:ring-offset-[#0D0D0D]";

  if (occupied) {
    return (
      <button
        type="button"
        disabled
        className={`${base} vb-seat-dead cursor-not-allowed border border-black/40 text-[#3d3d3d]`}
        aria-disabled
      >
        {seat.id}
      </button>
    );
  }

  if (selected) {
    return (
      <button
        type="button"
        onClick={() => onSelect(seat)}
        className={`${base} vb-seat-neon z-[1] border border-[#ffe08a]/60 bg-[#F0A500] text-[#1a0f00]`}
        aria-pressed="true"
      >
        {seat.id}
      </button>
    );
  }

  return (
    <button
      type="button"
      onClick={() => onSelect(seat)}
      className={`${base} border-2 border-neutral-300/40 bg-neutral-100 text-[#0D0D0D] shadow-[inset_0_1px_0_rgba(255,255,255,0.65)] hover:border-[#F0A500] hover:bg-white hover:shadow-[0_0_14px_rgba(240,165,0,0.35)] active:scale-[0.97]`}
      aria-pressed="false"
    >
      {seat.id}
    </button>
  );
}
