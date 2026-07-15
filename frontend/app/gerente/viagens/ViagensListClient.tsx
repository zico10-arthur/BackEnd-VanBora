"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { Header } from "@/components/Header";
import { GerenteGuard } from "@/app/gerente/vans/components/GerenteGuard";
import { listarViagensGerente } from "@/lib/api/viagens";
import type { ViagemGerenteResponse } from "@/lib/api/types";
import { ViagemRow } from "./components/ViagemRow";
import { ViagemRowSkeleton } from "./components/ViagemRowSkeleton";
import { CancelarViagemModal } from "./components/CancelarViagemModal";
import { ToastBanner } from "./components/ToastBanner";

const SUCCESS_MAP: Record<string, string> = {
  criada: "Viagem criada com sucesso!",
  editada: "Viagem atualizada com sucesso!",
};

export function ViagensListClient({ sucesso }: { sucesso: string | null }) {
  const router = useRouter();
  const [viagens, setViagens] = useState<ViagemGerenteResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [toastMessage, setToastMessage] = useState<string | null>(null);
  const [toastType, setToastType] = useState<"success" | "error">("success");
  const [cancelTarget, setCancelTarget] = useState<ViagemGerenteResponse | null>(null);

  // Show toast from URL success param, then clean URL
  useEffect(() => {
    if (sucesso) {
      if (SUCCESS_MAP[sucesso]) {
        setToastMessage(SUCCESS_MAP[sucesso]);
        setToastType("success");
      } else if (sucesso === "nao-editavel") {
        setToastMessage("Esta viagem não pode ser editada (status diferente de Agendada).");
        setToastType("error");
      } else if (sucesso === "redirecionado") {
        setToastMessage("Abrindo relatório da viagem…");
        setToastType("success");
      }
      router.replace("/gerente/viagens", { scroll: false });
    }
  }, [sucesso, router]);

  const fetchViagens = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const data = await listarViagensGerente();
      setViagens(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao carregar viagens.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchViagens();
  }, [fetchViagens]);

  function handleCancelled() {
    setCancelTarget(null);
    setToastMessage("Viagem cancelada com sucesso.");
    setToastType("success");
    fetchViagens();
  }

  // ── Toast banner ─────────────────────────────────────────────────
  const toastEl = useMemo(
    () => (
      <ToastBanner
        message={toastMessage}
        type={toastType}
        onDismiss={() => setToastMessage(null)}
      />
    ),
    [toastMessage, toastType],
  );

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <div className="mb-6 flex items-center justify-between">
          <h1 className="text-2xl font-bold text-zinc-100">Minhas Viagens</h1>
          <Link
            href="/gerente/viagens/nova"
            className="inline-flex items-center justify-center rounded-xl bg-van-amber px-5 py-3 text-sm font-semibold text-van-void shadow-[0_8px_28px_rgba(240,165,0,0.25)] transition hover:brightness-110"
          >
            Nova viagem
          </Link>
        </div>

        {/* Toast */}
        {toastEl}

        {/* Loading */}
        {loading && (
          <div className="overflow-x-auto rounded-2xl border border-zinc-800 bg-zinc-950/50">
            <table className="w-full">
              <thead>
                <tr className="border-b border-zinc-800 text-left text-xs font-medium uppercase tracking-wider text-zinc-500">
                  <th className="px-4 py-3">Evento</th>
                  <th className="px-4 py-3">Data Partida</th>
                  <th className="px-4 py-3">Alocação</th>
                  <th className="px-4 py-3">Assentos</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3">Ações</th>
                </tr>
              </thead>
              <tbody>
                <ViagemRowSkeleton />
                <ViagemRowSkeleton />
                <ViagemRowSkeleton />
              </tbody>
            </table>
          </div>
        )}

        {/* Error */}
        {!loading && error && (
          <div className="flex flex-col items-center gap-4 py-16 text-center">
            <p className="text-sm text-red-400">{error}</p>
            <VbButton variant="secondary" onClick={fetchViagens}>
              Tentar novamente
            </VbButton>
          </div>
        )}

        {/* Empty */}
        {!loading && !error && viagens.length === 0 && (
          <div className="flex flex-col items-center gap-4 py-16 text-center">
            <p className="text-zinc-500">Nenhuma viagem cadastrada</p>
            <VbButton onClick={() => router.push("/gerente/viagens/nova")}>
              Criar primeira viagem
            </VbButton>
          </div>
        )}

        {/* Data */}
        {!loading && !error && viagens.length > 0 && (
          <div className="overflow-x-auto rounded-2xl border border-zinc-800 bg-zinc-950/50">
            <table className="w-full">
              <thead>
                <tr className="border-b border-zinc-800 text-left text-xs font-medium uppercase tracking-wider text-zinc-500">
                  <th className="px-4 py-3">Evento</th>
                  <th className="px-4 py-3">Data Partida</th>
                  <th className="px-4 py-3">Alocação</th>
                  <th className="px-4 py-3">Assentos</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3">Ações</th>
                </tr>
              </thead>
              <tbody>
                {viagens.map((v) => (
                  <ViagemRow
                    key={v.viagemId}
                    viagem={v}
                    onView={() => router.push(`/gerente/viagens/${v.viagemId}/relatorio`)}
                    onEdit={() => router.push(`/gerente/viagens/${v.viagemId}/editar`)}
                    onCancel={() => setCancelTarget(v)}
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </main>

      {/* Cancel Modal */}
      {cancelTarget && (
        <CancelarViagemModal
          viagem={cancelTarget}
          open={!!cancelTarget}
          onClose={() => setCancelTarget(null)}
          onCancelled={handleCancelled}
        />
      )}
    </GerenteGuard>
  );
}
