"use client";

import { useRouter } from "next/navigation";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { MotoristaForm } from "../components/MotoristaForm";
import { criarMotorista } from "@/lib/api/motoristas";
import type { CriarMotoristaRequest } from "@/lib/api/types";
import { Header } from "@/components/Header";
import { GerenteGuard } from "../components/GerenteGuard";

export default function NovoMotoristaPage() {
  const router = useRouter();

  async function handleSubmit(data: CriarMotoristaRequest) {
    await criarMotorista(data);
    router.push("/gerente/motoristas?sucesso=criado");
  }

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-lg px-4 py-8 sm:px-6">
        <h1 className="mb-6 text-2xl font-bold text-zinc-100">Novo motorista</h1>
        <MotoristaForm onSubmit={handleSubmit} submitLabel="Cadastrar motorista" />
        <div className="mt-4 text-center">
          <VbButton variant="ghost" onClick={() => router.push("/gerente/motoristas")}>
            Cancelar
          </VbButton>
        </div>
      </main>
    </GerenteGuard>
  );
}
