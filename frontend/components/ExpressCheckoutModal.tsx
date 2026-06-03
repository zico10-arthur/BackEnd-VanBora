"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useId, useMemo, useState } from "react";
import { useAuth } from "@/components/providers/AuthProvider";
import { ApiError } from "@/lib/api/http";
import { criarReserva, pagarReserva } from "@/lib/api/reservas";
import { apenasDigitos, formatBrl } from "@/lib/format";
import { savePendingCheckout } from "@/lib/payment/pending-storage";
import { seatLabelToNumber } from "@/lib/seats";
import { LegalDocumentModal, type LegalDocumentKind } from "./LegalDocumentModal";

function formatWhatsAppInput(raw: string): string {
  const d = raw.replace(/\D/g, "").slice(0, 11);
  if (d.length <= 2) return `(${d}`;
  if (d.length <= 7) return `(${d.slice(0, 2)}) ${d.slice(2)}`;
  return `(${d.slice(0, 2)}) ${d.slice(2, 7)}-${d.slice(7)}`;
}

function formatCpfInput(raw: string): string {
  const d = raw.replace(/\D/g, "").slice(0, 11);
  if (d.length <= 3) return d;
  if (d.length <= 6) return `${d.slice(0, 3)}.${d.slice(3)}`;
  if (d.length <= 9) return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6)}`;
  return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6, 9)}-${d.slice(9)}`;
}

function isFormValid(name: string, whatsapp: string, cpf: string): boolean {
  if (name.trim().length < 3) return false;
  const w = apenasDigitos(whatsapp);
  if (w.length < 10) return false;
  return apenasDigitos(cpf).length === 11;
}

export interface ExpressCheckoutModalProps {
  open: boolean;
  onClose: () => void;
  viagemVanId: string;
  eventName: string;
  seatLabel: string;
  departureLocation?: string;
  ticketPrice?: number;
  vehicleModelColor?: string;
  vehiclePlate?: string;
}

