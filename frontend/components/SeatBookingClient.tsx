"use client";

import { useCallback, useState } from "react";
import type { ViagemVanDetalhe } from "@/lib/api/types";
import type { SeatItem } from "@/lib/types";
import { BookingPanel } from "./BookingPanel";
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
    if (seat.state !== "available") return;
    setSelectedId((prev) => (prev === seat.id ? null : seat.id));
  }, []);

  const onAdvance = useCallback(() => {
    if (!selectedId) return;
    setCheckoutOpen(true);
  }, [selectedId]);

  const bookingProps = {
    eventName: trip.nomeEvento,
    localPartida: trip.localPartida,
    precoAssento: trip.precoAssento,
    selectedSeatLabel: selectedId,
    onAdvance,
  };

  return (
    <>
      <div className="lg:grid lg:grid-cols-[minmax(0,1fr)_minmax(280px,340px)] lg:items-start lg:gap-8 xl:gap-10">
        <div className="pb-36 lg:pb-0">
          <SeatMap seats={seats} selectedId={selectedId} onSelect={onSelect} />
        </div>
        <BookingPanel {...bookingProps} variant="panel" />
      </div>
      <BookingPanel {...bookingProps} variant="bar" />
      <ExpressCheckoutModal
        open={checkoutOpen}
        onClose={() => setCheckoutOpen(false)}
        viagemVanId={trip.viagemVanId}
        eventName={trip.nomeEvento}
        seatLabel={selectedId ?? ""}
        departureLocation={trip.localPartida}
        vehicleModelColor={trip.modeloVan}
        vehiclePlate={trip.placaVan}
        ticketPrice={trip.precoAssento}
      />
    </>
  );
}
