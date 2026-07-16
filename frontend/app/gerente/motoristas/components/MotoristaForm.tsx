"use client";

import { useState, type FormEvent } from "react";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import type { CriarMotoristaRequest, MotoristaResponse } from "@/lib/api/types";
import { ApiError } from "@/lib/api/http";

type MotoristaFormData = CriarMotoristaRequest;

const CPF_REGEX = /^\d{11}$/;
const CNH_REGEX = /^\d{11}$/;
const TELEFONE_REGEX = /^\d{10,11}$/;

type FieldErrors = Partial<Record<keyof MotoristaFormData | "api", string>>;

type MotoristaFormProps = {
  motorista?: MotoristaResponse;
  onSubmit: (data: CriarMotoristaRequest) => Promise<void>;
  submitLabel: string;
};

export function MotoristaForm({ motorista, onSubmit, submitLabel }: MotoristaFormProps) {
  const [nome, setNome] = useState(motorista?.nome ?? "");
  const [cpf, setCpf] = useState(motorista?.cpf ?? "");
  const [telefone, setTelefone] = useState(motorista?.telefone ?? "");
  const [cnh, setCnh] = useState(motorista?.cnh ?? "");
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<FieldErrors>({});

  const isEditing = !!motorista;

  function validate(): FieldErrors {
    const errs: FieldErrors = {};

    if (!nome.trim()) errs.nome = "Informe o nome do motorista.";

    const cpfClean = cpf.replace(/\D/g, "");
    if (!cpfClean) {
      errs.cpf = "Informe o CPF.";
    } else if (!CPF_REGEX.test(cpfClean)) {
      errs.cpf = "CPF deve ter 11 dígitos.";
    }

    if (telefone) {
      const telClean = telefone.replace(/\D/g, "");
      if (!TELEFONE_REGEX.test(telClean)) {
        errs.telefone = "Telefone deve ter 10 ou 11 dígitos (DDD + número).";
      }
    }

    const cnhClean = cnh.replace(/\D/g, "");
    if (!cnhClean) {
      errs.cnh = "Informe a CNH.";
    } else if (!CNH_REGEX.test(cnhClean)) {
      errs.cnh = "CNH deve ter 11 dígitos.";
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
      const body: CriarMotoristaRequest = {
        nome: nome.trim(),
        cpf: cpf.replace(/\D/g, ""),
        telefone: telefone ? telefone.replace(/\D/g, "") : null,
        cnh: cnh.replace(/\D/g, ""),
      };
      await onSubmit(body);
    } catch (err: unknown) {
      if (err instanceof ApiError && err.status === 409) {
        if (err.code === "CPF_JA_CADASTRADO") {
          setErrors({ cpf: err.message });
        } else if (err.code === "CNH_JA_CADASTRADA") {
          setErrors({ cnh: err.message });
        } else {
          setErrors({ api: err.message });
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
          Nome
        </label>
        <input
          id="nome"
          type="text"
          className={`${inputClass} ${errors.nome ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={nome}
          onChange={(e) => setNome(e.target.value)}
          placeholder="Ex: Carlos Santos"
        />
        {errors.nome && <p className={errorClass}>{errors.nome}</p>}
      </div>

      {/* CPF */}
      <div>
        <label htmlFor="cpf" className="mb-1 block text-sm font-medium text-zinc-300">
          CPF
        </label>
        <input
          id="cpf"
          type="text"
          className={`${inputClass} ${errors.cpf ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={cpf}
          onChange={(e) => setCpf(e.target.value.replace(/\D/g, ""))}
          placeholder="00000000000"
          maxLength={11}
          disabled={isEditing}
        />
        {errors.cpf && <p className={errorClass}>{errors.cpf}</p>}
      </div>

      {/* Telefone */}
      <div>
        <label htmlFor="telefone" className="mb-1 block text-sm font-medium text-zinc-300">
          Telefone{" "}
          <span className="font-normal text-zinc-500">(opcional)</span>
        </label>
        <input
          id="telefone"
          type="text"
          className={`${inputClass} ${errors.telefone ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={telefone}
          onChange={(e) => setTelefone(e.target.value.replace(/\D/g, ""))}
          placeholder="11999999999"
          maxLength={11}
        />
        {errors.telefone && <p className={errorClass}>{errors.telefone}</p>}
      </div>

      {/* CNH */}
      <div>
        <label htmlFor="cnh" className="mb-1 block text-sm font-medium text-zinc-300">
          CNH
        </label>
        <input
          id="cnh"
          type="text"
          className={`${inputClass} ${errors.cnh ? "border-red-500 focus:border-red-500 focus:ring-red-500" : ""}`}
          value={cnh}
          onChange={(e) => setCnh(e.target.value.replace(/\D/g, ""))}
          placeholder="00000000000"
          maxLength={11}
        />
        {errors.cnh && <p className={errorClass}>{errors.cnh}</p>}
      </div>

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