export function ExpressCheckoutModal({
  open,
  onClose,
  viagemVanId,
  eventName,
  seatLabel,
  departureLocation = "—",
  ticketPrice = 0,
  vehicleModelColor = "—",
  vehiclePlate = "—",
}: ExpressCheckoutModalProps) {
  const titleId = useId();
  const router = useRouter();
  const { user, ready } = useAuth();
  const [name, setName] = useState("");
  const [whatsapp, setWhatsapp] = useState("");
  const [cpf, setCpf] = useState("");
  const [step, setStep] = useState<"form" | "loading" | "error">("form");
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [legalDoc, setLegalDoc] = useState<LegalDocumentKind | null>(null);

  const formOk = useMemo(() => isFormValid(name, whatsapp, cpf), [name, whatsapp, cpf]);
  const emailOk = Boolean(user?.email?.trim());

  useEffect(() => {
    if (!open) return;
    const prev = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.body.style.overflow = prev;
    };
  }, [open]);

  useEffect(() => {
    if (!open) return;
    setStep("form");
    setErrorMsg(null);
    if (user) {
      setName(user.nome);
    } else {
      setName("");
    }
    setWhatsapp("");
    setCpf("");
  }, [open, user]);

  const handleClose = useCallback(() => onClose(), [onClose]);

  const pagarComMercadoPago = useCallback(async () => {
    if (!formOk) return;
    if (!user) {
      const next = encodeURIComponent(`/reserva/${viagemVanId}`);
      router.push(`/entrar?next=${next}`);
      return;
    }
    if (!user.email?.trim()) {
      setErrorMsg("Sua conta precisa de um e-mail válido para concluir a reserva.");
      setStep("error");
      return;
    }

    const numeroAssento = seatLabelToNumber(seatLabel);
    if (numeroAssento < 1) {
      setErrorMsg("Assento inválido.");
      setStep("error");
      return;
    }

    setStep("loading");
    setErrorMsg(null);

    try {
      const reserva = await criarReserva({
        viagemVanId,
        itens: [
          {
            numeroAssento,
            nomePassageiro: name.trim(),
            emailPassageiro: user.email.trim(),
            telefonePassageiro: apenasDigitos(whatsapp),
            cpfPassageiro: apenasDigitos(cpf),
          },
        ],
      });

      const pagamento = await pagarReserva(reserva.id);
      const url = pagamento.sandboxInitPoint || pagamento.initPoint;
      if (!url) throw new Error("Link de pagamento indisponível");

      savePendingCheckout({
        reservaId: reserva.id,
        eventName,
        seatLabel,
        valorAPagar: pagamento.valorAPagar,
      });

      window.location.href = url;
    } catch (e) {
      const msg = e instanceof ApiError ? e.message : "Não foi possível iniciar o pagamento.";
      setErrorMsg(msg);
      setStep("error");
    }
  }, [formOk, user, viagemVanId, seatLabel, name, whatsapp, cpf, eventName, router]);

  if (!open) return null;

  const loginNext = encodeURIComponent(`/reserva/${viagemVanId}`);

  return (
    <>
      {legalDoc ? <LegalDocumentModal open kind={legalDoc} onClose={() => setLegalDoc(null)} /> : null}
      <div className="fixed inset-0 z-[100] flex items-end justify-center sm:items-center sm:p-4">
        <button
          type="button"
          className="absolute inset-0 bg-black/80 backdrop-blur-sm"
          aria-label="Fechar"
          onClick={step === "loading" ? undefined : handleClose}
        />
        <div
          role="dialog"
          aria-modal="true"
          aria-labelledby={titleId}
          className="relative flex max-h-[92vh] w-full max-w-lg flex-col overflow-hidden rounded-t-2xl border border-van-amber/20 bg-zinc-950 shadow-2xl sm:max-h-[90vh] sm:rounded-2xl"
        >
          <div className="flex shrink-0 items-center justify-between border-b border-white/10 px-5 py-4">
            <h2 id={titleId} className="text-lg font-bold text-white">
              {step === "loading" ? "Preparando pagamento…" : "Finalizar reserva"}
            </h2>
            {step !== "loading" ? (
              <button
                type="button"
                onClick={handleClose}
                className="rounded-lg p-2 text-zinc-500 hover:bg-white/10 hover:text-white"
                aria-label="Fechar"
              >
                ✕
              </button>
            ) : null}
          </div>

          <div className="overflow-y-auto px-5 py-5">
            {step === "loading" ? (
              <div className="flex flex-col items-center gap-6 py-12 text-center">
                <div className="h-12 w-12 animate-spin rounded-full border-2 border-zinc-700 border-t-van-amber" />
                <p className="text-sm text-zinc-400">Você será direcionado para pagar com Pix de forma segura.</p>
              </div>
            ) : step === "error" ? (
              <div className="space-y-4 py-4">
                <p className="rounded-lg border border-red-500/30 bg-red-950/30 px-4 py-3 text-sm text-red-100">
                  {errorMsg}
                </p>
                <button
                  type="button"
                  onClick={() => setStep("form")}
                  className="w-full rounded-xl bg-van-amber py-3 font-bold text-van-void"
                >
                  Tentar novamente
                </button>
              </div>
            ) : (
              <div className="flex flex-col gap-5">
                <div className="rounded-xl border border-zinc-800 bg-zinc-900/60 p-4">
                  <p className="text-xs font-semibold uppercase tracking-wider text-van-amber">Resumo</p>
                  <p className="mt-1 font-semibold text-white">{eventName}</p>
                  <p className="mt-2 text-sm text-zinc-400">
                    Assento <span className="font-bold text-white">{seatLabel}</span> · {departureLocation}
                  </p>
                  <p className="mt-1 text-xs text-zinc-500">
                    {vehicleModelColor} · {vehiclePlate}
                  </p>
                  <p className="mt-3 border-t border-white/10 pt-3 text-lg font-black text-van-amber">
                    A partir de {formatBrl(ticketPrice)} <span className="text-sm font-normal text-zinc-500">/ assento</span>
                  </p>
                  <p className="mt-1 text-xs text-zinc-500">
                    Taxas da plataforma, se houver, entram no total exibido no checkout Pix.
                  </p>
                </div>

                {ready && user && !emailOk ? (
                  <div className="rounded-xl border border-red-500/25 bg-red-950/20 p-4 text-sm text-red-100">
                    <p>Atualize o e-mail da sua conta para receber a confirmação da reserva.</p>
                  </div>
                ) : null}

                {ready && !user ? (
                  <div className="rounded-xl border border-amber-500/25 bg-amber-500/5 p-4 text-sm text-amber-100/90">
                    <p>Faça login para concluir a reserva.</p>
                    <Link href={`/entrar?next=${loginNext}`} className="mt-2 inline-block font-bold text-van-amber underline">
                      Entrar ou criar conta
                    </Link>
                  </div>
                ) : null}

                <div className="flex flex-col gap-3">
                  <label className="block">
                    <span className="mb-1.5 block text-xs font-medium text-zinc-400">Nome completo</span>
                    <input
                      value={name}
                      onChange={(e) => setName(e.target.value)}
                      className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-4 py-3 text-sm text-white focus:border-van-amber focus:outline-none focus:ring-1 focus:ring-van-amber"
                    />
                  </label>
                  <label className="block">
                    <span className="mb-1.5 block text-xs font-medium text-zinc-400">WhatsApp</span>
                    <input
                      type="tel"
                      value={whatsapp}
                      onChange={(e) => setWhatsapp(formatWhatsAppInput(e.target.value))}
                      className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-4 py-3 text-sm text-white focus:border-van-amber focus:outline-none"
                    />
                  </label>
                  <label className="block">
                    <span className="mb-1.5 block text-xs font-medium text-zinc-400">CPF</span>
                    <input
                      value={cpf}
                      onChange={(e) => setCpf(formatCpfInput(e.target.value))}
                      className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-4 py-3 text-sm text-white focus:border-van-amber focus:outline-none"
                    />
                  </label>
                </div>

                <button
                  type="button"
                  disabled={!formOk || (Boolean(user) && !emailOk)}
                  onClick={pagarComMercadoPago}
                  className="w-full rounded-xl bg-van-amber py-4 text-base font-bold text-van-void shadow-[0_8px_30px_rgba(240,165,0,0.35)] disabled:opacity-50"
                >
                  Pagar com Pix
                </button>

                <p className="text-center text-[11px] text-zinc-500">
                  Ao pagar, você concorda com os{" "}
                  <button type="button" onClick={() => setLegalDoc("terms")} className="text-van-amber underline">
                    Termos
                  </button>{" "}
                  e{" "}
                  <button type="button" onClick={() => setLegalDoc("privacy")} className="text-van-amber underline">
                    Privacidade
                  </button>
                  . Reserva válida por 10 minutos.
                </p>
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  );
}
