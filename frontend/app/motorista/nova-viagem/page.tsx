import Link from "next/link";
import { Header } from "@/components/Header";
import { TripRegistrationForm } from "@/components/motorista/TripRegistrationForm";

export const metadata = {
  title: "Nova viagem — VanBora",
  description: "Cadastre viagem, logística e regra financeira (break-even).",
};

export default function NovaViagemPage() {
  return (
    <div className="min-h-screen bg-[#0D0D0D]">
      <Header />
      <main className="mx-auto max-w-3xl px-4 py-8 sm:px-6 sm:py-10">
        <Link
          href="/"
          className="text-sm font-medium text-zinc-500 transition hover:text-amber-400/90"
        >
          ← Voltar
        </Link>

        <header className="mt-6 border-b border-zinc-800 pb-6">
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-500/90">
            Motorista / frotista
          </p>
          <h1 className="mt-2 text-2xl font-bold tracking-tight text-white sm:text-3xl">
            Nova viagem
          </h1>
          <p className="mt-2 max-w-xl text-sm text-zinc-400">
            Selecione o jogo, defina partida e capacidade e estabeleça o quórum mínimo para a
            viagem ser viável.
          </p>
        </header>

        <div className="mt-8">
          <TripRegistrationForm />
        </div>
      </main>
    </div>
  );
}
