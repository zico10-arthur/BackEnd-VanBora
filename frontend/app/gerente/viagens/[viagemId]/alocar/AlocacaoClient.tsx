"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import {
  obterViagemGerente,
  alocarVan,
  removerVanAlocada,
  alocarMotorista,
  removerMotoristaAlocado,
} from "@/lib/api/viagens";
import { listarVans } from "@/lib/api/vans";
import { listarMotoristas } from "@/lib/api/motoristas";
import type {
  ViagemGerenteResponse,
  VanResponse,
  MotoristaResponse,
} from "@/lib/api/types";

type ErrorType = "rede" | "404" | "403" | null;

type ToastState = {
  message: string;
  type: "success" | "error";
} | null;

export function AlocacaoClient({ viagemId }: { viagemId: string }) {
  const router = useRouter();

  const [viagem, setViagem] = useState<ViagemGerenteResponse | null>(null);
  const [vans, setVans] = useState<VanResponse[]>([]);
  const [motoristas, setMotoristas] = useState<MotoristaResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<ErrorType>(null);

  // Seleção para alocar nova van / motorista
  const [selectedVanId, setSelectedVanId] = useState("");
  const [selectedMotoristaId, setSelectedMotoristaId] = useState<
    Record<string, string>
  >({});

  // Modal de confirmação para remover van
  const [removingVanId, setRemovingVanId] = useState<string | null>(null);

  // Toast
  const [toast, setToast] = useState<ToastState>(null);

  const fetchAll = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [vData, mData, tData] = await Promise.all([
        listarVans(),
        listarMotoristas(),
        obterViagemGerente(viagemId),
      ]);
      setVans(vData);
      setMotoristas(mData);
      setViagem(tData);
    } catch (err: unknown) {
      if (err instanceof Error) {
        if (err.message.includes("404") || err.message.includes("não encontrad")) {
          setError("404");
        } else if (err.message.includes("403") || err.message.includes("Acesso negado")) {
          setError("403");
        } else {
          setError("rede");
        }
      } else {
        setError("rede");
      }
    } finally {
      setLoading(false);
    }
  }, [viagemId]);

  useEffect(() => {
    fetchAll();
  }, [fetchAll]);

  useEffect(() => {
    if (error === "403") {
      router.push("/entrar");
    }
  }, [error, router]);

  // ── Helpers ──
  // Re-fetch always provides fresh viagem.vans — compare by vanModelo + vanPlaca
  // since ViagemVanGerenteInfo does not carry the Van's original id.
  const allocatedVanKeys = new Set(
    viagem?.vans.map((vv) => `${vv.vanModelo}|${vv.vanPlaca}`) ?? [],
  );
  const vansDisponiveis = vans.filter(
    (v) => !allocatedVanKeys.has(`${v.modelo}|${v.placa}`),
  );

  // Motoristas ja alocados nesta viagem (por nome, ja que ViagemVanGerenteInfo.motoristaNome)
  const allocatedMotoristaNomes = new Set(
    viagem?.vans
      .filter((vv) => vv.motoristaNome)
      .map((vv) => vv.motoristaNome!) ?? [],
  );
  function availableMotoristasForVan(): MotoristaResponse[] {
    return motoristas.filter((m) => !allocatedMotoristaNomes.has(m.nome));
  }

  function hasAllMotoristas(): boolean {
    if (!viagem || viagem.vans.length === 0) return false;
    return viagem.vans.every((vv) => vv.motoristaNome);
  }

  function getBanner() {
    if (!viagem || viagem.vans.length === 0) {
      return {
        text: "Aloque pelo menos uma van",
        color: "bg-blue-950/40 border-blue-700/60 text-blue-300",
      };
    }
    if (hasAllMotoristas()) {
      return {
        text: "Viagem pronta para divulgação",
        color: "bg-green-950/40 border-green-700/60 text-green-300",
      };
    }
    return {
      text: "Aloque motoristas para completar",
      color: "bg-yellow-950/40 border-yellow-700/60 text-yellow-300",
    };
  }

  // ── Actions ──
  async function handleAlocarVan() {
    if (!selectedVanId) return;
    try {
      await alocarVan(viagemId, selectedVanId);
      setSelectedVanId("");
      setToast({ message: "Van alocada com sucesso", type: "success" });
      await fetchAll();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao alocar van";
      setToast({ message: msg, type: "error" });
    }
  }

  async function handleRemoverVan(viagemVanId: string) {
    setRemovingVanId(null);
    try {
      await removerVanAlocada(viagemId, viagemVanId);
      setToast({ message: "Van removida com sucesso", type: "success" });
      await fetchAll();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao remover van";
      setToast({ message: msg, type: "error" });
    }
  }

  async function handleAlocarMotorista(
    viagemVanId: string,
    motoristaId: string,
  ) {
    if (!motoristaId) return;
    try {
      await alocarMotorista(viagemId, viagemVanId, {
        motoristaId,
        viagemVanId,
      });
      setSelectedMotoristaId((prev) => ({ ...prev, [viagemVanId]: "" }));
      setToast({ message: "Motorista alocado com sucesso", type: "success" });
      await fetchAll();
    } catch (err: unknown) {
      const msg =
        err instanceof Error ? err.message : "Erro ao alocar motorista";
      setToast({ message: msg, type: "error" });
    }
  }

  async function handleRemoverMotorista(viagemVanId: string) {
    try {
      await removerMotoristaAlocado(viagemId, viagemVanId);
      setToast({ message: "Motorista removido com sucesso", type: "success" });
      await fetchAll();
    } catch (err: unknown) {
      const msg =
        err instanceof Error ? err.message : "Erro ao remover motorista";
      setToast({ message: msg, type: "error" });
    }
  }

  // ── Render states ──
  const banner = getBanner();

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="h-10 w-64 animate-pulse rounded-lg bg-zinc-800" />
        <div className="h-32 animate-pulse rounded-xl bg-zinc-800" />
        <div className="h-32 animate-pulse rounded-xl bg-zinc-800" />
      </div>
    );
  }

  if (error === "404") {
    return (
      <div className="flex flex-col items-center gap-4 py-16 text-center">
        <p className="text-zinc-500">Viagem não encontrada</p>
        <VbButton variant="secondary" onClick={() => router.push("/gerente/viagens")}>
          Voltar para lista
        </VbButton>
      </div>
    );
  }

  if (error === "rede") {
    return (
      <div className="flex flex-col items-center gap-4 py-16 text-center">
        <p className="text-red-400">
          Não foi possível carregar. Tentar novamente.
        </p>
        <VbButton variant="secondary" onClick={fetchAll}>
          Tentar novamente
        </VbButton>
      </div>
    );
  }

  if (!viagem) return null;

  return (
    <div className="space-y-6">
      {/* Toast */}
      {toast && (
        <div
          className={`rounded-lg border px-4 py-3 text-sm ${
            toast.type === "success"
              ? "border-green-700/60 bg-green-950/40 text-green-300"
              : "border-red-700/60 bg-red-950/40 text-red-300"
          }`}
        >
          <div className="flex items-center justify-between">
            <span>{toast.message}</span>
            <button
              onClick={() => setToast(null)}
              className="ml-3 text-zinc-400 hover:text-zinc-200"
            >
              ✕
            </button>
          </div>
        </div>
      )}

      {/* Banner de status */}
      <div className={`rounded-lg border px-4 py-3 text-sm ${banner.color}`}>
        {banner.text}
      </div>

      {/* Cabeçalho da viagem */}
      <div className="rounded-xl border border-zinc-800 bg-zinc-950/50 p-4">
        <h2 className="text-lg font-semibold text-zinc-100">
          {viagem.nomeEvento}
        </h2>
        <p className="text-sm text-zinc-400">
          {new Date(viagem.dataPartida).toLocaleString("pt-BR")} —{" "}
          {viagem.localPartida}
        </p>
      </div>

      {/* ── Seção: Vans alocadas ── */}
      <div>
        <h3 className="mb-3 text-sm font-semibold uppercase tracking-wider text-zinc-400">
          Vans alocadas
        </h3>

        {viagem.vans.length === 0 && (
          <p className="text-sm text-zinc-500">Nenhuma van alocada.</p>
        )}

        <div className="space-y-3">
          {viagem.vans.map((vv) => (
            <div
              key={vv.viagemVanId}
              className="rounded-xl border border-zinc-800 bg-zinc-950/50 p-4"
            >
              <div className="flex items-start justify-between">
                <div>
                  <p className="font-medium text-zinc-100">
                    {vv.vanModelo} — {vv.vanPlaca}
                  </p>
                  <p className="text-xs text-zinc-500">
                    Capacidade: {vv.capacidade} lugares
                  </p>
                  <p className="text-xs text-zinc-500">
                    Motorista:{" "}
                    <span
                      className={
                        vv.motoristaNome
                          ? "text-zinc-300"
                          : "italic text-zinc-600"
                      }
                    >
                      {vv.motoristaNome || "Nenhum motorista"}
                    </span>
                  </p>
                </div>
                <VbButton
                  variant="ghost"

                  onClick={() => setRemovingVanId(vv.viagemVanId)}
                >
                  Remover van
                </VbButton>
              </div>

              {/* Alocar/Remover motorista para esta van */}
              <div className="mt-3 border-t border-zinc-800 pt-3">
                {vv.motoristaNome ? (
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-zinc-300">
                      {vv.motoristaNome}
                    </span>
                    <VbButton
                      variant="ghost"
    
                      onClick={() => handleRemoverMotorista(vv.viagemVanId)}
                    >
                      Remover motorista
                    </VbButton>
                  </div>
                ) : (
                  <div className="flex items-center gap-2">
                    <select
                      value={selectedMotoristaId[vv.viagemVanId] ?? ""}
                      onChange={(e) =>
                        setSelectedMotoristaId((prev) => ({
                          ...prev,
                          [vv.viagemVanId]: e.target.value,
                        }))
                      }
                      className="flex-1 rounded-xl border border-zinc-700 bg-zinc-900 px-3 py-2 text-sm text-zinc-100 placeholder:text-zinc-500 focus:border-van-amber focus:outline-none focus:ring-1 focus:ring-van-amber"
                    >
                      <option value="">Selecione um motorista…</option>
                      {availableMotoristasForVan().map((m) => (
                        <option key={m.id} value={m.id}>
                          {m.nome} — CNH: {m.cnh}
                        </option>
                      ))}
                    </select>
                    <VbButton
    
                      disabled={
                        !selectedMotoristaId[vv.viagemVanId]
                      }
                      onClick={() =>
                        handleAlocarMotorista(
                          vv.viagemVanId,
                          selectedMotoristaId[vv.viagemVanId] ?? "",
                        )
                      }
                    >
                      Alocar motorista
                    </VbButton>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>

        {/* Alocar nova van */}
        {vansDisponiveis.length > 0 && (
          <div className="mt-4 flex items-center gap-2">
            <select
              value={selectedVanId}
              onChange={(e) => setSelectedVanId(e.target.value)}
              className="flex-1 rounded-xl border border-zinc-700 bg-zinc-900 px-3 py-2 text-sm text-zinc-100 placeholder:text-zinc-500 focus:border-van-amber focus:outline-none focus:ring-1 focus:ring-van-amber"
            >
              <option value="">Selecione uma van…</option>
              {vansDisponiveis.map((v) => (
                <option key={v.id} value={v.id}>
                  {v.nome} — {v.modelo} ({v.placa}) — {v.capacidade} lugares
                </option>
              ))}
            </select>
            <VbButton disabled={!selectedVanId} onClick={handleAlocarVan}>
              Alocar van
            </VbButton>
          </div>
        )}
      </div>

      {/* Modal de confirmação para remover van */}
      {removingVanId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
          <div className="mx-4 w-full max-w-sm rounded-2xl border border-zinc-800 bg-zinc-950 p-6 shadow-2xl">
            <p className="mb-4 text-sm text-zinc-200">
              Remover esta van da viagem?
            </p>
            <div className="flex justify-end gap-3">
              <VbButton
                variant="ghost"
                onClick={() => setRemovingVanId(null)}
              >
                Cancelar
              </VbButton>
              <VbButton onClick={() => handleRemoverVan(removingVanId)}>
                Remover
              </VbButton>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
