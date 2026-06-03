import { ReservaPageClient } from "@/components/reserva/ReservaPageClient";

export const metadata = {
  title: "Reservar assento — VanBora",
};

type Props = { params: { viagemVanId: string } };

export default function ReservaPage({ params }: Props) {
  return <ReservaPageClient viagemVanId={params.viagemVanId} />;
}
