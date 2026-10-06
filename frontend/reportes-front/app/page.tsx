"use client";

import { useCallback, useEffect, useState } from "react";

import { IconoMapa, IconoUbicacion } from "./components/Iconos";
import ReporteCreadoModal from "./components/ReporteCreadoModal";
import SeleccionUbicacionModal from "./components/SeleccionUbicacionModal";
import {
  dentroDeBogota,
  formatearCoordenada,
  mensajeErrorGps,
  type Coordenada,
  type Ubicacion,
} from "./lib/ubicacion";

// Línea entre la opción inicial ("Seleccione...") y las opciones reales.
const SEPARADOR = "──────────────────────";

// Forma de GET /api/v1/Reportes/catalogo (servicio de Reportes).
type TipoReporte = { codigo: string; nombre: string };
type CategoriaReporte = { codigo: string; nombre: string; tipos: TipoReporte[] };

// Errores de la API: {"message"} desde el middleware o ProblemDetails de ASP.NET
// ({"title", "errors"}) cuando el JSON no se puede convertir.
type ErrorApi = { message?: string; title?: string; errors?: Record<string, string[]> };

const FORMULARIO_VACIO = {
  category: "",
  reportType: "",
  description: "",
  imageUrl: "",
};

async function leerError(response: Response): Promise<string> {
  if (response.status === 503) {
    return "No fue posible registrar la ubicación del reporte. Intente nuevamente más tarde.";
  }

  const error = (await response.json().catch(() => null)) as ErrorApi | null;
  const detalle = error?.errors ? Object.values(error.errors).flat()[0] : undefined;
  return error?.message ?? detalle ?? error?.title ?? `Error inesperado (HTTP ${response.status}).`;
}

