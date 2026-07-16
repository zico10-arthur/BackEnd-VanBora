"use client";

import { formatBrl } from "@/lib/format";
import { MetallicFrame } from "./MetallicFrame";

interface BookingPanelProps {
  eventName: string;
  localPartida: string;
  precoAssento: number;
  selectedSeatLabel: string | null;
  onAdvance: () => void;
  /** desktop sticky side panel vs mobile bottom bar */
  variant?: "panel" | "bar";
}

function AdvanceButton({ disabled, onClick, label }: { disabled: boolean; onClick: () => void; label: string }) {
  if (disabled) {
    return (
      <button
        type="button"
        disabled
        className="vb-seat-dead w-full shrink-0 cursor-not-allowed rounded-vb border border-van-border py-3 text-sm font-semibold uppercase tracking-wide text-zinc-600"
      >
        {label}
      </button>
    );
  }

  return (
    <div className="w-full shrink-0 rounded-vb bg-gradient-to-br from-van-gold/45 via-van-amber/35 to-[#3d2600]/40 p-px">
      <button
        type="button"
        onClick={onClick}
        className="vb-cta-gold relative w-full overflow-hidden rounded-[11px] py-3 text-sm font-extrabold uppercase tracking-wide text-van-void transition duration-300 hover:scale-[1.02] active:scale-[0.99] focus:outline-none focus-visible:ring-2 focus-visible:ring-van-amber focus-visible:ring-offset-2 focus-visible:ring-offset-van-void"
      >
        <span
          className="absolute inset-0 bg-gradient-to-b from-[#fff3c2] via-van-amber to-[#a86a00]"
          aria-hidden
        />
        <span
          className="absolute inset-0 bg-gradient-to-t from-black/25 to-transparent opacity-50"
          aria-hidden
        />
        <span className="relative z-[1]">{label}</span>
      </button>
    </div>
  );
}

export function BookingPanel({
  eventName,
  localPartida,
  precoAssento,
  selectedSeatLabel,
  onAdvance,
  variant = "panel",
}: BookingPanelProps) {
  const hasSelection = Boolean(selectedSeatLabel);
  const ctaLabel = variant === "panel" ? "Ir para pagamento" : "Continuar reserva";

  if (variant === "bar") {
    return (
      <div className="pointer-events-none fixed inset-x-0 bottom-0 z-50 flex justify-center p-4 pb-[max(1rem,env(safe-area-inset-bottom))] lg:hidden">
        <MetallicFrame className="pointer-events-auto w-full max-w-lg shadow-[0_24px_80px_rgba(0,0,0,0.75)]">
          <div
            className="flex flex-col gap-3 rounded-[15px] bg-van-void/92 px-4 py-4 backdrop-blur-xl sm:flex-row sm:items-center sm:justify-between sm:px-5"
            role="region"
            aria-label="Resumo da reserva"
          >
            <p className="text-center text-sm text-zinc-300 sm:text-left">
              {hasSelection ? (
                <>
                  <span className="font-bold tracking-tight text-white">Assento {selectedSeatLabel}</span>
                  <span className="mt-0.5 block font-medium text-van-amber/90">{formatBrl(precoAssento)}</span>
                </>
              ) : (
                <span className="text-white/90">Selecione um assento no mapa</span>
              )}
            </p>
            <AdvanceButton disabled={!hasSelection} onClick={onAdvance} label={ctaLabel} />
          </div>
        </MetallicFrame>
      </div>
    );
  }

  return (
    <aside className="hidden lg:block" aria-label="Resumo da compra">
      <MetallicFrame className="sticky top-24 shadow-[0_24px_80px_rgba(0,0,0,0.55)]">
        <div className="rounded-[15px] bg-van-surface/95 p-5 backdrop-blur-xl">
          <p className="text-xs font-semibold uppercase tracking-wider text-van-amber">Sua compra</p>

          <h3 className="mt-2 text-lg font-black leading-snug text-white">{eventName}</h3>

          <dl className="mt-5 space-y-3 text-sm">
            <div className="flex justify-between gap-4 border-b border-van-border pb-3">
              <dt className="text-zinc-500">Assento</dt>
              <dd className="font-bold text-white">{hasSelection ? selectedSeatLabel : "—"}</dd>
            </div>
            <div className="flex justify-between gap-4 border-b border-van-border pb-3">
              <dt className="text-zinc-500">Embarque</dt>
              <dd className="max-w-[60%] text-right font-medium text-zinc-200">{localPartida}</dd>
            </div>
            <div className="flex justify-between gap-4 pt-1">
              <dt className="text-zinc-500">Valor</dt>
              <dd className="text-lg font-black text-van-amber">{formatBrl(precoAssento)}</dd>
            </div>
          </dl>

          <p className="mt-3 text-xs text-zinc-500">
            Taxas da plataforma, se houver, entram no total exibido no checkout Pix.
          </p>

          <div className="mt-5">
            <AdvanceButton disabled={!hasSelection} onClick={onAdvance} label={ctaLabel} />
          </div>

          {!hasSelection ? (
            <p className="mt-3 text-center text-xs text-zinc-500">Selecione um assento no mapa ao lado</p>
          ) : null}
        </div>
      </MetallicFrame>
    </aside>
  );
}
