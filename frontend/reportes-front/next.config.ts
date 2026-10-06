import type { NextConfig } from "next";

// El navegador llama a /api/v1/reportes/... en el mismo origen. Con el compose, el
// gateway atiende esa ruta antes de llegar aquí. Sin gateway (npm run dev), este
// rewrite la reenvía directo al servicio de Reportes, así no hace falta CORS.
// Se lee en `next build`.
const reportesApiUrl = process.env.REPORTES_API_URL ?? "http://localhost:5039";

const nextConfig: NextConfig = {
  // Imagen de Docker mínima: .next/standalone incluye server.js y solo los módulos necesarios.
  output: "standalone",
  rewrites() {
    return [
      {
        source: "/api/v1/reportes/:path*",
        destination: `${reportesApiUrl}/api/v1/Reportes/:path*`,
      },
    ];
  },
};

export default nextConfig;
