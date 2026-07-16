"use client";

import Link from "next/link";
import type { ReactNode } from "react";

type StatCardProps = {
  title: string;
  value: string;
  description?: string;
  href?: string;
  icon?: ReactNode;
};

export function StatCard({ title, value, description, href, icon }: StatCardProps) {
  const content = (
    <div className="rounded-2xl border border-zinc-800 bg-zinc-900/50 p-5 transition hover:border-zinc-700 hover:bg-zinc-900/80">
      <div className="flex items-start justify-between gap-3">
        <p className="text-xs font-medium uppercase tracking-wider text-zinc-500">{title}</p>
        {icon ? <span className="text-zinc-500">{icon}</span> : null}
      </div>
      <p className="mt-2 text-2xl font-bold text-van-amber">{value}</p>
      {description ? <p className="mt-1 text-xs text-zinc-500">{description}</p> : null}
    </div>
  );

  if (href) {
    return (
      <Link href={href} className="block focus:outline-none focus-visible:ring-2 focus-visible:ring-van-amber/50 rounded-2xl">
        {content}
      </Link>
    );
  }

  return content;
}
