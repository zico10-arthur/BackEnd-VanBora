"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { obterHistoricoViagensGerente } from "@/lib/api/admin";
import type { ViagemGerenteHistoricoResponse } from "@/lib/api/types";
import { formatBrl, formatDatePt } from "@/lib/format";

const STATUS_STYLES: Record<string, string> = {
  Agendada: "bg-blue-950 text-blue-300",
  Concluida: "bg-zinc-800 text-zinc-400",
  Cancelada: "bg-zinc-800 text-zinc-500",
};

export function GerenteHistoricoClient({ gerenteId }: { gerenteId: string }) {
  const [viagens, setViagens] = useState<ViagemGerenteHistoricoResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const fetchViagens = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const data = await obterHistoricoViagensGerente(gerenteId);
      setViagens(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao carregar histórico.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [gerenteId]);

  useEffect(() => {
    fetchViagens();
  }, [fetchViagens]);

  const totalArrecadado = viagens.reduce((s, v) => s + v.totalArrecadado, 0);
  const totalTaxa = viagens.reduce((s, v) => s + v.taxaPlataforma, 0);

  return (
    <div>
      <Link href="/admin/gerentes" className="text-sm text-zinc-400 hover:text-van-amber hover:underline">
        ← Voltar para gerentes
      </Link>
      <h1 className="mt-2 mb-6 text-2xl font-bold text-zinc-100">Histórico de viagens</h1>

      {loading && (
        <div className="space-y-2">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-16 animate-pulse rounded-xl border border-zinc-800 bg-zinc-900/50" />
          ))}
        </div>
      )}

      {!loading && error && (
        <div className="flex flex-col items-center gap-4 py-16 text-center">
          <p className="text-sm text-red-400">{error}</p>
          <VbButton variant="secondary" onClick={fetchViagens}>
            Tentar novamente
          </VbButton>
        </div>
      )}

      {!loading && !error && viagens.length === 0 && (
        <div className="flex flex-col items-center gap-2 py-16 text-center">
          <p className="text-zinc-500">Este gerente ainda não realizou viagens.</p>
        </div>
      )}

      {!loading && !error && viagens.length > 0 && (
        <>
          <div className="mb-4 grid gap-4 sm:grid-cols-2">
            <div className="rounded-2xl border border-zinc-800 bg-zinc-900/50 p-5">
              <p className="text-xs font-medium uppercase tracking-wider text-zinc-500">Total arrecadado</p>
              <p className="mt-2 text-2xl font-bold text-van-amber">{formatBrl(totalArrecadado)}</p>
            </div>
            <div className="rounded-2xl border border-zinc-800 bg-zinc-900/50 p-5">
              <p className="text-xs font-medium uppercase tracking-wider text-zinc-500">Taxa de plataforma</p>
              <p className="mt-2 text-2xl font-bold text-zinc-100">{formatBrl(totalTaxa)}</p>
            </div>
          </div>

          <div className="overflow-hidden rounded-2xl border border-zinc-800">
            <table className="w-full text-sm">
              <thead className="bg-zinc-900/80 text-left text-xs uppercase tracking-wider text-zinc-500">
                <tr>
                  <th className="px-4 py-3">Evento</th>
                  <th className="px-4 py-3">Rota</th>
                  <th className="px-4 py-3">Partida</th>
                  <th className="px-4 py-3">Reservas</th>
                  <th className="px-4 py-3">Arrecadado</th>
                  <th className="px-4 py-3">Taxa</th>
                  <th className="px-4 py-3">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-800">
                {viagens.map((v) => (
                  <tr key={v.viagemId} className="bg-zinc-900/30 transition hover:bg-zinc-900/70">
                    <td className="px-4 py-3 font-medium text-zinc-100">{v.nomeEvento}</td>
                    <td className="px-4 py-3 text-zinc-400">
                      {v.origem} → {v.destino}
                    </td>
                    <td className="px-4 py-3 text-zinc-400">{formatDatePt(v.dataPartida)}</td>
                    <td className="px-4 py-3 text-zinc-400">{v.totalReservas}</td>
                    <td className="px-4 py-3 text-van-amber">{formatBrl(v.totalArrecadado)}</td>
                    <td className="px-4 py-3 text-zinc-400">{formatBrl(v.taxaPlataforma)}</td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                          STATUS_STYLES[v.statusViagem] ?? "bg-zinc-800 text-zinc-400"
                        }`}
                      >
                        {v.statusViagem}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
