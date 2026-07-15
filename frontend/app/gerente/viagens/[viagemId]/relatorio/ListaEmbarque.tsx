"use client";

import type { PassageiroRelatorio } from "@/lib/api/types";

type ListaEmbarqueProps = {
  passageiros: PassageiroRelatorio[];
};

function pagamentoBadge(status: string | null) {
  if (status === "Confirmado") {
    return "bg-emerald-950/60 text-emerald-300 border-emerald-700/60";
  }
  if (status === "Pendente") {
    return "bg-yellow-950/60 text-yellow-400 border-yellow-700/60";
  }
  return "bg-zinc-800 text-zinc-500 border-zinc-700";
}

export function ListaEmbarque({ passageiros }: ListaEmbarqueProps) {
  if (passageiros.length === 0) {
    return (
      <div className="rounded-2xl border border-zinc-800 bg-zinc-950/50 px-6 py-12 text-center">
        <p className="text-sm text-zinc-500">Nenhum passageiro ainda</p>
      </div>
    );
  }

  const showVan = passageiros.some((p) => p.vanPlaca);

  return (
    <div className="overflow-x-auto rounded-2xl border border-zinc-800 bg-zinc-950/50">
      <table className="w-full min-w-[36rem]">
        <thead>
          <tr className="border-b border-zinc-800 text-left text-xs font-medium uppercase tracking-wider text-zinc-500">
            <th className="px-4 py-3">Assento</th>
            {showVan ? <th className="px-4 py-3">Van</th> : null}
            <th className="px-4 py-3">Passageiro</th>
            <th className="px-4 py-3">Pagamento</th>
            <th className="px-4 py-3">Contato</th>
          </tr>
        </thead>
        <tbody>
          {passageiros.map((p, idx) => {
            const vazio = !p.nomePassageiro;
            return (
              <tr
                key={`${p.vanPlaca ?? "van"}-${p.numeroAssento}-${idx}`}
                className="border-b border-zinc-800/80 last:border-0"
              >
                <td className="px-4 py-3 text-sm font-medium text-zinc-200">
                  {p.numeroAssento}
                </td>
                {showVan ? (
                  <td className="px-4 py-3 text-sm text-zinc-400">
                    {p.vanPlaca ?? "—"}
                  </td>
                ) : null}
                <td className="px-4 py-3 text-sm">
                  {vazio ? (
                    <span className="text-zinc-600">Disponível</span>
                  ) : (
                    <span className="text-zinc-100">{p.nomePassageiro}</span>
                  )}
                </td>
                <td className="px-4 py-3">
                  {p.statusPagamento ? (
                    <span
                      className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium ${pagamentoBadge(p.statusPagamento)}`}
                    >
                      {p.statusPagamento}
                    </span>
                  ) : (
                    <span className="text-xs text-zinc-600">—</span>
                  )}
                </td>
                <td className="px-4 py-3 text-sm text-zinc-400">
                  {p.telefonePassageiro ?? "—"}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
