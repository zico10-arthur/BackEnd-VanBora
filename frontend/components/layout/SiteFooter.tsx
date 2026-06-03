import Link from "next/link";
import { BRAND } from "@/lib/copy";

export function SiteFooter() {
  return (
    <footer className="border-t border-zinc-900/80 bg-van-void px-4 py-10 sm:px-6">
      <div className="mx-auto flex max-w-6xl flex-col gap-8 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="font-bold text-white">{BRAND.name}</p>
          <p className="mt-1 text-sm text-zinc-500">{BRAND.tagline}</p>
        </div>
        <nav className="flex flex-wrap gap-x-6 gap-y-2 text-sm text-zinc-400" aria-label="Rodapé">
          <Link href="/#viagens" className="transition hover:text-white">
            Viagens
          </Link>
          <Link href="/minhas-reservas" className="transition hover:text-white">
            Minhas reservas
          </Link>
          <Link href="/entrar" className="transition hover:text-white">
            Entrar
          </Link>
          <Link href="/cadastro/passageiro" className="transition hover:text-white">
            Criar conta
          </Link>
          <Link href="/motorista/login" className="transition hover:text-white">
            Área do frotista
          </Link>
        </nav>
      </div>
      <p className="mx-auto mt-8 max-w-6xl text-center text-xs text-zinc-600 sm:text-left">
        © {new Date().getFullYear()} {BRAND.name}. Todos os direitos reservados.
      </p>
    </footer>
  );
}
