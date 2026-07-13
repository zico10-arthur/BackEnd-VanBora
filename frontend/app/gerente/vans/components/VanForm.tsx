"use client";

import { useState, type FormEvent } from "react";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import type { CriarVanRequest, VanResponse } from "@/lib/api/types";
import { ApiError } from "@/lib/api/http";

type VanFormData = CriarVanRequest;

const PLACA_REGEX = /^[A-Z]{3}[0-9][A-Z][0-9]{2}$/;

type FieldErrors = Partial<Record<keyof VanFormData | "api", string>>;

type VanFormProps = {
  van?: VanResponse;
  onSubmit: (data: CriarVanRequest) => Promise<void>;
  submitLabel: string;
};

export function VanForm({ van, onSubmit, submitLabel }: VanFormProps) {
  const [nome, setNome] = useState(van?.nome ?? "");
  const [placa, setPlaca] = useState(van?.placa ?? "");
  const [modelo, setModelo] = useState(van?.modelo ?? "");
  const [capacidade, setCapacidade] = useState(van?.capacidade?.toString() ?? "");
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<FieldErrors>({});

  const isEditing = !!van;

  function validate(): FieldErrors {
    const errs: FieldErrors = {};

    if (!nome.trim()) errs.nome = "Informe o nome da van.";
    if (!modelo.trim()) errs.modelo = "Informe o modelo.";

    const placaUpper = placa.toUpperCase().trim();
    if (!placaUpper) {
      errs.placa = "Informe a placa.";
    } else if (!PLACA_REGEX.test(placaUpper)) {
      errs.placa = "Formato inválido. Use o padrão Mercosul: AAA0A00.";
    }

    if (!isEditing) {
      const cap = Number(capacidade);
      if (!capacidade || isNaN(cap)) {
        errs.capacidade = "Informe a capacidade.";
      } else if (cap < 8 || cap > 25) {
        errs.capacidade = "Capacidade deve ser entre 8 e 25 lugares.";
      }
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
      const body: CriarVanRequest = {
        nome: nome.trim(),
        placa: placa.toUpperCase().trim(),
        modelo: modelo.trim(),
        capacidade: Number(capacidade),
      };
      await onSubmit(body);
    } catch (err: unknown) {
      if (err instanceof ApiError && err.status === 409) {
        if (err.code === "NOME_EM_USO") {
          setErrors({ nome: err.message });
        } else {
          setErrors({ placa: "Esta placa já está cadastrada." });
        }
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
      {/* Nome */}
      <div>
        <label htmlFor="nome" className="mb-1 block text-sm font-medium text-zinc-300">
          Nome da van
        </label>
        <input
          id="nome"
          type="text"
          className={`${inputClass} ${errors.nome ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={nome}
          onChange={(e) => setNome(e.target.value)}
          placeholder="Ex: Van Principal"
        />
        {errors.nome && <p className={errorClass}>{errors.nome}</p>}
      </div>

      {/* Placa */}
      <div>
        <label htmlFor="placa" className="mb-1 block text-sm font-medium text-zinc-300">
          Placa
        </label>
        <input
          id="placa"
          type="text"
          className={`${inputClass} ${errors.placa ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={placa}
          onChange={(e) => setPlaca(e.target.value.toUpperCase())}
          placeholder="AAA0A00"
          maxLength={7}
        />
        {errors.placa && <p className={errorClass}>{errors.placa}</p>}
      </div>

      {/* Modelo (somente criação) */}
      {!isEditing && (
        <div>
          <label htmlFor="modelo" className="mb-1 block text-sm font-medium text-zinc-300">
            Modelo
          </label>
          <input
            id="modelo"
            type="text"
            className={inputClass}
            value={modelo}
            onChange={(e) => setModelo(e.target.value)}
            placeholder="Ex: Mercedes-Benz Sprinter"
          />
          {errors.modelo && <p className={errorClass}>{errors.modelo}</p>}
        </div>
      )}

      {/* Capacidade (somente criação) */}
      {!isEditing && (
        <div>
          <label htmlFor="capacidade" className="mb-1 block text-sm font-medium text-zinc-300">
            Capacidade (lugares)
          </label>
          <input
            id="capacidade"
            type="number"
            className={inputClass}
            value={capacidade}
            onChange={(e) => setCapacidade(e.target.value)}
            placeholder="8 a 25"
            min={8}
            max={25}
          />
          {errors.capacidade && <p className={errorClass}>{errors.capacidade}</p>}
        </div>
      )}

      {/* API error */}
      {errors.api && (
        <p className="rounded-lg border border-red-700/60 bg-red-950/40 px-4 py-3 text-sm text-red-300">
          {errors.api}
        </p>
      )}

      <VbButton type="submit" disabled={loading} className="w-full">
        {loading ? "Salvando…" : submitLabel}
      </VbButton>
    </form>
  );
}
