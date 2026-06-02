import Link from "next/link";
import { Header } from "@/components/Header";

export default function NotFound() {
  return (
    <div className="relative min-h-screen overflow-hidden bg-zinc-950">
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_80%_50%_at_50%_-20%,rgba(245,158,11,0.07),transparent_55%)]"
        aria-hidden
      />
      <Header />
      <main className="relative mx-auto flex max-w-lg flex-col items-center px-4 pb-24 pt-16 text-center sm:pt-24">
        <p className="text-xs font-semibold uppercase tracking-[0.35em] text-amber-500/90">Página não encontrada</p>
        <h1 className="mt-4 text-balance text-3xl font-black tracking-tight text-white sm:text-4xl">
          Não encontramos esta página
        </h1>
        <p className="mt-4 max-w-md text-pretty text-sm leading-relaxed text-zinc-400 sm:text-base">
          O endereço pode estar incorreto ou o conteúdo não está mais disponível. Volte ao início para ver as viagens.
        </p>
        <Link
          href="/"
          className="mt-10 inline-flex w-full max-w-xs items-center justify-center rounded-xl bg-amber-500 px-6 py-3.5 text-sm font-bold text-black shadow-[0_8px_28px_rgba(245,158,11,0.22)] transition hover:bg-amber-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-amber-400 focus-visible:ring-offset-2 focus-visible:ring-offset-zinc-950"
        >
          Voltar para o Início
        </Link>
      </main>
    </div>
  );
}
