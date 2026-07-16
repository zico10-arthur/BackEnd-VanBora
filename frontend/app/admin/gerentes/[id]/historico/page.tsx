import { Header } from "@/components/Header";
import { AdminGuard } from "@/components/admin/AdminGuard";
import { GerenteHistoricoClient } from "./GerenteHistoricoClient";

export const metadata = { title: "Histórico de viagens — Admin VanBora" };

export default async function AdminGerenteHistoricoPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  return (
    <AdminGuard>
      <Header />
      <main className="mx-auto max-w-5xl px-4 py-8 sm:px-6">
        <GerenteHistoricoClient gerenteId={id} />
      </main>
    </AdminGuard>
  );
}
