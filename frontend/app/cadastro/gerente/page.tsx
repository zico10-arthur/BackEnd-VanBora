import { CadastroGerenteCard } from "@/components/vanbora/cadastro/CadastroGerenteCard";
import { VanboraBlackLabelScaffold } from "@/components/vanbora/layout/VanboraBlackLabelScaffold";

export const metadata = { title: "Cadastro frotista — VanBora" };

export default function Page() {
  return (
    <VanboraBlackLabelScaffold>
      <CadastroGerenteCard />
    </VanboraBlackLabelScaffold>
  );
}
