import { Suspense } from "react";
import { ReservaPagamentoRetorno } from "@/components/reserva/ReservaPagamentoRetorno";

export const metadata = { title: "Pagamento não concluído" };

export default function ReservaFalhaPage() {
  return (
    <Suspense
      fallback={
        <div className="flex min-h-screen items-center justify-center bg-van-void text-zinc-500">
          Carregando…
        </div>
      }
    >
      <ReservaPagamentoRetorno variant="failure" />
    </Suspense>
  );
}
