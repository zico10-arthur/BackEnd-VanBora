import Image from "next/image";
import type { EventItem } from "@/lib/types";
import { MetallicFrame } from "./MetallicFrame";

const radialOverlay =
  "bg-[radial-gradient(ellipse_130%_95%_at_50%_0%,rgba(0,0,0,0.92)_0%,rgba(0,0,0,0.72)_38%,rgba(0,0,0,0.38)_62%,rgba(0,0,0,0.28)_100%)]";

interface EventHeroPanelProps {
  event: EventItem;
}

export function EventHeroPanel({ event }: EventHeroPanelProps) {
  return (
    <MetallicFrame className="mt-6" innerClassName="overflow-hidden">
      <div className="relative min-h-[200px] overflow-hidden sm:min-h-[220px]">
        <Image
          src={event.coverImage}
          alt=""
          fill
          className="object-cover object-center transition duration-700 ease-out hover:scale-[1.03]"
          sizes="(max-width: 1024px) 100vw, 1024px"
          priority
        />
        <div className={`pointer-events-none absolute inset-0 ${radialOverlay}`} aria-hidden />
        <div className="pointer-events-none absolute inset-0 bg-gradient-to-t from-black/85 via-black/25 to-transparent" aria-hidden />
        <div className="relative z-10 flex flex-col p-4 sm:p-6">
          <p className="text-xs font-bold uppercase tracking-[0.2em] text-[#ffe08a] drop-shadow-md">
            Reserva
          </p>
          <h1 className="mt-2 text-2xl font-black not-italic tracking-tight text-white [text-shadow:0_2px_24px_rgba(0,0,0,0.9)] sm:text-3xl">
            {event.name}
          </h1>
          <p className="mt-2 text-sm font-medium text-neutral-200/95">{event.subtitle}</p>
          <p className="mt-2 text-xs font-semibold uppercase tracking-wider text-[#f0a500]/90">
            {event.dateLabel}
          </p>
        </div>
      </div>
    </MetallicFrame>
  );
}
