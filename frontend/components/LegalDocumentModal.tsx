"use client";

import { useId } from "react";

const TERMS_PLACEHOLDER = `TERMOS DE USO — VANBORA (VERSÃO RESUMIDA / DEMONSTRAÇÃO)

1. Aceitação
Ao utilizar a plataforma VanBora, você declara ter lido e concordado com estes Termos. Este texto é um modelo para desenvolvimento; a versão oficial será publicada pelo responsável legal da empresa.

2. Natureza do serviço
A VanBora oferece intermediação digital entre torcedores e frotistas parceiros para organização de deslocamentos a eventos. A VanBora não é transportadora: a execução do transporte é de responsabilidade do parceiro contratado.

3. Conta e uso
Você compromete-se a fornecer dados verídicos e a utilizar a plataforma de forma lícita. É proibido uso fraudulento, revenda abusiva ou qualquer conduta que prejudique outros usuários ou a operação do serviço.

4. Pagamentos e políticas de viagem
Valores, quórum, cancelamentos e estornos seguem as regras exibidas no momento da compra e comunicações da viagem específica.

5. Alterações
Estes termos poderão ser atualizados. Recomendamos consulta periódica. Em caso de dúvidas: juridico@vanbora.com (placeholder).`;

const PRIVACY_PLACEHOLDER = `POLÍTICA DE PRIVACIDADE — LGPD (VERSÃO RESUMIDA / DEMONSTRAÇÃO)

Esta Política descreve como a VanBora trata dados pessoais em conformidade com a Lei nº 13.709/2018 (LGPD). Texto modelo para desenvolvimento; a política oficial será mantida pelo Encarregado (DPO) da empresa.

1. Quem somos
Controlador: VanBora (dados de contato a serem informados no site institucional).

2. Dados que podemos coletar
Exemplos: nome completo, CPF, telefone/WhatsApp, e-mail, dados da reserva e de pagamento processados por parceiros financeiros certificados.

3. Finalidades
Execução do contrato de intermediação, comunicação sobre a viagem, suporte, cumprimento de obrigações legais, prevenção a fraudes e melhoria do serviço, quando aplicável.

4. Bases legais
Execução de contrato, legítimo interesse (com balanceamento de direitos), cumprimento de obrigação legal ou regulatória, e consentimento quando exigido.

5. Compartilhamento
Dados podem ser compartilhados com frotistas da viagem contratada, processadores de pagamento e prestadores de infraestrutura, nos limites necessários.

6. Direitos do titular
Acesso, correção, anonimização, portabilidade, eliminação de dados desnecessários, informação sobre compartilhamentos, revogação de consentimento quando aplicável, entre outros previstos na LGPD.

7. Segurança
Adotamos medidas técnicas e organizacionais compatíveis com o risco; nenhum sistema é 100% invulnerável.

8. Contato / DPO
Encarregado de dados: dpo@vanbora.com (placeholder).`;

export type LegalDocumentKind = "terms" | "privacy";

interface LegalDocumentModalProps {
  open: boolean;
  onClose: () => void;
  kind: LegalDocumentKind;
}

export function LegalDocumentModal({ open, onClose, kind }: LegalDocumentModalProps) {
  const titleId = useId();
  const title = kind === "terms" ? "Termos de Uso" : "Política de Privacidade";
  const body = kind === "terms" ? TERMS_PLACEHOLDER : PRIVACY_PLACEHOLDER;

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-[110] flex items-end justify-center p-4 sm:items-center">
      <button
        type="button"
        className="absolute inset-0 bg-black/80 backdrop-blur-sm"
        aria-label="Fechar documento"
        onClick={onClose}
      />
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className="relative flex max-h-[85vh] w-full max-w-lg flex-col overflow-hidden rounded-2xl border border-amber-500/25 bg-zinc-950 shadow-2xl"
      >
        <div className="flex shrink-0 items-center justify-between border-b border-white/10 px-4 py-3 sm:px-5">
          <h2 id={titleId} className="pr-2 text-base font-bold tracking-tight text-white sm:text-lg">
            {title}
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg p-2 text-neutral-400 transition hover:bg-white/10 hover:text-white focus:outline-none focus-visible:ring-2 focus-visible:ring-amber-500"
            aria-label="Fechar"
          >
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth={2}>
              <path d="M18 6L6 18M6 6l12 12" />
            </svg>
          </button>
        </div>
        <div className="min-h-0 flex-1 overflow-y-auto px-4 py-4 sm:px-5 sm:py-5">
          <p className="whitespace-pre-wrap text-left text-xs leading-relaxed text-zinc-400 sm:text-sm">{body}</p>
          <p className="mt-6 text-[11px] leading-relaxed text-zinc-600">
            Este texto pode ser atualizado. Em caso de dúvida, entre em contato com o suporte VanBora.
          </p>
        </div>
        <div className="shrink-0 border-t border-white/10 p-4 sm:px-5">
          <button
            type="button"
            onClick={onClose}
            className="w-full rounded-xl bg-amber-500 py-3 text-sm font-bold text-black transition hover:bg-amber-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-amber-400 focus-visible:ring-offset-2 focus-visible:ring-offset-zinc-950"
          >
            Entendi
          </button>
        </div>
      </div>
    </div>
  );
}
