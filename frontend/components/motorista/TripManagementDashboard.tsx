"use client";

import Image from "next/image";
import Link from "next/link";
import { useCallback, useState } from "react";
import type { EventItem } from "@/lib/types";
import {
  buildPublicTripUrl,
  getPassengerRows,
  getTripManagementStats,
} from "@/lib/tripManagementMock";

const brl = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

function CopyIcon({ className }: { className?: string }) {
  return (
    <svg
      className={className}
      xmlns="http://www.w3.org/2000/svg"
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden
    >
      <rect width="14" height="14" x="8" y="8" rx="2" ry="2" />
      <path d="M4 16c-1.1 0-2-.9-2-2V4c0-1.1.9-2 2-2h10c1.1 0 2 .9 2 2" />
    </svg>
  );
}

interface TripManagementDashboardProps {
  event: EventItem;
  tripId: string;
}

export function TripManagementDashboard({ event, tripId }: TripManagementDashboardProps) {
  const [copied, setCopied] = useState(false);
  const stats = getTripManagementStats();
  const rows = getPassengerRows();
  const shareUrl = buildPublicTripUrl(tripId);

  const copy = useCallback(async () => {
    try {
      await navigator.clipboard.writeText(shareUrl);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 2200);
    } catch {
      setCopied(false);
    }
  }, [shareUrl]);

  return (
    <div className="min-h-screen bg-zinc-950 text-zinc-100">
      <header className="relative h-[min(42vh,320px)] w-full overflow-hidden sm:h-[min(38vh,360px)]">
        <Image
          src={event.coverImage}
          alt=""
          fill
          className="object-cover"
          sizes="100vw"
          priority
        />
        <div className="absolute inset-0 bg-gradient-to-t from-zinc-950 via-zinc-950/75 to-black/50" />
        <div className="absolute inset-0 bg-black/35" />
        <div className="absolute inset-x-0 bottom-0 z-10 px-4 pb-6 pt-12 sm:px-6">
          <div className="mx-auto max-w-3xl">
            <span className="inline-flex items-center gap-1.5 rounded-full border border-emerald-500/40 bg-emerald-950/50 px-2.5 py-1 text-xs font-semibold text-emerald-300 backdrop-blur-sm">
              <span aria-hidden>🟢</span> Ativa
            </span>
            <h1 className="mt-3 text-2xl font-bold tracking-tight text-white drop-shadow-sm sm:text-3xl">
              {event.name}
            </h1>
            <p className="mt-1 text-sm font-medium text-zinc-200">{event.subtitle}</p>
            <p className="mt-2 text-sm text-amber-400/95">{event.dateLabel}</p>
          </div>
        </div>
      </header>

      <div className="relative z-20 mx-auto max-w-3xl space-y-6 px-4 pb-16 pt-6 sm:px-6">
        <Link
          href="/motorista/nova-viagem"
          className="inline-flex text-sm font-medium text-zinc-400 transition hover:text-amber-400"
        >
          ← Nova viagem
        </Link>

        {/* Coração: copiar link */}
        <section
          className="rounded-xl border-2 border-amber-500/50 bg-zinc-900/90 p-5 shadow-[0_0_40px_-8px_rgba(245,158,11,0.35)] ring-1 ring-amber-500/20 sm:p-6"
          aria-labelledby="link-heading"
        >
          <h2 id="link-heading" className="text-xs font-bold uppercase tracking-[0.18em] text-amber-500">
            Link de divulgação
          </h2>
          <p className="mt-3 break-all rounded-lg border border-zinc-700 bg-zinc-950/80 px-3 py-2.5 font-mono text-sm text-amber-100/90">
            {shareUrl.replace(/^https:\/\//, "")}
          </p>
          <button
            type="button"
            onClick={copy}
            className="mt-4 flex w-full items-center justify-center gap-2 rounded-lg border border-zinc-500/60 bg-zinc-100 px-4 py-3.5 text-base font-bold text-zinc-900 shadow-lg transition hover:bg-white focus:outline-none focus-visible:ring-2 focus-visible:ring-amber-500 focus-visible:ring-offset-2 focus-visible:ring-offset-zinc-950 active:scale-[0.99]"
          >
            <CopyIcon className="h-5 w-5 shrink-0" />
            Copiar link para passageiros
          </button>
          {copied && (
            <p className="mt-2 text-center text-sm font-medium text-emerald-400" role="status">
              Link copiado para a área de transferência.
            </p>
          )}
        </section>

        {/* Quórum / receita */}
        <section
          className="rounded-xl border border-zinc-800 bg-zinc-900/80 p-5 sm:p-6"
          aria-labelledby="finance-heading"
        >
          <h2 id="finance-heading" className="text-xs font-bold uppercase tracking-[0.18em] text-amber-500">
            Quórum & receita
          </h2>
          <p className="mt-1 text-sm text-zinc-400">
            Capacidade {stats.capacity} lugares · Quórum mínimo {stats.quorumMin} · Vendidas{" "}
            {stats.soldCount}
          </p>

          <div className="mt-5">
            <div className="mb-2 flex justify-between text-xs font-medium text-zinc-400">
              <span>Progresso até o quórum</span>
              <span className="tabular-nums text-amber-400">
                {stats.soldCount}/{stats.quorumMin}
              </span>
            </div>
            <div className="h-3 w-full overflow-hidden rounded-full bg-zinc-800">
              <div
                className="h-full rounded-full bg-amber-500 shadow-[0_0_12px_rgba(245,158,11,0.45)] transition-[width] duration-500"
                style={{ width: `${stats.quorumProgress}%` }}
              />
            </div>
          </div>

          <p className="mt-4 text-sm leading-relaxed text-zinc-300">
            Faltam{" "}
            <strong className="text-amber-400">{stats.missingForQuorum}</strong> passageiro
            {stats.missingForQuorum !== 1 ? "s" : ""} para garantir a viagem.
          </p>
          <p className="mt-2 text-base font-semibold text-white">
            {brl.format(stats.revenueConfirmed)}{" "}
            <span className="text-sm font-normal text-zinc-500">garantidos até agora</span>
          </p>
        </section>

        {/* Lista de embarque */}
        <section aria-labelledby="boarding-heading">
          <h2 id="boarding-heading" className="text-xs font-bold uppercase tracking-[0.18em] text-amber-500">
            Lista de embarque
          </h2>
          <ul className="mt-4 space-y-2">
            {rows.map(({ seat, passenger }) => (
              <li
                key={seat}
                className="flex flex-col gap-2 rounded-lg border border-zinc-800 bg-zinc-900/60 px-4 py-3 sm:flex-row sm:items-center sm:justify-between"
              >
                <div className="flex items-center gap-3">
                  <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-zinc-800 text-sm font-bold text-amber-500">
                    {seat}
                  </span>
                  {passenger ? (
                    <div>
                      <p className="font-medium text-zinc-100">{passenger.name}</p>
                      <p className="text-xs text-zinc-500">{passenger.phoneMasked}</p>
                    </div>
                  ) : (
                    <p className="text-sm italic text-zinc-500">Aguardando compra…</p>
                  )}
                </div>
                {passenger && (
                  <div className="flex items-center gap-2 text-sm sm:shrink-0">
                    <span className="text-zinc-500">Pix</span>
                    <span className="font-medium text-emerald-400">
                      {passenger.pixStatus === "Pago" ? "✅ Pago" : passenger.pixStatus}
                    </span>
                  </div>
                )}
              </li>
            ))}
          </ul>
        </section>
      </div>
    </div>
  );
}
