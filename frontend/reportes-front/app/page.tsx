"use client";

import { useEffect, useState } from "react";

// Forma de GET /api/v1/Reportes/catalogo (servicio de Reportes).
type TipoReporte = { codigo: string; nombre: string };
type CategoriaReporte = { codigo: string; nombre: string; tipos: TipoReporte[] };

export default function Home() {
  const [form, setForm] = useState({
    category: "",
    reportType: "",
    description: "",
    latitude: "",
    longitude: "",
    imageUrl: "",
  });

  const [message, setMessage] = useState("");

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

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const reportData = {
      categoria: form.category,
      tipo: form.reportType,
      descripcion: form.description,
      lat: Number(form.latitude),
      lon: Number(form.longitude),
      urlImagen: form.imageUrl,
    };

    console.log("Datos del reporte:", reportData);

    setMessage("Formulario diligenciado correctamente");
  }

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

          {/* Coordenadas */}
          <div>
            <p className="mb-2 font-medium text-gray-700">
              Ubicación
            </p>

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div>
                <label className="mb-1 block text-sm text-gray-600">
                  Latitud
                </label>

                <input
                  type="number"
                  step="any"
                  name="latitude"
                  value={form.latitude}
                  onChange={handleChange}
                  required
                  placeholder="Ej. 4.7110"
                  className="w-full rounded-lg border border-gray-300 p-3 text-gray-900"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm text-gray-600">
                  Longitud
                </label>

                <input
                  type="number"
                  step="any"
                  name="longitude"
                  value={form.longitude}
                  onChange={handleChange}
                  required
                  placeholder="Ej. -74.0721"
                  className="w-full rounded-lg border border-gray-300 p-3 text-gray-900"
                />
              </div>
            </div>
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
              placeholder="https://..."
              className="w-full rounded-lg border border-gray-300 p-3 text-gray-900"
            />
          </div>

          {/* Botón */}
          <button
            type="submit"
            className="w-full rounded-lg bg-black py-3 font-medium text-white hover:bg-gray-800"
          >
            Enviar reporte
          </button>
        </form>

        {message && (
          <p className="mt-5 font-medium text-green-600">
            {message}
          </p>
        )}
      </div>
    </main>
  );
}