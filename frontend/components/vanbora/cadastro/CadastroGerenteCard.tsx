"use client";

import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { authRegistrarGerente } from "@/lib/api/auth";
import { ApiError } from "@/lib/api/http";
import { apenasDigitos } from "@/lib/format";
import { useAuth } from "@/components/providers/AuthProvider";
import { VanboraAuthAlert } from "@/components/vanbora/auth/VanboraAuthAlert";
import {
  vanboraAuthCardClassName,
  vanboraAuthInputClassName,
  vanboraAuthPrimaryButtonClassName,
} from "@/components/vanbora/auth/vanboraAuthStyles";

export function CadastroGerenteCard() {
  const router = useRouter();
  const { setUserFromToken } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [nome, setNome] = useState("");
  const [cpf, setCpf] = useState("");
  const [email, setEmail] = useState("");
  const [telefone, setTelefone] = useState("");
  const [senha, setSenha] = useState("");
  const [slug, setSlug] = useState("");

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const res = await authRegistrarGerente({
        nome: nome.trim(),
        cpf: apenasDigitos(cpf),
        email: email.trim(),
        senha,
        slug: slug.trim().toLowerCase(),
        telefone: apenasDigitos(telefone) || null,
      });
      setUserFromToken(res.token, {
        usuarioId: res.usuarioId,
        nome: res.nome,
        email: email.trim(),
        perfis: [res.tipo || "Gerente"],
      });
      router.push("/motorista/nova-viagem");
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
        <h1 className="text-xl font-bold text-white">Cadastrar frota</h1>
        <p className="mt-2 text-sm text-zinc-400">Use uma senha forte com letras, números e símbolos.</p>
      </div>
      {error ? (
        <div className="mt-6">
          <VanboraAuthAlert message={error} />
        </div>
      ) : null}
      <form onSubmit={handleSubmit} className="mt-8 flex flex-col gap-3">
        <input placeholder="Nome" required value={nome} onChange={(e) => setNome(e.target.value)} className={vanboraAuthInputClassName} disabled={loading} />
        <input placeholder="CPF" required value={cpf} onChange={(e) => setCpf(e.target.value)} className={vanboraAuthInputClassName} disabled={loading} />
        <input type="email" placeholder="E-mail" required value={email} onChange={(e) => setEmail(e.target.value)} className={vanboraAuthInputClassName} disabled={loading} />
        <input placeholder="Telefone (opcional)" value={telefone} onChange={(e) => setTelefone(e.target.value)} className={vanboraAuthInputClassName} disabled={loading} />
        <input
          placeholder="Identificador da frota (ex.: minha-frota)"
          required
          value={slug}
          onChange={(e) => setSlug(e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, ""))}
          className={vanboraAuthInputClassName}
          disabled={loading}
        />
        <input type="password" placeholder="Senha" required value={senha} onChange={(e) => setSenha(e.target.value)} className={vanboraAuthInputClassName} disabled={loading} />
        <button type="submit" disabled={loading} className={vanboraAuthPrimaryButtonClassName}>
          {loading ? "Cadastrando…" : "Criar conta de frotista"}
        </button>
      </form>
      <p className="mt-6 text-center text-xs text-zinc-500">
        <Link href="/motorista/login" className="text-van-amber hover:underline">
          Já tenho conta
        </Link>
      </p>
    </div>
  );
}
