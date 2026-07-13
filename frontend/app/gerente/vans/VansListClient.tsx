"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { listarVans, alternarStatusVan } from "@/lib/api/vans";
import type { VanResponse } from "@/lib/api/types";
import { VanCard } from "./components/VanCard";
import { ToastBanner } from "./components/ToastBanner";

const SUCCESS_MAP: Record<string, string> = {
  criada: "Van cadastrada com sucesso!",
  editada: "Van atualizada com sucesso!",
};

export function VansListClient({ sucesso }: { sucesso: string | null }) {
  const router = useRouter();
  const [vans, setVans] = useState<VanResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  // Show toast from URL success param, then clean URL
  useEffect(() => {
    if (sucesso && SUCCESS_MAP[sucesso]) {
      setToastMessage(SUCCESS_MAP[sucesso]);
      router.replace("/gerente/vans", { scroll: false });
    }
  }, [sucesso, router]);

  const fetchVans = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const data = await listarVans();
      setVans(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao carregar vans.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchVans();
  }, [fetchVans]);

  async function handleToggleStatus(id: string) {
    const updated = await alternarStatusVan(id);
    setVans((prev) => prev.map((v) => (v.id === id ? updated : v)));
    setToastMessage(updated.ativo ? "Van ativada com sucesso!" : "Van desativada com sucesso!");
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
          <VbButton variant="secondary" onClick={fetchVans}>
            Tentar novamente
          </VbButton>
        </div>
      </>
    );
  }

  // ── Empty ────────────────────────────────────────────────────────
  if (vans.length === 0) {
    return (
      <>
        {toastEl}
        <div className="flex flex-col items-center gap-4 py-16 text-center">
          <p className="text-zinc-500">Nenhuma van cadastrada</p>
          <VbButton onClick={() => router.push("/gerente/vans/nova")}>
            Cadastrar primeira van
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
        {vans.map((van) => (
          <VanCard
            key={van.id}
            van={van}
            onEdit={() => router.push(`/gerente/vans/${van.id}/editar`)}
            onToggleStatus={() => handleToggleStatus(van.id)}
          />
        ))}
      </div>
    </>
  );
}
