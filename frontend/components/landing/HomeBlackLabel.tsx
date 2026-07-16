import Image from "next/image";
import Link from "next/link";
import { Header } from "@/components/Header";
import { SiteFooter } from "@/components/layout/SiteFooter";
import { BRAND, FAQ_ITEMS } from "@/lib/copy";
import { HeroSeatMock } from "./HeroSeatMock";
import { ViagensSection } from "./ViagensSection";

const STADIUM_BG =
  "https://images.unsplash.com/photo-1574629810360-7efbbe195018?auto=format&fit=crop&w=1920&q=80";

const STEPS = [
  { n: "01", title: "Escolha a viagem", desc: "Veja os jogos e shows com vans parceiras disponíveis." },
  { n: "02", title: "Selecione o assento", desc: "Escolha seu lugar no mapa da van — o assento fica reservado enquanto você paga." },
  { n: "03", title: "Pague com Pix", desc: "Finalize pelo Mercado Pago. Confirmação automática após aprovação." },
  { n: "04", title: "Embarque", desc: "Compareça no ponto combinado e viaje com tranquilidade até o evento." },
] as const;

const BENEFITS = [
  {
    title: "Assento garantido",
    desc: "Escolha seu lugar no mapa da van. Enquanto você paga, o assento fica reservado por até 10 minutos.",
    icon: BenefitSeatIcon,
  },
  {
    title: "Van parceira verificada",
    desc: "Frotas parceiras com dados visíveis na reserva. Você sabe com quem vai viajar antes de embarcar.",
    icon: BenefitVanIcon,
  },
  {
    title: "Pix seguro",
    desc: "Pagamento processado pelo Mercado Pago. Sua reserva é confirmada automaticamente após a aprovação.",
    icon: BenefitPixIcon,
  },
  {
    title: "Tudo no celular",
    desc: "Reserve, pague e acompanhe sua viagem em um só lugar — ideal para o dia do evento.",
    icon: BenefitPhoneIcon,
  },
] as const;

function splitHeroHeadline(headline: string) {
  const target = "embarque";
  const idx = headline.toLowerCase().indexOf(target);
  if (idx === -1) return { before: headline, highlight: null, after: "" };
  return {
    before: headline.slice(0, idx),
    highlight: headline.slice(idx, idx + target.length),
    after: headline.slice(idx + target.length),
  };
}

function BenefitSeatIcon() {
  return (
    <svg viewBox="0 0 32 32" fill="none" className="h-8 w-8" aria-hidden>
      <rect x="6" y="14" width="20" height="10" rx="2" stroke="currentColor" strokeWidth="2.5" />
      <path d="M10 14V10a6 6 0 0 1 12 0v4" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" />
      <path d="M8 24v3M24 24v3" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" />
    </svg>
  );
}

