"use client";

import { BookingPanel } from "./BookingPanel";

interface BookingBarProps {
  eventName: string;
  localPartida: string;
  precoAssento: number;
  selectedSeatLabel: string | null;
  onAdvance: () => void;
}

/** Barra fixa mobile — delega para BookingPanel variant="bar". */
export function BookingBar(props: BookingBarProps) {
  return <BookingPanel {...props} variant="bar" />;
}
