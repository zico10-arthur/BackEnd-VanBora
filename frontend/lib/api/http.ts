import { getApiBaseUrl } from "./base-url";
import { getAuthToken } from "@/lib/auth/token";

export class ApiError extends Error {
  readonly status: number;
  readonly code?: string;

  constructor(status: number, message: string, code?: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
  }
}

function isRecord(v: unknown): v is Record<string, unknown> {
  return typeof v === "object" && v !== null;
}

export function extractApiErrorMessage(status: number, body: unknown): { message: string; code?: string } {
  if (!isRecord(body)) return { message: `Erro HTTP ${status}` };

  const nested = body.error;
  if (isRecord(nested)) {
    const code = typeof nested.code === "string" ? nested.code : undefined;
    const message = typeof nested.message === "string" ? nested.message : undefined;
    if (message) return { message, code };
  }

  const codeTop = typeof body.code === "string" ? body.code : undefined;
  const msgTop = typeof body.message === "string" ? body.message : undefined;
  if (msgTop) return { message: msgTop, code: codeTop };

  const title = typeof body.title === "string" ? body.title : undefined;
  if (title) return { message: title };

  return { message: `Erro HTTP ${status}` };
}

type FetchOptions = {
  method?: string;
  body?: unknown;
  auth?: boolean;
};

async function request<T>(path: string, options: FetchOptions = {}): Promise<T> {
  const { method = "GET", body, auth = false } = options;
  const headers: Record<string, string> = {
    Accept: "application/json",
  };

  if (body !== undefined) headers["Content-Type"] = "application/json";

  if (auth) {
    const token = getAuthToken();
    if (!token) throw new ApiError(401, "Faça login para continuar.");
    headers.Authorization = `Bearer ${token}`;
  }

  const url = `${getApiBaseUrl()}${path.startsWith("/") ? path : `/${path}`}`;
  const res = await fetch(url, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  const text = await res.text();
  let json: unknown = null;
  if (text) {
    try {
      json = JSON.parse(text) as unknown;
    } catch {
      throw new ApiError(res.status, text.slice(0, 200) || "Resposta inválida");
    }
  }

  if (!res.ok) {
    const { message, code } = extractApiErrorMessage(res.status, json);
    throw new ApiError(res.status, message, code);
  }

  return json as T;
}

export function apiGet<T>(path: string, auth = false) {
  return request<T>(path, { auth });
}

export function apiPost<T>(path: string, body: unknown, auth = false) {
  return request<T>(path, { method: "POST", body, auth });
}

export function apiPut<T>(path: string, body: unknown, auth = false) {
  return request<T>(path, { method: "PUT", body, auth });
}

export function apiDelete<T>(path: string, auth = false) {
  return request<T>(path, { method: "DELETE", auth });
}

export function apiPatch<T>(path: string, body: unknown, auth = false) {
  return request<T>(path, { method: "PATCH", body, auth });
}
