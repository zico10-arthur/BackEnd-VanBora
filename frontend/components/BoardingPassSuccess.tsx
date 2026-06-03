"use client";

import { useCallback, useMemo, useState } from "react";

function BoardingQrPlaceholder({ seed }: { seed: number }) {
  const cells = useMemo(() => {
    const rng = (n: number) => {
      let x = n * 7919 + seed;
      return () => {
        x = (x * 1103515245 + 12345) & 0x7fffffff;
        return (x / 0x7fffffff) > 0.42;
      };
    };
    const r = rng(seed);
    return Array.from({ length: 21 * 21 }, () => r());
  }, [seed]);

  return (
    <div
      className="mx-auto flex h-36 w-36 shrink-0 items-center justify-center rounded-lg bg-white p-1.5 shadow-inner sm:h-40 sm:w-40 print:h-32 print:w-32"
      role="img"
      aria-label="QR Code de embarque"
    >
      <svg viewBox="0 0 21 21" className="h-full w-full text-zinc-900" aria-hidden>
        {cells.map((on, i) => {
          const x = i % 21;
          const y = Math.floor(i / 21);
          return on ? <rect key={i} x={x} y={y} width={1} height={1} fill="currentColor" /> : null;
        })}
      </svg>
    </div>
  );
}

function CheckSuccessIcon({ className }: { className?: string }) {
  return (
    <div
      className={`relative flex h-16 w-16 items-center justify-center rounded-full bg-emerald-500/15 ring-2 ring-emerald-400/60 print:bg-emerald-100 print:ring-emerald-600 ${className ?? ""}`}
      aria-hidden
    >
      <svg
        viewBox="0 0 24 24"
        className="h-9 w-9 origin-center text-emerald-400 animate-[vb-check-pop_0.55s_ease-out_forwards] print:text-emerald-700"
        fill="none"
        stroke="currentColor"
        strokeWidth={2.5}
      >
        <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
      </svg>
    </div>
  );
}

function VanIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} aria-hidden>
      <path d="M4 17h2M18 17h2" strokeLinecap="round" />
      <path d="M4 17v-4.5l2.5-5h9L18 12.5V17" strokeLinejoin="round" />
      <circle cx="7.5" cy="17" r="2.25" />
      <circle cx="16.5" cy="17" r="2.25" />
      <path d="M6.5 8h11l-1-2H7.5l-1 2z" strokeLinejoin="round" />
      <path d="M12 8V5M10 5h4" strokeLinecap="round" />
    </svg>
  );
}

export interface BoardingPassSuccessProps {
  eventName: string;
  dateLabel: string;
  departureLocation: string;
  vehicleModelColor: string;
  vehiclePlate: string;
  passengerName: string;
  cpfMasked: string;
  seatLabel: string;
  quorumRemaining: number;
  quorumDeadlineLabel: string;
}

