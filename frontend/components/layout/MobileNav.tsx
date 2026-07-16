"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useAuth } from "@/components/providers/AuthProvider";
import { isAdmin, isGerenteOuMotorista } from "@/lib/auth/token";

type Props = {
  onNavigate?: () => void;
};

export function MobileNav({ onNavigate }: Props) {
  const [open, setOpen] = useState(false);
  const { user, ready, logout } = useAuth();

  useEffect(() => {
    if (!open) return;
    const prev = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.body.style.overflow = prev;
    };
  }, [open]);

  function close() {
    setOpen(false);
    onNavigate?.();
  }

  const linkClass =
    "block rounded-xl px-4 py-3.5 text-base font-medium text-zinc-200 transition hover:bg-white/5 hover:text-white";

  return (
    <div className="sm:hidden">
      <button
        type="button"
        onClick={() => setOpen(true)}
        className="flex h-10 w-10 items-center justify-center rounded-lg border border-zinc-800 text-zinc-300 transition hover:bg-white/5 hover:text-white"
        aria-expanded={open}
        aria-controls="mobile-nav-panel"
        aria-label="Abrir menu"
      >
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden>
          <path d="M4 7h16M4 12h16M4 17h16" strokeLinecap="round" />
        </svg>
      </button>

      {open ? (
        <>
          <button
            type="button"
            className="fixed inset-0 z-50 bg-black/70 backdrop-blur-sm"
            aria-label="Fechar menu"
            onClick={close}
          />
          <nav
            id="mobile-nav-panel"
            className="fixed inset-y-0 right-0 z-50 flex w-[min(100%,20rem)] flex-col border-l border-zinc-800 bg-van-void p-5 shadow-2xl"
            aria-label="Menu principal"
          >
            <div className="mb-6 flex items-center justify-between">
              <span className="text-sm font-semibold text-white">Menu</span>
              <button
                type="button"
                onClick={close}
                className="rounded-lg p-2 text-zinc-400 hover:bg-white/5 hover:text-white"
                aria-label="Fechar"
              >
                ✕
              </button>
            </div>
            <div className="flex flex-1 flex-col gap-1 overflow-y-auto">
              <Link href="/#viagens" className={linkClass} onClick={close}>
                Viagens
              </Link>
              {ready && user ? (
                <>
                  <Link href="/minhas-reservas" className={linkClass} onClick={close}>
                    Minhas reservas
                  </Link>
                  {isGerenteOuMotorista(user.perfis) ? (
                    <>
                      <Link href="/gerente/dashboard" className={linkClass} onClick={close}>
                        Dashboard
                      </Link>
                      <Link href="/gerente/viagens" className={linkClass} onClick={close}>
                        Minhas Viagens
                      </Link>
                      <Link href="/gerente/vans" className={linkClass} onClick={close}>
                        Minhas Vans
                      </Link>
                      <Link href="/gerente/motoristas" className={linkClass} onClick={close}>
                        Motoristas
                      </Link>
                    </>
                  ) : null}
                  {isAdmin(user.perfis) ? (
                    <Link href="/admin/dashboard" className={linkClass} onClick={close}>
                      Admin
                    </Link>
                  ) : null}
                  <Link href="/perfil" className={linkClass} onClick={close}>
                    Perfil
                  </Link>
                  <button
                    type="button"
                    className={`${linkClass} w-full text-left text-zinc-500`}
                    onClick={() => {
                      logout();
                      close();
                    }}
                  >
                    Sair
                  </button>
                </>
              ) : (
                <>
                  <Link href="/entrar" className={linkClass} onClick={close}>
                    Entrar
                  </Link>
                  <Link
                    href="/cadastro"
                    className="mt-2 block rounded-xl bg-van-amber px-4 py-3.5 text-center text-base font-bold text-van-void"
                    onClick={close}
                  >
                    Criar conta
                  </Link>
                </>
              )}
            </div>
          </nav>
        </>
      ) : null}
    </div>
  );
}
