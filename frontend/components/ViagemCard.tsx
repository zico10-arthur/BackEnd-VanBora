import Image from "next/image";
import Link from "next/link";
import type { ViagemPublica, ViagemVanPublica } from "@/lib/api/types";
import { coverForId } from "@/lib/covers";
import { formatBrl, formatDatePt } from "@/lib/format";
import { MetallicFrame } from "./MetallicFrame";

const radialOverlay =
  "bg-[radial-gradient(ellipse_125%_90%_at_50%_0%,rgba(0,0,0,0.9)_0%,rgba(0,0,0,0.65)_40%,rgba(0,0,0,0.32)_100%)]";

type Props = {
  viagem: ViagemPublica;
  van: ViagemVanPublica;
  priority?: boolean;
};

export function ViagemCard({ viagem, van, priority = false }: Props) {
  const soldOut = van.assentosDisponiveis <= 0;
  const cover = coverForId(viagem.viagemId);
  const dateLabel = formatDatePt(viagem.dataEvento);

  return (
    <MetallicFrame
      className="group h-full transition duration-500 hover:shadow-[0_0_0_1px_rgba(240,165,0,0.25),0_20px_50px_rgba(0,0,0,0.55),0_0_60px_rgba(240,165,0,0.08)]"
      innerClassName="overflow-hidden"
    >
      <article className="relative flex min-h-[360px] flex-col overflow-hidden rounded-[15px]">
        <Image
          src={cover}
          alt=""
          fill
          priority={priority}
          sizes="(max-width: 768px) 100vw, 33vw"
          className={`object-cover object-center transition duration-700 ease-out group-hover:scale-[1.06] ${soldOut ? "grayscale opacity-70" : ""}`}
        />
        <div className={`pointer-events-none absolute inset-0 ${radialOverlay}`} aria-hidden />
        <div className="pointer-events-none absolute inset-0 bg-gradient-to-t from-black via-black/55 to-transparent" aria-hidden />

        <div className="relative z-10 mt-auto flex flex-col p-4 sm:p-5">
          <p className="text-[10px] font-bold uppercase tracking-[0.2em] text-van-amber/90">
            {formatBrl(viagem.precoAssento)} / assento
          </p>
          <h2 className="mt-2 text-lg font-black leading-tight tracking-tight text-white sm:text-xl">
            {viagem.nomeEvento}
          </h2>
          <p className="mt-1 text-sm text-zinc-300">{viagem.localEvento}</p>
          <p className="mt-1 text-xs text-zinc-500">Saída: {viagem.localPartida}</p>
          <p className="mt-2 text-xs font-bold uppercase tracking-[0.16em] text-van-amber/95">{dateLabel}</p>
          <p className="mt-1 text-xs text-zinc-400">
            {van.nomeVan} · {van.assentosDisponiveis} assento(s) livre(s)
          </p>

          {soldOut ? (
            <button
              type="button"
              disabled
              className="vb-seat-dead mt-4 w-full cursor-not-allowed rounded-xl py-3 text-sm font-semibold text-zinc-600"
            >
              Esgotado
            </button>
          ) : (
            <div className="mt-4 rounded-xl bg-gradient-to-br from-[#fff6d4]/50 via-van-amber/40 to-[#3d2600]/45 p-px">
              <Link
                href={`/reserva/${van.viagemVanId}`}
                className="vb-cta-gold relative flex w-full items-center justify-center overflow-hidden rounded-[11px] py-3 text-sm font-extrabold uppercase tracking-wide text-van-void"
              >
                <span className="absolute inset-0 bg-gradient-to-b from-[#fff3c2] via-van-amber to-[#a86a00]" aria-hidden />
                <span className="relative z-[1]">Reservar assento</span>
              </Link>
            </div>
          )}
        </div>
      </article>
    </MetallicFrame>
  );
}