export function BoardingPassSuccess({
  eventName,
  dateLabel,
  departureLocation,
  vehicleModelColor,
  vehiclePlate,
  passengerName,
  cpfMasked,
  seatLabel,
  quorumRemaining,
  quorumDeadlineLabel,
}: BoardingPassSuccessProps) {
  const [shareHint, setShareHint] = useState<string | null>(null);

  const share = useCallback(async () => {
    const text = `Ingresso VanBora — ${eventName} · Poltrona ${seatLabel}. Ajude a fechar o ônibus!`;
    const url = typeof window !== "undefined" ? window.location.href : "";
    try {
      if (navigator.share) {
        await navigator.share({ title: "Meu ingresso VanBora", text, url });
        return;
      }
      await navigator.clipboard.writeText(`${text}\n${url}`);
      setShareHint("Link copiado para a área de transferência.");
      window.setTimeout(() => setShareHint(null), 3000);
    } catch {
      setShareHint(null);
    }
  }, [eventName, seatLabel]);

  const saveOrPrint = useCallback(() => {
    window.print();
  }, []);

  return (
    <div className="flex flex-col items-center gap-6 print:max-w-none print:gap-4">
      <div className="flex flex-col items-center gap-3 text-center print:gap-2">
        <CheckSuccessIcon />
        <h3 className="text-2xl font-black tracking-tight text-white print:text-2xl print:text-zinc-900 sm:text-3xl">
          Pagamento Confirmado!
        </h3>
        <p className="max-w-sm text-sm text-zinc-400 print:text-zinc-600">
          Apresente este comprovante no embarque. Você pode imprimir ou salvar em PDF.
        </p>
      </div>

      {/* Boarding pass */}
      <div className="relative w-full max-w-md print:max-w-md">
        <div className="pointer-events-none absolute -left-2 top-[42%] z-10 h-5 w-5 -translate-y-1/2 rounded-full bg-zinc-950 ring-1 ring-amber-500/25 print:bg-white print:ring-zinc-400" />
        <div className="pointer-events-none absolute -right-2 top-[42%] z-10 h-5 w-5 -translate-y-1/2 rounded-full bg-zinc-950 ring-1 ring-amber-500/25 print:bg-white print:ring-zinc-400" />

        <article className="overflow-hidden rounded-2xl border border-amber-500/25 bg-zinc-900 shadow-[0_20px_50px_rgba(0,0,0,0.5)] print:border-zinc-400 print:bg-white print:shadow-none">
          <header className="border-b border-dashed border-white/15 bg-gradient-to-br from-zinc-800/80 to-zinc-900 px-4 py-4 print:border-zinc-300 print:from-white print:to-zinc-100">
            <p className="text-[10px] font-bold uppercase tracking-[0.2em] text-amber-500 print:text-amber-700">VanBora · Embarque</p>
            <h4 className="mt-2 text-lg font-bold leading-snug text-white print:text-zinc-900">{eventName}</h4>
            <dl className="mt-3 grid gap-2 text-xs text-zinc-300 print:text-zinc-700 sm:grid-cols-2">
              <div>
                <dt className="text-zinc-500 print:text-zinc-500">Data e hora</dt>
                <dd className="font-semibold text-white print:text-zinc-900">{dateLabel}</dd>
              </div>
              <div className="sm:text-right">
                <dt className="text-zinc-500 print:text-zinc-500 sm:text-right">Local de saída</dt>
                <dd className="font-semibold text-white print:text-zinc-900 sm:text-right">{departureLocation}</dd>
              </div>
            </dl>
          </header>

          <div className="border-b border-dashed border-white/15 px-4 py-4 print:border-zinc-300 print:py-3">
            <div className="flex gap-3 sm:gap-4">
              <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-amber-500/10 text-amber-500 ring-1 ring-amber-500/25 print:bg-amber-100 print:text-amber-800 print:ring-amber-600/30 sm:h-12 sm:w-12">
                <VanIcon className="h-6 w-6 sm:h-7 sm:w-7" />
              </div>
              <div className="min-w-0 flex-1">
                <p className="text-[10px] font-bold uppercase tracking-[0.2em] text-amber-500 print:text-amber-800">Veículo</p>
                <p className="mt-1 break-words text-sm font-semibold leading-snug text-white print:text-zinc-900 sm:text-base">
                  {vehicleModelColor}
                </p>
                <p className="mt-1 font-mono text-sm font-bold tracking-[0.12em] text-zinc-200 print:text-zinc-800 sm:text-base">
                  {vehiclePlate}
                </p>
              </div>
            </div>
          </div>

          <div className="px-4 py-4 print:py-3">
            <p className="text-[10px] font-bold uppercase tracking-wider text-zinc-500 print:text-zinc-500">Passageiro</p>
            <p className="mt-1 text-base font-semibold text-white print:text-zinc-900">{passengerName}</p>
            <p className="mt-1 font-mono text-sm text-zinc-400 print:text-zinc-600">CPF {cpfMasked}</p>

            <div className="mt-5 rounded-xl border-2 border-amber-500/50 bg-black/30 px-4 py-4 text-center print:border-amber-600 print:bg-amber-50">
              <p className="text-[10px] font-bold uppercase tracking-[0.25em] text-amber-500/90 print:text-amber-800">Poltrona</p>
              <p className="mt-1 font-mono text-3xl font-black tracking-wider text-amber-400 print:text-amber-900 sm:text-4xl">
                POLTRONA {seatLabel}
              </p>
            </div>

            <div className="mt-5 flex flex-col items-center gap-2 border-t border-dashed border-white/10 pt-5 print:border-zinc-300">
              <p className="text-[10px] font-semibold uppercase tracking-wider text-zinc-500 print:text-zinc-600">Check-in / Embarque</p>
              <BoardingQrPlaceholder seed={7} />
              <p className="max-w-[240px] text-center text-[10px] leading-relaxed text-zinc-500 print:text-zinc-600">
                Código válido para este evento. O motorista poderá validar no dia.
              </p>
            </div>
          </div>
        </article>
      </div>

      {/* Quorum */}
      <section
        className="w-full max-w-md rounded-xl border border-amber-500/30 bg-amber-500/5 px-4 py-4 print:border-zinc-300 print:bg-zinc-100"
        aria-live="polite"
      >
        <p className="text-xs font-semibold text-zinc-300 print:text-zinc-700">
          A viagem só sai se atingirmos o número mínimo de passageiros. Seu lugar fica garantido após a confirmação do pagamento.
        </p>
        <div className="mt-3 inline-flex items-center gap-2 rounded-full bg-amber-500 px-3 py-1.5 text-xs font-bold text-black print:bg-amber-400">
          <span className="h-2 w-2 animate-pulse rounded-full bg-black/40 print:animate-none" aria-hidden />
          Status: Aguardando Quórum
        </div>
        <p className="mt-3 text-sm leading-relaxed text-zinc-200 print:text-zinc-800">
          Faltam <span className="font-bold text-amber-400 print:text-amber-900">{quorumRemaining} pessoas</span> para
          confirmar a viagem. Se a meta não for atingida até{" "}
          <span className="font-semibold text-white print:text-zinc-950">{quorumDeadlineLabel}</span>, o valor pago será
          estornado automaticamente.
        </p>
      </section>

      <div className="flex w-full max-w-md flex-col gap-2 sm:flex-row sm:justify-center print:hidden">
        <button
          type="button"
          onClick={saveOrPrint}
          className="rounded-xl border border-zinc-700 px-4 py-3 text-sm font-medium text-zinc-300 transition hover:border-zinc-500 hover:bg-zinc-900 hover:text-white focus:outline-none focus-visible:ring-2 focus-visible:ring-amber-500"
        >
          Salvar ingresso na galeria
        </button>
        <button
          type="button"
          onClick={share}
          className="rounded-xl border border-zinc-700 px-4 py-3 text-sm font-medium text-zinc-300 transition hover:border-zinc-500 hover:bg-zinc-900 hover:text-white focus:outline-none focus-visible:ring-2 focus-visible:ring-amber-500"
        >
          Compartilhar com amigos
        </button>
      </div>
      {shareHint ? <p className="text-center text-xs text-amber-500/90 print:hidden">{shareHint}</p> : null}

      <p className="hidden print:block print:text-center print:text-[10px] print:text-zinc-500">
        Documento gerado em {new Date().toLocaleString("pt-BR")} · VanBora
      </p>
    </div>
  );
}
