"use client";

import { VbButton } from "@/components/vanbora/ui/VbButton";
import type { ViagemGerenteResponse } from "@/lib/api/types";
import { formatDate, formatDateTime } from "@/lib/format";

type ViagemRowProps = {
  viagem: ViagemGerenteResponse;
  onView: () => void;
  onEdit: () => void;
  onCancel: () => void;
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

function alocacaoBadge(viagem: ViagemGerenteResponse): {
  label: string;
  className: string;
} {
  const hasVans = viagem.vans.length > 0;
  const allHaveMotorista = hasVans && viagem.vans.every((v) => v.motoristaNome);

  if (hasVans && allHaveMotorista) {
    return {
      label: "Completa",
      className:
        "bg-emerald-950/60 text-emerald-300 border-emerald-700/60",
    };
  }
  return {
    label: "Pendente",
    className:
      "bg-yellow-950/60 text-yellow-400 border-yellow-700/60",
  };
}

export function ViagemRow({ viagem, onView, onEdit, onCancel }: ViagemRowProps) {
  const isAgendada = viagem.status === "Agendada";

  const totalVendidos = viagem.vans.reduce((sum, v) => sum + v.assentosVendidos, 0);
  const totalCapacidade = viagem.vans.reduce((sum, v) => sum + v.capacidade, 0);

  const aloc = alocacaoBadge(viagem);

  return (
    <tr className="border-b border-zinc-800 transition hover:bg-zinc-900/50">
      {/* Evento */}
      <td className="px-4 py-4">
        <p className="font-medium text-zinc-100">{viagem.nomeEvento}</p>
        <p className="text-xs text-zinc-500">{formatDate(viagem.dataEvento)}</p>
      </td>

      {/* Data Partida */}
      <td className="whitespace-nowrap px-4 py-4 text-sm text-zinc-300">
        {formatDateTime(viagem.dataPartida)}
      </td>

      {/* Alocação — badge de completude (Spec 50) */}
      <td className="px-4 py-4">
        <span
          className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium ${aloc.className}`}
        >
          {aloc.label}
        </span>
      </td>

      {/* Assentos */}
      <td className="px-4 py-4 text-sm text-zinc-300">
        {totalCapacidade > 0 ? `${totalVendidos}/${totalCapacidade}` : "—"}
      </td>

      {/* Status */}
      <td className="px-4 py-4">
        <span
          className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium ${statusBadgeClass(viagem.status)}`}
        >
          {viagem.status}
        </span>
      </td>

      {/* Ações */}
      <td className="px-4 py-4">
        <div className="flex items-center gap-2">
          <VbButton variant="secondary" onClick={onView}>
            Ver
          </VbButton>
          <VbButton
            variant="secondary"
            onClick={onEdit}
            disabled={!isAgendada}
          >
            Editar
          </VbButton>
          <VbButton
            variant="secondary"
            onClick={onCancel}
            disabled={!isAgendada}
          >
            Cancelar
          </VbButton>
        </div>
      </td>
    </tr>
  );
}
