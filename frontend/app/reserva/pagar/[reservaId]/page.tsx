"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { Header } from "@/components/Header";
import { pagarReserva } from "@/lib/api/reservas";
import { ApiError } from "@/lib/api/http";
import { useAuth } from "@/components/providers/AuthProvider";
import Link from "next/link";

export default function ContinuarPagamentoPage() {
  const { reservaId } = useParams<{ reservaId: string }>();
  const router = useRouter();
  const { user, ready } = useAuth();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!ready) return;
    if (!user) {
      router.replace(`/entrar?next=/reserva/pagar/${reservaId}`);
      return;
    }
    (async () => {
      try {
        const p = await pagarReserva(reservaId);
        window.location.href = p.sandboxInitPoint || p.initPoint;
      } catch (e) {
        setError(e instanceof ApiError ? e.message : "Erro ao gerar pagamento");
      }
    })();
  }, [ready, user, reservaId, router]);

  return (
    <div className="min-h-screen bg-van-void">
      <Header />
      <div className="mx-auto max-w-md px-4 py-24 text-center">
        {!error ? (
          <>
            <div className="mx-auto h-12 w-12 animate-spin rounded-full border-2 border-zinc-700 border-t-van-amber" />
            <p className="mt-6 text-zinc-400">Redirecionando para o pagamento com Pix…</p>
          </>
        ) : (
          <>
            <p className="text-red-300">{error}</p>
            <Link href="/minhas-reservas" className="mt-4 inline-block text-van-amber underline">
              Voltar às reservas
            </Link>
          </>
        )}
      </div>
    </div>
  );
}
