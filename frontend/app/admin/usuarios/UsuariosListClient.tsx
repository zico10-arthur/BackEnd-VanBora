"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { listarUsuariosAdmin } from "@/lib/api/admin";
import type { UsuarioAdminResponse } from "@/lib/api/types";
import { formatDate } from "@/lib/format";

const TIPO_LABEL: Record<string, string> = {
  Passageiro: "Passageiro",
  Gerente: "Gerente",
  Motorista: "Motorista",
  Admin: "Admin",
};

export function UsuariosListClient() {
  const [search, setSearch] = useState("");
  const [usuarios, setUsuarios] = useState<UsuarioAdminResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const fetchUsuarios = useCallback(async (term: string) => {
    setLoading(true);
    setError("");
    try {
      const data = await listarUsuariosAdmin(term || undefined);
      setUsuarios(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro ao carregar usuários.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => fetchUsuarios(search), 350);
    return () => clearTimeout(timer);
  }, [search, fetchUsuarios]);

  return (
    <div>
      <input
        type="text"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        placeholder="Buscar por nome, CPF ou email…"
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
          <VbButton variant="secondary" onClick={() => fetchUsuarios(search)}>
            Tentar novamente
          </VbButton>
        </div>
      )}

      {!loading && !error && usuarios.length === 0 && (
        <div className="flex flex-col items-center gap-2 py-16 text-center">
          <p className="text-zinc-500">Nenhum usuário encontrado</p>
        </div>
      )}

      {!loading && !error && usuarios.length > 0 && (
        <div className="overflow-hidden rounded-2xl border border-zinc-800">
          <table className="w-full text-sm">
            <thead className="bg-zinc-900/80 text-left text-xs uppercase tracking-wider text-zinc-500">
              <tr>
                <th className="px-4 py-3">Nome</th>
                <th className="px-4 py-3">Tipo</th>
                <th className="px-4 py-3">CPF</th>
                <th className="px-4 py-3">Email</th>
                <th className="px-4 py-3">Reservas</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Desde</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-800">
              {usuarios.map((u) => (
                <tr key={u.usuarioId} className="bg-zinc-900/30 transition hover:bg-zinc-900/70">
                  <td className="px-4 py-3">
                    <Link
                      href={`/admin/usuarios/${u.usuarioId}`}
                      className="font-medium text-zinc-100 hover:text-van-amber hover:underline"
                    >
                      {u.nome}
                    </Link>
                  </td>
                  <td className="px-4 py-3 text-zinc-400">{TIPO_LABEL[u.tipo] ?? u.tipo}</td>
                  <td className="px-4 py-3 font-mono text-xs text-zinc-400">{u.cpf}</td>
                  <td className="px-4 py-3 text-zinc-400">{u.email ?? "—"}</td>
                  <td className="px-4 py-3 text-zinc-400">{u.totalReservas}</td>
                  <td className="px-4 py-3">
                    <span
                      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                        u.ativo ? "bg-emerald-950 text-emerald-300" : "bg-zinc-800 text-zinc-500"
                      }`}
                    >
                      {u.ativo ? "Ativo" : "Inativo"}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-zinc-500">{formatDate(u.criadoEm)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
