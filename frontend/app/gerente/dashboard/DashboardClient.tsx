"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { Header } from "@/components/Header";
import { GerenteGuard } from "@/app/gerente/vans/components/GerenteGuard";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { StatCard } from "@/components/gerente/dashboard/StatCard";
import { StatCardSkeleton } from "@/components/gerente/dashboard/StatCardSkeleton";
import { ViagensRecentes } from "@/components/gerente/dashboard/ViagensRecentes";
import { listarViagensGerente } from "@/lib/api/viagens";
import type { DashboardData, ViagemGerenteResponse, ViagemResumo } from "@/lib/api/types";
import { formatBrl } from "@/lib/format";

function toResumo(v: ViagemGerenteResponse): ViagemResumo {
  const primeira = v.vans[0];
  const assentosVendidos = v.vans.reduce((s, van) => s + van.assentosVendidos, 0);
  const capacidade = v.vans.reduce((s, van) => s + van.capacidade, 0);
  return {
    viagemId: v.viagemId,
    nomeEvento: v.nomeEvento,
    dataPartida: v.dataPartida,
    vanModelo: primeira?.vanModelo ?? "—",
    vanPlaca: primeira?.vanPlaca ?? "—",
    assentosVendidos,
    capacidade,
    status: v.status,
    receita: v.receita ?? 0,
    totalReservas: v.totalReservas ?? 0,
  };
}

function deriveDashboard(viagens: ViagemGerenteResponse[]): DashboardData {
  const viagensAtivas = viagens.filter((v) => v.status === "Agendada").length;
  const totalReservas = viagens.reduce((s, v) => s + (v.totalReservas ?? 0), 0);
  const receitaTotal = viagens.reduce((s, v) => s + (v.receita ?? 0), 0);

  const comCapacidade = viagens
    .map((v) => {
      const vendidos = v.vans.reduce((s, van) => s + van.assentosVendidos, 0);
      const cap = v.vans.reduce((s, van) => s + van.capacidade, 0);
      return { vendidos, cap };
    })
    .filter((x) => x.cap > 0);

  const ocupacaoMedia =
    comCapacidade.length === 0
      ? 0
      : comCapacidade.reduce((s, x) => s + x.vendidos / x.cap, 0) / comCapacidade.length;

  const agora = Date.now();
  const viagensRecentes = [...viagens]
    .filter((v) => v.status !== "Cancelada")
    .sort((a, b) => new Date(a.dataPartida).getTime() - new Date(b.dataPartida).getTime())
    .filter((v) => new Date(v.dataPartida).getTime() >= agora - 24 * 60 * 60 * 1000)
    .slice(0, 5)
    .map(toResumo);

  // Se não houver futuras, mostra as 5 mais próximas (incluindo passadas)
  const recentes =
    viagensRecentes.length > 0
      ? viagensRecentes
      : [...viagens]
          .sort((a, b) => new Date(a.dataPartida).getTime() - new Date(b.dataPartida).getTime())
          .slice(0, 5)
          .map(toResumo);

  return {
    viagensAtivas,
    totalReservas,
    ocupacaoMedia,
    receitaTotal,
    viagensRecentes: recentes,
  };
}

export function DashboardClient() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [empty, setEmpty] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const viagens = await listarViagensGerente();
      if (viagens.length === 0) {
        setEmpty(true);
        setData(null);
      } else {
        setEmpty(false);
        setData(deriveDashboard(viagens));
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao carregar dashboard.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const ocupacaoLabel = useMemo(() => {
    if (!data) return "0%";
    return `${Math.round(data.ocupacaoMedia * 100)}%`;
  }, [data]);

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-zinc-100">Dashboard</h1>
          <p className="mt-1 text-sm text-zinc-400">
            Visão consolidada das suas viagens, reservas e receita.
          </p>
        </div>

        {loading && (
          <div className="space-y-8">
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <StatCardSkeleton />
              <StatCardSkeleton />
              <StatCardSkeleton />
              <StatCardSkeleton />
            </div>
            <div className="h-48 animate-pulse rounded-2xl border border-zinc-800 bg-zinc-900/50" />
          </div>
        )}

        {!loading && error && (
          <div className="flex flex-col items-center gap-4 py-16 text-center">
            <p className="text-sm text-red-400">{error}</p>
            <VbButton variant="secondary" onClick={fetchData}>
              Tentar novamente
            </VbButton>
          </div>
        )}

        {!loading && !error && empty && (
          <div className="flex flex-col items-center gap-4 rounded-2xl border border-dashed border-zinc-800 bg-zinc-900/30 px-6 py-16 text-center">
            <p className="text-lg font-semibold text-zinc-100">Nenhuma viagem cadastrada</p>
            <p className="max-w-md text-sm text-zinc-400">
              Crie sua primeira viagem para começar a acompanhar reservas, ocupação e receita aqui.
            </p>
            <Link
              href="/gerente/viagens/nova"
              className="inline-flex items-center justify-center rounded-xl bg-van-amber px-5 py-3 text-sm font-semibold text-van-void shadow-[0_8px_28px_rgba(240,165,0,0.25)] transition hover:brightness-110"
            >
              Criar primeira viagem
            </Link>
          </div>
        )}

        {!loading && !error && data && (
          <div className="space-y-8">
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <StatCard
                title="Viagens ativas"
                value={String(data.viagensAtivas)}
                description="Status Agendada"
                href="/gerente/viagens"
              />
              <StatCard
                title="Total de reservas"
                value={String(data.totalReservas)}
                description="Reservas confirmadas"
                href="/gerente/viagens"
              />
              <StatCard
                title="Ocupação média"
                value={ocupacaoLabel}
                description="Assentos vendidos / capacidade"
                href="/gerente/viagens"
              />
              <StatCard
                title="Receita total"
                value={formatBrl(data.receitaTotal)}
                description="Soma das reservas confirmadas"
                href="/gerente/viagens"
              />
            </div>

            <ViagensRecentes viagens={data.viagensRecentes} />

            <div className="flex justify-end">
              <Link
                href="/gerente/viagens"
                className="text-sm font-medium text-van-amber hover:underline"
              >
                Ver todas as viagens →
              </Link>
            </div>
          </div>
        )}
      </main>
    </GerenteGuard>
  );
}
