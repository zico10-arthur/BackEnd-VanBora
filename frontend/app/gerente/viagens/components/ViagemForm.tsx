"use client";

import { useState, type FormEvent } from "react";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import type { CriarViagemRequest, ViagemGerenteResponse } from "@/lib/api/types";
import { ApiError } from "@/lib/api/http";

type FieldErrors = Partial<Record<keyof CriarViagemRequest | "api", string>>;

type ViagemFormProps = {
  viagem?: ViagemGerenteResponse;
  onSubmit: (data: CriarViagemRequest) => Promise<void>;
  submitLabel: string;
};

export function ViagemForm({ viagem, onSubmit, submitLabel }: ViagemFormProps) {
  const [nomeEvento, setNomeEvento] = useState(viagem?.nomeEvento ?? "");
  const [dataEvento, setDataEvento] = useState(viagem?.dataEvento ? viagem.dataEvento.slice(0, 16) : "");
  const [localEvento, setLocalEvento] = useState("");
  const [origemDescricao, setOrigemDescricao] = useState("");
  const [origemCidade, setOrigemCidade] = useState("");
  const [origemEstado, setOrigemEstado] = useState("");
  const [destinoDescricao, setDestinoDescricao] = useState("");
  const [destinoCidade, setDestinoCidade] = useState("");
  const [destinoEstado, setDestinoEstado] = useState("");
  const [dataSaida, setDataSaida] = useState(viagem?.dataPartida ? viagem.dataPartida.slice(0, 16) : "");
  const [dataChegada, setDataChegada] = useState("");
  const [precoAssento, setPrecoAssento] = useState(viagem?.precoAssento?.toString() ?? "");
  const [possuiIngresso, setPossuiIngresso] = useState(viagem?.possuiIngresso ?? false);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<FieldErrors>({});

  function validate(): FieldErrors {
    const errs: FieldErrors = {};
    const now = new Date();

    if (!nomeEvento.trim()) errs.nomeEvento = "Informe o nome do evento.";
    if (!dataEvento) errs.dataEvento = "Informe a data do evento.";
    else if (new Date(dataEvento) <= now) errs.dataEvento = "Data do evento deve ser futura.";

    if (!localEvento.trim()) errs.localEvento = "Informe o local do evento.";

    if (!origemDescricao.trim()) errs.origemDescricao = "Informe a descrição da origem.";
    if (!origemCidade.trim()) errs.origemCidade = "Informe a cidade de origem.";
    if (!origemEstado.trim() || origemEstado.trim().length !== 2)
      errs.origemEstado = "UF inválida.";

    if (!destinoDescricao.trim()) errs.destinoDescricao = "Informe a descrição do destino.";
    if (!destinoCidade.trim()) errs.destinoCidade = "Informe a cidade de destino.";
    if (!destinoEstado.trim() || destinoEstado.trim().length !== 2)
      errs.destinoEstado = "UF inválida.";

    if (!dataSaida) errs.dataSaida = "Informe a data de saída.";
    else if (new Date(dataSaida) <= now) errs.dataSaida = "Data de saída deve ser futura.";

    if (!dataChegada) errs.dataChegada = "Informe a data de chegada.";
    else if (dataSaida && new Date(dataChegada) <= new Date(dataSaida))
      errs.dataChegada = "Data de chegada deve ser posterior à saída.";

    const preco = Number(precoAssento);
    if (!precoAssento || isNaN(preco) || preco <= 0) errs.precoAssento = "Preço deve ser maior que zero.";

    // Origem e destino iguais (mesma cidade + estado) → erro
    if (
      origemCidade.trim() &&
      destinoCidade.trim() &&
      origemEstado.trim() &&
      destinoEstado.trim() &&
      origemCidade.trim() === destinoCidade.trim() &&
      origemEstado.trim() === destinoEstado.trim()
    ) {
      errs.destinoCidade = "Origem e destino não podem ser iguais.";
    }

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
        origemDescricao: origemDescricao.trim(),
        origemCidade: origemCidade.trim(),
        origemEstado: origemEstado.trim().toUpperCase(),
        destinoDescricao: destinoDescricao.trim(),
        destinoCidade: destinoCidade.trim(),
        destinoEstado: destinoEstado.trim().toUpperCase(),
        dataSaida: new Date(dataSaida).toISOString(),
        dataChegada: new Date(dataChegada).toISOString(),
        precoAssento: Number(precoAssento),
        possuiIngresso,
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

      {/* Origem */}
      <fieldset className="rounded-xl border border-zinc-800 p-4">
        <legend className="text-sm font-semibold text-zinc-300">Origem</legend>
        <div className="mt-3 space-y-4">
          <div>
            <label htmlFor="origemDescricao" className="mb-1 block text-xs text-zinc-400">
              Descrição
            </label>
            <input
              id="origemDescricao"
              type="text"
              className={`${inputClass} ${errors.origemDescricao ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
              value={origemDescricao}
              onChange={(e) => setOrigemDescricao(e.target.value)}
              placeholder="Ex: Rodoviária Central"
            />
            {errors.origemDescricao && <p className={errorClass}>{errors.origemDescricao}</p>}
          </div>
          <div className="grid grid-cols-3 gap-3">
            <div className="col-span-2">
              <label htmlFor="origemCidade" className="mb-1 block text-xs text-zinc-400">
                Cidade
              </label>
              <input
                id="origemCidade"
                type="text"
                className={`${inputClass} ${errors.origemCidade ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
                value={origemCidade}
                onChange={(e) => setOrigemCidade(e.target.value)}
                placeholder="Rio de Janeiro"
              />
              {errors.origemCidade && <p className={errorClass}>{errors.origemCidade}</p>}
            </div>
            <div>
              <label htmlFor="origemEstado" className="mb-1 block text-xs text-zinc-400">
                UF
              </label>
              <input
                id="origemEstado"
                type="text"
                maxLength={2}
                className={`${inputClass} ${errors.origemEstado ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
                value={origemEstado}
                onChange={(e) => setOrigemEstado(e.target.value.toUpperCase())}
                placeholder="RJ"
              />
              {errors.origemEstado && <p className={errorClass}>{errors.origemEstado}</p>}
            </div>
          </div>
        </div>
      </fieldset>

      {/* Destino */}
      <fieldset className="rounded-xl border border-zinc-800 p-4">
        <legend className="text-sm font-semibold text-zinc-300">Destino</legend>
        <div className="mt-3 space-y-4">
          <div>
            <label htmlFor="destinoDescricao" className="mb-1 block text-xs text-zinc-400">
              Descrição
            </label>
            <input
              id="destinoDescricao"
              type="text"
              className={`${inputClass} ${errors.destinoDescricao ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
              value={destinoDescricao}
              onChange={(e) => setDestinoDescricao(e.target.value)}
              placeholder="Ex: Estádio Y"
            />
            {errors.destinoDescricao && <p className={errorClass}>{errors.destinoDescricao}</p>}
          </div>
          <div className="grid grid-cols-3 gap-3">
            <div className="col-span-2">
              <label htmlFor="destinoCidade" className="mb-1 block text-xs text-zinc-400">
                Cidade
              </label>
              <input
                id="destinoCidade"
                type="text"
                className={`${inputClass} ${errors.destinoCidade ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
                value={destinoCidade}
                onChange={(e) => setDestinoCidade(e.target.value)}
                placeholder="São Paulo"
              />
              {errors.destinoCidade && <p className={errorClass}>{errors.destinoCidade}</p>}
            </div>
            <div>
              <label htmlFor="destinoEstado" className="mb-1 block text-xs text-zinc-400">
                UF
              </label>
              <input
                id="destinoEstado"
                type="text"
                maxLength={2}
                className={`${inputClass} ${errors.destinoEstado ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
                value={destinoEstado}
                onChange={(e) => setDestinoEstado(e.target.value.toUpperCase())}
                placeholder="SP"
              />
              {errors.destinoEstado && <p className={errorClass}>{errors.destinoEstado}</p>}
            </div>
          </div>
        </div>
      </fieldset>

      {/* Data de Saída */}
      <div>
        <label htmlFor="dataSaida" className="mb-1 block text-sm font-medium text-zinc-300">
          Data e hora de saída
        </label>
        <input
          id="dataSaida"
          type="datetime-local"
          className={`${inputClass} ${errors.dataSaida ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={dataSaida}
          onChange={(e) => setDataSaida(e.target.value)}
        />
        {errors.dataSaida && <p className={errorClass}>{errors.dataSaida}</p>}
      </div>

      {/* Data de Chegada */}
      <div>
        <label htmlFor="dataChegada" className="mb-1 block text-sm font-medium text-zinc-300">
          Data e hora de chegada
        </label>
        <input
          id="dataChegada"
          type="datetime-local"
          className={`${inputClass} ${errors.dataChegada ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={dataChegada}
          onChange={(e) => setDataChegada(e.target.value)}
        />
        {errors.dataChegada && <p className={errorClass}>{errors.dataChegada}</p>}
      </div>

      {/* Preço do Assento */}
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

      {/* API error */}
      {errors.api && (
        <div className="rounded-lg border border-red-700/60 bg-red-950/40 px-4 py-3 text-sm text-red-300">
          {errors.api}
        </div>
      )}

      <VbButton type="submit" disabled={loading} className="w-full">
        {loading ? "Salvando…" : submitLabel}
      </VbButton>
    </form>
  );
}
