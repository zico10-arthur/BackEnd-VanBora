"use client";

import { useEffect, useState } from "react";

type ToastBannerProps = {
  message: string | null;
  type?: "success" | "error";
  onDismiss?: () => void;
};

export function ToastBanner({ message, type = "success", onDismiss }: ToastBannerProps) {
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    if (message) {
      setVisible(true);
      const timer = setTimeout(() => {
        setVisible(false);
        onDismiss?.();
      }, 4000);
      return () => clearTimeout(timer);
    }
  }, [message, onDismiss]);

  if (!message || !visible) return null;

  const styles = {
    success: "border-emerald-700/60 bg-emerald-950/60 text-emerald-300",
    error: "border-red-700/60 bg-red-950/60 text-red-300",
  };

  return (
    <div
      className={`mb-6 rounded-xl border px-4 py-3 text-sm font-medium ${styles[type]}`}
      role="alert"
    >
      {message}
    </div>
  );
}
