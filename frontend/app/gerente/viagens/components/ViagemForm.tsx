"use client";

import { useState, type FormEvent } from "react";
import Link from "next/link";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import type {
  CriarViagemRequest,
  ViagemGerenteResponse,
  VanResponse,
  MotoristaResponse,
} from "@/lib/api/types";
import { ApiError } from "@/lib/api/http";

type FieldErrors = Partial<Record<
  "nomeEvento" | "dataEvento" | "localEvento" | "dataPartida" |
  "localPartida" | "precoAssento" | "quorumMinimo" | "api" |
  "vanId" | "motoristaId",
  string
>>;

type ViagemFormProps = {
  viagem?: ViagemGerenteResponse;
  onSubmit: (data: CriarViagemRequest) => Promise<void>;
  submitLabel: string;
  // ── Spec 50 — Alocação ──
  vans?: VanResponse[];
  motoristas?: MotoristaResponse[];
  vanId?: string;
  motoristaId?: string;
  onVanChange?: (vanId: string) => void;
  onMotoristaChange?: (motoristaId: string) => void;
  loadingRecursos?: boolean;
  erroRecursos?: string | null;
  onRetryRecursos?: () => void;
};

export function ViagemForm({
  viagem,
  onSubmit,
  submitLabel,
  vans = [],
  motoristas = [],
  vanId = "",
  motoristaId = "",
  onVanChange = () => {},
  onMotoristaChange = () => {},
  loadingRecursos = false,
  erroRecursos = null,
  onRetryRecursos = () => {},
}: ViagemFormProps) {
  const isEdit = !!viagem;

  const [nomeEvento, setNomeEvento] = useState(viagem?.nomeEvento ?? "");
  const [dataEvento, setDataEvento] = useState(viagem?.dataEvento ? viagem.dataEvento.slice(0, 16) : "");
  const [localEvento, setLocalEvento] = useState(viagem?.localEvento ?? "");
  const [localPartida, setLocalPartida] = useState(viagem?.localPartida ?? "");
  const [dataPartida, setDataPartida] = useState(viagem?.dataPartida ? viagem.dataPartida.slice(0, 16) : "");
  const [precoAssento, setPrecoAssento] = useState(viagem?.precoAssento?.toString() ?? "");
  const [quorumMinimo, setQuorumMinimo] = useState(viagem?.quorumMinimo?.toString() ?? "");
  const [possuiIngresso, setPossuiIngresso] = useState(viagem?.possuiIngresso ?? false);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<FieldErrors>({});

  function validate(): FieldErrors {
    const errs: FieldErrors = {};
    const now = new Date();

    if (!nomeEvento.trim()) errs.nomeEvento = "Nome do evento é obrigatório.";
    else if (nomeEvento.trim().length > 200) errs.nomeEvento = "Nome do evento deve ter no máximo 200 caracteres.";

    if (!dataEvento) errs.dataEvento = "Data do evento é obrigatória.";
    else if (new Date(dataEvento) <= now) errs.dataEvento = "Data do evento deve ser futura.";

    if (!localEvento.trim()) errs.localEvento = "Local do evento é obrigatório.";
    else if (localEvento.trim().length > 300) errs.localEvento = "Local do evento deve ter no máximo 300 caracteres.";

    if (!dataPartida) errs.dataPartida = "Data de partida é obrigatória.";
    else if (new Date(dataPartida) <= now) errs.dataPartida = "Data de partida deve ser futura.";
    else if (dataEvento && new Date(dataPartida) >= new Date(dataEvento))
      errs.dataPartida = "Data de partida deve ser anterior à data do evento.";

    if (!localPartida.trim()) errs.localPartida = "Informe o local de partida.";
    else if (localPartida.trim().length > 300) errs.localPartida = "Local de partida deve ter no máximo 300 caracteres.";

    const preco = Number(precoAssento);
    if (!precoAssento || isNaN(preco) || preco <= 0) errs.precoAssento = "Preço deve ser maior que zero.";

    const q = Number(quorumMinimo);
    if (!quorumMinimo || isNaN(q) || q <= 0 || !Number.isInteger(q))
      errs.quorumMinimo = "Quórum mínimo deve ser um número inteiro maior que zero.";

    // Spec 50 — validação de van e motorista
    if (!isEdit && !vanId) errs.vanId = "Selecione uma van";
    if (!isEdit && !motoristaId) errs.motoristaId = "Selecione um motorista";

    return errs;
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    const validationErrors = validate();
    setErrors(validationErrors);
    if (Object.keys(validationErrors).length > 0) return;

    setLoading(true);
    setErrors({});

    try {
      const body: CriarViagemRequest = {
        nomeEvento: nomeEvento.trim(),
        dataEvento: new Date(dataEvento).toISOString(),
        localEvento: localEvento.trim(),
        dataPartida: new Date(dataPartida).toISOString(),
        localPartida: localPartida.trim(),
        precoAssento: Number(precoAssento),
        possuiIngresso,
        quorumMinimo: Number(quorumMinimo),
      };
      await onSubmit(body);
    } catch (err: unknown) {
      if (err instanceof ApiError) {
        setErrors({ api: err.message });
      } else {
        const msg =
          err instanceof Error ? err.message : "Erro ao salvar. Tente novamente.";
        setErrors({ api: msg });
      }
    } finally {
      setLoading(false);
    }
  }

  const inputClass =
    "w-full rounded-xl border border-zinc-700 bg-zinc-900 px-4 py-3 text-sm text-zinc-100 placeholder:text-zinc-500 focus:border-van-amber focus:outline-none focus:ring-1 focus:ring-van-amber";
  const errorClass = "mt-1 text-xs text-red-400";

  const submitDisabled =
    loading || (!isEdit && (vans.length === 0 || motoristas.length === 0));

  return (
    <form onSubmit={handleSubmit} className="space-y-5" noValidate>
      {/* Nome do Evento */}
      <div>
        <label htmlFor="nomeEvento" className="mb-1 block text-sm font-medium text-zinc-300">
          Nome do evento
        </label>
        <input
          id="nomeEvento"
          type="text"
          className={`${inputClass} ${errors.nomeEvento ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={nomeEvento}
          onChange={(e) => setNomeEvento(e.target.value)}
          placeholder="Ex: Show da Banda X"
        />
        {errors.nomeEvento && <p className={errorClass}>{errors.nomeEvento}</p>}
      </div>

      {/* Data do Evento */}
      <div>
        <label htmlFor="dataEvento" className="mb-1 block text-sm font-medium text-zinc-300">
          Data do evento
        </label>
        <input
          id="dataEvento"
          type="datetime-local"
          className={`${inputClass} ${errors.dataEvento ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={dataEvento}
          onChange={(e) => setDataEvento(e.target.value)}
        />
        {errors.dataEvento && <p className={errorClass}>{errors.dataEvento}</p>}
      </div>

      {/* Local do Evento */}
      <div>
        <label htmlFor="localEvento" className="mb-1 block text-sm font-medium text-zinc-300">
          Local do evento
        </label>
        <input
          id="localEvento"
          type="text"
          className={`${inputClass} ${errors.localEvento ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={localEvento}
          onChange={(e) => setLocalEvento(e.target.value)}
          placeholder="Ex: Estádio X, Rua Y, 123"
        />
        {errors.localEvento && <p className={errorClass}>{errors.localEvento}</p>}
      </div>

      {/* Local de Partida */}
      <div>
        <label htmlFor="localPartida" className="mb-1 block text-sm font-medium text-zinc-300">
          Local de partida
        </label>
        <input
          id="localPartida"
          type="text"
          className={`${inputClass} ${errors.localPartida ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={localPartida}
          onChange={(e) => setLocalPartida(e.target.value)}
          placeholder="Ex: Rodoviária Central, Rio de Janeiro - RJ"
        />
        {errors.localPartida && <p className={errorClass}>{errors.localPartida}</p>}
      </div>

      {/* Data e hora de partida */}
      <div>
        <label htmlFor="dataPartida" className="mb-1 block text-sm font-medium text-zinc-300">
          Data e hora de partida
        </label>
        <input
          id="dataPartida"
          type="datetime-local"
          className={`${inputClass} ${errors.dataPartida ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={dataPartida}
          onChange={(e) => setDataPartida(e.target.value)}
        />
        {errors.dataPartida && <p className={errorClass}>{errors.dataPartida}</p>}
      </div>

      {/* Preço + Quórum (apenas criação) */}
      {!isEdit && (
        <>
          <div>
            <label htmlFor="precoAssento" className="mb-1 block text-sm font-medium text-zinc-300">
              Preço do assento (R$)
            </label>
            <input
              id="precoAssento"
              type="number"
              step="0.01"
              min="0.01"
              className={`${inputClass} ${errors.precoAssento ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
              value={precoAssento}
              onChange={(e) => setPrecoAssento(e.target.value)}
              placeholder="0,00"
            />
            {errors.precoAssento && <p className={errorClass}>{errors.precoAssento}</p>}
          </div>

          <div>
            <label htmlFor="quorumMinimo" className="mb-1 block text-sm font-medium text-zinc-300">
              Quórum mínimo de passageiros
            </label>
            <input
              id="quorumMinimo"
              type="number"
              step="1"
              min="1"
              className={`${inputClass} ${errors.quorumMinimo ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
              value={quorumMinimo}
              onChange={(e) => setQuorumMinimo(e.target.value)}
              placeholder="Ex: 20"
            />
            {errors.quorumMinimo && <p className={errorClass}>{errors.quorumMinimo}</p>}
          </div>
        </>
      )}

      {/* Possui Ingresso */}
      <div className="flex items-center gap-3">
        <input
          id="possuiIngresso"
          type="checkbox"
          className="h-4 w-4 rounded border-zinc-600 bg-zinc-800 text-van-amber focus:ring-van-amber"
          checked={possuiIngresso}
          onChange={(e) => setPossuiIngresso(e.target.checked)}
        />
        <label htmlFor="possuiIngresso" className="text-sm text-zinc-300">
          Este evento cobra ingresso separado
        </label>
      </div>

      {/* ── Spec 50: Seção Van (apenas criação) ── */}
      {!isEdit && (
        <div>
          <label className="mb-1 block text-sm font-medium text-zinc-300">
            Selecione a van para esta viagem
          </label>

          {loadingRecursos && (
            <div className="flex items-center gap-2 rounded-xl border border-zinc-700 bg-zinc-900 px-4 py-3">
              <div className="h-4 w-4 animate-spin rounded-full border-2 border-van-amber border-t-transparent" />
              <span className="text-sm text-zinc-500">Carregando vans…</span>
            </div>
          )}

          {!loadingRecursos && erroRecursos && (
            <div className="rounded-lg border border-red-700/60 bg-red-950/40 px-4 py-3 text-sm text-red-300">
              {erroRecursos}{" "}
              <button
                type="button"
                onClick={onRetryRecursos}
                className="font-semibold underline hover:text-red-200"
              >
                Tentar novamente
              </button>
            </div>
          )}

          {!loadingRecursos && !erroRecursos && vans.length === 0 && (
            <div className="rounded-xl border border-zinc-700 bg-zinc-900 px-4 py-3 text-sm text-zinc-500">
              Nenhuma van cadastrada.{" "}
              <Link href="/gerente/vans/nova" className="font-semibold text-van-amber underline hover:text-van-amber/80">
                Cadastre uma van primeiro.
              </Link>
            </div>
          )}

          {!loadingRecursos && !erroRecursos && vans.length > 0 && (
            <>
              <select
                id="vanId"
                value={vanId}
                onChange={(e) => onVanChange(e.target.value)}
                className={`${inputClass} ${errors.vanId ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
              >
                <option value="">Selecione uma van…</option>
                {vans.map((v) => (
                  <option key={v.id} value={v.id}>
                    {v.nome} — {v.modelo} ({v.placa}) — {v.capacidade} lugares
                  </option>
                ))}
              </select>
              {errors.vanId && <p className={errorClass}>{errors.vanId}</p>}
            </>
          )}
        </div>
      )}

      {/* ── Spec 50: Seção Motorista (apenas criação) ── */}
      {!isEdit && (
        <div>
          <label className="mb-1 block text-sm font-medium text-zinc-300">
            Selecione o motorista para esta viagem
          </label>

          {loadingRecursos && (
            <div className="flex items-center gap-2 rounded-xl border border-zinc-700 bg-zinc-900 px-4 py-3">
              <div className="h-4 w-4 animate-spin rounded-full border-2 border-van-amber border-t-transparent" />
              <span className="text-sm text-zinc-500">Carregando motoristas…</span>
            </div>
          )}

          {!loadingRecursos && erroRecursos && (
            <div className="rounded-lg border border-red-700/60 bg-red-950/40 px-4 py-3 text-sm text-red-300">
              {erroRecursos}{" "}
              <button
                type="button"
                onClick={onRetryRecursos}
                className="font-semibold underline hover:text-red-200"
              >
                Tentar novamente
              </button>
            </div>
          )}

          {!loadingRecursos && !erroRecursos && motoristas.length === 0 && (
            <div className="rounded-xl border border-zinc-700 bg-zinc-900 px-4 py-3 text-sm text-zinc-500">
              Nenhum motorista cadastrado.{" "}
              <Link href="/gerente/motoristas/novo" className="font-semibold text-van-amber underline hover:text-van-amber/80">
                Cadastre um motorista primeiro.
              </Link>
            </div>
          )}

          {!loadingRecursos && !erroRecursos && motoristas.length > 0 && (
            <>
              <select
                id="motoristaId"
                value={motoristaId}
                onChange={(e) => onMotoristaChange(e.target.value)}
                className={`${inputClass} ${errors.motoristaId ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
              >
                <option value="">Selecione um motorista…</option>
                {motoristas.map((m) => (
                  <option key={m.id} value={m.id}>
                    {m.nome} — CNH: {m.cnh}
                  </option>
                ))}
              </select>
              {errors.motoristaId && <p className={errorClass}>{errors.motoristaId}</p>}
            </>
          )}
        </div>
      )}

      {/* API error */}
      {errors.api && (
        <div className="rounded-lg border border-red-700/60 bg-red-950/40 px-4 py-3 text-sm text-red-300">
          {errors.api}
        </div>
      )}

      <VbButton type="submit" disabled={submitDisabled} className="w-full">
        {loading ? "Salvando…" : submitLabel}
      </VbButton>
    </form>
  );
}
