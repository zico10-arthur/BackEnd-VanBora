import { redirect } from "next/navigation";

export default async function ViagemDetalhePage({
  params,
}: {
  params: Promise<{ viagemId: string }>;
}) {
  const { viagemId } = await params;
  redirect(`/gerente/viagens/${viagemId}/relatorio`);
}
