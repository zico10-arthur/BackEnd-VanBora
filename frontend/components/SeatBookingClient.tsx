"use client";

import { useCallback, useState } from "react";
import type { ViagemVanDetalhe } from "@/lib/api/types";
import type { SeatItem } from "@/lib/types";
import { BookingBar } from "./BookingBar";
import { ExpressCheckoutModal } from "./ExpressCheckoutModal";
import { SeatMap } from "./SeatMap";

type Props = {
  trip: ViagemVanDetalhe;
  seats: SeatItem[];
};

export function SeatBookingClient({ trip, seats }: Props) {
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [checkoutOpen, setCheckoutOpen] = useState(false);

  const onSelect = useCallback((seat: SeatItem) => {
    if (seat.state === "occupied") return;
    setSelectedId((prev) => (prev === seat.id ? null : seat.id));
  }, []);

  const onAdvance = useCallback(() => {
    if (!selectedId) return;
    setCheckoutOpen(true);
  }, [selectedId]);

  const ticketPrice = trip.precoAssento;

  return (
    <>
      <div className="pb-36">
        <SeatMap seats={seats} selectedId={selectedId} onSelect={onSelect} />
      </div>
      <BookingBar selectedSeatLabel={selectedId} onAdvance={onAdvance} />
      <ExpressCheckoutModal
        open={checkoutOpen}
        onClose={() => setCheckoutOpen(false)}
        viagemVanId={trip.viagemVanId}
        eventName={trip.nomeEvento}
        seatLabel={selectedId ?? ""}
        departureLocation={trip.localPartida}
        vehicleModelColor={trip.modeloVan}
        vehiclePlate={trip.placaVan}
        ticketPrice={ticketPrice}
      />
    </>
  );
}
