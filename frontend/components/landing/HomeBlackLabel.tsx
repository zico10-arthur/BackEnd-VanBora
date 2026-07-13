import Image from "next/image";
import Link from "next/link";
import { Header } from "@/components/Header";
import { SiteFooter } from "@/components/layout/SiteFooter";
import { BRAND } from "@/lib/copy";
import { ViagensSection } from "./ViagensSection";

const STADIUM_BG =
  "https://images.unsplash.com/photo-1574629810360-7efbbe195018?auto=format&fit=crop&w=1920&q=80";

const BENEFITS = [
  {
    title: "Assento garantido",
    desc: "Escolha seu lugar no mapa da van. Enquanto você paga, o assento fica reservado por até 10 minutos.",
  },
  {
    title: "Pagamento seguro",
    desc: "Finalize com Pix pelo Mercado Pago. Sua reserva é confirmada automaticamente após a aprovação.",
  },
  {
    title: "Tudo no celular",
    desc: "Reserve, pague e acompanhe sua viagem em um só lugar — ideal para o dia do evento.",
  },
] as const;

export function HomeBlackLabel() {
  return (
    <div className="relative min-h-screen bg-van-void text-zinc-100">
      <Header />

      <section className="relative flex min-h-[min(85vh,800px)] flex-col justify-center overflow-hidden">
        <div className="pointer-events-none absolute inset-0" aria-hidden>
          <Image
            src={STADIUM_BG}
            alt=""
            fill
            priority
            sizes="100vw"
            className="object-cover object-[center_35%] brightness-[0.22] saturate-[0.85]"
          />
          <div className="absolute inset-0 bg-gradient-to-b from-van-void via-van-void/90 to-van-void" />
          <div className="absolute inset-0 bg-[radial-gradient(ellipse_90%_55%_at_50%_0%,rgba(240,165,0,0.12),transparent_62%)]" />
        </div>

        <div className="relative z-10 mx-auto w-full max-w-4xl px-4 pb-14 pt-24 text-center sm:pb-20 sm:pt-32">
          <p className="text-xs font-semibold uppercase tracking-[0.35em] text-van-amber sm:tracking-[0.4em]">
            {BRAND.heroEyebrow}
          </p>
          <h1 className="mt-5 text-balance text-4xl font-black tracking-tight text-white sm:text-5xl md:text-6xl md:leading-[1.06]">
            O evento começa no <span className="vb-text-gradient">embarque</span>
          </h1>
          <p className="mx-auto mt-5 max-w-2xl text-pretty text-base leading-relaxed text-zinc-300 sm:mt-6 sm:text-lg">
            Vans parceiras para o estádio. Reserve seu assento, pague com Pix e viaje com tranquilidade.
          </p>
          <div className="mt-8 flex flex-col items-stretch gap-3 sm:mt-10 sm:flex-row sm:justify-center">
            <Link
              href="/#viagens"
              className="inline-flex min-h-[48px] items-center justify-center rounded-xl bg-van-amber px-8 py-3.5 text-sm font-bold text-van-void shadow-[0_8px_32px_rgba(240,165,0,0.28)] transition hover:brightness-110"
            >
              Ver viagens disponíveis
            </Link>
            <Link
              href="/cadastro"
              className="inline-flex min-h-[48px] items-center justify-center rounded-xl border border-zinc-600 bg-zinc-950/40 px-8 py-3.5 text-sm font-semibold text-zinc-100 transition hover:border-zinc-400 hover:bg-white/5"
            >
              Criar conta gratuita
            </Link>
          </div>
        </div>
      </section>

      <section className="border-y border-zinc-900/80 bg-zinc-950/40 px-4 py-14 sm:py-20 sm:px-6">
        <div className="mx-auto grid max-w-5xl gap-6 sm:grid-cols-2 md:grid-cols-3 md:gap-8">
          {BENEFITS.map((item) => (
            <article
              key={item.title}
              className="rounded-2xl border border-zinc-800/70 bg-zinc-900/25 p-6 text-center sm:text-left"
            >
              <h2 className="text-base font-bold text-van-amber">{item.title}</h2>
              <p className="mt-2 text-sm leading-relaxed text-zinc-400">{item.desc}</p>
            </article>
          ))}
        </div>
      </section>

      <section id="viagens" className="scroll-mt-20 px-4 py-16 sm:scroll-mt-24 sm:py-24 sm:px-6">
        <div className="mx-auto max-w-6xl">
          <header className="mb-10 max-w-2xl sm:mb-12">
            <h2 className="text-2xl font-black tracking-tight text-white sm:text-3xl">Viagens disponíveis</h2>
            <p className="mt-3 text-sm leading-relaxed text-zinc-400 sm:text-base">
              Selecione o jogo, escolha o assento e conclua o pagamento em poucos passos.
            </p>
          </header>
          <ViagensSection />
        </div>
      </section>

      <section className="px-4 pb-16 sm:px-6 sm:pb-20">
        <div className="mx-auto flex max-w-5xl flex-col items-center gap-6 rounded-2xl border border-zinc-800/80 bg-gradient-to-br from-zinc-900/50 to-zinc-950/80 p-8 text-center sm:p-12">
          <h2 className="text-xl font-bold text-white sm:text-2xl">Você é frotista?</h2>
          <p className="max-w-md text-sm leading-relaxed text-zinc-400">
            Cadastre sua frota, publique viagens e acompanhe reservas no painel {BRAND.name}.
          </p>
          <div className="flex w-full max-w-sm flex-col gap-3 sm:flex-row sm:justify-center">
            <Link
              href="/cadastro/gerente"
              className="inline-flex min-h-[48px] flex-1 items-center justify-center rounded-xl bg-van-amber px-6 py-3 text-sm font-bold text-van-void"
            >
              Cadastrar frota
            </Link>
            <Link
              href="/motorista/login"
              className="inline-flex min-h-[48px] flex-1 items-center justify-center rounded-xl border border-zinc-600 px-6 py-3 text-sm font-medium text-zinc-200"
            >
              Acessar painel
            </Link>
          </div>
        </div>
      </section>

      <SiteFooter />
    </div>
  );
}
