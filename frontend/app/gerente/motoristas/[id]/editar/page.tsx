"use client";

import { useParams, useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { Header } from "@/components/Header";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { MotoristaForm } from "../../components/MotoristaForm";
import { atualizarMotorista, obterMotorista } from "@/lib/api/motoristas";
import type { CriarMotoristaRequest, MotoristaResponse } from "@/lib/api/types";
import { GerenteGuard } from "../../components/GerenteGuard";

export default function EditarMotoristaPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const motoristaId = params?.id;

  const [motorista, setMotorista] = useState<MotoristaResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const fetchMotorista = useCallback(async () => {
    if (!motoristaId) return;
    setLoading(true);
    setError("");
    try {
      const data = await obterMotorista(motoristaId);
      setMotorista(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao carregar motorista.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [motoristaId]);

  useEffect(() => {
    fetchMotorista();
  }, [fetchMotorista]);

  async function handleSubmit(data: CriarMotoristaRequest) {
    if (!motoristaId) return;
    await atualizarMotorista(motoristaId, data);
    router.push("/gerente/motoristas?sucesso=editado");
  }

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-lg px-4 py-8 sm:px-6">
        <h1 className="mb-6 text-2xl font-bold text-zinc-100">Editar motorista</h1>

        {loading && (
          <div className="animate-pulse space-y-5">
            {[1, 2, 3, 4].map((i) => (
              <div key={i} className="h-12 rounded-xl bg-zinc-800" />
            ))}
          </div>
        )}

        {error && (
          <div className="flex flex-col items-center gap-4 py-8 text-center">
            <p className="text-sm text-red-400">{error}</p>
            <VbButton variant="secondary" onClick={fetchMotorista}>
              Tentar novamente
            </VbButton>
          </div>
        )}

        {!loading && !error && motorista && (
          <>
            <MotoristaForm motorista={motorista} onSubmit={handleSubmit} submitLabel="Salvar alterações" />
            <div className="mt-4 text-center">
              <VbButton variant="ghost" onClick={() => router.push("/gerente/motoristas")}>
                Cancelar
              </VbButton>
            </div>
          </>
        )}
      </main>
    </GerenteGuard>
  );
}
