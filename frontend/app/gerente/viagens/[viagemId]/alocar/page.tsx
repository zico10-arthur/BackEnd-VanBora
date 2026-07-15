import { Header } from "@/components/Header";
import { GerenteGuard } from "@/app/gerente/vans/components/GerenteGuard";
import { AlocacaoClient } from "./AlocacaoClient";

export const metadata = {
  title: "Gerenciar alocações — VanBora",
};

export default async function AlocarPage({
  params,
}: {
  params: Promise<{ viagemId: string }>;
}) {
  const { viagemId } = await params;

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-3xl px-4 py-8 sm:px-6">
        <div className="mb-6">
          <p className="text-xs font-medium uppercase tracking-wider text-zinc-500">
            Viagens / Gerenciar alocações
          </p>
          <h1 className="mt-1 text-2xl font-bold text-zinc-100">
            Alocar Recursos
          </h1>
        </div>
        <AlocacaoClient viagemId={viagemId} />
      </main>
    </GerenteGuard>
  );
}
