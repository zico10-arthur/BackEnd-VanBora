const COVERS = [
  "https://images.unsplash.com/photo-1574629810360-7efbbe195018?auto=format&fit=crop&w=1400&q=85",
  "https://images.unsplash.com/photo-1522778119026-d647f0596c20?auto=format&fit=crop&w=1400&q=85",
  "https://images.unsplash.com/photo-1431324155629-1a6deb48526a?auto=format&fit=crop&w=1400&q=85",
];

export function coverForId(id: string | undefined | null): string {
  if (!id) return COVERS[0];
  let hash = 0;
  for (let i = 0; i < id.length; i++) hash = (hash + id.charCodeAt(i)) % COVERS.length;
  return COVERS[hash] ?? COVERS[0];
}
