"use client";

import { useParams, useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { Header } from "@/components/Header";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { VanForm } from "../../components/VanForm";
import { atualizarVan, obterVan } from "@/lib/api/vans";
import type { CriarVanRequest, VanResponse } from "@/lib/api/types";
import { GerenteGuard } from "../../components/GerenteGuard";

export default function EditarVanPage() {
  const router = useRouter();
  const params = useParams<{ vanId: string }>();
  const vanId = params?.vanId;

  const [van, setVan] = useState<VanResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const fetchVan = useCallback(async () => {
    if (!vanId) return;
    setLoading(true);
    setError("");
    try {
      const data = await obterVan(vanId);
      setVan(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao carregar van.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [vanId]);

  useEffect(() => {
    fetchVan();
  }, [fetchVan]);

  async function handleSubmit(data: CriarVanRequest) {
    if (!vanId) return;
    const { nome, placa } = data;
    await atualizarVan(vanId, { nome, placa });
    router.push("/gerente/vans?sucesso=editada");
  }

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-lg px-4 py-8 sm:px-6">
        <h1 className="mb-6 text-2xl font-bold text-zinc-100">Editar van</h1>

        {loading && (
          <div className="animate-pulse space-y-5">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-12 rounded-xl bg-zinc-800" />
            ))}
          </div>
        )}

        {error && (
          <div className="flex flex-col items-center gap-4 py-8 text-center">
            <p className="text-sm text-red-400">{error}</p>
            <VbButton variant="secondary" onClick={fetchVan}>
              Tentar novamente
            </VbButton>
          </div>
        )}

        {!loading && !error && van && (
          <>
            <VanForm van={van} onSubmit={handleSubmit} submitLabel="Salvar alterações" />
            <div className="mt-4 text-center">
              <VbButton variant="ghost" onClick={() => router.push("/gerente/vans")}>
                Cancelar
              </VbButton>
            </div>
          </>
        )}
      </main>
    </GerenteGuard>
  );
}
