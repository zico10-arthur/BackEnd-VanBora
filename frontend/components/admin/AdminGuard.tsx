"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/providers/AuthProvider";

export function AdminGuard({ children }: { children: React.ReactNode }) {
  const { user, ready } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!ready) return;
    if (!user) {
      router.push("/entrar");
      return;
    }
    if (!user.perfis.includes("Admin")) {
      router.push("/");
    }
  }, [user, ready, router]);

  if (!ready || !user) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-van-void">
        <div className="h-8 w-8 animate-spin rounded-full border-2 border-van-amber border-t-transparent" />
      </div>
    );
  }

  if (!user.perfis.includes("Admin")) {
    return null;
  }

  return <>{children}</>;
}
