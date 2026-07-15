import Link from "next/link";
import { Header } from "@/components/Header";
import { AdminGuard } from "@/components/admin/AdminGuard";
import { GerentesListClient } from "./GerentesListClient";

export const metadata = { title: "Gerentes — Admin VanBora" };

export default async function AdminGerentesPage({
  searchParams,
}: {
  searchParams: Promise<{ [key: string]: string | string[] | undefined }>;
}) {
  const params = await searchParams;
  const sucesso = typeof params.sucesso === "string" ? params.sucesso : null;

  return (
    <AdminGuard>
      <Header />
      <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-zinc-100">Gerentes</h1>
            <p className="mt-1 text-sm text-zinc-400">
              Busque gerentes cadastrados, ajuste taxa/gratuidade, ative/desative ou veja o histórico de viagens.
            </p>
          </div>
          <Link
            href="/admin/gerentes/novo"
            className="inline-flex items-center justify-center rounded-xl bg-van-amber px-5 py-3 text-sm font-semibold text-van-void shadow-[0_8px_28px_rgba(240,165,0,0.25)] transition hover:brightness-110"
          >
            Novo gerente
          </Link>
        </div>
        <GerentesListClient sucesso={sucesso} />
      </main>
    </AdminGuard>
  );
}
