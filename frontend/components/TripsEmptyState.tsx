import Link from "next/link";

function SadVanIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.6} aria-hidden>
      <path d="M4 17h2M18 17h2" strokeLinecap="round" />
      <path d="M4 17v-4.5l2.5-5h9L18 12.5V17" strokeLinejoin="round" />
      <circle cx="7.5" cy="17" r="2.25" />
      <circle cx="16.5" cy="17" r="2.25" />
      <path d="M6.5 8h11l-1-2H7.5l-1 2z" strokeLinejoin="round" />
    </svg>
  );
}

export interface TripsEmptyStateProps {
  className?: string;
}

export function TripsEmptyState({ className }: TripsEmptyStateProps) {
  return (
    <div
      className={`mx-auto flex max-w-lg flex-col items-center rounded-2xl border border-dashed border-zinc-800 bg-zinc-900/30 px-6 py-12 text-center sm:px-10 sm:py-14 ${className ?? ""}`}
      role="status"
    >
      <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-zinc-800/80 text-zinc-400 ring-1 ring-zinc-700/80">
        <SadVanIcon className="h-9 w-9" />
      </div>
      <h3 className="mt-6 text-lg font-semibold text-white">Nenhuma viagem no momento</h3>
      <p className="mt-2 text-sm leading-relaxed text-zinc-400">
        Ainda não há vans disponíveis para os próximos eventos. Volte em breve ou cadastre-se para receber novidades.
      </p>
      <Link
        href="/cadastro/passageiro"
        className="mt-8 inline-flex min-h-[44px] w-full max-w-xs items-center justify-center rounded-xl bg-van-amber/15 px-4 py-3 text-sm font-semibold text-van-amber ring-1 ring-van-amber/25 transition hover:bg-van-amber/25"
      >
        Criar minha conta
      </Link>
    </div>
  );
}
