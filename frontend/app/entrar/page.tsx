import { Suspense } from "react";
import { LoginPageClient } from "@/components/auth/LoginPageClient";
import { VanboraBlackLabelScaffold } from "@/components/vanbora/layout/VanboraBlackLabelScaffold";

export const metadata = { title: "Entrar — VanBora" };

export default function EntrarPage() {
  return (
    <VanboraBlackLabelScaffold>
      <Suspense fallback={<div className="text-zinc-500">Carregando…</div>}>
        <LoginPageClient />
      </Suspense>
    </VanboraBlackLabelScaffold>
  );
}
