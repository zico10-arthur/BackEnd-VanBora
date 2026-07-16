"use client";

import { useParams, useRouter } from "next/navigation";
import { useCallback, useEffect, useState, type FormEvent } from "react";
import Link from "next/link";
import { Header } from "@/components/Header";
import { AdminGuard } from "@/components/admin/AdminGuard";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { atualizarGerenteAdmin, obterGerenteAdmin } from "@/lib/api/admin";
import type { GerenteAdminResponse } from "@/lib/api/types";
import { ApiError } from "@/lib/api/http";

const inputClass =
  "w-full rounded-xl border border-zinc-700 bg-zinc-900 px-4 py-3 text-sm text-zinc-100 placeholder:text-zinc-500 focus:border-van-amber focus:outline-none focus:ring-1 focus:ring-van-amber";

export default function EditarGerenteAdminPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const id = params?.id;

  const [gerente, setGerente] = useState<GerenteAdminResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [apiError, setApiError] = useState<string | null>(null);

  const [gratuito, setGratuito] = useState(false);
  const [taxaPlataforma, setTaxaPlataforma] = useState("0");
  const [ativo, setAtivo] = useState(true);

  const fetchGerente = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError("");
    try {
      const data = await obterGerenteAdmin(id);
      setGerente(data);
      setGratuito(!!data.gratuito);
      setTaxaPlataforma(String(data.taxaPlataforma ?? 0));
      setAtivo(data.ativo);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao carregar gerente.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchGerente();
  }, [fetchGerente]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!id) return;
    setApiError(null);
    setSaving(true);
    try {
      await atualizarGerenteAdmin(id, {
        taxaPlataforma: gratuito ? null : Number(taxaPlataforma),
        gratuito,
        ativo,
      });
      router.push("/admin/gerentes?sucesso=editado");
    } catch (err) {
      setApiError(err instanceof ApiError ? err.message : "Erro ao salvar alterações.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <AdminGuard>
      <Header />
      <main className="mx-auto max-w-lg px-4 py-8 sm:px-6">
        <h1 className="mb-2 text-2xl font-bold text-zinc-100">Editar gerente</h1>

        {loading && (
          <div className="animate-pulse space-y-5">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-12 rounded-xl bg-zinc-800" />
            ))}
          </div>
        )}

        {!loading && error && (
          <div className="flex flex-col items-center gap-4 py-8 text-center">
            <p className="text-sm text-red-400">{error}</p>
            <VbButton variant="secondary" onClick={fetchGerente}>
              Tentar novamente
            </VbButton>
          </div>
        )}

        {!loading && !error && gerente && (
          <>
            <p className="mb-6 text-sm text-zinc-400">
              {gerente.nome} · {gerente.email ?? "sem email"} · {gerente.slug ?? "sem slug"}
            </p>

            <form onSubmit={handleSubmit} className="space-y-5" noValidate>
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
                  <label className="mb-1 block text-sm font-medium text-zinc-300">
                    Taxa de plataforma (%)
                  </label>
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

              <div className="flex items-center gap-2">
                <input
                  id="ativo"
                  type="checkbox"
                  checked={ativo}
                  onChange={(e) => setAtivo(e.target.checked)}
                  className="h-4 w-4 rounded border-zinc-700 bg-zinc-900 text-van-amber focus:ring-van-amber"
                />
                <label htmlFor="ativo" className="text-sm text-zinc-300">
                  Conta ativa
                </label>
              </div>

              {apiError && (
                <p className="rounded-lg border border-red-700/60 bg-red-950/40 px-4 py-3 text-sm text-red-300">
                  {apiError}
                </p>
              )}

              <VbButton type="submit" disabled={saving} className="w-full">
                {saving ? "Salvando…" : "Salvar alterações"}
              </VbButton>
            </form>

            <div className="mt-4 flex justify-center gap-4">
              <Link
                href={`/admin/gerentes/${gerente.id}/historico`}
                className="text-sm text-zinc-400 hover:text-van-amber hover:underline"
              >
                Ver histórico de viagens
              </Link>
              <VbButton variant="ghost" onClick={() => router.push("/admin/gerentes")}>
                Cancelar
              </VbButton>
            </div>
          </>
        )}
      </main>
    </AdminGuard>
  );
}
