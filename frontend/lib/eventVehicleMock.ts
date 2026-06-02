/** Dados de veículo por evento (mock). Substituir por API quando o frotista cadastrar a viagem. */
export interface EventVehicleInfo {
  modelColor: string;
  plate: string;
}

const DEFAULT_VEHICLE: EventVehicleInfo = {
  modelColor: "Mercedes Sprinter Branca",
  plate: "ABC-1D34",
};

/** Extensível por evento quando houver dados reais da viagem. */
const BY_EVENT: Partial<Record<string, EventVehicleInfo>> = {};

export function getVehicleForEvent(eventId: string): EventVehicleInfo {
  return BY_EVENT[eventId] ?? DEFAULT_VEHICLE;
}
