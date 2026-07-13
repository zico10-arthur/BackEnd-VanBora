"use client";

import { useRouter } from "next/navigation";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { VanForm } from "../components/VanForm";
import { criarVan } from "@/lib/api/vans";
import type { CriarVanRequest } from "@/lib/api/types";
import { Header } from "@/components/Header";
import { GerenteGuard } from "../components/GerenteGuard";

export default function NovaVanPage() {
  const router = useRouter();

  async function handleSubmit(data: CriarVanRequest) {
    await criarVan(data);
    router.push("/gerente/vans?sucesso=criada");
  }

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-lg px-4 py-8 sm:px-6">
        <h1 className="mb-6 text-2xl font-bold text-zinc-100">Nova van</h1>
        <VanForm onSubmit={handleSubmit} submitLabel="Cadastrar van" />
        <div className="mt-4 text-center">
          <VbButton variant="ghost" onClick={() => router.push("/gerente/vans")}>
            Cancelar
          </VbButton>
        </div>
      </main>
    </GerenteGuard>
  );
}
