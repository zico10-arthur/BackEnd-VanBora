"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";

type Step = "email" | "code" | "success";

interface Props {
  backToLoginHref: string;
  title: string;
  subtitle: string;
}

export function EsqueciSenhaCard({ backToLoginHref, title, subtitle }: Props) {
  const [step, setStep] = useState<Step>("email");
  const [email, setEmail] = useState("");
  const [codigo, setCodigo] = useState("");
  const [novaSenha, setNovaSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const inputClass =
    "w-full rounded-lg border border-zinc-700 bg-zinc-900 px-4 py-2.5 text-sm text-white placeholder:text-zinc-500 focus:border-amber-500 focus:outline-none focus:ring-1 focus:ring-amber-500";
  const primaryBtnClass =
    "w-full rounded-lg bg-amber-500 px-4 py-2.5 text-sm font-semibold text-black transition hover:bg-amber-400 disabled:opacity-50";
  const secondaryBtnClass =
    "w-full rounded-lg border border-zinc-700 bg-zinc-900 px-4 py-2.5 text-center text-sm font-medium text-zinc-300 transition hover:border-zinc-500";

  async function handleSendCode(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5151"}/api/auth/esqueci-senha`,
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ email: email.trim() }),
        }
      );

      const data = await res.json();
      setStep("code");
    } catch {
      setError("Erro ao conectar ao servidor. Tente novamente.");
    } finally {
      setLoading(false);
    }
  }

  async function handleResetPassword(e: FormEvent) {
    e.preventDefault();
    setError(null);

    if (novaSenha !== confirmarSenha) {
      setError("As senhas não conferem.");
      return;
    }

    if (novaSenha.length < 6) {
      setError("A senha deve ter pelo menos 6 caracteres.");
      return;
    }

    setLoading(true);

    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5151"}/api/auth/redefinir-senha`,
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            email: email.trim(),
            codigo: codigo.trim(),
            novaSenha: novaSenha,
          }),
        }
      );

      if (!res.ok) {
        const data = await res.json();
        setError(data?.error?.message ?? "Código inválido ou expirado.");
        return;
      }

      setStep("success");
    } catch {
      setError("Erro ao conectar ao servidor. Tente novamente.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="w-full max-w-md rounded-xl border border-zinc-800 bg-zinc-950 p-8 shadow-2xl">
      <div className="flex flex-col items-center">
        <p className="text-center text-xs font-semibold uppercase tracking-[0.2em] text-zinc-500">
          {subtitle}
        </p>

        {step === "email" && (
          <>
            <h1 className="mt-2 text-center text-xl font-bold tracking-tight text-white sm:text-2xl">
              {title}
            </h1>
            <p className="mt-2 max-w-sm text-center text-sm leading-relaxed text-zinc-400">
              Digite seu e-mail cadastrado e enviaremos um código para você
              criar uma nova senha.
            </p>
          </>
        )}

        {step === "code" && (
          <>
            <h1 className="mt-2 text-center text-xl font-bold tracking-tight text-white sm:text-2xl">
              Redefinir senha
            </h1>
            <p className="mt-2 max-w-sm text-center text-sm leading-relaxed text-zinc-400">
              Enviamos um código de 6 dígitos para <strong>{email}</strong>.
              Digite-o abaixo junto com sua nova senha.
            </p>
          </>
        )}

        {step === "success" && (
          <p className="mt-4 max-w-sm text-center text-sm leading-relaxed text-zinc-300">
            Senha redefinida com sucesso! Agora você pode fazer login com sua
            nova senha.
          </p>
        )}
      </div>

      {error && (
        <div className="mt-4 rounded-lg border border-red-700/60 bg-red-950/40 px-4 py-3 text-sm text-red-300">
          {error}
        </div>
      )}

      {step === "email" && (
        <form onSubmit={handleSendCode} className="mt-8 flex flex-col gap-5">
          <div>
            <label
              htmlFor="recuperar-email"
              className="mb-1.5 block text-xs font-medium text-zinc-400"
            >
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
              className={inputClass}
              placeholder="seu@email.com"
              disabled={loading}
            />
          </div>
          <button
            type="submit"
            disabled={loading || !email.trim()}
            className={primaryBtnClass}
          >
            {loading ? "Enviando…" : "Enviar código"}
          </button>
          <Link href={backToLoginHref} className={secondaryBtnClass}>
            Voltar para o Login
          </Link>
        </form>
      )}

      {step === "code" && (
        <form onSubmit={handleResetPassword} className="mt-8 flex flex-col gap-5">
          <div>
            <label
              htmlFor="codigo-reset"
              className="mb-1.5 block text-xs font-medium text-zinc-400"
            >
              Código de 6 dígitos
            </label>
            <input
              id="codigo-reset"
              name="codigo"
              type="text"
              inputMode="numeric"
              autoComplete="one-time-code"
              required
              maxLength={6}
              minLength={6}
              value={codigo}
              onChange={(e) => setCodigo(e.target.value.replace(/\D/g, ""))}
              className={inputClass}
              placeholder="000000"
              disabled={loading}
            />
          </div>
          <div>
            <label
              htmlFor="nova-senha"
              className="mb-1.5 block text-xs font-medium text-zinc-400"
            >
              Nova senha
            </label>
            <input
              id="nova-senha"
              name="novaSenha"
              type="password"
              autoComplete="new-password"
              required
              minLength={6}
              value={novaSenha}
              onChange={(e) => setNovaSenha(e.target.value)}
              className={inputClass}
              placeholder="Mínimo 6 caracteres"
              disabled={loading}
            />
          </div>
          <div>
            <label
              htmlFor="confirmar-senha"
              className="mb-1.5 block text-xs font-medium text-zinc-400"
            >
              Confirmar senha
            </label>
            <input
              id="confirmar-senha"
              name="confirmarSenha"
              type="password"
              autoComplete="new-password"
              required
              minLength={6}
              value={confirmarSenha}
              onChange={(e) => setConfirmarSenha(e.target.value)}
              className={inputClass}
              placeholder="Repita a senha"
              disabled={loading}
            />
          </div>
          <button
            type="submit"
            disabled={
              loading ||
              codigo.length !== 6 ||
              novaSenha.length < 6 ||
              confirmarSenha.length < 6
            }
            className={primaryBtnClass}
          >
            {loading ? "Redefinindo…" : "Redefinir senha"}
          </button>
          <Link href={backToLoginHref} className={secondaryBtnClass}>
            Voltar para o Login
          </Link>
        </form>
      )}

      {step === "success" && (
        <div className="mt-8 flex flex-col gap-4">
          <Link href={backToLoginHref} className={primaryBtnClass}>
            Ir para o Login
          </Link>
        </div>
      )}
    </div>
  );
}
