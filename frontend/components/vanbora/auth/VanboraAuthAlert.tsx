type Props = { message: string; variant?: "error" | "success" };

export function VanboraAuthAlert({ message, variant = "error" }: Props) {
  const styles =
    variant === "success"
      ? "border-emerald-500/35 bg-emerald-950/25 text-emerald-100"
      : "border-red-500/40 bg-red-950/30 text-red-100";
  return (
    <div role="alert" className={`rounded-xl border px-4 py-3 text-sm ${styles}`}>
      {message}
    </div>
  );
}
