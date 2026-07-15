"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState, useCallback } from "react";
import Link from "next/link";
import { VbButton } from "@/components/vanbora/ui/VbButton";
import { Header } from "@/components/Header";
import { GerenteGuard } from "@/app/gerente/vans/components/GerenteGuard";
import { ViagemForm } from "../components/ViagemForm";
import {
  criarViagemGerente,
  alocarVan,
  alocarMotorista,
} from "@/lib/api/viagens";
import { listarVans } from "@/lib/api/vans";
import { listarMotoristas } from "@/lib/api/motoristas";
import type {
  CriarViagemRequest,
  VanResponse,
  MotoristaResponse,
} from "@/lib/api/types";

type ErroEtapa = "criar" | "alocar-van" | "alocar-motorista" | null;

export default function NovaViagemPage() {
  const router = useRouter();

  // ── Recursos (vans + motoristas) ──
  const [vans, setVans] = useState<VanResponse[]>([]);
  const [motoristas, setMotoristas] = useState<MotoristaResponse[]>([]);
  const [loadingRecursos, setLoadingRecursos] = useState(true);
  const [erroRecursos, setErroRecursos] = useState<string | null>(null);

  // ── Seleção ──
  const [vanId, setVanId] = useState("");
  const [motoristaId, setMotoristaId] = useState("");

  // ── Submissão ──
  const [loading, setLoading] = useState(false);
  const [erroEtapa, setErroEtapa] = useState<ErroEtapa>(null);
  const [erroMensagem, setErroMensagem] = useState("");
  const [viagemCriadaId, setViagemCriadaId] = useState<string | null>(null);

  const fetchRecursos = useCallback(async () => {
    setLoadingRecursos(true);
    setErroRecursos(null);
    try {
      const [vansData, motoristasData] = await Promise.all([
        listarVans(),
        listarMotoristas(),
      ]);
      setVans(vansData);
      setMotoristas(motoristasData);
    } catch (err: unknown) {
      if (err instanceof Error && err.message.includes("401")) {
        router.push("/entrar");
        return;
      }
      if (err instanceof Error && err.message.includes("403")) {
        setErroRecursos("Acesso negado");
      } else {
        setErroRecursos(
          "Não foi possível carregar os recursos. Tente novamente.",
        );
      }
    } finally {
      setLoadingRecursos(false);
    }
  }, [router]);

  useEffect(() => {
    fetchRecursos();
  }, [fetchRecursos]);

  async function handleSubmit(data: CriarViagemRequest) {
    setLoading(true);
    setErroEtapa(null);
    setErroMensagem("");
    setViagemCriadaId(null);

    // Passo 1: criar viagem
    let viagem;
    try {
      viagem = await criarViagemGerente(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro desconhecido";
      setErroEtapa("criar");
      setErroMensagem(msg);
      setLoading(false);
      return;
    }

    setViagemCriadaId(viagem.viagemId);

    // Passo 2: alocar van
    let viagemVanResponse;
    try {
      viagemVanResponse = await alocarVan(viagem.viagemId, vanId);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro desconhecido";
      setErroEtapa("alocar-van");
      setErroMensagem(msg);
      setLoading(false);
      return;
    }

    // Extrair viagemVanId da van recém-alocada
    const viagemVanId =
      viagemVanResponse.vans[viagemVanResponse.vans.length - 1].viagemVanId;

    // Passo 3: alocar motorista
    try {
      await alocarMotorista(viagem.viagemId, viagemVanId, {
        motoristaId,
        viagemVanId,
      });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Erro desconhecido";
      setErroEtapa("alocar-motorista");
      setErroMensagem(msg);
      setLoading(false);
      return;
    }

    router.push("/gerente/viagens?sucesso=criada");
  }

  return (
    <GerenteGuard>
      <Header />
      <main className="mx-auto max-w-lg px-4 py-8 sm:px-6">
        <h1 className="mb-6 text-2xl font-bold text-zinc-100">
          Nova viagem
        </h1>

        {/* Banner de erro por etapa */}
        {erroEtapa && (
          <div className="mb-4 rounded-lg border p-4 text-sm space-y-2 bg-red-950/40 border-red-700/60 text-red-300">
            {erroEtapa === "criar" && (
              <p>Erro ao criar viagem: {erroMensagem}</p>
            )}
            {erroEtapa === "alocar-van" && (
              <>
                <p>
                  Viagem criada, mas não foi possível alocar a van:{" "}
                  {erroMensagem}
                </p>
                {viagemCriadaId && (
                  <Link
                    href={`/gerente/viagens/${viagemCriadaId}/alocar`}
                    className="inline-block font-semibold underline hover:text-red-200"
                  >
                    Gerenciar alocações
                  </Link>
                )}
              </>
            )}
            {erroEtapa === "alocar-motorista" && (
              <>
                <p>
                  Van alocada, mas não foi possível alocar o motorista:{" "}
                  {erroMensagem}
                </p>
                {viagemCriadaId && (
                  <Link
                    href={`/gerente/viagens/${viagemCriadaId}/alocar`}
                    className="inline-block font-semibold underline hover:text-red-200"
                  >
                    Gerenciar alocações
                  </Link>
                )}
              </>
            )}
          </div>
        )}

        <ViagemForm
          onSubmit={handleSubmit}
          submitLabel={loading ? "Criando viagem e alocando recursos…" : "Criar viagem"}
          vans={vans}
          motoristas={motoristas}
          vanId={vanId}
          motoristaId={motoristaId}
          onVanChange={setVanId}
          onMotoristaChange={setMotoristaId}
          loadingRecursos={loadingRecursos}
          erroRecursos={erroRecursos}
          onRetryRecursos={fetchRecursos}
        />
        <div className="mt-4 text-center">
          <VbButton
            variant="ghost"
            onClick={() => router.push("/gerente/viagens")}
          >
            Cancelar
          </VbButton>
        </div>
      </main>
    </GerenteGuard>
  );
}
