"use client";

type ProgressBarProps = {
  value: number;
  max: number;
  label?: string;
};

export function ProgressBar({ value, max, label }: ProgressBarProps) {
  const safeMax = Math.max(max, 1);
  const pct = Math.min(100, Math.round((Math.max(0, value) / safeMax) * 100));

  return (
    <div className="w-full">
      {label ? (
        <div className="mb-1.5 flex items-center justify-between text-xs text-zinc-400">
          <span>{label}</span>
          <span className="font-medium text-zinc-300">
            {value} / {max}
          </span>
        </div>
      ) : (
        <p className="mb-1.5 text-right text-xs font-medium text-zinc-300">
          {value} / {max}
        </p>
      )}
      <div
        className="h-2.5 overflow-hidden rounded-full bg-zinc-800"
        role="progressbar"
        aria-valuenow={value}
        aria-valuemin={0}
        aria-valuemax={max}
      >
        <div
          className="h-full rounded-full bg-van-amber transition-all duration-500 ease-out"
          style={{ width: `${pct}%` }}
        />
      </div>
    </div>
  );
}
