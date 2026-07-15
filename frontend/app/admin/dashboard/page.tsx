import Link from "next/link";
import { Header } from "@/components/Header";
import { AdminGuard } from "@/components/admin/AdminGuard";

export const metadata = { title: "Painel Admin — VanBora" };

export default function AdminDashboardPage() {
  return (
    <AdminGuard>
      <Header />
      <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-zinc-100">Painel administrativo</h1>
          <p className="mt-1 text-sm text-zinc-400">
            Gerencie usuários, gerentes e acompanhe o desempenho da plataforma.
          </p>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <Link
            href="/admin/usuarios"
            className="rounded-2xl border border-zinc-800 bg-zinc-900/50 p-6 transition hover:border-van-amber/60 hover:bg-zinc-900/80"
          >
            <h2 className="text-lg font-semibold text-zinc-100">Usuários</h2>
            <p className="mt-2 text-sm text-zinc-400">
              Busque passageiros, motoristas e gerentes cadastrados e veja o histórico de reservas de cada um.
            </p>
            <span className="mt-4 inline-block text-sm font-medium text-van-amber">Ver usuários →</span>
          </Link>

          <Link
            href="/admin/gerentes"
            className="rounded-2xl border border-zinc-800 bg-zinc-900/50 p-6 transition hover:border-van-amber/60 hover:bg-zinc-900/80"
          >
            <h2 className="text-lg font-semibold text-zinc-100">Gerentes</h2>
            <p className="mt-2 text-sm text-zinc-400">
              Cadastre novos gerentes, ajuste a taxa de plataforma, ative/desative contas e veja o histórico de
              viagens.
            </p>
            <span className="mt-4 inline-block text-sm font-medium text-van-amber">Ver gerentes →</span>
          </Link>
        </div>
      </main>
    </AdminGuard>
  );
}
