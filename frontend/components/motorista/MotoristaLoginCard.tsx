"use client";

import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { ApiError } from "@/components/providers/AuthProvider";
import { useAuth } from "@/components/providers/AuthProvider";
import { VanboraAuthAlert } from "@/components/vanbora/auth/VanboraAuthAlert";
import {
  motoristaAuthCardClassName,
  motoristaAuthInputClassName,
  motoristaAuthPrimaryButtonClassName,
  motoristaAuthSecondaryButtonClassName,
} from "./motoristaAuthCardStyles";

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
    <div className={motoristaAuthCardClassName}>
      <div className="flex flex-col items-center">
        <Link href="/" className="mb-8 block rounded-lg">
          <Image src="/brand/vanbora-logo.svg" alt="VanBora" width={200} height={32} className="h-8 w-auto opacity-95" priority />
        </Link>
        <p className="text-center text-xs font-semibold uppercase tracking-[0.2em] text-zinc-500">Frotista</p>
        <h1 className="mt-2 text-center text-xl font-bold text-white">Painel VanBora</h1>
      </div>
      {error ? (
        <div className="mt-6">
          <VanboraAuthAlert message={error} />
        </div>
      ) : null}
      <form onSubmit={handleSubmit} className="mt-8 flex flex-col gap-5">
        <input id="motorista-email" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} className={motoristaAuthInputClassName} placeholder="seu@email.com" disabled={loading} />
        <input id="motorista-password" type="password" required value={password} onChange={(e) => setPassword(e.target.value)} className={motoristaAuthInputClassName} placeholder="••••••••" disabled={loading} />
        <button type="submit" disabled={loading} className={motoristaAuthPrimaryButtonClassName}>
          {loading ? "Entrando…" : "Acessar painel"}
        </button>
      </form>
      <div className="mt-8 flex flex-col gap-3">
        <Link href="/cadastro/gerente" className={motoristaAuthSecondaryButtonClassName}>
          Cadastrar minha frota
        </Link>
      </div>
    </div>
  );
}
