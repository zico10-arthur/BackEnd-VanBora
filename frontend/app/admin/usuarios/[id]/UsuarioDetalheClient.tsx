"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { obterHistoricoReservasUsuario } from "@/lib/api/admin";
import type { ReservaHistoricoResponse } from "@/lib/api/types";
import { formatBrl, formatDatePt } from "@/lib/format";

const STATUS_STYLES: Record<string, string> = {
  Confirmada: "bg-emerald-950 text-emerald-300",
  PendentePagamento: "bg-amber-950 text-amber-300",
  Cancelada: "bg-zinc-800 text-zinc-500",
  Expirada: "bg-zinc-800 text-zinc-500",
  EmAndamento: "bg-blue-950 text-blue-300",
  Concluida: "bg-zinc-800 text-zinc-400",
};

export function UsuarioDetalheClient({ usuarioId }: { usuarioId: string }) {
  const [reservas, setReservas] = useState<ReservaHistoricoResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const fetchReservas = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const data = await obterHistoricoReservasUsuario(usuarioId);
      setReservas(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao carregar histórico.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [usuarioId]);

  useEffect(() => {
    fetchReservas();
  }, [fetchReservas]);

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <Link href="/admin/usuarios" className="text-sm text-zinc-400 hover:text-van-amber hover:underline">
            ← Voltar para usuários
          </Link>
          <h1 className="mt-2 text-2xl font-bold text-zinc-100">Histórico de reservas</h1>
        </div>
      </div>

      {loading && (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-24 animate-pulse rounded-2xl border border-zinc-800 bg-zinc-900/50" />
          ))}
        </div>
      )}

      {!loading && error && (
        <div className="flex flex-col items-center gap-4 py-16 text-center">
          <p className="text-sm text-red-400">{error}</p>
          <VbButton variant="secondary" onClick={fetchReservas}>
            Tentar novamente
          </VbButton>
        </div>
      )}

      {!loading && !error && reservas.length === 0 && (
        <div className="flex flex-col items-center gap-2 py-16 text-center">
          <p className="text-zinc-500">Este usuário ainda não fez nenhuma reserva.</p>
        </div>
      )}

      {!loading && !error && reservas.length > 0 && (
        <div className="space-y-4">
          {reservas.map((r) => (
            <div key={r.id} className="rounded-2xl border border-zinc-800 bg-zinc-900/50 p-5">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <h3 className="text-base font-semibold text-zinc-100">{r.viagem.nomeEvento}</h3>
                  <p className="mt-0.5 text-sm text-zinc-400">
                    {r.viagem.origem} → {r.viagem.destino} · {formatDatePt(r.viagem.dataPartida)}
                  </p>
                </div>
                <span
                  className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                    STATUS_STYLES[r.status] ?? "bg-zinc-800 text-zinc-400"
                  }`}
                >
                  {r.status}
                </span>
              </div>

              <div className="mt-3 flex flex-wrap items-center gap-4 text-sm text-zinc-400">
                <span>Total: <span className="font-semibold text-van-amber">{formatBrl(r.valorTotal)}</span></span>
                <span>Taxa plataforma: {formatBrl(r.taxaPlataforma)}</span>
                <span>Criada em {formatDatePt(r.criadaEm)}</span>
              </div>

              <div className="mt-3 border-t border-zinc-800 pt-3">
                <p className="mb-2 text-xs font-medium uppercase tracking-wider text-zinc-500">Passageiros</p>
                <ul className="space-y-1">
                  {r.itens.map((item, idx) => (
                    <li key={idx} className="flex flex-wrap justify-between gap-2 text-sm text-zinc-400">
                      <span>
                        Assento {item.assento} — {item.passageiroNome} ({item.passageiroDocumento})
                      </span>
                      <span className="text-zinc-500">{formatBrl(item.valor)}</span>
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
