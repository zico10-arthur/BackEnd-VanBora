"use client";

import Link from "next/link";
import { useState } from "react";
import { LegalDocumentModal, type LegalDocumentKind } from "@/components/LegalDocumentModal";
import { BRAND } from "@/lib/copy";

export function SiteFooter() {
  const [legalDoc, setLegalDoc] = useState<LegalDocumentKind | null>(null);

  return (
    <>
      <footer className="border-t border-van-border bg-van-void px-4 py-12 sm:px-6 sm:py-14">
        <div className="mx-auto max-w-container">
          <div className="grid gap-10 sm:grid-cols-2 lg:grid-cols-4 lg:gap-8">
            <div className="sm:col-span-2 lg:col-span-1">
              <p className="font-display text-xl text-white">{BRAND.name}</p>
              <p className="mt-2 text-sm text-zinc-500">{BRAND.tagline}</p>
              <p className="mt-4 text-xs leading-relaxed text-zinc-600">
                CNPJ 00.000.000/0001-00 (placeholder)
              </p>
            </div>

            <div>
              <p className="text-xs font-semibold uppercase tracking-wider text-zinc-500">Navegação</p>
              <nav className="mt-3 flex flex-col gap-2 text-sm text-zinc-400" aria-label="Rodapé">
                <Link href="/#viagens" className="transition hover:text-white">
                  Viagens
                </Link>
                <Link href="/#faq" className="transition hover:text-white">
                  FAQ
                </Link>
                <Link href="/minhas-reservas" className="transition hover:text-white">
                  Minhas reservas
                </Link>
                <Link href="/entrar" className="transition hover:text-white">
                  Entrar
                </Link>
                <Link href="/cadastro" className="transition hover:text-white">
                  Criar conta
                </Link>
                <Link href="/motorista/login" className="transition hover:text-white">
                  Área do frotista
                </Link>
              </nav>
            </div>

            <div>
              <p className="text-xs font-semibold uppercase tracking-wider text-zinc-500">Contato</p>
              <ul className="mt-3 space-y-2 text-sm text-zinc-400">
                <li>
                  <a href="mailto:contato@vanbora.com" className="transition hover:text-white">
                    contato@vanbora.com
                  </a>
                  <span className="ml-1 text-xs text-zinc-600">(placeholder)</span>
                </li>
                <li>
                  <a href="https://wa.me/5500000000000" className="transition hover:text-white">
                    WhatsApp (11) 00000-0000
                  </a>
                  <span className="ml-1 text-xs text-zinc-600">(placeholder)</span>
                </li>
              </ul>
            </div>

            <div>
              <p className="text-xs font-semibold uppercase tracking-wider text-zinc-500">Legal</p>
              <ul className="mt-3 space-y-2 text-sm text-zinc-400">
                <li>
                  <button
                    type="button"
                    onClick={() => setLegalDoc("terms")}
                    className="text-left transition hover:text-white"
                  >
                    Termos de uso
                  </button>
                </li>
                <li>
                  <button
                    type="button"
                    onClick={() => setLegalDoc("privacy")}
                    className="text-left transition hover:text-white"
                  >
                    Política de privacidade
                  </button>
                </li>
              </ul>

              <p className="mt-5 text-xs font-semibold uppercase tracking-wider text-zinc-500">Redes</p>
              <ul className="mt-3 flex flex-wrap gap-3 text-sm text-zinc-500">
                <li>
                  <span className="rounded-vb border border-van-border px-2.5 py-1 text-xs">Instagram</span>
                </li>
                <li>
                  <span className="rounded-vb border border-van-border px-2.5 py-1 text-xs">LinkedIn</span>
                </li>
              </ul>
            </div>
          </div>

          <div className="mt-10 flex flex-col gap-4 rounded-vb border border-van-border bg-van-surface/40 p-5 sm:flex-row sm:items-center sm:justify-between">
            <ul className="flex flex-wrap gap-x-5 gap-y-2 text-xs text-zinc-500">
              <li className="flex items-center gap-1.5">
                <span className="h-1.5 w-1.5 rounded-full bg-van-amber" aria-hidden />
                Pagamentos via Mercado Pago
              </li>
              <li className="flex items-center gap-1.5">
                <span className="h-1.5 w-1.5 rounded-full bg-van-amber" aria-hidden />
                Pix instantâneo
              </li>
              <li className="flex items-center gap-1.5">
                <span className="h-1.5 w-1.5 rounded-full bg-van-amber" aria-hidden />
                Dados protegidos (LGPD)
              </li>
            </ul>
            <p className="text-xs text-zinc-600">Intermediação — não somos transportadora.</p>
          </div>

          <p className="mt-8 text-center text-xs text-zinc-600 sm:text-left">
            © {new Date().getFullYear()} {BRAND.name}. Todos os direitos reservados.
          </p>
        </div>
      </footer>

      {legalDoc ? (
        <LegalDocumentModal open kind={legalDoc} onClose={() => setLegalDoc(null)} />
      ) : null}
    </>
  );
}
