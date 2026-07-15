import type { Metadata } from "next";
import { ViagensListClient } from "./ViagensListClient";

export const metadata: Metadata = {
  title: "Minhas Viagens — VanBora",
};

export default async function ViagensPage({
  searchParams,
}: {
  searchParams: Promise<{ [key: string]: string | string[] | undefined }>;
}) {
  const params = await searchParams;
  const sucesso = typeof params.sucesso === "string" ? params.sucesso : null;

  return <ViagensListClient sucesso={sucesso} />;
}
