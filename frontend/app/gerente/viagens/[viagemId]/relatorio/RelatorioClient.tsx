"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Header } from "@/components/Header";
import { GerenteGuard } from "@/app/gerente/vans/components/GerenteGuard";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { ProgressBar } from "@/components/ui/ProgressBar";
import { ApiError } from "@/lib/api/http";
import { obterRelatorio } from "@/lib/api/viagens";
import type { RelatorioResponse } from "@/lib/api/types";
import { formatBrl, formatDatePt } from "@/lib/format";
import { ListaEmbarque } from "./ListaEmbarque";

function statusBadgeClass(status: string): string {
  switch (status) {
    case "Agendada":
      return "bg-emerald-950/60 text-emerald-300 border-emerald-700/60";
    case "EmAndamento":
      return "bg-blue-950/60 text-blue-300 border-blue-700/60";
    case "Concluida":
      return "bg-zinc-800 text-zinc-400 border-zinc-700";
    case "Cancelada":
      return "bg-red-950/60 text-red-300 border-red-700/60";
    default:
      return "bg-zinc-800 text-zinc-400 border-zinc-700";
  }
}

function RelatorioSkeleton() {
  return (
    <div className="animate-pulse space-y-6">
      <div className="h-8 w-2/3 rounded bg-zinc-800" />
      <div className="h-4 w-1/2 rounded bg-zinc-800" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="h-28 rounded-2xl border border-zinc-800 bg-zinc-900/50" />
        ))}
      </div>
      <div className="h-64 rounded-2xl border border-zinc-800 bg-zinc-900/50" />
    </div>
  );
}

export function RelatorioClient({ viagemId }: { viagemId: string }) {
  const router = useRouter();
  const [relatorio, setRelatorio] = useState<RelatorioResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [linkCopiado, setLinkCopiado] = useState(false);

  const fetchRelatorio = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const data = await obterRelatorio(viagemId);
      setRelatorio(data);
    } catch (err: unknown) {
      if (err instanceof ApiError && err.status === 403) {
        router.replace("/gerente/viagens");
        return;
      }
      const msg = err instanceof Error ? err.message : "Erro ao carregar relatório.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [viagemId, router]);

  useEffect(() => {
    fetchRelatorio();
  }, [fetchRelatorio]);

  async function handleCompartilhar() {
    if (!relatorio?.viagemVanId) return;
    const url = `${window.location.origin}/reserva/${relatorio.viagemVanId}`;
    try {
      await navigator.clipboard.writeText(url);
      setLinkCopiado(true);
      window.setTimeout(() => setLinkCopiado(false), 2000);
    } catch {
      setError("Não foi possível copiar o link.");
    }
  }

  const faltam = relatorio
    ? Math.max(0, relatorio.quorumMinimo - relatorio.assentosVendidos)
    : 0;

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <nav className="mb-4 text-sm text-zinc-500">
          <Link href="/gerente/viagens" className="hover:text-zinc-300">
            Viagens
          </Link>
          <span className="mx-2">›</span>
          <span className="text-zinc-300">{relatorio?.nomeEvento ?? "…"}</span>
          <span className="mx-2">›</span>
          <span className="text-zinc-400">Relatório</span>
        </nav>

        {loading && <RelatorioSkeleton />}

        {!loading && error && (
          <div className="flex flex-col items-center gap-4 py-16 text-center">
            <p className="text-sm text-red-400">{error}</p>
            <VbButton variant="secondary" onClick={fetchRelatorio}>
              Tentar novamente
            </VbButton>
          </div>
        )}

        {!loading && !error && relatorio && (
          <div className="space-y-8">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <h1 className="text-2xl font-bold text-zinc-100">{relatorio.nomeEvento}</h1>
                <p className="mt-1 text-sm text-zinc-400">
                  {formatDatePt(relatorio.dataEvento)} · {relatorio.origem} → {relatorio.destino}
                </p>
                <div className="mt-3 flex flex-wrap gap-2">
                  <span
                    className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium ${statusBadgeClass(relatorio.status)}`}
                  >
                    {relatorio.status}
                  </span>
                  {relatorio.breakEvenAtingido ? (
                    <span className="inline-flex items-center rounded-full border border-emerald-700/60 bg-emerald-950/60 px-2.5 py-0.5 text-xs font-medium text-emerald-300">
                      Viagem viável
                    </span>
                  ) : (
                    <span className="inline-flex items-center rounded-full border border-yellow-700/60 bg-yellow-950/60 px-2.5 py-0.5 text-xs font-medium text-yellow-400">
                      Faltam {faltam} assento{faltam === 1 ? "" : "s"}
                    </span>
                  )}
                </div>
              </div>

              <div className="flex flex-wrap gap-2">
                <VbButton
                  variant="secondary"
                  onClick={handleCompartilhar}
                  disabled={!relatorio.viagemVanId}
                >
                  {linkCopiado ? "Link copiado!" : "Compartilhar link"}
                </VbButton>
                <Link
                  href="/gerente/viagens"
                  className="inline-flex items-center justify-center rounded-xl border border-zinc-700 bg-zinc-900 px-4 py-2.5 text-sm font-medium text-zinc-200 transition hover:bg-zinc-800"
                >
                  ← Voltar
                </Link>
              </div>
            </div>

            {relatorio.status === "Cancelada" && (
              <div className="rounded-xl border border-red-800/50 bg-red-950/30 px-4 py-3 text-sm text-red-300">
                Esta viagem está cancelada. Os indicadores abaixo refletem o estado antes do cancelamento.
              </div>
            )}

            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <div className="rounded-2xl border border-zinc-800 bg-zinc-900/50 p-5">
                <p className="text-xs font-medium uppercase tracking-wider text-zinc-500">
                  Receita total
                </p>
                <p className="mt-2 text-2xl font-bold text-van-amber">
                  {formatBrl(relatorio.receitaTotal)}
                </p>
                <p className="mt-1 text-xs text-zinc-500">
                  Líquido: {formatBrl(relatorio.faturamentoLiquido)}
                </p>
              </div>

              <div className="rounded-2xl border border-zinc-800 bg-zinc-900/50 p-5">
                <p className="text-xs font-medium uppercase tracking-wider text-zinc-500">
                  Assentos
                </p>
                <p className="mt-2 text-2xl font-bold text-zinc-100">
                  {relatorio.assentosVendidos}{" "}
                  <span className="text-base font-normal text-zinc-500">
                    / {relatorio.capacidadeTotal} vendidos
                  </span>
                </p>
              </div>

              <div className="rounded-2xl border border-zinc-800 bg-zinc-900/50 p-5">
                <p className="mb-3 text-xs font-medium uppercase tracking-wider text-zinc-500">
                  Break-even
                </p>
                <ProgressBar
                  value={relatorio.assentosVendidos}
                  max={relatorio.quorumMinimo}
                  label="Progresso até o quórum"
                />
              </div>

              <div className="rounded-2xl border border-zinc-800 bg-zinc-900/50 p-5">
                <p className="text-xs font-medium uppercase tracking-wider text-zinc-500">
                  Valor por assento
                </p>
                <p className="mt-2 text-2xl font-bold text-zinc-100">
                  {formatBrl(relatorio.precoAssento)}
                </p>
              </div>
            </div>

            <section>
              <h2 className="mb-3 text-lg font-semibold text-zinc-100">Lista de embarque</h2>
              <ListaEmbarque passageiros={relatorio.passageiros} />
            </section>
          </div>
        )}
      </main>
    </GerenteGuard>
  );
}