function BenefitVanIcon() {
  return (
    <svg viewBox="0 0 32 32" fill="none" className="h-8 w-8" aria-hidden>
      <rect x="4" y="12" width="18" height="10" rx="2" stroke="currentColor" strokeWidth="2.5" />
      <path d="M22 16h4l3 4v6h-7V16Z" stroke="currentColor" strokeWidth="2.5" strokeLinejoin="round" />
      <circle cx="10" cy="24" r="2.5" stroke="currentColor" strokeWidth="2.5" />
      <circle cx="24" cy="24" r="2.5" stroke="currentColor" strokeWidth="2.5" />
      <path d="M26 8l2 2-4 4" stroke="#F0A500" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

function BenefitPixIcon() {
  return (
    <svg viewBox="0 0 32 32" fill="none" className="h-8 w-8" aria-hidden>
      <path
        d="M16 4L6 10v12l10 6 10-6V10L16 4Z"
        stroke="currentColor"
        strokeWidth="2.5"
        strokeLinejoin="round"
      />
      <path d="M16 16v12M6 10l10 6 10-6" stroke="currentColor" strokeWidth="2.5" strokeLinejoin="round" />
      <path d="M12 20h8" stroke="#F0A500" strokeWidth="2.5" strokeLinecap="round" />
    </svg>
  );
}

function BenefitPhoneIcon() {
  return (
    <svg viewBox="0 0 32 32" fill="none" className="h-8 w-8" aria-hidden>
      <rect x="9" y="4" width="14" height="24" rx="3" stroke="currentColor" strokeWidth="2.5" />
      <path d="M13 8h6" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" />
      <circle cx="16" cy="23" r="1.5" fill="#F0A500" />
    </svg>
  );
}

export function HomeBlackLabel() {
  const { before, highlight, after } = splitHeroHeadline(BRAND.heroHeadline);

  return (
    <div className="relative min-h-screen bg-van-void text-zinc-100">
      <Header />

      {/* Hero */}
      <section className="relative overflow-hidden">
        <div className="pointer-events-none absolute inset-0" aria-hidden>
          <Image
            src={STADIUM_BG}
            alt=""
            fill
            priority
            sizes="100vw"
            className="object-cover object-[center_35%] brightness-[0.18] saturate-[0.8]"
          />
          <div className="absolute inset-0 bg-gradient-to-b from-van-void via-van-void/92 to-van-void" />
          <div className="absolute inset-0 vb-bg-grid opacity-40" />
          <div className="absolute inset-0 vb-bg-stripes opacity-50" />
          <div className="absolute inset-0 bg-[radial-gradient(ellipse_80%_50%_at_30%_0%,rgba(240,165,0,0.18),transparent_60%)]" />
        </div>

        <div className="vb-container relative z-10 pb-16 pt-24 sm:pb-20 sm:pt-28 lg:pb-24">
          <div className="grid items-center gap-10 lg:grid-cols-[3fr_2fr] lg:gap-14">
            <div className="max-w-xl">
              <span className="inline-flex items-center rounded-full border border-van-amber/50 bg-van-amber/10 px-3.5 py-1 text-xs font-semibold tracking-wide text-van-amber">
                {BRAND.heroChip}
              </span>

              <h1 className="font-display mt-6 text-balance text-4xl leading-[0.95] text-white sm:text-5xl lg:text-6xl">
                {before}
                {highlight ? <span className="vb-text-gradient">{highlight}</span> : null}
                {after}
              </h1>

              <p className="mt-5 text-pretty text-base leading-relaxed text-zinc-300 sm:mt-6 sm:text-lg">
                {BRAND.heroSubheadline}
              </p>

              <div className="mt-8 flex flex-col items-start gap-4 sm:mt-10">
                <Link
                  href="/#viagens"
                  className="vb-cta-gold inline-flex min-h-[48px] items-center justify-center rounded-vb bg-van-amber px-8 py-3.5 text-sm font-bold text-van-void transition hover:brightness-110"
                >
                  {BRAND.heroCta}
                </Link>
                <Link
                  href="#frotista"
                  className="text-sm font-medium text-zinc-400 underline-offset-4 transition hover:text-van-amber hover:underline"
                >
                  {BRAND.heroCtaSecondary} →
                </Link>
              </div>

              <p className="mt-6 text-xs text-zinc-500 sm:mt-8">{BRAND.socialProof}</p>
            </div>

            <div className="mx-auto w-full max-w-md lg:max-w-none lg:justify-self-end">
              <HeroSeatMock />
            </div>
          </div>
        </div>
      </section>

      {/* Como funciona */}
      <section
        id="como-funciona"
        className="scroll-mt-20 border-y border-van-border bg-van-surface px-4 py-14 sm:scroll-mt-24 sm:px-6 sm:py-20"
      >
        <div className="mx-auto max-w-container">
          <header className="mb-10 max-w-2xl sm:mb-14">
            <h2 className="font-display text-3xl text-white sm:text-4xl">Como funciona</h2>
            <p className="mt-3 text-sm leading-relaxed text-zinc-400 sm:text-base">
              Do primeiro clique ao embarque — quatro passos simples para garantir seu lugar.
            </p>
          </header>

          <ol className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4 lg:gap-8">
            {STEPS.map((step) => (
              <li
                key={step.n}
                className="relative rounded-vb border border-van-border bg-van-elevated/60 p-6"
              >
                <span className="font-display text-3xl text-van-amber/80">{step.n}</span>
                <h3 className="mt-3 text-base font-bold text-white">{step.title}</h3>
                <p className="mt-2 text-sm leading-relaxed text-zinc-400">{step.desc}</p>
              </li>
            ))}
          </ol>
        </div>
      </section>

      {/* Social proof strip */}
      <section className="border-b border-van-border bg-van-void py-8 sm:py-10" aria-label="Prova social">
        <div className="mx-auto flex max-w-container flex-col items-center gap-6 px-4 sm:px-6 lg:flex-row lg:justify-between">
          <p className="text-center text-sm font-medium text-zinc-300 lg:text-left">{BRAND.socialProof}</p>
          <div className="flex flex-wrap items-center justify-center gap-3 sm:gap-4" aria-hidden>
            {["Arena", "Estádio", "Show", "Parceiro"].map((label) => (
              <span
                key={label}
                className="rounded-vb border border-van-border bg-van-surface px-4 py-2 text-xs font-semibold uppercase tracking-wider text-zinc-500"
              >
                {label}
              </span>
            ))}
          </div>
        </div>
      </section>

      {/* Feature benefits */}
      <section className="px-4 py-14 sm:px-6 sm:py-20">
        <div className="mx-auto max-w-container">
          <header className="mb-10 max-w-2xl sm:mb-14">
            <h2 className="font-display text-3xl text-white sm:text-4xl">Por que a VanBora</h2>
            <p className="mt-3 text-sm leading-relaxed text-zinc-400 sm:text-base">
              Mobilidade pensada para o dia do evento — com segurança, clareza e tudo no celular.
            </p>
          </header>

          <div className="grid gap-6 sm:grid-cols-2 lg:gap-8">
            {BENEFITS.map((item) => (
              <article
                key={item.title}
                className="flex gap-4 rounded-vb border border-van-border bg-van-surface/50 p-6 sm:gap-5 sm:p-7"
              >
                <div className="shrink-0 text-white">
                  <item.icon />
                </div>
                <div>
                  <h3 className="text-base font-bold text-van-amber">{item.title}</h3>
                  <p className="mt-2 text-sm leading-relaxed text-zinc-400">{item.desc}</p>
                </div>
              </article>
            ))}
          </div>
        </div>
      </section>

      {/* Viagens disponíveis */}
      <section id="viagens" className="scroll-mt-20 border-t border-van-border bg-van-surface/40 px-4 py-16 sm:scroll-mt-24 sm:py-24 sm:px-6">
        <div className="mx-auto max-w-container">
          <header className="mb-10 max-w-2xl sm:mb-12">
            <h2 className="font-display text-3xl text-white sm:text-4xl">Viagens disponíveis</h2>
            <p className="mt-3 text-sm leading-relaxed text-zinc-400 sm:text-base">
              Selecione o jogo, escolha o assento e conclua o pagamento em poucos passos.
            </p>
          </header>
          <ViagensSection />
        </div>
      </section>

      {/* FAQ */}
      <section id="faq" className="scroll-mt-20 px-4 py-14 sm:scroll-mt-24 sm:px-6 sm:py-20">
        <div className="mx-auto max-w-container">
          <header className="mb-8 max-w-2xl sm:mb-10">
            <h2 className="font-display text-3xl text-white sm:text-4xl">Perguntas frequentes</h2>
            <p className="mt-3 text-sm text-zinc-400 sm:text-base">
              Dúvidas comuns sobre reserva, pagamento e embarque.
            </p>
          </header>

          <div className="mx-auto max-w-3xl divide-y divide-van-border rounded-vb border border-van-border bg-van-surface/40">
            {FAQ_ITEMS.map((item) => (
              <details key={item.q} className="group px-5 py-1 sm:px-6">
                <summary className="flex cursor-pointer list-none items-center justify-between gap-4 py-4 text-sm font-semibold text-white marker:content-none sm:text-base [&::-webkit-details-marker]:hidden">
                  {item.q}
                  <span
                    className="shrink-0 text-van-amber transition group-open:rotate-45"
                    aria-hidden
                  >
                    +
                  </span>
                </summary>
                <p className="pb-4 text-sm leading-relaxed text-zinc-400">{item.a}</p>
              </details>
            ))}
          </div>
        </div>
      </section>

      {/* Frotista */}
      <section id="frotista" className="scroll-mt-20 px-4 pb-16 sm:scroll-mt-24 sm:px-6 sm:pb-20">
        <div className="mx-auto max-w-container">
          <div className="flex flex-col items-center gap-6 rounded-vb border border-van-border bg-gradient-to-br from-van-elevated/80 to-van-surface p-8 text-center sm:p-12">
            <h2 className="font-display text-2xl text-white sm:text-3xl">Você é frotista?</h2>
            <p className="max-w-md text-sm leading-relaxed text-zinc-400">
              Cadastre sua frota, publique viagens e acompanhe reservas no painel {BRAND.name}.
            </p>
            <div className="flex w-full max-w-sm flex-col gap-3 sm:flex-row sm:justify-center">
              <Link
                href="/cadastro/gerente"
                className="inline-flex min-h-[48px] flex-1 items-center justify-center rounded-vb bg-van-amber px-6 py-3 text-sm font-bold text-van-void transition hover:brightness-110"
              >
                Cadastrar frota
              </Link>
              <Link
                href="/motorista/login"
                className="inline-flex min-h-[48px] flex-1 items-center justify-center rounded-vb border border-van-border px-6 py-3 text-sm font-medium text-zinc-200 transition hover:border-zinc-500 hover:bg-white/5"
              >
                Acessar painel
              </Link>
            </div>
          </div>
        </div>
      </section>

      <SiteFooter />
    </div>
  );
}
