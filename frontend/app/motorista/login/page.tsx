import { MotoristaLoginCard } from "@/components/motorista/MotoristaLoginCard";
import { VanboraBlackLabelScaffold } from "@/components/vanbora/layout/VanboraBlackLabelScaffold";

export const metadata = { title: "Login frotista — VanBora" };

export default function Page() {
  return (
    <VanboraBlackLabelScaffold>
      <MotoristaLoginCard />
    </VanboraBlackLabelScaffold>
  );
}
