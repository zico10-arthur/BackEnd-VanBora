"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { VanBoraLogo } from "@/components/VanBoraLogo";
import { MobileNav } from "@/components/layout/MobileNav";
import { useAuth } from "@/components/providers/AuthProvider";
import { isGerenteOuMotorista } from "@/lib/auth/token";

const navLink =
  "rounded-lg px-3 py-2 text-sm font-medium text-zinc-400 transition hover:bg-white/5 hover:text-white";

export function Header() {
  const { user, ready, logout } = useAuth();
  const router = useRouter();

  return (
    <header className="sticky top-0 z-40 border-b border-zinc-800/60 bg-van-void/95 backdrop-blur-xl supports-[backdrop-filter]:bg-van-void/80">
      <div className="mx-auto flex h-14 max-w-6xl items-center justify-between gap-3 px-4 sm:h-16 sm:gap-4 sm:px-6">
        <VanBoraLogo />

        <nav className="hidden items-center gap-1 sm:flex" aria-label="Principal">
          <Link href="/#viagens" className={navLink}>
            Viagens
          </Link>
          {ready && user ? (
            <>
              <Link href="/minhas-reservas" className={navLink}>
                Minhas reservas
              </Link>
              {isGerenteOuMotorista(user.perfis) ? (
                <Link href="/motorista/nova-viagem" className={`${navLink} text-van-amber/90`}>
                  Painel
                </Link>
              ) : null}
              <button
                type="button"
                onClick={() => {
                  logout();
                  router.push("/");
                }}
                className={navLink}
              >
                Sair
              </button>
            </>
          ) : (
            <>
              <Link href="/entrar" className={navLink}>
                Entrar
              </Link>
              <Link
                href="/cadastro/passageiro"
                className="rounded-xl bg-van-amber px-4 py-2 text-sm font-bold text-van-void transition hover:brightness-110"
              >
                Criar conta
              </Link>
            </>
          )}
        </nav>

        <MobileNav />
      </div>
    </header>
  );
}
