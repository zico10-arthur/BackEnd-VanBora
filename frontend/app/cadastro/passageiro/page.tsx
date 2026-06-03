import { CadastroPassageiroCard } from "@/components/vanbora/cadastro/CadastroPassageiroCard";
import { VanboraBlackLabelScaffold } from "@/components/vanbora/layout/VanboraBlackLabelScaffold";

export const metadata = { title: "Cadastro — VanBora" };

export default function Page() {
  return (
    <VanboraBlackLabelScaffold>
      <CadastroPassageiroCard />
    </VanboraBlackLabelScaffold>
  );
}
