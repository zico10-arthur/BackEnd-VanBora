const TOKEN_KEY = "vanbora_token";
const USER_KEY = "vanbora_user";

export type StoredUser = {
  usuarioId: string;
  nome: string;
  email: string;
  perfis: string[];
};

export function setAuthSession(token: string, user: StoredUser): void {
  if (typeof window === "undefined") return;
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function clearAuthSession(): void {
  if (typeof window === "undefined") return;
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

export function getAuthToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(TOKEN_KEY);
}

export function getStoredUser(): StoredUser | null {
  if (typeof window === "undefined") return null;
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as StoredUser;
  } catch {
    return null;
  }
}

export function isGerenteOuMotorista(perfis: string[]): boolean {
  return perfis.some((p) => ["Gerente", "Motorista", "Admin"].includes(p));
}

export function isAdmin(perfis: string[]): boolean {
  return perfis.includes("Admin");
}
