import Link from "next/link";
import { VanboraBlackLabelScaffold } from "@/components/vanbora/layout/VanboraBlackLabelScaffold";

export const metadata = { title: "Criar conta — VanBora" };

export default function CadastroPage() {
  return (
    <VanboraBlackLabelScaffold>
      <div className="w-full max-w-md space-y-4">
        <h1 className="text-center text-2xl font-bold text-zinc-100">Criar conta</h1>
        <p className="text-center text-sm text-zinc-400">Escolha seu perfil para continuar</p>

        <div className="mt-8 grid gap-4">
          <Link
            href="/cadastro/passageiro"
            className="group rounded-2xl border border-zinc-800 bg-zinc-900/70 p-6 transition hover:border-van-amber/50 hover:bg-zinc-900/90"
          >
            <h2 className="text-lg font-semibold text-zinc-100 group-hover:text-van-amber">
              🎟️ Passageiro
            </h2>
            <p className="mt-2 text-sm text-zinc-400">
              Reserve assentos em vans para eventos. Acompanhe suas viagens e pague com Pix.
            </p>
          </Link>

          <Link
            href="/cadastro/gerente"
            className="group rounded-2xl border border-zinc-800 bg-zinc-900/70 p-6 transition hover:border-van-amber/50 hover:bg-zinc-900/90"
          >
            <h2 className="text-lg font-semibold text-zinc-100 group-hover:text-van-amber">
              🚐 Frotista
            </h2>
            <p className="mt-2 text-sm text-zinc-400">
              Cadastre suas vans, crie viagens para eventos e gerencie reservas de passageiros.
            </p>
          </Link>
        </div>

        <p className="mt-6 text-center text-sm text-zinc-500">
          Já tem conta?{" "}
          <Link href="/entrar" className="text-van-amber hover:underline">
            Entrar
          </Link>
        </p>
      </div>
    </VanboraBlackLabelScaffold>
  );
}
