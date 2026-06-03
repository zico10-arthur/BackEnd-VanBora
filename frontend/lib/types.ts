export type EventStatus = "active" | "sold_out";

export interface EventItem {
  id: string;
  name: string;
  status: EventStatus;
  subtitle: string;
  dateLabel: string;
  /** URL da arte de capa (apresentação — mocks). */
  coverImage: string;
}

export type SeatState = "available" | "occupied";

export interface SeatItem {
  id: string;
  row: string;
  position: 1 | 2 | 3 | 4;
  state: SeatState;
}
