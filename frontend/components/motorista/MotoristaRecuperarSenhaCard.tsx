"use client";

import Image from "next/image";
import Link from "next/link";
import { FormEvent, useState } from "react";
import {
  motoristaAuthCardClassName,
  motoristaAuthInputClassName,
  motoristaAuthPrimaryButtonClassName,
  motoristaAuthSecondaryButtonClassName,
} from "./motoristaAuthCardStyles";

export function MotoristaRecuperarSenhaCard() {
  const [email, setEmail] = useState("");
  const [isSent, setIsSent] = useState(false);

  function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    // MVP: sem backend — apenas feedback de UI.
    setIsSent(true);
  }

  return (
    <div className={motoristaAuthCardClassName}>
      <div className="flex flex-col items-center">
        <Link
          href="/"
          className="mb-8 block rounded-lg outline-none focus-visible:ring-2 focus-visible:ring-amber-500 focus-visible:ring-offset-2 focus-visible:ring-offset-zinc-950"
        >
          <Image
            src="/brand/vanbora-logo.svg"
            alt="VanBora"
            width={200}
            height={32}
            className="h-8 w-auto opacity-95"
            priority
          />
        </Link>
        <p className="text-center text-xs font-semibold uppercase tracking-[0.2em] text-zinc-500">Motorista / frotista</p>

        {!isSent ? (
          <>
            <h1 className="mt-2 text-center text-xl font-bold tracking-tight text-white sm:text-2xl">Recuperar senha</h1>
            <p className="mt-2 max-w-sm text-center text-sm leading-relaxed text-zinc-400">
              Digite seu e-mail cadastrado e enviaremos as instruções para você criar uma nova senha.
            </p>
          </>
        ) : (
          <p className="mt-4 max-w-sm text-center text-sm leading-relaxed text-zinc-300">
            E-mail enviado! Verifique sua caixa de entrada (e a pasta de spam) em alguns instantes.
          </p>
        )}
      </div>

      {!isSent ? (
        <form onSubmit={handleSubmit} className="mt-8 flex flex-col gap-5">
          <div>
            <label htmlFor="recuperar-email" className="mb-1.5 block text-xs font-medium text-zinc-400">
              E-mail
            </label>
            <input
              id="recuperar-email"
              name="email"
              type="email"
              autoComplete="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className={motoristaAuthInputClassName}
              placeholder="seu@email.com"
            />
          </div>
          <button type="submit" className={`mt-1 ${motoristaAuthPrimaryButtonClassName}`}>
            Enviar Link de Recuperação
          </button>
        </form>
      ) : (
        <div className="mt-8 flex flex-col gap-4">
          <Link href="/motorista/login" className={motoristaAuthSecondaryButtonClassName}>
            Voltar para o Login
          </Link>
        </div>
      )}
    </div>
  );
}
