import { notFound } from "next/navigation";
import { TripManagementDashboard } from "@/components/motorista/TripManagementDashboard";
import { getEventById } from "@/lib/mockData";

interface PageProps {
  params: { tripId: string };
}

export const metadata = {
  title: "Gestão da viagem — VanBora",
  description: "Acompanhe vendas, quórum e link de divulgação.",
};

export default function GestaoViagemPage({ params }: PageProps) {
  const event = getEventById(params.tripId);
  if (!event || event.status === "sold_out") {
    notFound();
  }

  return <TripManagementDashboard event={event} tripId={params.tripId} />;
}
