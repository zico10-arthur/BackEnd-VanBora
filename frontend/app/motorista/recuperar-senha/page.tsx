import { MotoristaRecuperarSenhaCard } from "@/components/motorista/MotoristaRecuperarSenhaCard";

export const metadata = {
  title: "Recuperar senha — VanBora Motorista",
  description: "Solicite o link para criar uma nova senha de acesso ao painel.",
};

export default function MotoristaRecuperarSenhaPage() {
  return (
    <div className="min-h-screen bg-zinc-950">
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_80%_50%_at_50%_-20%,rgba(245,158,11,0.06),transparent_55%)]"
        aria-hidden
      />
      <div className="relative flex min-h-screen flex-col items-center justify-center px-4 py-12">
        <MotoristaRecuperarSenhaCard />
      </div>
    </div>
  );
}
