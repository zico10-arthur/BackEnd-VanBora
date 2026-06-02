"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { Header } from "@/components/Header";
import { BoardingPassSuccess } from "@/components/BoardingPassSuccess";
import { useAuth } from "@/components/providers/AuthProvider";
import { labelReservaStatus } from "@/lib/copy";
import type { ReservaResponse } from "@/lib/api/types";
import { mercadoPagoCollectionStatus, resolveReservaIdFromReturn } from "@/lib/payment/return-params";
import { pollReservaStatus } from "@/lib/payment/poll-reserva";
import {
  clearPendingCheckout,
  readPendingCheckout,
  type PendingReservaCheckout,
} from "@/lib/payment/pending-storage";
import { formatBrl } from "@/lib/format";

type Variant = "success" | "failure" | "pending";

type Phase = "loading" | "confirmed" | "waiting" | "error" | "idle";

type Props = {
  variant: Variant;
};

export function ReservaPagamentoRetorno({ variant }: Props) {
  const searchParams = useSearchParams();
  const queryKey = searchParams.toString();
  const { user, ready } = useAuth();
  const [phase, setPhase] = useState<Phase>("loading");
  const [message, setMessage] = useState<string | null>(null);
  const [pending, setPending] = useState<PendingReservaCheckout | null>(null);
  const [reserva, setReserva] = useState<ReservaResponse | null>(null);

  const titles = useMemo(
    () => ({
      failure: "Pagamento não concluído",
      pending: "Pagamento em processamento",
      success: "Reserva confirmada",
      waiting: "Confirmando pagamento",
      error: "Pagamento não concluído",
    }),
    [],
  );

  useEffect(() => {
    setPending(readPendingCheckout());
  }, []);

  useEffect(() => {
    if (!ready) return;

    if (!user) {
      setPhase("idle");
      setMessage("Faça login para acompanhar o status da sua reserva.");
      return;
    }

    if (variant === "failure") {
      setPhase("idle");
      setMessage("O pagamento não foi concluído. Você pode tentar novamente em Minhas reservas.");
      return;
    }

    if (variant === "pending") {
      setPhase("waiting");
      setMessage("Seu pagamento está em processamento. Assim que for aprovado, a reserva será confirmada.");
      return;
    }

    const reservaId = resolveReservaIdFromReturn(searchParams);
    if (!reservaId) {
      setPhase("waiting");
      setMessage("Estamos confirmando seu pagamento. Consulte Minhas reservas em instantes.");
      return;
    }

    const mpStatus = mercadoPagoCollectionStatus(searchParams);
    if (mpStatus === "rejected" || mpStatus === "failure") {
      setPhase("error");
      setMessage("Pagamento recusado ou cancelado.");
      return;
    }

    let cancelled = false;
    setPhase("loading");

    (async () => {
      const result = await pollReservaStatus(reservaId);
      if (cancelled) return;

      if (result.kind === "confirmed") {
        clearPendingCheckout();
        setReserva(result.reserva);
        setPhase("confirmed");
        return;
      }

      if (result.kind === "terminal") {
        setReserva(result.reserva);
        setPhase("error");
        setMessage(`Reserva ${labelReservaStatus(result.reserva.status).toLowerCase()}.`);
        return;
      }

      if (result.kind === "timeout" && result.last) {
        setReserva(result.last);
      }

      setPhase("waiting");
      setMessage(
        "Recebemos seu retorno do pagamento. A confirmação pode levar alguns segundos — veja Minhas reservas.",
      );
    })();

    return () => {
      cancelled = true;
    };
  }, [ready, user, variant, queryKey, searchParams]);

  const title =
    phase === "confirmed"
      ? titles.success
      : phase === "error"
        ? titles.error
        : phase === "waiting"
          ? titles.waiting
          : variant === "failure"
            ? titles.failure
            : variant === "pending"
              ? titles.pending
              : titles.waiting;

  const showBoarding = phase === "confirmed" && pending;

  return (
    <div className="min-h-screen bg-van-void">
      <Header />
      <main className="mx-auto max-w-lg px-4 py-10 sm:py-12">
        {phase === "loading" ? (
          <div className="flex flex-col items-center py-16 text-center">
            <div className="h-12 w-12 animate-spin rounded-full border-2 border-zinc-700 border-t-van-amber" />
            <p className="mt-6 text-sm text-zinc-400">Verificando status do pagamento…</p>
          </div>
        ) : null}

        {showBoarding ? (
          <BoardingPassSuccess
            eventName={pending.eventName}
            seatLabel={pending.seatLabel}
            dateLabel="Reserva confirmada"
            departureLocation="VanBora"
            vehicleModelColor="—"
            vehiclePlate="—"
            passengerName={user?.nome ?? "Passageiro"}
            cpfMasked="***.***.***-**"
            quorumRemaining={0}
            quorumDeadlineLabel="—"
          />
        ) : null}

        {phase !== "loading" && !showBoarding ? (
          <div className="text-center">
            <h1 className="text-2xl font-bold text-white">{title}</h1>
            {message ? <p className="mt-4 text-sm leading-relaxed text-zinc-400">{message}</p> : null}
            {pending ? (
              <p className="mt-4 text-sm text-van-amber">
                {pending.eventName} · assento {pending.seatLabel}
                {pending.valorAPagar > 0 ? ` · ${formatBrl(pending.valorAPagar)}` : ""}
              </p>
            ) : null}
            {reserva && phase === "confirmed" ? (
              <p className="mt-3 text-xs text-zinc-500">
                Reserva {reserva.id.slice(0, 8)} · {labelReservaStatus(reserva.status)}
              </p>
            ) : null}
            {!user && ready ? (
              <Link
                href={`/entrar?next=${encodeURIComponent("/minhas-reservas")}`}
                className="mt-6 inline-block text-sm font-semibold text-van-amber hover:underline"
              >
                Entrar na conta
              </Link>
            ) : null}
          </div>
        ) : null}

        <nav className="mt-10 flex flex-wrap justify-center gap-4 text-sm">
          <Link href="/minhas-reservas" className="font-semibold text-van-amber hover:underline">
            Minhas reservas
          </Link>
          {pending?.reservaId && variant !== "failure" ? (
            <Link href={`/reserva/pagar/${pending.reservaId}`} className="text-zinc-400 hover:text-white">
              Continuar pagamento
            </Link>
          ) : null}
          <Link href="/" className="text-zinc-400 hover:text-white">
            Início
          </Link>
        </nav>
      </main>
    </div>
  );
}
