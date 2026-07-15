"use client";

import Image from "next/image";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { FormEvent, useState } from "react";
import { ApiError } from "@/components/providers/AuthProvider";
import { useAuth } from "@/components/providers/AuthProvider";
import { VanboraAuthAlert } from "@/components/vanbora/auth/VanboraAuthAlert";
import {
  vanboraAuthCardClassName,
  vanboraAuthInputClassName,
  vanboraAuthPrimaryButtonClassName,
} from "@/components/vanbora/auth/vanboraAuthStyles";

export function LoginPageClient() {
  const router = useRouter();
  const search = useSearchParams();
  const next = search.get("next") || "/";
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const { redirectPainel, redirectAdmin } = await login(email, senha);
      router.push(redirectAdmin ? "/admin/dashboard" : redirectPainel ? "/gerente/dashboard" : next);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Não foi possível entrar.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className={vanboraAuthCardClassName}>
      <div className="flex flex-col items-center">
        <Link href="/" className="mb-8">
          <Image src="/brand/vanbora-logo.svg" alt="VanBora" width={200} height={32} className="h-8 w-auto" priority />
        </Link>
        <h1 className="text-xl font-bold text-white">Entrar na VanBora</h1>
        <p className="mt-2 text-center text-sm text-zinc-400">Reserve assentos e acompanhe suas viagens.</p>
      </div>
      {error ? (
        <div className="mt-6">
          <VanboraAuthAlert message={error} />
        </div>
      ) : null}
      <form onSubmit={handleSubmit} className="mt-8 flex flex-col gap-4">
        <input
          type="email"
          required
          placeholder="E-mail"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className={vanboraAuthInputClassName}
          disabled={loading}
        />
        <input
          type="password"
          required
          placeholder="Senha"
          value={senha}
          onChange={(e) => setSenha(e.target.value)}
          className={vanboraAuthInputClassName}
          disabled={loading}
        />
        <button type="submit" disabled={loading} className={vanboraAuthPrimaryButtonClassName}>
          {loading ? "Entrando…" : "Entrar"}
        </button>
      </form>
      <p className="mt-8 text-center text-xs text-zinc-500">
        Novo por aqui?{" "}
        <Link href="/cadastro/passageiro" className="text-van-amber hover:underline">
          Criar conta
        </Link>
        {" · "}
        <Link href="/cadastro/gerente" className="text-van-amber hover:underline">
          Sou frotista
        </Link>
      </p>
      <p className="mt-3 text-center text-xs text-zinc-500">
        <Link href="/recuperar-senha" className="text-van-amber hover:underline">
          Esqueci minha senha
        </Link>
      </p>
    </div>
  );
}
