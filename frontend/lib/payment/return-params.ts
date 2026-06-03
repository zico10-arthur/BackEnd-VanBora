const UUID_RE =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

function isUuid(value: string): boolean {
  return UUID_RE.test(value);
}

/**
 * Resolve o id da reserva após retorno do Mercado Pago.
 * Ordem: query `reservaId` (nossa) → `external_reference` (MP) → sessionStorage.
 */
export function resolveReservaIdFromReturn(searchParams: URLSearchParams): string | null {
  const fromQuery = searchParams.get("reservaId")?.trim();
  if (fromQuery && isUuid(fromQuery)) return fromQuery;

  const externalRef = searchParams.get("external_reference")?.trim();
  if (externalRef && isUuid(externalRef)) return externalRef;

  const pending = typeof sessionStorage !== "undefined" ? sessionStorage.getItem("vanbora_pending_reserva") : null;
  if (pending) {
    try {
      const parsed = JSON.parse(pending) as { reservaId?: string };
      if (parsed.reservaId && isUuid(parsed.reservaId)) return parsed.reservaId;
    } catch {
      /* ignore */
    }
  }

  return null;
}

/** Status retornado pelo Checkout Pro na URL (collection_status / status). */
export function mercadoPagoCollectionStatus(searchParams: URLSearchParams): string | null {
  return (
    searchParams.get("collection_status")?.trim() ||
    searchParams.get("status")?.trim() ||
    null
  );
}
