import { redirect } from "next/navigation";

export default function ViagemDetalhePage() {
  // Placeholder até Spec 60 — redireciona para a lista com toast informativo
  redirect("/gerente/viagens?sucesso=redirecionado");
}
