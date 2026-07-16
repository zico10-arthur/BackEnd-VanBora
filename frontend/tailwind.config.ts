import type { Config } from "tailwindcss";

const config: Config = {
  darkMode: "class",
  content: [
    "./pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ["var(--font-inter)", "system-ui", "sans-serif"],
        display: ["var(--font-display)", "var(--font-inter)", "system-ui", "sans-serif"],
      },
      borderRadius: {
        vb: "12px",
      },
      maxWidth: {
        container: "1280px",
      },
      colors: {
        "van-void": "#0A0A0A",
        "van-surface": "#141414",
        "van-elevated": "#1C1C1C",
        "van-border": "#2A2A2A",
        "van-amber": "#F0A500",
        "van-gold": "#FFE08A",
        "van-card": "#1C1C1C",
        "van-seat-occupied": "#2A2A2A",
      },
      boxShadow: {
        "seat-selected": "0 0 20px rgba(240, 165, 0, 0.45)",
        "van-glow": "0 0 40px rgba(240, 165, 0, 0.15)",
      },
      animation: {
        "fade-in": "fadeIn 0.5s ease-out",
      },
      keyframes: {
        fadeIn: {
          "0%": { opacity: "0", transform: "translateY(8px)" },
          "100%": { opacity: "1", transform: "translateY(0)" },
        },
      },
    },
  },
  plugins: [],
};
export default config;
