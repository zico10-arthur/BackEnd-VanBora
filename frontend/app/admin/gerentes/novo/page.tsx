"use client";

import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { Header } from "@/components/Header";
import { AdminGuard } from "@/components/admin/AdminGuard";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { criarGerenteAdmin } from "@/lib/api/admin";
import { ApiError } from "@/lib/api/http";
import { apenasDigitos } from "@/lib/format";

const inputClass =
  "w-full rounded-xl border border-zinc-700 bg-zinc-900 px-4 py-3 text-sm text-zinc-100 placeholder:text-zinc-500 focus:border-van-amber focus:outline-none focus:ring-1 focus:ring-van-amber";

export default function NovoGerenteAdminPage() {
  const router = useRouter();
  const [nome, setNome] = useState("");
  const [cpf, setCpf] = useState("");
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [telefone, setTelefone] = useState("");
  const [slug, setSlug] = useState("");
  const [taxaPlataforma, setTaxaPlataforma] = useState("10");
  const [gratuito, setGratuito] = useState(false);
  const [chavePix, setChavePix] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);

    if (!nome.trim() || !cpf.trim() || !email.trim() || !senha.trim() || !slug.trim()) {
      setError("Preencha os campos obrigatórios.");
      return;
    }

    setLoading(true);
    try {
      await criarGerenteAdmin({
        nome: nome.trim(),
        cpf: apenasDigitos(cpf),
        email: email.trim(),
        senha,
        telefone: telefone.trim() || null,
        slug: slug.trim(),
        taxaPlataforma: gratuito ? null : Number(taxaPlataforma),
        gratuito,
        chavePix: chavePix.trim() || null,
      });
      router.push("/admin/gerentes?sucesso=criado");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Erro ao cadastrar gerente.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <AdminGuard>
      <Header />
      <main className="mx-auto max-w-lg px-4 py-8 sm:px-6">
        <h1 className="mb-6 text-2xl font-bold text-zinc-100">Novo gerente</h1>

        <form onSubmit={handleSubmit} className="space-y-5" noValidate>
          <div>
            <label className="mb-1 block text-sm font-medium text-zinc-300">Nome</label>
            <input className={inputClass} value={nome} onChange={(e) => setNome(e.target.value)} />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-zinc-300">CPF</label>
            <input className={inputClass} value={cpf} onChange={(e) => setCpf(e.target.value)} maxLength={14} />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-zinc-300">Email</label>
            <input type="email" className={inputClass} value={email} onChange={(e) => setEmail(e.target.value)} />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-zinc-300">Senha provisória</label>
            <input
              type="password"
              className={inputClass}
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-zinc-300">Telefone</label>
            <input className={inputClass} value={telefone} onChange={(e) => setTelefone(e.target.value)} />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-zinc-300">Slug (URL pública)</label>
            <input
              className={inputClass}
              value={slug}
              onChange={(e) => setSlug(e.target.value.toLowerCase())}
              placeholder="ex: vanbora-rj"
            />
          </div>

          <div className="flex items-center gap-2">
            <input
              id="gratuito"
              type="checkbox"
              checked={gratuito}
              onChange={(e) => setGratuito(e.target.checked)}
              className="h-4 w-4 rounded border-zinc-700 bg-zinc-900 text-van-amber focus:ring-van-amber"
            />
            <label htmlFor="gratuito" className="text-sm text-zinc-300">
              Isento de taxa de plataforma (parceiro gratuito)
            </label>
          </div>

          {!gratuito && (
            <div>
              <label className="mb-1 block text-sm font-medium text-zinc-300">Taxa de plataforma (%)</label>
              <input
                type="number"
                min={0}
                max={100}
                step="0.1"
                className={inputClass}
                value={taxaPlataforma}
                onChange={(e) => setTaxaPlataforma(e.target.value)}
              />
            </div>
          )}

          <div>
            <label className="mb-1 block text-sm font-medium text-zinc-300">Chave Pix (opcional)</label>
            <input className={inputClass} value={chavePix} onChange={(e) => setChavePix(e.target.value)} />
          </div>

          {error && (
            <p className="rounded-lg border border-red-700/60 bg-red-950/40 px-4 py-3 text-sm text-red-300">
              {error}
            </p>
          )}

          <VbButton type="submit" disabled={loading} className="w-full">
            {loading ? "Salvando…" : "Cadastrar gerente"}
          </VbButton>
        </form>

        <div className="mt-4 text-center">
          <VbButton variant="ghost" onClick={() => router.push("/admin/gerentes")}>
            Cancelar
          </VbButton>
        </div>
      </main>
    </AdminGuard>
  );
}
