"use client";

import { useState } from "react";

export default function Home() {
  const [form, setForm] = useState({
    damageType: "",
    description: "",
    latitude: "",
    longitude: "",
    imageUrl: "",
  });

  const [message, setMessage] = useState("");

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
      tipoDano: form.damageType,
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

          {/* Tipo de daño */}
          <div>
            <label className="mb-1 block font-medium text-gray-700">
              Tipo de daño
            </label>

            <select
              name="damageType"
              value={form.damageType}
              onChange={handleChange}
              required
              className="w-full rounded-lg border border-gray-300 p-3 text-gray-900"
            >
              <option value="">
                Seleccione un tipo de daño
              </option>

              <option value="infraestructura">
                Infraestructura
              </option>

              <option value="vial">
                Daño vial
              </option>

              <option value="inundacion">
                Inundación
              </option>

              <option value="deslizamiento">
                Deslizamiento
              </option>

              <option value="otro">
                Otro
              </option>
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