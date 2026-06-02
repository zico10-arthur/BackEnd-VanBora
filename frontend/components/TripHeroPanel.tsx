import Image from "next/image";
import { MetallicFrame } from "./MetallicFrame";

type Props = {
  title: string;
  subtitle: string;
  dateLabel: string;
  coverImage: string;
  priceLabel?: string;
};

export function TripHeroPanel({ title, subtitle, dateLabel, coverImage, priceLabel }: Props) {
  return (
    <MetallicFrame className="mt-6" innerClassName="overflow-hidden">
      <div className="relative min-h-[220px] overflow-hidden sm:min-h-[240px]">
        <Image src={coverImage} alt="" fill className="object-cover" sizes="1024px" priority />
        <div className="pointer-events-none absolute inset-0 bg-gradient-to-t from-black/90 via-black/40 to-transparent" />
        <div className="relative z-10 flex flex-col p-5 sm:p-6">
          <p className="text-xs font-bold uppercase tracking-[0.2em] text-van-gold">Reserva de assento</p>
          {priceLabel ? (
            <p className="mt-2 text-sm font-semibold text-van-amber">{priceLabel}</p>
          ) : null}
          <h1 className="mt-2 text-2xl font-black tracking-tight text-white sm:text-3xl">{title}</h1>
          <p className="mt-2 text-sm text-zinc-300">{subtitle}</p>
          <p className="mt-2 text-xs font-semibold uppercase tracking-wider text-van-amber/90">{dateLabel}</p>
        </div>
      </div>
    </MetallicFrame>
  );
}
