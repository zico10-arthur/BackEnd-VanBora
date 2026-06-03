"use client";

import { MetallicFrame } from "./MetallicFrame";

interface BookingBarProps {
  selectedSeatLabel: string | null;
  onAdvance: () => void;
}

export function BookingBar({ selectedSeatLabel, onAdvance }: BookingBarProps) {
  const hasSelection = Boolean(selectedSeatLabel);

  return (
    <div className="pointer-events-none fixed inset-x-0 bottom-0 z-50 flex justify-center p-4 pb-[max(1rem,env(safe-area-inset-bottom))]">
      <MetallicFrame className="pointer-events-auto w-full max-w-lg shadow-[0_24px_80px_rgba(0,0,0,0.75)]">
        <div
          className="flex flex-col gap-3 rounded-[15px] bg-[#0D0D0D]/92 px-4 py-4 backdrop-blur-xl sm:flex-row sm:items-center sm:justify-between sm:px-5"
          role="region"
          aria-label="Resumo da reserva"
        >
          <p className="text-center text-sm text-neutral-300 sm:text-left">
            {hasSelection ? (
              <>
                <span className="font-bold tracking-tight text-white">1 Assento Selecionado</span>
                <span className="mt-0.5 block font-medium text-[#f0a500]/90">
                  Assento {selectedSeatLabel}
                </span>
              </>
            ) : (
              <span className="text-white/90">Selecione um assento no mapa</span>
            )}
          </p>
          {hasSelection ? (
            <div className="w-full shrink-0 rounded-xl bg-gradient-to-br from-[#fff6d4]/45 via-[#f0a500]/35 to-[#3d2600]/40 p-px sm:w-auto">
              <button
                type="button"
                onClick={onAdvance}
                className="vb-cta-gold relative w-full overflow-hidden rounded-[11px] py-3 text-sm font-extrabold uppercase tracking-wide text-[#1a0f00] transition duration-300 hover:scale-[1.02] active:scale-[0.99] focus:outline-none focus-visible:ring-2 focus-visible:ring-[#F0A500] focus-visible:ring-offset-2 focus-visible:ring-offset-[#0D0D0D] sm:min-w-[200px]"
              >
                <span
                  className="absolute inset-0 bg-gradient-to-b from-[#fff3c2] via-[#f0a500] to-[#a86a00]"
                  aria-hidden
                />
                <span
                  className="absolute inset-0 bg-gradient-to-t from-black/25 to-transparent opacity-50"
                  aria-hidden
                />
                <span className="relative z-[1]">Continuar reserva</span>
              </button>
            </div>
          ) : (
            <button
              type="button"
              disabled
              className="vb-seat-dead w-full shrink-0 cursor-not-allowed rounded-xl border border-white/5 py-3 text-sm font-semibold uppercase tracking-wide text-neutral-600 sm:w-auto sm:min-w-[200px]"
            >
              Continuar reserva
            </button>
          )}
        </div>
      </MetallicFrame>
    </div>
  );
}
