import type { ReactNode } from "react";

export function VanboraBlackLabelScaffold({ children }: { children: ReactNode }) {
  return (
    <div className="relative min-h-screen bg-van-void">
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_80%_50%_at_50%_-15%,rgba(240,165,0,0.09),transparent_58%)]"
        aria-hidden
      />
      <div className="pointer-events-none absolute inset-0 bg-[url('/brand/noise.svg')] opacity-[0.03]" aria-hidden />
      <div className="relative flex min-h-screen flex-col items-center justify-center px-4 py-12">
        {children}
      </div>
    </div>
  );
}
