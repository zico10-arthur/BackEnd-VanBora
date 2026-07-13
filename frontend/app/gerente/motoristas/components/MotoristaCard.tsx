import { VbButton } from "@/components/vanbora/ui/VbButton";
import type { MotoristaResponse } from "@/lib/api/types";

type MotoristaCardProps = {
  motorista: MotoristaResponse;
  onEdit: () => void;
  onToggleStatus: () => void;
};

export function MotoristaCard({ motorista, onEdit, onToggleStatus }: MotoristaCardProps) {
  return (
    <div
      className={`group rounded-2xl border p-5 transition hover:bg-zinc-900/90 ${
        motorista.ativo
          ? "border-zinc-800 bg-zinc-900/70 hover:border-zinc-700"
          : "border-zinc-800/50 bg-zinc-900/30 opacity-70"
      }`}
    >
      <div className="mb-3 flex items-start justify-between">
        <div>
          <h3 className="text-base font-semibold text-zinc-100">{motorista.nome}</h3>
          <p className="mt-0.5 text-sm text-zinc-400">
            CNH: {motorista.cnh}
          </p>
        </div>
        <span
          className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
            motorista.ativo
              ? "bg-emerald-950 text-emerald-300"
              : "bg-zinc-800 text-zinc-500"
          }`}
        >
          {motorista.ativo ? "Ativo" : "Inativo"}
        </span>
      </div>

      <div className="flex flex-wrap items-center gap-2 text-sm text-zinc-400">
        <span className="rounded-md bg-zinc-800 px-2 py-0.5 text-xs font-mono text-zinc-300">
          CPF: {motorista.cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, "$1.$2.$3-$4")}
        </span>
        {motorista.telefone && (
          <span className="text-xs text-zinc-500">{motorista.telefone}</span>
        )}
      </div>

      <div className="mt-4 flex gap-2 border-t border-zinc-800 pt-4">
        <VbButton variant="secondary" onClick={onEdit} className="flex-1 text-xs">
          Editar
        </VbButton>
        {motorista.ativo ? (
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
