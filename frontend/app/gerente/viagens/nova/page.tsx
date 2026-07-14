"use client";

import { useRouter } from "next/navigation";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { Header } from "@/components/Header";
import { GerenteGuard } from "@/app/gerente/vans/components/GerenteGuard";
import { ViagemForm } from "../components/ViagemForm";
import { criarViagemGerente } from "@/lib/api/viagens";
import type { CriarViagemRequest } from "@/lib/api/types";

export default function NovaViagemPage() {
  const router = useRouter();

  async function handleSubmit(data: CriarViagemRequest) {
    await criarViagemGerente(data);
    router.push("/gerente/viagens?sucesso=criada");
  }

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-lg px-4 py-8 sm:px-6">
        <h1 className="mb-6 text-2xl font-bold text-zinc-100">Nova viagem</h1>
        <ViagemForm
          onSubmit={handleSubmit}
          submitLabel="Criar viagem"
        />
        <div className="mt-4 text-center">
          <VbButton variant="ghost" onClick={() => router.push("/gerente/viagens")}>
            Cancelar
          </VbButton>
        </div>
      </main>
    </GerenteGuard>
  );
}
