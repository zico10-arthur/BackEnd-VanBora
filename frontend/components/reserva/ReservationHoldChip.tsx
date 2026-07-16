"use client";

import { useEffect, useState } from "react";
import { readPendingCheckout } from "@/lib/payment/pending-storage";

function formatCountdown(ms: number): string {
  if (ms <= 0) return "0:00";
  const totalSec = Math.ceil(ms / 1000);
  const min = Math.floor(totalSec / 60);
  const sec = totalSec % 60;
  return `${min}:${sec.toString().padStart(2, "0")}`;
}

type Props = {
  viagemVanId: string;
};

export function ReservationHoldChip({ viagemVanId }: Props) {
  const [expiraEm, setExpiraEm] = useState<string | null>(null);

  useEffect(() => {
    const pending = readPendingCheckout();
    if (pending?.viagemVanId === viagemVanId && pending.expiraEm) {
      setExpiraEm(pending.expiraEm);
    }
  }, [viagemVanId]);

  const [remainingMs, setRemainingMs] = useState<number | null>(null);

  useEffect(() => {
    if (!expiraEm) {
      setRemainingMs(null);
      return;
    }

    const target = new Date(expiraEm).getTime();
    if (Number.isNaN(target)) {
      setRemainingMs(null);
      return;
    }

    function tick() {
      setRemainingMs(Math.max(0, target - Date.now()));
    }

    tick();
    const id = window.setInterval(tick, 1000);
    return () => window.clearInterval(id);
  }, [expiraEm]);

  const hasActiveCountdown = remainingMs !== null && remainingMs > 0;

  return (
    <div
      className="flex items-start gap-2 rounded-vb border border-van-amber/25 bg-van-amber/5 px-4 py-3 text-sm text-zinc-300"
      role="status"
    >
      <span className="mt-0.5 shrink-0 text-van-amber" aria-hidden>
        <svg viewBox="0 0 16 16" fill="none" className="h-4 w-4" stroke="currentColor" strokeWidth="1.75">
          <circle cx="8" cy="9" r="5.25" />
          <path d="M8 6.5v3l2 1" strokeLinecap="round" strokeLinejoin="round" />
          <path d="M6 2.5h4" strokeLinecap="round" />
        </svg>
      </span>
      <p>
        {hasActiveCountdown ? (
          <>
            Sua reserva expira em{" "}
            <span className="font-bold tabular-nums text-van-amber">{formatCountdown(remainingMs)}</span>
            {" · "}
            conclua o pagamento para garantir o assento.
          </>
        ) : (
          <>
            <span className="font-medium text-white">Assento reservado por até 10 min ao pagar.</span>
            {" "}
            O hold começa quando você iniciar o checkout — enquanto isso, escolha seu lugar com calma.
          </>
        )}
      </p>
    </div>
  );
}
