import type { NextConfig } from "next";

// URL del servicio de Reportes. El navegador llama a /api/reportes/... en el
// mismo origen y Next.js reenvía la petición, así no hace falta CORS.
const reportesApiUrl = process.env.REPORTES_API_URL ?? "http://localhost:5039";

const nextConfig: NextConfig = {
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
