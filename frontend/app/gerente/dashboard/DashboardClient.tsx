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
import { formatBrl, formatDateTime } from "@/lib/format";

function startOfDay(d: Date): Date {
  const x = new Date(d);
  x.setHours(0, 0, 0, 0);
  return x;
}

function isSameDay(a: Date, b: Date): boolean {
  return startOfDay(a).getTime() === startOfDay(b).getTime();
}

function startOfWeek(d: Date): Date {
  const x = startOfDay(d);
  const day = x.getDay();
  const diff = day === 0 ? -6 : 1 - day;
  x.setDate(x.getDate() + diff);
  return x;
}

function endOfWeek(d: Date): Date {
  const end = startOfWeek(d);
  end.setDate(end.getDate() + 7);
  return end;
}

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

  const hoje = new Date();
  const reservasHoje = viagens
    .filter((v) => isSameDay(new Date(v.dataPartida), hoje))
    .reduce((s, v) => s + (v.totalReservas ?? 0), 0);

  const weekStart = startOfWeek(hoje).getTime();
  const weekEnd = endOfWeek(hoje).getTime();
  const receitaSemana = viagens
    .filter((v) => {
      const t = new Date(v.dataPartida).getTime();
      return t >= weekStart && t < weekEnd;
    })
    .reduce((s, v) => s + (v.receita ?? 0), 0);

  const proxima = [...viagens]
    .filter((v) => v.status !== "Cancelada" && new Date(v.dataPartida).getTime() >= Date.now())
    .sort((a, b) => new Date(a.dataPartida).getTime() - new Date(b.dataPartida).getTime())[0];

  const proximaViagemEvento = proxima?.nomeEvento ?? "—";
  const proximaViagemData = proxima ? formatDateTime(proxima.dataPartida) : "Nenhuma agendada";

  const agora = Date.now();
  const viagensRecentes = [...viagens]
    .filter((v) => v.status !== "Cancelada")
    .sort((a, b) => new Date(a.dataPartida).getTime() - new Date(b.dataPartida).getTime())
    .filter((v) => new Date(v.dataPartida).getTime() >= agora - 24 * 60 * 60 * 1000)
    .slice(0, 5)
    .map(toResumo);

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
    reservasHoje,
    receitaSemana,
    proximaViagemEvento,
    proximaViagemData,
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

  const proximaViagemDisplay = useMemo(() => {
    if (!data || data.proximaViagemEvento === "—") return "—";
    if (data.proximaViagemEvento.length <= 22) return data.proximaViagemEvento;
    return `${data.proximaViagemEvento.slice(0, 19)}…`;
  }, [data]);

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <h1 className="font-display text-3xl text-zinc-100">Dashboard</h1>
            <p className="mt-1 text-sm text-zinc-400">
              Visão consolidada das suas viagens, reservas e receita.
            </p>
          </div>
          <Link
            href="/gerente/viagens/nova"
            className="inline-flex shrink-0 items-center justify-center gap-2 rounded-vb bg-van-amber px-5 py-3 text-sm font-semibold text-van-void shadow-[0_8px_28px_rgba(240,165,0,0.25)] transition hover:brightness-110 focus:outline-none focus-visible:ring-2 focus-visible:ring-van-amber focus-visible:ring-offset-2 focus-visible:ring-offset-van-void"
          >
            <span aria-hidden className="text-lg leading-none">
              +
            </span>
            Nova viagem
          </Link>
        </div>

        {loading && (
          <div className="space-y-8">
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <StatCardSkeleton />
              <StatCardSkeleton />
              <StatCardSkeleton />
              <StatCardSkeleton />
            </div>
            <div className="h-48 animate-pulse rounded-vb border border-van-border bg-van-surface/50" />
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
          <div className="flex flex-col items-center gap-5 rounded-vb border border-dashed border-van-border bg-van-surface/40 px-6 py-20 text-center">
            <p className="font-display text-3xl text-zinc-100">Nenhuma viagem ainda</p>
            <p className="max-w-md text-sm leading-relaxed text-zinc-400">
              Publique sua primeira viagem em poucos minutos e acompanhe reservas, ocupação e receita
              no Pix — tudo em um só lugar.
            </p>
            <Link
              href="/gerente/viagens/nova"
              className="inline-flex items-center justify-center gap-2 rounded-vb bg-van-amber px-6 py-3.5 text-sm font-semibold text-van-void shadow-[0_8px_28px_rgba(240,165,0,0.25)] transition hover:brightness-110"
            >
              <span aria-hidden className="text-lg leading-none">
                +
              </span>
              Criar primeira viagem
            </Link>
            <p className="text-xs text-zinc-500">
              Já tem vans cadastradas?{" "}
              <Link href="/gerente/vans" className="text-van-amber hover:underline">
                Gerenciar frota
              </Link>
            </p>
          </div>
        )}

        {!loading && !error && data && (
          <div className="space-y-8">
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <StatCard
                title="Reservas hoje"
                value={String(data.reservasHoje)}
                description="Viagens com partida hoje"
                href="/gerente/viagens"
              />
              <StatCard
                title="Receita semana"
                value={formatBrl(data.receitaSemana)}
                description="Viagens desta semana"
                href="/gerente/viagens"
              />
              <StatCard
                title="Próxima viagem"
                value={proximaViagemDisplay}
                description={data.proximaViagemData}
                href="/gerente/viagens"
              />
              <StatCard
                title="Ocupação"
                value={ocupacaoLabel}
                description="Média de assentos vendidos"
                href="/gerente/viagens"
              />
            </div>

            <ViagensRecentes viagens={data.viagensRecentes} />

            <div className="flex justify-end">
              <Link href="/gerente/viagens" className="text-sm font-medium text-van-amber hover:underline">
                Ver todas as viagens →
              </Link>
            </div>
          </div>
        )}
      </main>
    </GerenteGuard>
  );
}