export default function Home() {
  const [form, setForm] = useState(FORMULARIO_VACIO);

  const [createdId, setCreatedId] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const [location, setLocation] = useState<Ubicacion | null>(null);
  const [locationError, setLocationError] = useState("");
  const [mapOpen, setMapOpen] = useState(false);
  const [locatingGps, setLocatingGps] = useState(false);

  const [categories, setCategories] = useState<CategoriaReporte[]>([]);
  const [catalogError, setCatalogError] = useState("");
  const [loadingCatalog, setLoadingCatalog] = useState(true);

  useEffect(() => {
    fetch("/api/reportes/catalogo")
      .then((response) => {
        if (!response.ok) {
          throw new Error(`HTTP ${response.status}`);
        }
        return response.json() as Promise<CategoriaReporte[]>;
      })
      .then(setCategories)
      .catch(() =>
        setCatalogError(
          "No fue posible cargar las categorías. Verifique que el servicio de Reportes esté disponible."
        )
      )
      .finally(() => setLoadingCatalog(false));
  }, []);

  const selectedCategory = categories.find(
    (category) => category.codigo === form.category
  );

  function handleCategoryChange(event: React.ChangeEvent<HTMLSelectElement>) {
    // Al cambiar de categoría, el tipo elegido deja de ser válido.
    setForm({ ...form, category: event.target.value, reportType: "" });
  }

  function handleChange(
    event: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >
  ) {
    setForm({
      ...form,
      [event.target.name]: event.target.value,
    });
  }

  function updateLocation(nueva: Ubicacion) {
    if (dentroDeBogota(nueva)) {
      setLocation(nueva);
      setLocationError("");
    } else {
      setLocationError("La ubicación está fuera de Bogotá. Seleccione el punto en el mapa.");
    }
  }

  function locateWithGps() {
    setLocationError("");

    // La geolocalización solo funciona en HTTPS (o en localhost).
    if (!window.isSecureContext || !("geolocation" in navigator)) {
      setLocationError(
        "Este navegador no permite obtener la ubicación aquí (requiere HTTPS). Seleccione el punto en el mapa."
      );
      return;
    }

    setLocatingGps(true);
    navigator.geolocation.getCurrentPosition(
      (position) => {
        setLocatingGps(false);
        updateLocation({
          lat: position.coords.latitude,
          lon: position.coords.longitude,
          origen: "gps",
          precisionMetros: position.coords.accuracy,
        });
      },
      (error) => {
        setLocatingGps(false);
        setLocationError(mensajeErrorGps(error));
      },
      { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 }
    );
  }

  const closeMap = useCallback(() => setMapOpen(false), []);

  function saveMapLocation(coordenada: Coordenada) {
    setMapOpen(false);
    updateLocation({ ...coordenada, origen: "mapa" });
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!location) {
      setLocationError("Indique la ubicación del reporte: en el mapa o con su ubicación actual.");
      return;
    }

    setSubmitting(true);
    setSubmitError("");

    const reportData = {
      categoria: form.category,
      tipo: form.reportType,
      descripcion: form.description,
      latitud: location.lat,
      longitud: location.lon,
      urlImagen: form.imageUrl,
    };

    try {
      const response = await fetch("/api/reportes", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(reportData),
      });

      if (response.ok) {
        const creado = (await response.json()) as { idReporte: string };
        setForm(FORMULARIO_VACIO);
        setLocation(null);
        setLocationError("");
        setCreatedId(creado.idReporte);
      } else {
        setSubmitError(await leerError(response));
      }
    } catch {
      setSubmitError("No fue posible conectar con el servicio de Reportes.");
    } finally {
      setSubmitting(false);
    }
  }

  const closeModal = useCallback(() => setCreatedId(null), []);

  return (
    <main className="min-h-screen bg-gray-100 flex items-center justify-center p-6">
      <div className="w-full max-w-xl rounded-xl bg-white p-8 shadow-lg">
        <h1 className="text-3xl font-bold text-gray-900">
          Crear reporte
        </h1>

        <p className="mt-2 mb-6 text-gray-500">
          Complete la información del daño reportado.
        </p>

        <form onSubmit={handleSubmit} className="space-y-5">

          {catalogError && (
            <p className="rounded-lg bg-red-50 p-3 text-sm text-red-700">
              {catalogError}
            </p>
          )}

          {/* Categoría */}
          <div>
            <label className="mb-1 block font-medium text-gray-700">
              Categoría
            </label>

            <select
              name="category"
              value={form.category}
              onChange={handleCategoryChange}
              required
              disabled={loadingCatalog || categories.length === 0}
              className="w-full rounded-lg border border-gray-300 p-3 text-gray-900 disabled:bg-gray-100"
            >
              <option value="">
                {loadingCatalog ? "Cargando categorías..." : "Seleccione una categoría"}
              </option>

              {categories.length > 0 && <option disabled>{SEPARADOR}</option>}

              {categories.map((category) => (
                <option key={category.codigo} value={category.codigo}>
                  {category.nombre}
                </option>
              ))}
            </select>
          </div>

          {/* Tipo de reporte (depende de la categoría) */}
          <div>
            <label className="mb-1 block font-medium text-gray-700">
              Tipo de reporte
            </label>

            <select
              name="reportType"
              value={form.reportType}
              onChange={handleChange}
              required
              disabled={!selectedCategory}
              className="w-full rounded-lg border border-gray-300 p-3 text-gray-900 disabled:bg-gray-100"
            >
              <option value="">
                {selectedCategory ? "Seleccione un tipo de reporte" : "Primero seleccione una categoría"}
              </option>

              {selectedCategory && <option disabled>{SEPARADOR}</option>}

              {selectedCategory?.tipos.map((type) => (
                <option key={type.codigo} value={type.codigo}>
                  {type.nombre}
                </option>
              ))}
            </select>
          </div>

          {/* Descripción */}
          <div>
            <label className="mb-1 block font-medium text-gray-700">
              Descripción
            </label>

            <textarea
              name="description"
              value={form.description}
              onChange={handleChange}
              required
              rows={4}
              placeholder="Describa el daño..."
              className="w-full rounded-lg border border-gray-300 p-3 text-gray-900"
            />
          </div>

          {/* Ubicación: en el mapa o con el GPS del dispositivo */}
          <div>
            <p className="mb-2 font-medium text-gray-700">
              Ubicación
            </p>

            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <button
                type="button"
                onClick={() => setMapOpen(true)}
                className="flex items-center justify-center gap-2 rounded-lg border border-gray-300 px-4 py-3 font-medium text-gray-800 hover:bg-gray-50"
              >
                <IconoMapa />
                Seleccionar en el mapa
              </button>

              <button
                type="button"
                onClick={locateWithGps}
                disabled={locatingGps}
                className="flex items-center justify-center gap-2 rounded-lg border border-gray-300 px-4 py-3 font-medium text-gray-800 hover:bg-gray-50 disabled:bg-gray-100 disabled:text-gray-500"
              >
                <IconoUbicacion className={`h-5 w-5 ${locatingGps ? "animate-pulse" : ""}`} />
                {locatingGps ? "Obteniendo ubicación..." : "Usar mi ubicación"}
              </button>
            </div>

            {location ? (
              <p className="mt-3 rounded-lg bg-gray-50 p-3 text-sm text-gray-700">
                <span className="font-medium">
                  {location.origen === "gps" ? "Ubicación del dispositivo" : "Punto seleccionado en el mapa"}:
                </span>{" "}
                <span className="font-mono">{formatearCoordenada(location)}</span>
                {location.precisionMetros !== undefined && (
                  <span className="text-gray-500"> (precisión ±{Math.round(location.precisionMetros)} m)</span>
                )}
              </p>
            ) : (
              <p className="mt-3 text-sm text-gray-500">Aún no se ha indicado la ubicación.</p>
            )}

            {locationError && (
              <p role="alert" className="mt-2 text-sm text-red-600">
                {locationError}
              </p>
            )}
          </div>

          {/* Imagen */}
          <div>
            <label className="mb-1 block font-medium text-gray-700">
              URL de imagen
            </label>

            <input
              type="url"
              name="imageUrl"
              value={form.imageUrl}
              onChange={handleChange}
              required
              placeholder="https://..."
              className="w-full rounded-lg border border-gray-300 p-3 text-gray-900"
            />
          </div>

          {/* Botón */}
          <button
            type="submit"
            disabled={submitting}
            className="w-full rounded-lg bg-black py-3 font-medium text-white hover:bg-gray-800 disabled:bg-gray-400"
          >
            {submitting ? "Enviando..." : "Enviar reporte"}
          </button>
        </form>

        {submitError && (
          <p role="alert" className="mt-5 font-medium text-red-600">
            {submitError}
          </p>
        )}
      </div>

      {createdId && <ReporteCreadoModal idReporte={createdId} onClose={closeModal} />}

      {mapOpen && (
        <SeleccionUbicacionModal inicial={location} onGuardar={saveMapLocation} onCerrar={closeMap} />
      )}
    </main>
  );
}