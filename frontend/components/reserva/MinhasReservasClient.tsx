"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { Header } from "@/components/Header";
import { listarMinhasReservas } from "@/lib/api/reservas";
import type { ReservaResponse } from "@/lib/api/types";
import { formatBrl, formatDatePt } from "@/lib/format";
import { seatNumberToLabel } from "@/lib/seats";
import { useAuth } from "@/components/providers/AuthProvider";
import { labelReservaStatus } from "@/lib/copy";

export function MinhasReservasClient() {
  const { user, ready } = useAuth();
  const [reservas, setReservas] = useState<ReservaResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!ready) return;
    if (!user) {
      setLoading(false);
      return;
    }
    (async () => {
      try {
        const data = await listarMinhasReservas();
        setReservas(data);
      } catch (e) {
        setError(e instanceof Error ? e.message : "Erro ao carregar");
      } finally {
        setLoading(false);
      }
    })();
  }, [ready, user]);

  if (!ready) return null;

  if (!user) {
    return (
      <div className="min-h-screen bg-van-void">
        <Header />
        <div className="mx-auto max-w-lg px-4 py-24 text-center">
          <p className="text-zinc-400">Faça login para ver suas reservas.</p>
          <Link href="/entrar?next=/minhas-reservas" className="mt-4 inline-block text-van-amber font-semibold">
            Entrar
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-van-void">
      <Header />
      <main className="mx-auto max-w-3xl px-4 py-8 pb-12 sm:px-6 sm:py-12">
        <h1 className="text-2xl font-black text-white">Minhas reservas</h1>
        {loading ? (
          <div className="mt-8 space-y-4">
            {[1, 2].map((i) => (
              <div key={i} className="h-24 animate-pulse rounded-xl bg-zinc-900" />
            ))}
          </div>
        ) : error ? (
          <p className="mt-6 text-red-300">{error}</p>
        ) : reservas.length === 0 ? (
          <div className="mt-8 rounded-2xl border border-dashed border-zinc-800 bg-zinc-900/30 px-6 py-10 text-center">
            <p className="font-medium text-zinc-300">Nenhuma reserva ainda</p>
            <p className="mt-2 text-sm text-zinc-500">Quando você reservar um assento, ele aparecerá aqui.</p>
            <Link href="/#viagens" className="mt-6 inline-block text-sm font-semibold text-van-amber hover:underline">
              Ver viagens disponíveis
            </Link>
          </div>
        ) : (
          <ul className="mt-8 space-y-4">
            {reservas.map((r) => (
              <li key={r.id} className="rounded-xl border border-zinc-800 bg-zinc-900/50 p-5">
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <p className="font-semibold text-white">Reserva #{r.id.slice(0, 8)}</p>
                    <p className="mt-1 text-sm text-zinc-400">
                      Assentos: {r.itens.map((i) => seatNumberToLabel(i.numeroAssento)).join(", ")}
                    </p>
                    <p className="mt-1 text-xs text-zinc-500">Criada {formatDatePt(r.criadoEm)}</p>
                  </div>
                  <span
                    className={`rounded-full px-3 py-1 text-xs font-bold uppercase ${
                      r.status === "Confirmada"
                        ? "bg-emerald-500/15 text-emerald-400"
                        : r.status === "PendentePagamento"
                          ? "bg-amber-500/15 text-amber-400"
                          : "bg-zinc-700 text-zinc-300"
                    }`}
                  >
                    {labelReservaStatus(r.status)}
                  </span>
                </div>
                <p className="mt-3 text-sm font-semibold text-van-amber">{formatBrl(r.valorAPagar)}</p>
                {r.status === "PendentePagamento" ? (
                  <Link
                    href={`/reserva/pagar/${r.id}`}
                    className="mt-3 inline-block text-sm font-medium text-van-amber underline"
                  >
                    Continuar pagamento
                  </Link>
                ) : null}
              </li>
            ))}
          </ul>
        )}
      </main>
    </div>
  );
}
