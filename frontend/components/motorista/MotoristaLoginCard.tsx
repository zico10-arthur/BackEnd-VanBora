"use client";

import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { ApiError } from "@/components/providers/AuthProvider";
import { useAuth } from "@/components/providers/AuthProvider";
import { VanboraAuthAlert } from "@/components/vanbora/auth/VanboraAuthAlert";
import {
  motoristaAuthInputClassName,
  motoristaAuthPrimaryButtonClassName,
  motoristaAuthSecondaryButtonClassName,
} from "./motoristaAuthCardStyles";

const VALUE_PROPS = [
  {
    title: "Publique em 2 minutos",
    description: "Cadastre a viagem, defina o preço e comece a vender assentos na hora.",
  },
  {
    title: "Receba no Pix em D+1",
    description: "Pagamentos confirmados caem direto na sua conta, sem burocracia.",
  },
  {
    title: "Pague só quando vender",
    description: "Sem mensalidade fixa — a plataforma só cobra quando há reserva confirmada.",
  },
] as const;

function MockDashboardPreview() {
  return (
    <div className="mt-10 rounded-vb border border-van-amber/20 bg-van-void/80 p-5 shadow-van-glow backdrop-blur-sm">
      <p className="text-xs font-semibold uppercase tracking-[0.18em] text-van-amber/80">Seu painel</p>
      <div className="mt-4 grid grid-cols-2 gap-3">
        <div className="rounded-vb border border-van-border bg-van-surface/90 p-3">
          <p className="text-[10px] font-medium uppercase tracking-wider text-zinc-500">Reservas hoje</p>
          <p className="mt-1 font-display text-2xl text-van-amber">12</p>
        </div>
        <div className="rounded-vb border border-van-border bg-van-surface/90 p-3">
          <p className="text-[10px] font-medium uppercase tracking-wider text-zinc-500">Receita semana</p>
          <p className="mt-1 font-display text-2xl text-van-amber">R$ 4,8k</p>
        </div>
        <div className="col-span-2 rounded-vb border border-van-border bg-van-surface/90 p-3">
          <p className="text-[10px] font-medium uppercase tracking-wider text-zinc-500">Próxima viagem</p>
          <p className="mt-1 text-sm font-semibold text-zinc-100">Flamengo x Vasco · sáb 16h</p>
          <div className="mt-2 h-1.5 overflow-hidden rounded-full bg-van-border">
            <div className="h-full w-[68%] rounded-full bg-van-amber" />
          </div>
          <p className="mt-1 text-xs text-zinc-500">68% de ocupação</p>
        </div>
      </div>
    </div>
  );
}

export function MotoristaLoginCard() {
  const router = useRouter();
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const { redirectPainel } = await login(email, password);
      router.push(redirectPainel ? "/motorista/nova-viagem" : "/");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Não foi possível entrar.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="grid min-h-screen lg:grid-cols-2">
      <div className="relative flex flex-col justify-center bg-van-void px-6 py-12 sm:px-10 lg:px-14 xl:px-20">
        <div
          className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_70%_45%_at_0%_0%,rgba(240,165,0,0.06),transparent_55%)]"
          aria-hidden
        />
        <div className="relative mx-auto w-full max-w-md">
          <Link href="/" className="mb-8 inline-block rounded-vb">
            <Image
              src="/brand/vanbora-logo.svg"
              alt="VanBora"
              width={200}
              height={32}
              className="h-8 w-auto opacity-95"
              priority
            />
          </Link>
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-zinc-500">Frotista</p>
          <h1 className="mt-2 font-display text-3xl leading-tight text-white sm:text-4xl">
            Publique viagens e receba no Pix
          </h1>
          <p className="mt-3 text-sm text-zinc-400">
            Acesse seu painel para gerenciar vans, reservas e recebimentos.
          </p>

          {error ? (
            <div className="mt-6">
              <VanboraAuthAlert message={error} />
            </div>
          ) : null}

          <form onSubmit={handleSubmit} className="mt-8 flex flex-col gap-5">
            <input
              id="motorista-email"
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className={motoristaAuthInputClassName}
              placeholder="seu@email.com"
              disabled={loading}
            />
            <input
              id="motorista-password"
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className={motoristaAuthInputClassName}
              placeholder="••••••••"
              disabled={loading}
            />
            <button type="submit" disabled={loading} className={motoristaAuthPrimaryButtonClassName}>
              {loading ? "Entrando…" : "Acessar painel"}
            </button>
          </form>

          <div className="mt-8 flex flex-col gap-3">
            <Link href="/cadastro/gerente" className={motoristaAuthSecondaryButtonClassName}>
              Cadastrar minha frota
            </Link>
          </div>

          <p className="mt-8 text-center text-xs text-zinc-500">
            <Link href="/#como-funciona" className="text-van-amber transition hover:underline">
              ← Como funciona a VanBora
            </Link>
          </p>

          <div className="mt-10 space-y-4 lg:hidden">
            {VALUE_PROPS.map((item) => (
              <div key={item.title} className="rounded-vb border border-van-border bg-van-surface/60 px-4 py-3">
                <p className="text-sm font-semibold text-zinc-100">{item.title}</p>
                <p className="mt-0.5 text-xs text-zinc-400">{item.description}</p>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="relative hidden flex-col justify-center overflow-hidden bg-gradient-to-br from-[#2a1f00] via-van-void to-van-void px-14 py-12 xl:px-20 lg:flex">
        <div
          className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_90%_60%_at_80%_20%,rgba(240,165,0,0.22),transparent_60%)]"
          aria-hidden
        />
        <div className="pointer-events-none absolute inset-0 bg-[url('/brand/noise.svg')] opacity-[0.04]" aria-hidden />
        <div className="relative max-w-lg">
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-van-amber">Para frotistas</p>
          <h2 className="mt-3 font-display text-4xl leading-none text-white xl:text-5xl">
            Venda assentos sem dor de cabeça
          </h2>
          <ul className="mt-8 space-y-5">
            {VALUE_PROPS.map((item) => (
              <li key={item.title} className="flex gap-4">
                <span className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-van-amber/15 text-sm text-van-amber">
                  ✓
                </span>
                <div>
                  <p className="font-semibold text-zinc-100">{item.title}</p>
                  <p className="mt-0.5 text-sm leading-relaxed text-zinc-400">{item.description}</p>
                </div>
              </li>
            ))}
          </ul>
          <MockDashboardPreview />
        </div>
      </div>
    </div>
  );
}
