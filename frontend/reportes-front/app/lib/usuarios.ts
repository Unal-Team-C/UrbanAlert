// Forma de GET /api/v1/users (servicio de Usuarios).
export type Usuario = { id: string; name: string; email: string; role: "USER" | "ADMIN" };

// Mientras no haya inicio de sesión, el reporte se atribuye a un ciudadano (rol USER)
// elegido al azar entre los registrados.
export function elegirCiudadanoAlAzar(usuarios: Usuario[]): Usuario | null {
  const ciudadanos = usuarios.filter((usuario) => usuario.role === "USER");
  if (ciudadanos.length === 0) return null;
  return ciudadanos[Math.floor(Math.random() * ciudadanos.length)];
}
