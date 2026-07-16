/** Textos de produto — evitar jargão interno na interface. */

export const BRAND = {
  name: "VanBora",
  tagline: "Mobilidade para megaeventos",
  heroChip: "Vans oficiais para eventos",
  heroHeadline: "O evento começa no embarque",
  heroSubheadline:
    "Vans parceiras para o estádio. Reserve seu assento, pague com Pix e viaje com tranquilidade.",
  heroCta: "Ver viagens do próximo jogo",
  heroCtaSecondary: "Sou frotista",
  socialProof: "+2.400 assentos reservados em jogos e shows",
} as const;

export const FAQ_ITEMS = [
  {
    q: "E se a van atrasar?",
    a: "Combinamos ponto e horário com o frotista. Em caso de atraso relevante, você é avisado pelo app e pelo WhatsApp cadastrado na reserva.",
  },
  {
    q: "O que acontece se o jogo for cancelado?",
    a: "Se o evento for cancelado ou remarcado, a reserva pode ser reembolsada ou remanejada conforme a política da viagem e do frotista parceiro.",
  },
  {
    q: "É seguro viajar?",
    a: "Trabalhamos com frotas parceiras verificadas. Você escolhe o assento, vê os dados da van e acompanha a reserva até o embarque.",
  },
  {
    q: "Recebo comprovante?",
    a: "Sim. Após o Pix ser aprovado, a reserva fica confirmada e você acessa o comprovante em Minhas reservas.",
  },
  {
    q: "Posso levar acompanhante?",
    a: "Cada assento é individual. Para levar alguém, reserve outro assento na mesma van — assim o lugar de cada um fica garantido.",
  },
  {
    q: "Quanto tempo tenho para pagar?",
    a: "Ao iniciar o pagamento, o assento fica reservado por até 10 minutos. Se o Pix não for concluído a tempo, o lugar volta a ficar disponível.",
  },
] as const;

export const RESERVA_STATUS_LABEL: Record<string, string> = {
  PendentePagamento: "Aguardando pagamento",
  Confirmada: "Confirmada",
  Cancelada: "Cancelada",
  Expirada: "Expirada",
  EmAndamento: "Em viagem",
  Concluida: "Concluída",
};

export function labelReservaStatus(status: string): string {
  return RESERVA_STATUS_LABEL[status] ?? status;
}
