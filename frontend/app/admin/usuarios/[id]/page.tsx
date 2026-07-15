import { Header } from "@/components/Header";
import { AdminGuard } from "@/components/admin/AdminGuard";
import { UsuarioDetalheClient } from "./UsuarioDetalheClient";

export const metadata = { title: "Histórico de reservas — Admin VanBora" };

export default async function AdminUsuarioDetalhePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  return (
    <AdminGuard>
      <Header />
      <main className="mx-auto max-w-4xl px-4 py-8 sm:px-6">
        <UsuarioDetalheClient usuarioId={id} />
      </main>
    </AdminGuard>
  );
}
