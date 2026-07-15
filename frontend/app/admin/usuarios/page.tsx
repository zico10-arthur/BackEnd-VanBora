import { Header } from "@/components/Header";
import { AdminGuard } from "@/components/admin/AdminGuard";
import { UsuariosListClient } from "./UsuariosListClient";

export const metadata = { title: "Usuários — Admin VanBora" };

export default function AdminUsuariosPage() {
  return (
    <AdminGuard>
      <Header />
      <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-zinc-100">Usuários</h1>
          <p className="mt-1 text-sm text-zinc-400">
            Busque por nome, CPF ou email para encontrar um usuário e ver seu histórico de reservas.
          </p>
        </div>
        <UsuariosListClient />
      </main>
    </AdminGuard>
  );
}
