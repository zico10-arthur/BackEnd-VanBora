"use client";

import { useEffect, useState } from "react";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import type { VanResponse } from "@/lib/api/types";

type RemoverVanModalProps = {
  van: VanResponse | null;
  open: boolean;
  onClose: () => void;
  onConfirm: (id: string) => Promise<void>;
};

export function RemoverVanModal({ van, open, onClose, onConfirm }: RemoverVanModalProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (open) {
      setError("");
      setLoading(false);
    }
  }, [open]);

  if (!open || !van) return null;

  async function handleConfirm() {
    setLoading(true);
    setError("");
    try {
      await onConfirm(van!.id);
      onClose();
    } catch (err: unknown) {
      const msg =
        err instanceof Error ? err.message : "Erro ao remover. Tente novamente.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      {/* backdrop */}
      <div className="absolute inset-0 bg-black/70" onClick={onClose} />

      {/* modal */}
      <div className="relative w-full max-w-sm rounded-2xl border border-zinc-800 bg-zinc-950 p-6 shadow-2xl">
        <h3 className="text-lg font-semibold text-zinc-100">Remover van</h3>
        <p className="mt-2 text-sm text-zinc-400">
          Tem certeza que deseja remover <strong className="text-zinc-200">{van.nome}</strong>{" "}
          ({van.placa})? Esta van não poderá mais ser usada em novas viagens.
        </p>

        {error && (
          <p className="mt-3 rounded-lg border border-red-700/60 bg-red-950/40 px-3 py-2 text-sm text-red-300">
            {error}
          </p>
        )}

        <div className="mt-6 flex gap-3">
          <VbButton variant="secondary" onClick={onClose} className="flex-1">
            Cancelar
          </VbButton>
          <VbButton
            onClick={handleConfirm}
            disabled={loading}
            className="flex-1 bg-red-600 text-white shadow-none hover:bg-red-500"
          >
            {loading ? "Removendo…" : "Remover"}
          </VbButton>
        </div>
      </div>
    </div>
  );
}
