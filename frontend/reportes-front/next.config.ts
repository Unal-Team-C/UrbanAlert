import type { NextConfig } from "next";

// URL del servicio de Reportes. El navegador llama a /api/reportes/... en el
// mismo origen y Next.js reenvía la petición, así no hace falta CORS.
// Se lee en `next build`: en Docker se pasa como argumento de build.
const reportesApiUrl = process.env.REPORTES_API_URL ?? "http://localhost:5039";

const nextConfig: NextConfig = {
  // Imagen de Docker mínima: .next/standalone incluye server.js y solo los módulos necesarios.
  output: "standalone",
  rewrites() {
    return [
      {
        source: "/api/reportes/:path*",
        destination: `${reportesApiUrl}/api/v1/Reportes/:path*`,
      },
    ];
  },
};

export default nextConfig;
