import Image from "next/image";
import Link from "next/link";
import type { EventItem } from "@/lib/types";
import { MetallicFrame } from "./MetallicFrame";

const radialOverlay =
  "bg-[radial-gradient(ellipse_125%_90%_at_50%_0%,rgba(0,0,0,0.9)_0%,rgba(0,0,0,0.65)_40%,rgba(0,0,0,0.32)_100%)]";

interface EventCardProps {
  event: EventItem;
  priority?: boolean;
}

export function EventCard({ event, priority = false }: EventCardProps) {
  const isSoldOut = event.status === "sold_out";

  return (
    <MetallicFrame
      className="group h-full transition duration-500 hover:shadow-[0_0_0_1px_rgba(240,165,0,0.25),0_20px_50px_rgba(0,0,0,0.55),0_0_60px_rgba(240,165,0,0.08)]"
      innerClassName="overflow-hidden"
    >
      <article className="relative flex min-h-[340px] flex-col overflow-hidden rounded-[15px]">
        <Image
          src={event.coverImage}
          alt=""
          fill
          priority={priority}
          sizes="(max-width: 768px) 100vw, 33vw"
          className={`object-cover object-center transition duration-700 ease-out group-hover:scale-[1.06] ${
            isSoldOut ? "grayscale saturate-50 opacity-75" : ""
          }`}
        />
        <div className={`pointer-events-none absolute inset-0 ${radialOverlay}`} aria-hidden />
        <div
          className="pointer-events-none absolute inset-0 bg-gradient-to-t from-black via-black/55 to-transparent"
          aria-hidden
        />
        {isSoldOut && (
          <div
            className="pointer-events-none absolute inset-0 bg-[#0D0D0D]/35 mix-blend-multiply"
            aria-hidden
          />
        )}

        <div className="relative z-10 mt-auto flex flex-col p-4 sm:p-5">
          <div className="mb-3 flex flex-wrap items-start justify-between gap-2">
            <div className="min-w-0 flex-1">
              <h2 className="text-lg font-black not-italic leading-tight tracking-tight text-white [text-shadow:0_2px_20px_rgba(0,0,0,0.9)] sm:text-xl">
                {event.name}
              </h2>
              <p className="mt-1 text-sm font-medium text-neutral-200/95">{event.subtitle}</p>
              <p className="mt-2 text-xs font-bold uppercase tracking-[0.18em] text-[#f0a500]/95">
                {event.dateLabel}
              </p>
            </div>
            {isSoldOut && (
              <span className="shrink-0 rounded-full border border-white/10 bg-black/55 px-2.5 py-1 text-xs font-bold uppercase tracking-wide text-neutral-400 backdrop-blur-sm">
                Esgotado
              </span>
            )}
          </div>

          {isSoldOut ? (
            <button
              type="button"
              disabled
              className="vb-seat-dead mt-auto w-full cursor-not-allowed rounded-xl border border-white/5 py-3 text-sm font-semibold text-neutral-600"
            >
              Comprar Passagem
            </button>
          ) : (
            <div className="mt-auto rounded-xl bg-gradient-to-br from-[#fff6d4]/50 via-[#f0a500]/40 to-[#3d2600]/45 p-px shadow-[0_0_24px_rgba(240,165,0,0.12)] transition duration-300 group-hover:shadow-[0_0_36px_rgba(240,165,0,0.28)]">
              <Link
                href={`/reserva/${event.id}`}
                className="vb-cta-gold relative flex w-full items-center justify-center overflow-hidden rounded-[11px] py-3 text-sm font-extrabold uppercase tracking-wide text-[#1a0f00] transition duration-300 hover:scale-[1.02] active:scale-[0.99] focus:outline-none focus-visible:ring-2 focus-visible:ring-[#F0A500] focus-visible:ring-offset-2 focus-visible:ring-offset-[#0D0D0D]"
              >
                <span
                  className="absolute inset-0 bg-gradient-to-b from-[#fff3c2] via-[#f0a500] to-[#a86a00]"
                  aria-hidden
                />
                <span className="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent opacity-60" aria-hidden />
                <span className="relative z-[1] drop-shadow-sm">Comprar Passagem</span>
              </Link>
            </div>
          )}
        </div>
      </article>
    </MetallicFrame>
  );
}
