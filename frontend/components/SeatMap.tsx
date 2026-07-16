"use client";

import type { SeatItem, SeatState } from "@/lib/types";

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

function isSelectable(state: SeatState): boolean {
  return state === "available";
}

export function SeatMap({ seats, selectedId, onSelect }: SeatMapProps) {
  const rows = groupByRow(seats);

  return (
    <div className="mx-auto w-full max-w-md">
      <div
        className="relative mb-4 overflow-hidden rounded-vb border border-van-border bg-gradient-to-br from-van-amber/10 via-van-amber/5 to-van-void/80 p-px shadow-[0_0_24px_rgba(0,0,0,0.35)]"
        aria-hidden
      >
        <div className="rounded-[11px] bg-van-void/90 px-3 py-2.5 text-center text-[11px] font-bold uppercase tracking-[0.28em] text-van-amber/80">
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
                className="vb-seat-dead flex items-center justify-center rounded-md border border-van-border opacity-90"
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

      <ul className="mt-6 flex flex-wrap justify-center gap-x-5 gap-y-2 text-xs text-zinc-500">
        <li className="flex items-center gap-2">
          <span className="h-4 w-4 rounded border-2 border-van-amber/70 bg-van-void" />
          Livre
        </li>
        <li className="flex items-center gap-2">
          <span className="vb-seat-reserved h-4 w-4 rounded border border-zinc-600/50" />
          Reservado
        </li>
        <li className="flex items-center gap-2">
          <span className="vb-seat-dead h-4 w-4 rounded border border-van-border" />
          Indisponível
        </li>
        <li className="flex items-center gap-2">
          <span className="vb-seat-neon h-4 w-4 rounded bg-van-amber" />
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
  const base =
    "relative flex min-h-[44px] flex-col items-center justify-center rounded-vb text-sm font-bold transition-[border-color,box-shadow,background-color,transform] duration-200 ease-out focus:outline-none focus-visible:ring-2 focus-visible:ring-van-amber focus-visible:ring-offset-2 focus-visible:ring-offset-van-void";

  if (seat.state === "unavailable") {
    return (
      <button
        type="button"
        disabled
        className={`${base} vb-seat-dead cursor-not-allowed border border-black/40 text-zinc-600`}
        aria-disabled
        aria-label={`Assento ${seat.id} indisponível`}
      >
        {seat.id}
      </button>
    );
  }

  if (seat.state === "reserved") {
    return (
      <button
        type="button"
        disabled
        title="Reservado por outro passageiro"
        className={`${base} vb-seat-reserved cursor-not-allowed border border-zinc-600/40 text-zinc-500`}
        aria-disabled
        aria-label={`Assento ${seat.id} reservado por outro passageiro`}
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
        className={`${base} vb-seat-neon z-[1] border border-van-gold/60 bg-van-amber text-van-void`}
        aria-pressed="true"
        aria-label={`Assento ${seat.id} selecionado`}
      >
        {seat.id}
      </button>
    );
  }

  if (!isSelectable(seat.state)) {
    return null;
  }

  return (
    <button
      type="button"
      onClick={() => onSelect(seat)}
      className={`${base} border-2 border-van-amber/70 bg-van-void text-van-amber hover:border-van-amber hover:bg-van-elevated hover:shadow-[0_0_14px_rgba(240,165,0,0.35)] active:scale-[0.97]`}
      aria-pressed="false"
      aria-label={`Assento ${seat.id} disponível`}
    >
      {seat.id}
    </button>
  );
}
