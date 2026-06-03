/**
 * Mocks: próximos jogos do Botafogo no Nilton Santos (abr/mai 2026).
 * Dados fictícios para desenvolvimento — calendário real pode divergir.
 */
export interface BotafogoGame {
  id: string;
  homeTeam: string;
  awayTeam: string;
  competition: string;
  venue: string;
  city: string;
  /** ISO 8601 data/hora de início (local) */
  kickoff: string;
  dateLabel: string;
}

export const MOCK_BOTAFOGO_GAMES: BotafogoGame[] = [
  {
    id: "bf-2026-04-12",
    homeTeam: "Botafogo",
    awayTeam: "Flamengo",
    competition: "Brasileirão Série A",
    venue: "Estádio Nilton Santos",
    city: "Rio de Janeiro",
    kickoff: "2026-04-12T16:00:00-03:00",
    dateLabel: "12 abr 2026 · 16h",
  },
  {
    id: "bf-2026-04-23",
    homeTeam: "Botafogo",
    awayTeam: "LDU Quito",
    competition: "Copa Sul-Americana — oitavas (volta)",
    venue: "Estádio Nilton Santos",
    city: "Rio de Janeiro",
    kickoff: "2026-04-23T19:00:00-03:00",
    dateLabel: "23 abr 2026 · 19h",
  },
  {
    id: "bf-2026-05-03",
    homeTeam: "Botafogo",
    awayTeam: "Palmeiras",
    competition: "Brasileirão Série A",
    venue: "Estádio Nilton Santos",
    city: "Rio de Janeiro",
    kickoff: "2026-05-03T18:30:00-03:00",
    dateLabel: "03 mai 2026 · 18h30",
  },
  {
    id: "bf-2026-05-17",
    homeTeam: "Botafogo",
    awayTeam: "Grêmio",
    competition: "Brasileirão Série A",
    venue: "Estádio Nilton Santos",
    city: "Rio de Janeiro",
    kickoff: "2026-05-17T16:00:00-03:00",
    dateLabel: "17 mai 2026 · 16h",
  },
];

export function getGameById(id: string): BotafogoGame | undefined {
  return MOCK_BOTAFOGO_GAMES.find((g) => g.id === id);
}
