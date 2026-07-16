import Image from "next/image";

export function HeroSeatMock() {
  return (
    <div className="relative mx-auto w-full max-w-[320px] sm:max-w-[380px] lg:max-w-[420px]">
      <div
        className="pointer-events-none absolute inset-[12%] rounded-full bg-van-amber/25 blur-3xl"
        aria-hidden
      />
      <Image
        src="/vanbora-app-mockup.png"
        alt="Prévia do app VanBora: escolha de assento na van para o jogo"
        width={840}
        height={1680}
        priority
        sizes="(max-width: 1024px) 320px, 420px"
        className="relative h-auto w-full drop-shadow-[0_20px_50px_rgba(240,165,0,0.18)]"
      />
    </div>
  );
}
