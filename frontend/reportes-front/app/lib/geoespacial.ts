import { obtenerJson } from "./api";

// Forma de GET /api/v1/geoespacial/coordinates/{id} (servicio Geoespacial).
export type Coordenada = {
  coordinateId: string;
  reportId: string;
  coordinate: { lat: number; lon: number };
};

export function obtenerCoordenada(idCoordenada: string, signal?: AbortSignal): Promise<Coordenada> {
  return obtenerJson<Coordenada>(
    `/api/v1/geoespacial/coordinates/${encodeURIComponent(idCoordenada)}`,
    signal
  );
}
