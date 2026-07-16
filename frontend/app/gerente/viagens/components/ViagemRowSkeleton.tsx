"use client";

export function ViagemRowSkeleton() {
  return (
    <tr className="border-b border-zinc-800">
      <td className="px-4 py-4">
        <div className="mb-1 h-5 w-40 animate-pulse rounded bg-zinc-800" />
        <div className="h-3 w-24 animate-pulse rounded bg-zinc-800" />
      </td>
      <td className="px-4 py-4">
        <div className="h-4 w-32 animate-pulse rounded bg-zinc-800" />
      </td>
      <td className="px-4 py-4">
        <div className="h-4 w-36 animate-pulse rounded bg-zinc-800" />
      </td>
      <td className="px-4 py-4">
        <div className="h-4 w-12 animate-pulse rounded bg-zinc-800" />
      </td>
      <td className="px-4 py-4">
        <div className="h-5 w-20 animate-pulse rounded-full bg-zinc-800" />
      </td>
      <td className="px-4 py-4">
        <div className="flex gap-2">
          <div className="h-8 w-14 animate-pulse rounded-xl bg-zinc-800" />
          <div className="h-8 w-16 animate-pulse rounded-xl bg-zinc-800" />
          <div className="h-8 w-20 animate-pulse rounded-xl bg-zinc-800" />
        </div>
      </td>
    </tr>
  );
}
