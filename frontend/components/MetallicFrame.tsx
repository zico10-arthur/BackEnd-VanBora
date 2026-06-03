import type { ReactNode } from "react";

/**
 * Borda com degradê metálico (gold) — wrapper “skeu light” para cards / painéis.
 */
export function MetallicFrame({
  children,
  className = "",
  innerClassName = "",
}: {
  children: ReactNode;
  className?: string;
  innerClassName?: string;
}) {
  return (
    <div
      className={`rounded-2xl bg-gradient-to-br from-[#fff6d4]/55 via-[#f0a500]/35 via-[#c77d08]/25 to-[#3d2600]/50 p-px shadow-[0_0_0_1px_rgba(0,0,0,0.35),0_12px_40px_rgba(0,0,0,0.45)] ${className}`}
    >
      <div className={`h-full w-full rounded-[15px] ${innerClassName}`}>{children}</div>
    </div>
  );
}
