"use client";

import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { authRegistrarPassageiro } from "@/lib/api/auth";
import { ApiError } from "@/lib/api/http";
import { apenasDigitos } from "@/lib/format";
import { useAuth } from "@/components/providers/AuthProvider";
import { VanboraAuthAlert } from "@/components/vanbora/auth/VanboraAuthAlert";
import {
  vanboraAuthCardClassName,
  vanboraAuthInputClassName,
  vanboraAuthPrimaryButtonClassName,
} from "@/components/vanbora/auth/vanboraAuthStyles";

export function CadastroPassageiroCard() {
  const router = useRouter();
  const { setUserFromToken } = useAuth();
  const [nome, setNome] = useState("");
  const [cpf, setCpf] = useState("");
  const [email, setEmail] = useState("");
  const [telefone, setTelefone] = useState("");
  const [senha, setSenha] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const res = await authRegistrarPassageiro({
        nome: nome.trim(),
        cpf: apenasDigitos(cpf),
        email: email.trim(),
        telefone: apenasDigitos(telefone),
        senha,
      });
      setUserFromToken(res.token, {
        usuarioId: res.usuarioId,
        nome: nome.trim(),
        email: email.trim(),
        perfis: ["Passageiro"],
      });
      router.push("/");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Erro no cadastro.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className={vanboraAuthCardClassName}>
      <div className="flex flex-col items-center text-center">
        <Link href="/" className="mb-8">
          <Image src="/brand/vanbora-logo.svg" alt="VanBora" width={200} height={32} className="h-8 w-auto" />
        </Link>
        <h1 className="text-xl font-bold text-white">Criar conta</h1>
        <p className="mt-2 text-sm text-zinc-400">Reserve assentos e acompanhe suas viagens em um só lugar.</p>
      </div>
      {error ? (
        <div className="mt-6">
          <VanboraAuthAlert message={error} />
        </div>
      ) : null}
      <form onSubmit={handleSubmit} className="mt-8 flex flex-col gap-3">
        {[
          { label: "Nome", value: nome, set: setNome },
          { label: "CPF", value: cpf, set: setCpf },
          { label: "E-mail", value: email, set: setEmail, type: "email" },
          { label: "Telefone", value: telefone, set: setTelefone },
          { label: "Senha (mín. 6)", value: senha, set: setSenha, type: "password" },
        ].map((f) => (
          <label key={f.label} className="block">
            <span className="mb-1 block text-xs text-zinc-500">{f.label}</span>
            <input
              type={f.type ?? "text"}
              required
              value={f.value}
              onChange={(e) => f.set(e.target.value)}
              className={vanboraAuthInputClassName}
              disabled={loading}
            />
          </label>
        ))}
        <button type="submit" disabled={loading} className={`mt-2 ${vanboraAuthPrimaryButtonClassName}`}>
          {loading ? "Cadastrando…" : "Criar conta"}
        </button>
      </form>
      <p className="mt-6 text-center text-xs text-zinc-500">
        <Link href="/entrar" className="text-van-amber hover:underline">
          Já tenho conta
        </Link>
      </p>
    </div>
  );
}
