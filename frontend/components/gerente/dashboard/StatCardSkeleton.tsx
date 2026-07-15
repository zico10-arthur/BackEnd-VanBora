export function StatCardSkeleton() {
  return (
    <div className="animate-pulse rounded-2xl border border-zinc-800 bg-zinc-900/50 p-5">
      <div className="mb-3 h-3 w-1/3 rounded bg-zinc-800" />
      <div className="h-8 w-1/2 rounded bg-zinc-800" />
      <div className="mt-2 h-3 w-2/3 rounded bg-zinc-800" />
    </div>
  );
}
