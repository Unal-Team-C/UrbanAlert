import type { NextConfig } from "next";

// URLs de los servicios. El navegador llama a /api/reportes/... y /api/usuarios/... en el
// mismo origen y Next.js reenvía la petición, así no hace falta CORS.
const reportesApiUrl = process.env.REPORTES_API_URL ?? "http://localhost:5039";
const usuariosApiUrl = process.env.USUARIOS_API_URL ?? "http://localhost:8081";

const nextConfig: NextConfig = {
  rewrites() {
    return [
      {
        source: "/api/reportes/:path*",
        destination: `${reportesApiUrl}/api/v1/Reportes/:path*`,
      },
      {
        source: "/api/usuarios/:path*",
        destination: `${usuariosApiUrl}/api/v1/users/:path*`,
      },
    ];
  },
};

export default nextConfig;
