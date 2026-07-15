import Link from "next/link";
import { Header } from "@/components/Header";
import { GerenteGuard } from "./components/GerenteGuard";
import { VansListClient } from "./VansListClient";

export const metadata = { title: "Minhas Vans — VanBora" };

export default async function VansPage({
  searchParams,
}: {
  searchParams: Promise<{ [key: string]: string | string[] | undefined }>;
}) {
  const params = await searchParams;
  const sucesso = typeof params.sucesso === "string" ? params.sucesso : null;

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
        <div className="mb-6 flex items-center justify-between">
          <h1 className="text-2xl font-bold text-zinc-100">Minhas Vans</h1>
          <Link
            href="/gerente/vans/nova"
            className="inline-flex items-center justify-center rounded-xl bg-van-amber px-5 py-3 text-sm font-semibold text-van-void shadow-[0_8px_28px_rgba(240,165,0,0.25)] transition hover:brightness-110"
          >
            Nova van
          </Link>
        </div>
        <VansListClient sucesso={sucesso} />
      </main>
    </GerenteGuard>
  );
}
