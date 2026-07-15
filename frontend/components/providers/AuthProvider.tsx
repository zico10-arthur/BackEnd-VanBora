"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { authLogin } from "@/lib/api/auth";
import { ApiError } from "@/lib/api/http";
import {
  clearAuthSession,
  getStoredUser,
  isAdmin,
  isGerenteOuMotorista,
  setAuthSession,
  type StoredUser,
} from "@/lib/auth/token";

type AuthContextValue = {
  user: StoredUser | null;
  ready: boolean;
  login: (email: string, senha: string) => Promise<{ redirectPainel: boolean; redirectAdmin: boolean }>;
  logout: () => void;
  setUserFromToken: (token: string, user: StoredUser) => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<StoredUser | null>(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    setUser(getStoredUser());
    setReady(true);
  }, []);

  const login = useCallback(async (email: string, senha: string) => {
    const data = await authLogin(email.trim(), senha);
    const stored: StoredUser = {
      usuarioId: data.usuarioId,
      nome: data.nome,
      email: data.email,
      perfis: data.perfis ?? [],
    };
    setAuthSession(data.token, stored);
    setUser(stored);
    return {
      redirectPainel: isGerenteOuMotorista(stored.perfis),
      redirectAdmin: isAdmin(stored.perfis),
    };
  }, []);

  const logout = useCallback(() => {
    clearAuthSession();
    setUser(null);
  }, []);

  const setUserFromToken = useCallback((token: string, u: StoredUser) => {
    setAuthSession(token, u);
    setUser(u);
  }, []);

  const value = useMemo(
    () => ({ user, ready, login, logout, setUserFromToken }),
    [user, ready, login, logout, setUserFromToken],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth deve ser usado dentro de AuthProvider");
  return ctx;
}

export function useAuthOptional() {
  return useContext(AuthContext);
}

export { ApiError };
