import { RelatorioClient } from "./RelatorioClient";

export const metadata = { title: "Relatório da Viagem — VanBora" };

export default async function RelatorioPage({
  params,
}: {
  params: Promise<{ viagemId: string }>;
}) {
  const { viagemId } = await params;
  return <RelatorioClient viagemId={viagemId} />;
}
