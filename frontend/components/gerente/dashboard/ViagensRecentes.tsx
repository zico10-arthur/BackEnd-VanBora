"use client";

import Link from "next/link";
import type { ViagemResumo } from "@/lib/api/types";
import { formatDateTime } from "@/lib/format";

type ViagensRecentesProps = {
  viagens: ViagemResumo[];
};

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

export function ViagensRecentes({ viagens }: ViagensRecentesProps) {
  if (viagens.length === 0) return null;

  return (
    <section>
      <h2 className="mb-3 text-lg font-semibold text-zinc-100">Próximas viagens</h2>
      <div className="overflow-x-auto rounded-2xl border border-zinc-800 bg-zinc-950/50">
        <table className="w-full min-w-[40rem]">
          <thead>
            <tr className="border-b border-zinc-800 text-left text-xs font-medium uppercase tracking-wider text-zinc-500">
              <th className="px-4 py-3">Evento</th>
              <th className="px-4 py-3">Data partida</th>
              <th className="px-4 py-3">Van</th>
              <th className="px-4 py-3">Assentos</th>
              <th className="px-4 py-3">Status</th>
            </tr>
          </thead>
          <tbody>
            {viagens.map((v) => (
              <tr key={v.viagemId} className="border-b border-zinc-800 last:border-0">
                <td className="px-4 py-3">
                  <Link
                    href={`/gerente/viagens/${v.viagemId}/relatorio`}
                    className="font-medium text-zinc-100 hover:text-van-amber"
                  >
                    {v.nomeEvento}
                  </Link>
                </td>
                <td className="whitespace-nowrap px-4 py-3 text-sm text-zinc-300">
                  {formatDateTime(v.dataPartida)}
                </td>
                <td className="px-4 py-3 text-sm text-zinc-300">
                  {v.vanModelo !== "—" ? (
                    <>
                      <span>{v.vanModelo}</span>
                      {v.vanPlaca !== "—" ? (
                        <span className="text-zinc-500"> · {v.vanPlaca}</span>
                      ) : null}
                    </>
                  ) : (
                    <span className="text-zinc-600">Sem van</span>
                  )}
                </td>
                <td className="px-4 py-3 text-sm text-zinc-300">
                  {v.capacidade > 0 ? `${v.assentosVendidos}/${v.capacidade}` : "—"}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium ${statusBadgeClass(v.status)}`}
                  >
                    {v.status}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}
