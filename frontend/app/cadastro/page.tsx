import Image from "next/image";
import Link from "next/link";
import { VanboraBlackLabelScaffold } from "@/components/vanbora/layout/VanboraBlackLabelScaffold";

export const metadata = { title: "Criar conta — VanBora" };

function TicketPersonIcon({ className }: { className?: string }) {
  return (
    <svg
      viewBox="0 0 48 48"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.75"
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
      aria-hidden
    >
      <path d="M8 18v-4a2 2 0 0 1 2-2h28a2 2 0 0 1 2 2v4" />
      <path d="M8 30v4a2 2 0 0 0 2 2h28a2 2 0 0 0 2-2v-4" />
      <path d="M20 12v24" strokeDasharray="3 3" />
      <circle cx="32" cy="20" r="3.5" />
      <path d="M26 32c0-3.314 2.686-5 6-5s6 1.686 6 5" />
    </svg>
  );
}

function VanFleetIcon({ className }: { className?: string }) {
  return (
    <svg
      viewBox="0 0 48 48"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.75"
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
      aria-hidden
    >
      <path d="M6 28h30l4-8H10l-4 8z" />
      <path d="M6 28v6a2 2 0 0 0 2 2h2" />
      <path d="M36 36h2a2 2 0 0 0 2-2v-6" />
      <circle cx="14" cy="36" r="3" />
      <circle cx="34" cy="36" r="3" />
      <path d="M17 36h14" />
      <path d="M14 20h8M26 20h4" />
      <rect x="12" y="14" width="8" height="6" rx="1" />
    </svg>
  );
}

const PROFILES = [
  {
    href: "/cadastro/passageiro",
    title: "Passageiro",
    description: "Reserve assentos em vans para eventos. Acompanhe suas viagens e pague com Pix.",
    Icon: TicketPersonIcon,
    bullets: [
      "Escolha seu assento no mapa da van",
      "Pagamento rápido e seguro com Pix",
      "Acompanhe reservas e embarque no celular",
    ],
  },
  {
    href: "/cadastro/gerente",
    title: "Frotista",
    description: "Cadastre sua frota, publique viagens e gerencie reservas de passageiros.",
    Icon: VanFleetIcon,
    bullets: [
      "Cadastre vans e crie viagens em minutos",
      "Receba reservas e pagamentos automaticamente",
      "Painel completo para frota, motoristas e embarque",
    ],
  },
] as const;

export default function CadastroPage() {
  return (
    <VanboraBlackLabelScaffold>
      <div className="w-full max-w-3xl space-y-8 px-4 sm:px-6">
        <div className="flex flex-col items-center text-center">
          <Link href="/" className="mb-8">
            <Image
              src="/brand/vanbora-logo.svg"
              alt="VanBora"
              width={200}
              height={32}
              className="h-8 w-auto"
              priority
            />
          </Link>
          <h1 className="font-display text-3xl tracking-wide text-white sm:text-4xl">Criar conta</h1>
          <p className="mt-3 max-w-md text-sm leading-relaxed text-zinc-400">
            Escolha seu perfil para continuar. É gratuito e leva poucos minutos.
          </p>
        </div>

        <div className="grid gap-4 sm:grid-cols-2 sm:gap-5">
          {PROFILES.map(({ href, title, description, Icon, bullets }) => (
            <Link
              key={href}
              href={href}
              className="group flex flex-col rounded-vb border border-van-border bg-van-surface p-6 transition hover:border-van-amber/50 hover:bg-van-elevated sm:p-7"
            >
              <div className="flex items-start gap-4">
                <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-vb border border-van-border bg-van-elevated text-van-amber transition group-hover:border-van-amber/40 group-hover:shadow-van-glow">
                  <Icon className="h-7 w-7" />
                </div>
                <div className="min-w-0 pt-0.5">
                  <h2 className="text-lg font-semibold text-white transition group-hover:text-van-amber">{title}</h2>
                  <p className="mt-1.5 text-sm leading-relaxed text-zinc-400">{description}</p>
                </div>
              </div>
              <ul className="mt-5 space-y-2 border-t border-van-border/80 pt-5">
                {bullets.map((item) => (
                  <li key={item} className="flex items-start gap-2.5 text-sm text-zinc-300">
                    <span
                      className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-van-amber"
                      aria-hidden
                    />
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
              <span className="mt-5 inline-flex items-center text-sm font-medium text-van-amber transition group-hover:gap-2">
                Continuar
                <svg
                  viewBox="0 0 16 16"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  className="ml-1 h-4 w-4 transition group-hover:translate-x-0.5"
                  aria-hidden
                >
                  <path d="M3 8h10M9 4l4 4-4 4" />
                </svg>
              </span>
            </Link>
          ))}
        </div>

        <p className="text-center text-sm text-zinc-500">
          Já tem conta?{" "}
          <Link href="/entrar" className="text-van-amber transition hover:underline">
            Entrar
          </Link>
        </p>
      </div>
    </VanboraBlackLabelScaffold>
  );
}
