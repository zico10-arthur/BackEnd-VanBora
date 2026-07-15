"use client";

import Link from "next/link";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { listarGerentesAdmin } from "@/lib/api/admin";
import type { GerenteAdminResponse } from "@/lib/api/types";
import { formatDate } from "@/lib/format";

const SUCCESS_MAP: Record<string, string> = {
  criado: "Gerente cadastrado com sucesso!",
  editado: "Gerente atualizado com sucesso!",
};

export function GerentesListClient({ sucesso }: { sucesso: string | null }) {
  const router = useRouter();
  const [search, setSearch] = useState("");
  const [gerentes, setGerentes] = useState<GerenteAdminResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  useEffect(() => {
    if (sucesso && SUCCESS_MAP[sucesso]) {
      setToastMessage(SUCCESS_MAP[sucesso]);
      router.replace("/admin/gerentes", { scroll: false });
    }
  }, [sucesso, router]);

  const fetchGerentes = useCallback(async (term: string) => {
    setLoading(true);
    setError("");
    try {
      const data = await listarGerentesAdmin(term || undefined);
      setGerentes(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao carregar gerentes.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => fetchGerentes(search), 350);
    return () => clearTimeout(timer);
  }, [search, fetchGerentes]);

  const toastEl = useMemo(() => {
    if (!toastMessage) return null;
    return (
      <div className="mb-6 rounded-xl border border-emerald-700/60 bg-emerald-950/60 px-4 py-3 text-sm font-medium text-emerald-300">
        {toastMessage}
      </div>
    );
  }, [toastMessage]);

  return (
    <div>
      {toastEl}

      <input
        type="text"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        placeholder="Buscar por nome, CPF, email ou slug…"
        className="mb-6 w-full rounded-xl border border-zinc-700 bg-zinc-900 px-4 py-3 text-sm text-zinc-100 placeholder:text-zinc-500 focus:border-van-amber focus:outline-none focus:ring-1 focus:ring-van-amber sm:max-w-md"
      />

      {loading && (
        <div className="space-y-2">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-16 animate-pulse rounded-xl border border-zinc-800 bg-zinc-900/50" />
          ))}
        </div>
      )}

      {!loading && error && (
        <div className="flex flex-col items-center gap-4 py-16 text-center">
          <p className="text-sm text-red-400">{error}</p>
          <VbButton variant="secondary" onClick={() => fetchGerentes(search)}>
            Tentar novamente
          </VbButton>
        </div>
      )}

      {!loading && !error && gerentes.length === 0 && (
        <div className="flex flex-col items-center gap-4 py-16 text-center">
          <p className="text-zinc-500">Nenhum gerente encontrado</p>
          <VbButton onClick={() => router.push("/admin/gerentes/novo")}>Cadastrar gerente</VbButton>
        </div>
      )}

      {!loading && !error && gerentes.length > 0 && (
        <div className="overflow-hidden rounded-2xl border border-zinc-800">
          <table className="w-full text-sm">
            <thead className="bg-zinc-900/80 text-left text-xs uppercase tracking-wider text-zinc-500">
              <tr>
                <th className="px-4 py-3">Nome</th>
                <th className="px-4 py-3">Slug</th>
                <th className="px-4 py-3">Taxa</th>
                <th className="px-4 py-3">Vans</th>
                <th className="px-4 py-3">Viagens</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Desde</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-800">
              {gerentes.map((g) => (
                <tr key={g.id} className="bg-zinc-900/30 transition hover:bg-zinc-900/70">
                  <td className="px-4 py-3">
                    <p className="font-medium text-zinc-100">{g.nome}</p>
                    <p className="text-xs text-zinc-500">{g.email ?? "—"}</p>
                  </td>
                  <td className="px-4 py-3 text-zinc-400">{g.slug ?? "—"}</td>
                  <td className="px-4 py-3 text-zinc-400">
                    {g.gratuito ? "Gratuito" : `${g.taxaPlataforma ?? 0}%`}
                  </td>
                  <td className="px-4 py-3 text-zinc-400">{g.totalVans}</td>
                  <td className="px-4 py-3 text-zinc-400">{g.totalViagens}</td>
                  <td className="px-4 py-3">
                    <span
                      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                        g.ativo ? "bg-emerald-950 text-emerald-300" : "bg-zinc-800 text-zinc-500"
                      }`}
                    >
                      {g.ativo ? "Ativo" : "Inativo"}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-zinc-500">{formatDate(g.criadoEm)}</td>
                  <td className="px-4 py-3 text-right">
                    <div className="flex justify-end gap-3">
                      <Link
                        href={`/admin/gerentes/${g.id}/historico`}
                        className="text-xs font-medium text-zinc-400 hover:text-van-amber hover:underline"
                      >
                        Histórico
                      </Link>
                      <Link
                        href={`/admin/gerentes/${g.id}/editar`}
                        className="text-xs font-medium text-van-amber hover:underline"
                      >
                        Editar
                      </Link>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
