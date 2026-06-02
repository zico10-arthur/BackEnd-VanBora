"use client";

import { useRouter } from "next/navigation";
import { useMemo, useState } from "react";
import type { BotafogoGame } from "@/lib/mockBotafogoGames";
import { MOCK_BOTAFOGO_GAMES } from "@/lib/mockBotafogoGames";

const brl = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

/** Preço da passagem em reais (valor estritamente positivo). */
function parsePrice(raw: string): number | null {
  const n = Number.parseFloat(raw.replace(",", "."));
  if (Number.isNaN(n) || n <= 0) return null;
  return n;
}

function parsePositiveInt(raw: string): number | null {
  const n = Number.parseInt(raw, 10);
  if (Number.isNaN(n) || n < 1) return null;
  return n;
}

export interface TripPublishPayload {
  game: BotafogoGame;
  departurePoint: string;
  vehicleModelColor: string;
  vehiclePlate: string;
  capacity: number;
  ticketPrice: number;
  minimumQuorum: number;
  financials: {
    maxRevenue: number;
    breakEvenRevenue: number;
    quorumShareOfCapacity: number;
  };
}

export function TripRegistrationForm() {
  const router = useRouter();
  const [gameId, setGameId] = useState("");
  const [departurePoint, setDeparturePoint] = useState("");
  const [vehicleModelColor, setVehicleModelColor] = useState("");
  const [vehiclePlate, setVehiclePlate] = useState("");
  const [capacityRaw, setCapacityRaw] = useState("");
  const [priceRaw, setPriceRaw] = useState("");
  const [quorumRaw, setQuorumRaw] = useState("");
  const [submitError, setSubmitError] = useState<string | null>(null);

  const capacity = parsePositiveInt(capacityRaw);
  const ticketPrice = parsePrice(priceRaw);
  const quorumParsed = parsePositiveInt(quorumRaw);

  const quorum =
    capacity != null && quorumParsed != null ? Math.min(quorumParsed, capacity) : quorumParsed;

  const financials = useMemo(() => {
    if (capacity == null || ticketPrice == null || quorum == null) return null;
    const maxRevenue = capacity * ticketPrice;
    const breakEvenRevenue = quorum * ticketPrice;
    const quorumShareOfCapacity = capacity > 0 ? quorum / capacity : 0;
    return { maxRevenue, breakEvenRevenue, quorumShareOfCapacity };
  }, [capacity, ticketPrice, quorum]);

  const handleCapacityChange = (v: string) => {
    setCapacityRaw(v);
    const cap = parsePositiveInt(v);
    const q = parsePositiveInt(quorumRaw);
    if (cap != null && q != null && q > cap) {
      setQuorumRaw(String(cap));
    }
  };

  const handleQuorumChange = (v: string) => {
    setQuorumRaw(v);
    const q = parsePositiveInt(v);
    const cap = parsePositiveInt(capacityRaw);
    if (cap != null && q != null && q > cap) {
      setQuorumRaw(String(cap));
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);

    if (!gameId.trim()) {
      setSubmitError("Selecione um jogo.");
      return;
    }
    if (!departurePoint.trim()) {
      setSubmitError("Informe o ponto de partida.");
      return;
    }
    if (!vehicleModelColor.trim()) {
      setSubmitError("Informe o modelo e a cor do veículo.");
      return;
    }
    if (!vehiclePlate.trim()) {
      setSubmitError("Informe a placa do veículo.");
      return;
    }
    if (capacity == null) {
      setSubmitError("Informe a capacidade da van (número inteiro ≥ 1).");
      return;
    }
    if (ticketPrice == null) {
      setSubmitError("Informe o valor da passagem.");
      return;
    }
    if (quorum == null) {
      setSubmitError("Informe o quórum mínimo (break-even).");
      return;
    }
    if (quorum > capacity) {
      setSubmitError("O quórum não pode ser maior que a capacidade.");
      return;
    }
    if (!financials) {
      setSubmitError("Não foi possível calcular o resumo financeiro.");
      return;
    }

    const game = MOCK_BOTAFOGO_GAMES.find((g) => g.id === gameId);
    if (!game) {
      setSubmitError("Jogo inválido.");
      return;
    }

    const payload: TripPublishPayload = {
      game,
      departurePoint: departurePoint.trim(),
      vehicleModelColor: vehicleModelColor.trim(),
      vehiclePlate: vehiclePlate.trim().toUpperCase(),
      capacity,
      ticketPrice,
      minimumQuorum: quorum,
      financials: {
        maxRevenue: financials.maxRevenue,
        breakEvenRevenue: financials.breakEvenRevenue,
        quorumShareOfCapacity: financials.quorumShareOfCapacity,
      },
    };

    console.log("[VanBora] Publicar viagem — payload:", payload);
    router.push(`/motorista/viagem/${encodeURIComponent(game.id)}`);
  };

  return (
    <div className="mx-auto max-w-2xl">
      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="space-y-2">
          <label htmlFor="game" className="block text-sm font-semibold text-zinc-200">
            Jogo / evento
          </label>
          <select
            id="game"
            value={gameId}
            onChange={(e) => setGameId(e.target.value)}
            className="w-full rounded-lg border border-zinc-700 bg-zinc-800 px-3 py-2.5 text-sm text-zinc-100 shadow-sm outline-none ring-amber-500/0 transition focus:border-amber-500/50 focus:ring-2 focus:ring-amber-500/30"
          >
            <option value="">Selecione o jogo no Nilton Santos</option>
            {MOCK_BOTAFOGO_GAMES.map((g) => (
              <option key={g.id} value={g.id}>
                {g.homeTeam} x {g.awayTeam} — {g.dateLabel} ({g.competition})
              </option>
            ))}
          </select>
        </div>

        <div className="space-y-2">
          <label htmlFor="departure" className="block text-sm font-semibold text-zinc-200">
            Ponto de partida
          </label>
          <input
            id="departure"
            type="text"
            autoComplete="off"
            placeholder="Ex.: Barra da Tijuca — posto X"
            value={departurePoint}
            onChange={(e) => setDeparturePoint(e.target.value)}
            className="w-full rounded-lg border border-zinc-700 bg-zinc-800 px-3 py-2.5 text-sm text-zinc-100 placeholder:text-zinc-500 outline-none transition focus:border-amber-500/50 focus:ring-2 focus:ring-amber-500/30"
          />
        </div>

        <div className="grid min-w-0 grid-cols-2 gap-3 sm:gap-4">
          <div className="min-w-0 space-y-2">
            <label htmlFor="vehicle-model" className="block text-sm font-semibold text-zinc-200">
              Modelo / cor
            </label>
            <input
              id="vehicle-model"
              type="text"
              autoComplete="off"
              placeholder="Ex.: Mercedes Sprinter Branca"
              value={vehicleModelColor}
              onChange={(e) => setVehicleModelColor(e.target.value)}
              className="w-full min-w-0 rounded-lg border border-zinc-700 bg-zinc-900 px-3 py-2.5 text-sm text-zinc-100 placeholder:text-zinc-500 outline-none transition focus:border-amber-500 focus:ring-1 focus:ring-amber-500"
            />
          </div>
          <div className="min-w-0 space-y-2">
            <label htmlFor="vehicle-plate" className="block text-sm font-semibold text-zinc-200">
              Placa
            </label>
            <input
              id="vehicle-plate"
              type="text"
              autoComplete="off"
              placeholder="Ex.: ABC-1234"
              value={vehiclePlate}
              onChange={(e) => setVehiclePlate(e.target.value)}
              className="w-full min-w-0 rounded-lg border border-zinc-700 bg-zinc-900 px-3 py-2.5 text-sm text-zinc-100 placeholder:text-zinc-500 outline-none transition focus:border-amber-500 focus:ring-1 focus:ring-amber-500"
            />
          </div>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-2">
            <label htmlFor="capacity" className="block text-sm font-semibold text-zinc-200">
              Capacidade da van (lugares)
            </label>
            <input
              id="capacity"
              type="number"
              min={1}
              step={1}
              inputMode="numeric"
              placeholder="15"
              value={capacityRaw}
              onChange={(e) => handleCapacityChange(e.target.value)}
              className="w-full rounded-lg border border-zinc-700 bg-zinc-800 px-3 py-2.5 text-sm text-zinc-100 outline-none transition focus:border-amber-500/50 focus:ring-2 focus:ring-amber-500/30"
            />
          </div>
          <div className="space-y-2">
            <label htmlFor="price" className="block text-sm font-semibold text-zinc-200">
              Valor da passagem (R$)
            </label>
            <input
              id="price"
              type="number"
              min={0}
              step={0.01}
              inputMode="decimal"
              placeholder="60,00"
              value={priceRaw}
              onChange={(e) => setPriceRaw(e.target.value)}
              className="w-full rounded-lg border border-zinc-700 bg-zinc-800 px-3 py-2.5 text-sm text-zinc-100 outline-none transition focus:border-amber-500/50 focus:ring-2 focus:ring-amber-500/30"
            />
          </div>
        </div>

        <div className="space-y-2">
          <label htmlFor="quorum" className="block text-sm font-semibold text-zinc-200">
            Quórum mínimo (break-even)
          </label>
          <input
            id="quorum"
            type="number"
            min={1}
            max={capacity ?? undefined}
            step={1}
            inputMode="numeric"
            placeholder="Mínimo de passageiros para a viagem sair"
            value={quorumRaw}
            onChange={(e) => handleQuorumChange(e.target.value)}
            className="w-full rounded-lg border border-zinc-700 bg-zinc-800 px-3 py-2.5 text-sm text-zinc-100 outline-none transition focus:border-amber-500/50 focus:ring-2 focus:ring-amber-500/30"
          />
          <p className="text-xs text-zinc-500">
            Não pode ultrapassar a capacidade da van; o valor é limitado automaticamente.
          </p>
        </div>

        {financials && capacity != null && ticketPrice != null && quorum != null && (
          <div className="rounded-xl border border-amber-500/35 bg-gradient-to-br from-amber-500/15 via-zinc-900/80 to-zinc-900 p-4 shadow-[0_0_0_1px_rgba(240,165,0,0.08)] sm:p-5">
            <h3 className="text-sm font-bold uppercase tracking-wide text-amber-400/95">
              Resumo da operação
            </h3>
            <dl className="mt-4 space-y-3 text-sm">
              <div className="flex flex-wrap items-baseline justify-between gap-2 border-b border-zinc-700/80 pb-3">
                <dt className="text-zinc-400">Receita máxima (van cheia)</dt>
                <dd className="font-semibold tabular-nums text-white">
                  {brl.format(financials.maxRevenue)}
                </dd>
              </div>
              <div className="flex flex-wrap items-baseline justify-between gap-2 border-b border-zinc-700/80 pb-3">
                <dt className="text-zinc-400">Receita no break-even ({quorum} lugares)</dt>
                <dd className="font-semibold tabular-nums text-amber-300">
                  {brl.format(financials.breakEvenRevenue)}
                </dd>
              </div>
              <div className="flex flex-wrap items-baseline justify-between gap-2">
                <dt className="text-zinc-400">Quórum vs. capacidade</dt>
                <dd className="font-medium tabular-nums text-zinc-200">
                  {Math.round(financials.quorumShareOfCapacity * 100)}% dos lugares
                </dd>
              </div>
            </dl>
          </div>
        )}

        {submitError && (
          <p className="text-sm font-medium text-red-400" role="alert">
            {submitError}
          </p>
        )}

        <button
          type="submit"
          className="w-full rounded-xl bg-[#F0A500] py-3.5 text-sm font-bold uppercase tracking-wide text-[#0D0D0D] shadow-[0_4px_24px_rgba(240,165,0,0.25)] transition hover:bg-[#ffb31a] focus:outline-none focus-visible:ring-2 focus-visible:ring-amber-400 focus-visible:ring-offset-2 focus-visible:ring-offset-[#0D0D0D] active:scale-[0.99]"
        >
          Publicar viagem e gerar link
        </button>
      </form>
    </div>
  );
}
