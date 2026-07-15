"use client";

import { useRouter } from "next/navigation";
import { use } from "react";
import { useEffect, useState } from "react";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { Header } from "@/components/Header";
import { GerenteGuard } from "@/app/gerente/vans/components/GerenteGuard";
import { ViagemForm } from "../../components/ViagemForm";
import { obterViagemGerente, atualizarViagemGerente } from "@/lib/api/viagens";
import type { CriarViagemRequest, AtualizarViagemRequest, ViagemGerenteResponse } from "@/lib/api/types";

export default function EditarViagemPage({
  params,
}: {
  params: Promise<{ viagemId: string }>;
}) {
  const router = useRouter();
  const { viagemId } = use(params);
  const [viagem, setViagem] = useState<ViagemGerenteResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function load() {
      try {
        const data = await obterViagemGerente(viagemId);
        if (data.status !== "Agendada") {
          router.replace("/gerente/viagens?sucesso=nao-editavel");
          return;
        }
        setViagem(data);
      } catch (err: unknown) {
        const msg = err instanceof Error ? err.message : "Erro ao carregar viagem.";
        setError(msg);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, [viagemId, router]);

  async function handleSubmit(data: CriarViagemRequest) {
    const body: AtualizarViagemRequest = {
      nomeEvento: data.nomeEvento,
      dataEvento: data.dataEvento,
      localEvento: data.localEvento,
      dataPartida: data.dataPartida,
      localPartida: data.localPartida,
      possuiIngresso: data.possuiIngresso,
    };
    await atualizarViagemGerente(viagemId, body);
    router.push("/gerente/viagens?sucesso=editada");
  }

  if (loading) {
    return (
      <GerenteGuard>
        <Header />
        <main className="mx-auto max-w-lg px-4 py-8 sm:px-6">
          <div className="flex min-h-[50vh] items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-2 border-van-amber border-t-transparent" />
          </div>
        </main>
      </GerenteGuard>
    );
  }

  if (error || !viagem) {
    return (
      <GerenteGuard>
        <Header />
        <main className="mx-auto max-w-lg px-4 py-8 sm:px-6">
          <div className="flex flex-col items-center gap-4 py-16 text-center">
            <p className="text-sm text-red-400">{error || "Viagem não encontrada."}</p>
            <VbButton variant="secondary" onClick={() => router.push("/gerente/viagens")}>
              Voltar para lista
            </VbButton>
          </div>
        </main>
      </GerenteGuard>
    );
  }

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-lg px-4 py-8 sm:px-6">
        <h1 className="mb-6 text-2xl font-bold text-zinc-100">Editar viagem</h1>
        <ViagemForm
          viagem={viagem}
          onSubmit={handleSubmit}
          submitLabel="Salvar alterações"
        />
        <div className="mt-4 text-center space-y-3">
          <VbButton variant="ghost" onClick={() => router.push("/gerente/viagens")}>
            Cancelar
          </VbButton>
          <div>
            <VbButton
              variant="secondary"
              onClick={() => router.push(`/gerente/viagens/${viagemId}/alocar`)}
            >
              Gerenciar alocações
            </VbButton>
          </div>
        </div>
      </main>
    </GerenteGuard>
  );
}
