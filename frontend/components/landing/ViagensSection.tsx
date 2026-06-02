"use client";

import { useCallback, useEffect, useState } from "react";
import { listarViagens } from "@/lib/api/viagens";
import type { ViagemPublica } from "@/lib/api/types";
import { ViagemCard } from "@/components/ViagemCard";
import { TripsEmptyState } from "@/components/TripsEmptyState";

export function ViagensSection() {
  const [viagens, setViagens] = useState<ViagemPublica[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setLoadError(false);
    try {
      const data = await listarViagens();
      setViagens(data);
    } catch {
      setViagens([]);
      setLoadError(true);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  if (loading) {
    return (
      <div className="grid gap-5 sm:grid-cols-2 sm:gap-6 lg:grid-cols-3" aria-busy="true" aria-label="Carregando viagens">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-[min(360px,70vw)] animate-pulse rounded-2xl bg-zinc-900/50" />
        ))}
      </div>
    );
  }

  if (loadError) {
    return (
      <div className="space-y-6">
        <p className="text-center text-sm text-zinc-400">
          Não foi possível carregar as viagens no momento. Verifique sua conexão e tente novamente.
        </p>
        <div className="flex justify-center">
          <button
            type="button"
            onClick={load}
            className="rounded-xl border border-zinc-600 px-6 py-3 text-sm font-semibold text-zinc-200 transition hover:border-zinc-400 hover:bg-white/5"
          >
            Tentar novamente
          </button>
        </div>
      </div>
    );
  }

  const cards = viagens.flatMap((v) => v.vans.map((van) => ({ viagem: v, van })));

  if (cards.length === 0) {
    return <TripsEmptyState />;
  }

  return (
    <div className="grid gap-5 sm:grid-cols-2 sm:gap-6 lg:grid-cols-3">
      {cards.map(({ viagem, van }, i) => (
        <ViagemCard key={van.viagemVanId} viagem={viagem} van={van} priority={i < 2} />
      ))}
    </div>
  );
}
