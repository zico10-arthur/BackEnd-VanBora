"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { Header } from "@/components/Header";
import { ReservationHoldChip } from "@/components/reserva/ReservationHoldChip";
import { SeatBookingClient } from "@/components/SeatBookingClient";
import { TripHeroPanel } from "@/components/TripHeroPanel";
import { obterDetalheViagemVan } from "@/lib/api/viagens";
import type { ViagemVanDetalhe } from "@/lib/api/types";
import { coverForId } from "@/lib/covers";
import { formatBrl, formatDatePt } from "@/lib/format";
import { buildSeatMap } from "@/lib/seats";

export function ReservaPageClient({ viagemVanId }: { viagemVanId: string }) {
  const [trip, setTrip] = useState<ViagemVanDetalhe | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const data = await obterDetalheViagemVan(viagemVanId);
        if (!cancelled) setTrip(data);
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : "Viagem não encontrada");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [viagemVanId]);

  if (loading) {
    return (
      <div className="min-h-screen bg-van-void">
        <Header />
        <div className="mx-auto max-w-5xl px-4 py-20">
          <div className="h-64 animate-pulse rounded-2xl bg-zinc-900" />
        </div>
      </div>
    );
  }

  if (error || !trip) {
    return (
      <div className="min-h-screen bg-van-void">
        <Header />
        <div className="mx-auto max-w-lg px-4 py-24 text-center">
          <p className="text-lg font-semibold text-white">{error ?? "Viagem não encontrada"}</p>
          <Link href="/" className="mt-6 inline-block text-van-amber hover:underline">
            Voltar ao início
          </Link>
        </div>
      </div>
    );
  }

  const seats = buildSeatMap(trip.capacidadePassageiros, trip.assentosOcupados);
  const cover = coverForId(trip.viagemId);

  return (
    <div className="relative min-h-screen bg-van-void">
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_70%_45%_at_50%_-5%,rgba(240,165,0,0.08),transparent_60%)]" />
      <Header />
      <main className="relative mx-auto max-w-6xl px-4 py-6 pb-32 sm:px-6 sm:py-8 sm:pb-36 lg:max-w-7xl">
        <Link href="/#viagens" className="text-sm font-medium text-zinc-500 transition hover:text-van-amber">
          ← Voltar às viagens
        </Link>
        <div className="mt-4">
          <ReservationHoldChip viagemVanId={trip.viagemVanId} />
        </div>
        <TripHeroPanel
          title={trip.nomeEvento}
          subtitle={`${trip.localPartida} → ${trip.localEvento}`}
          dateLabel={formatDatePt(trip.dataEvento)}
          coverImage={cover}
          priceLabel={`${formatBrl(trip.precoAssento)} por assento · ${trip.nomeVan}`}
        />
        <section className="mt-8" aria-labelledby="seat-map-heading">
          <h2 id="seat-map-heading" className="mb-4 text-center text-xl font-black text-white">
            Escolha seu assento
          </h2>
          <p className="mb-6 text-center text-sm text-zinc-500">
            {trip.modeloVan} · placa {trip.placaVan}
            {trip.possuiIngresso ? " · Ingresso do evento: combine com o frotista após confirmar a reserva" : ""}
          </p>
          <SeatBookingClient trip={trip} seats={seats} />
        </section>
      </main>
    </div>
  );
}
