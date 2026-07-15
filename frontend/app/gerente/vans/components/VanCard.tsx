import { VbButton } from "@/components/vanbora/ui/VbButton";
import type { VanResponse } from "@/lib/api/types";

type VanCardProps = {
  van: VanResponse;
  onEdit: () => void;
  onToggleStatus: () => void;
};

export function VanCard({ van, onEdit, onToggleStatus }: VanCardProps) {
  return (
    <div
      className={`group rounded-2xl border p-5 transition hover:bg-zinc-900/90 ${
        van.ativo
          ? "border-zinc-800 bg-zinc-900/70 hover:border-zinc-700"
          : "border-zinc-800/50 bg-zinc-900/30 opacity-70"
      }`}
    >
      <div className="mb-3 flex items-start justify-between">
        <div>
          <h3 className="text-base font-semibold text-zinc-100">{van.nome}</h3>
          <p className="mt-0.5 text-sm text-zinc-400">{van.modelo}</p>
        </div>
        <span
          className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
            van.ativo
              ? "bg-emerald-950 text-emerald-300"
              : "bg-zinc-800 text-zinc-500"
          }`}
        >
          {van.ativo ? "Ativa" : "Inativa"}
        </span>
      </div>

      <div className="flex items-center gap-4 text-sm text-zinc-400">
        <span className="rounded-md bg-zinc-800 px-2 py-0.5 text-xs font-mono text-zinc-300">
          {van.placa}
        </span>
        <span>{van.capacidade} lugares</span>
      </div>

      <div className="mt-4 flex gap-2 border-t border-zinc-800 pt-4">
        <VbButton variant="secondary" onClick={onEdit} className="flex-1 text-xs">
          Editar
        </VbButton>
        {van.ativo ? (
          <VbButton
            variant="ghost"
            onClick={onToggleStatus}
            className="flex-1 text-xs text-red-400 hover:text-red-300"
          >
            Desativar
          </VbButton>
        ) : (
          <VbButton
            variant="secondary"
            onClick={onToggleStatus}
            className="flex-1 text-xs text-emerald-400 hover:text-emerald-300"
          >
            Ativar
          </VbButton>
        )}
      </div>
    </div>
  );
}
