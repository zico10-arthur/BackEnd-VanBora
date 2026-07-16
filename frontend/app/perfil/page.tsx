import { Header } from "@/components/Header";
import { PerfilClient } from "./PerfilClient";

export const metadata = { title: "Meu perfil — VanBora" };

export default function PerfilPage() {
  return (
    <>
      <Header />
      <PerfilClient />
    </>
  );
}
