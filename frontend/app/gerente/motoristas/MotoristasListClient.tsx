"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { listarMotoristas, alternarStatusMotorista } from "@/lib/api/motoristas";
import type { MotoristaResponse } from "@/lib/api/types";
import { MotoristaCard } from "./components/MotoristaCard";
import { ToastBanner } from "./components/ToastBanner";

const SUCCESS_MAP: Record<string, string> = {
  criado: "Motorista cadastrado com sucesso!",
  editado: "Motorista atualizado com sucesso!",
};

export function MotoristasListClient({ sucesso }: { sucesso: string | null }) {
  const router = useRouter();
  const [motoristas, setMotoristas] = useState<MotoristaResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  // Show toast from URL success param, then clean URL
  useEffect(() => {
    if (sucesso && SUCCESS_MAP[sucesso]) {
      setToastMessage(SUCCESS_MAP[sucesso]);
      router.replace("/gerente/motoristas", { scroll: false });
    }
  }, [sucesso, router]);

  const fetchMotoristas = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const data = await listarMotoristas();
      setMotoristas(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao carregar motoristas.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchMotoristas();
  }, [fetchMotoristas]);

  async function handleToggleStatus(id: string) {
    const updated = await alternarStatusMotorista(id);
    setMotoristas((prev) => prev.map((m) => (m.id === id ? updated : m)));
    setToastMessage(updated.ativo ? "Motorista ativado com sucesso!" : "Motorista desativado com sucesso!");
  }

  // ── Toast banner ─────────────────────────────────────────────────
  const toastEl = useMemo(
    () => <ToastBanner message={toastMessage} onDismiss={() => setToastMessage(null)} />,
    [toastMessage],
  );

  // ── Loading ──────────────────────────────────────────────────────
  if (loading) {
    return (
      <>
        {toastEl}
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <div
              key={i}
              className="animate-pulse rounded-2xl border border-zinc-800 bg-zinc-900/50 p-5"
            >
              <div className="mb-3 h-5 w-2/3 rounded bg-zinc-800" />
              <div className="mb-2 h-4 w-1/3 rounded bg-zinc-800" />
              <div className="mt-4 flex gap-2 border-t border-zinc-800 pt-4">
                <div className="h-9 flex-1 rounded-xl bg-zinc-800" />
                <div className="h-9 flex-1 rounded-xl bg-zinc-800" />
              </div>
            </div>
          ))}
        </div>
      </>
    );
  }

  // ── Error ────────────────────────────────────────────────────────
  if (error) {
    return (
      <>
        {toastEl}
        <div className="flex flex-col items-center gap-4 py-16 text-center">
          <p className="text-sm text-red-400">{error}</p>
          <VbButton variant="secondary" onClick={fetchMotoristas}>
            Tentar novamente
          </VbButton>
        </div>
      </>
    );
  }

  // ── Empty ────────────────────────────────────────────────────────
  if (motoristas.length === 0) {
    return (
      <>
        {toastEl}
        <div className="flex flex-col items-center gap-4 py-16 text-center">
          <p className="text-zinc-500">Nenhum motorista cadastrado</p>
          <VbButton onClick={() => router.push("/gerente/motoristas/novo")}>
            Cadastrar primeiro motorista
          </VbButton>
        </div>
      </>
    );
  }

  // ── List ─────────────────────────────────────────────────────────
  return (
    <>
      {toastEl}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {motoristas.map((motorista) => (
          <MotoristaCard
            key={motorista.id}
            motorista={motorista}
            onEdit={() => router.push(`/gerente/motoristas/${motorista.id}/editar`)}
            onToggleStatus={() => handleToggleStatus(motorista.id)}
          />
        ))}
      </div>
    </>
  );
}
