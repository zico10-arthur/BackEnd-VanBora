import Image from "next/image";
import Link from "next/link";

/** Logo como imagem (SVG) — degradê metálico e itálico preservados no asset. */
export function VanBoraLogo() {
  return (
    <Link
      href="/"
      className="relative block shrink-0 transition-[filter,opacity] duration-300 hover:brightness-110 focus:outline-none focus-visible:ring-2 focus-visible:ring-[#F0A500]/70 focus-visible:ring-offset-2 focus-visible:ring-offset-[#0D0D0D] rounded-sm"
      aria-label="VanBora — Início"
    >
      <Image
        src="/brand/vanbora-logo.svg"
        alt=""
        width={248}
        height={40}
        className="h-7 w-auto sm:h-8"
        priority
      />
    </Link>
  );
}
