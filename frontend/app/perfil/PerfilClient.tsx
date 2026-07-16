"use client";

import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { useAuth } from "@/components/providers/AuthProvider";
import {
  alterarSenha,
  atualizarSlug,
  atualizarUsuario,
  confirmarExclusao,
  obterUsuario,
  solicitarExclusao,
} from "@/lib/api/auth";
import type { AtualizarUsuarioResponse } from "@/lib/api/types";
import { ApiError } from "@/lib/api/http";

const inputClass =
  "w-full rounded-xl border border-zinc-700 bg-zinc-900 px-4 py-3 text-sm text-zinc-100 placeholder:text-zinc-500 focus:border-van-amber focus:outline-none focus:ring-1 focus:ring-van-amber";
const sectionClass = "rounded-2xl border border-zinc-800 bg-zinc-900/50 p-6";

export function PerfilClient() {
  const { user, ready, logout } = useAuth();
  const router = useRouter();

  const [perfil, setPerfil] = useState<AtualizarUsuarioResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Dados básicos
  const [nome, setNome] = useState("");
  const [email, setEmail] = useState("");
  const [telefone, setTelefone] = useState("");
  const [chavePix, setChavePix] = useState("");
  const [savingDados, setSavingDados] = useState(false);
  const [dadosMsg, setDadosMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  // Slug (gerente)
  const [slug, setSlug] = useState("");
  const [savingSlug, setSavingSlug] = useState(false);
  const [slugMsg, setSlugMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  // Senha
  const [senhaAtual, setSenhaAtual] = useState("");
  const [senhaNova, setSenhaNova] = useState("");
  const [savingSenha, setSavingSenha] = useState(false);
  const [senhaMsg, setSenhaMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  // Exclusão de conta
  const [exclusaoEtapa, setExclusaoEtapa] = useState<"idle" | "codigo">("idle");
  const [codigo, setCodigo] = useState("");
  const [exclusaoLoading, setExclusaoLoading] = useState(false);
  const [exclusaoMsg, setExclusaoMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  const fetchPerfil = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const data = await obterUsuario();
      setPerfil(data);
      setNome(data.nome);
      setEmail(data.email);
      setTelefone(data.telefone ?? "");
      setChavePix(data.chavePix ?? "");
      setSlug(data.slug ?? "");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Erro ao carregar perfil.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!ready) return;
    if (!user) {
      router.push("/entrar?next=/perfil");
      return;
    }
    fetchPerfil();
  }, [ready, user, router, fetchPerfil]);

  async function handleSalvarDados(e: FormEvent) {
    e.preventDefault();
    setDadosMsg(null);
    setSavingDados(true);
    try {
      const updated = await atualizarUsuario({
        nome: nome.trim(),
        email: email.trim(),
        telefone: telefone.trim() || null,
        chavePix: perfil?.tipo === "Gerente" ? chavePix.trim() || null : undefined,
      });
      setPerfil(updated);
      setDadosMsg({ type: "success", text: "Dados atualizados com sucesso!" });
    } catch (err) {
      setDadosMsg({ type: "error", text: err instanceof ApiError ? err.message : "Erro ao salvar dados." });
    } finally {
      setSavingDados(false);
    }
  }

  async function handleSalvarSlug(e: FormEvent) {
    e.preventDefault();
    setSlugMsg(null);
    setSavingSlug(true);
    try {
      const updated = await atualizarSlug({ slug: slug.trim() });
      setPerfil(updated);
      setSlugMsg({ type: "success", text: "Slug atualizado com sucesso!" });
    } catch (err) {
      setSlugMsg({ type: "error", text: err instanceof ApiError ? err.message : "Erro ao salvar slug." });
    } finally {
      setSavingSlug(false);
    }
  }

  async function handleAlterarSenha(e: FormEvent) {
    e.preventDefault();
    setSenhaMsg(null);

    if (senhaNova.length < 6) {
      setSenhaMsg({ type: "error", text: "A nova senha deve ter pelo menos 6 caracteres." });
      return;
    }

    setSavingSenha(true);
    try {
      await alterarSenha({ senhaAtual, senhaNova });
      setSenhaMsg({ type: "success", text: "Senha alterada com sucesso!" });
      setSenhaAtual("");
      setSenhaNova("");
    } catch (err) {
      setSenhaMsg({ type: "error", text: err instanceof ApiError ? err.message : "Erro ao alterar senha." });
    } finally {
      setSavingSenha(false);
    }
  }

  async function handleSolicitarExclusao() {
    setExclusaoMsg(null);
    setExclusaoLoading(true);
    try {
      const resp = await solicitarExclusao();
      setExclusaoMsg({ type: "success", text: resp.mensagem });
      setExclusaoEtapa("codigo");
    } catch (err) {
      setExclusaoMsg({ type: "error", text: err instanceof ApiError ? err.message : "Erro ao solicitar exclusão." });
    } finally {
      setExclusaoLoading(false);
    }
  }

  async function handleConfirmarExclusao(e: FormEvent) {
    e.preventDefault();
    setExclusaoMsg(null);
    setExclusaoLoading(true);
    try {
      await confirmarExclusao(codigo.trim());
      logout();
      router.push("/");
    } catch (err) {
      setExclusaoMsg({ type: "error", text: err instanceof ApiError ? err.message : "Código inválido ou expirado." });
    } finally {
      setExclusaoLoading(false);
    }
  }

  if (!ready || loading) {
    return (
      <main className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
        <div className="animate-pulse space-y-5">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-32 rounded-2xl bg-zinc-800" />
          ))}
        </div>
      </main>
    );
  }

  if (error) {
    return (
      <main className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
        <div className="flex flex-col items-center gap-4 py-16 text-center">
          <p className="text-sm text-red-400">{error}</p>
          <VbButton variant="secondary" onClick={fetchPerfil}>
            Tentar novamente
          </VbButton>
        </div>
      </main>
    );
  }

  if (!perfil) return null;

  return (
    <main className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-zinc-100">Meu perfil</h1>
        <p className="mt-1 text-sm text-zinc-400">
          {perfil.tipo} · CPF {perfil.cpf}
        </p>
      </div>

      <div className="space-y-6">
        {/* Dados básicos */}
        <section className={sectionClass}>
          <h2 className="mb-4 text-base font-semibold text-zinc-100">Dados pessoais</h2>
          <form onSubmit={handleSalvarDados} className="space-y-4" noValidate>
            <div>
              <label className="mb-1 block text-sm font-medium text-zinc-300">Nome</label>
              <input className={inputClass} value={nome} onChange={(e) => setNome(e.target.value)} />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-zinc-300">Email</label>
              <input type="email" className={inputClass} value={email} onChange={(e) => setEmail(e.target.value)} />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-zinc-300">Telefone</label>
              <input className={inputClass} value={telefone} onChange={(e) => setTelefone(e.target.value)} />
            </div>
            {perfil.tipo === "Gerente" && (
              <div>
                <label className="mb-1 block text-sm font-medium text-zinc-300">Chave Pix</label>
                <input className={inputClass} value={chavePix} onChange={(e) => setChavePix(e.target.value)} />
              </div>
            )}

            {dadosMsg && (
              <p
                className={`rounded-lg border px-4 py-3 text-sm ${
                  dadosMsg.type === "success"
                    ? "border-emerald-700/60 bg-emerald-950/40 text-emerald-300"
                    : "border-red-700/60 bg-red-950/40 text-red-300"
                }`}
              >
                {dadosMsg.text}
              </p>
            )}

            <VbButton type="submit" disabled={savingDados}>
              {savingDados ? "Salvando…" : "Salvar dados"}
            </VbButton>
          </form>
        </section>

        {/* Slug (gerente) */}
        {perfil.tipo === "Gerente" && (
          <section className={sectionClass}>
            <h2 className="mb-4 text-base font-semibold text-zinc-100">Página pública</h2>
            <form onSubmit={handleSalvarSlug} className="space-y-4" noValidate>
              <div>
                <label className="mb-1 block text-sm font-medium text-zinc-300">Slug</label>
                <input
                  className={inputClass}
                  value={slug}
                  onChange={(e) => setSlug(e.target.value.toLowerCase())}
                  placeholder="ex: vanbora-rj"
                />
              </div>

              {slugMsg && (
                <p
                  className={`rounded-lg border px-4 py-3 text-sm ${
                    slugMsg.type === "success"
                      ? "border-emerald-700/60 bg-emerald-950/40 text-emerald-300"
                      : "border-red-700/60 bg-red-950/40 text-red-300"
                  }`}
                >
                  {slugMsg.text}
                </p>
              )}

              <VbButton type="submit" variant="secondary" disabled={savingSlug}>
                {savingSlug ? "Salvando…" : "Salvar slug"}
              </VbButton>
            </form>
          </section>
        )}

        {/* Senha */}
        <section className={sectionClass}>
          <h2 className="mb-4 text-base font-semibold text-zinc-100">Alterar senha</h2>
          <form onSubmit={handleAlterarSenha} className="space-y-4" noValidate>
            <div>
              <label className="mb-1 block text-sm font-medium text-zinc-300">Senha atual</label>
              <input
                type="password"
                className={inputClass}
                value={senhaAtual}
                onChange={(e) => setSenhaAtual(e.target.value)}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-zinc-300">Nova senha</label>
              <input
                type="password"
                className={inputClass}
                value={senhaNova}
                onChange={(e) => setSenhaNova(e.target.value)}
              />
            </div>

            {senhaMsg && (
              <p
                className={`rounded-lg border px-4 py-3 text-sm ${
                  senhaMsg.type === "success"
                    ? "border-emerald-700/60 bg-emerald-950/40 text-emerald-300"
                    : "border-red-700/60 bg-red-950/40 text-red-300"
                }`}
              >
                {senhaMsg.text}
              </p>
            )}

            <VbButton type="submit" variant="secondary" disabled={savingSenha}>
              {savingSenha ? "Salvando…" : "Alterar senha"}
            </VbButton>
          </form>
        </section>

        {/* Exclusão de conta */}
        <section className={`${sectionClass} border-red-900/60`}>
          <h2 className="mb-2 text-base font-semibold text-red-300">Excluir conta</h2>
          <p className="mb-4 text-sm text-zinc-400">
            Esta ação desativa sua conta permanentemente. Enviaremos um código de confirmação para seu email.
          </p>

          {exclusaoEtapa === "idle" ? (
            <VbButton
              variant="ghost"
              className="text-red-400 hover:text-red-300"
              onClick={handleSolicitarExclusao}
              disabled={exclusaoLoading}
            >
              {exclusaoLoading ? "Enviando…" : "Solicitar exclusão de conta"}
            </VbButton>
          ) : (
            <form onSubmit={handleConfirmarExclusao} className="space-y-4" noValidate>
              <div>
                <label className="mb-1 block text-sm font-medium text-zinc-300">
                  Código recebido por email
                </label>
                <input
                  className={inputClass}
                  value={codigo}
                  onChange={(e) => setCodigo(e.target.value)}
                  maxLength={6}
                  placeholder="000000"
                />
              </div>
              <VbButton
                type="submit"
                className="bg-red-600 text-white hover:brightness-110"
                disabled={exclusaoLoading}
              >
                {exclusaoLoading ? "Confirmando…" : "Confirmar exclusão"}
              </VbButton>
            </form>
          )}

          {exclusaoMsg && (
            <p
              className={`mt-4 rounded-lg border px-4 py-3 text-sm ${
                exclusaoMsg.type === "success"
                  ? "border-emerald-700/60 bg-emerald-950/40 text-emerald-300"
                  : "border-red-700/60 bg-red-950/40 text-red-300"
              }`}
            >
              {exclusaoMsg.text}
            </p>
          )}
        </section>
      </div>
    </main>
  );
}
