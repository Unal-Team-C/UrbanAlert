"use client";

import { useState } from "react";
import { verificarIntegridad, type Integridad } from "../../lib/auditoria";
import { formatearFecha } from "../../lib/formato";

export default function VerificacionIntegridad({ idReporte }: { idReporte: string }) {
  const [resultado, setResultado] = useState<Integridad | null>(null);
  const [error, setError] = useState("");
  const [verificando, setVerificando] = useState(false);

  async function verificar() {
    setVerificando(true);
    setError("");
    try {
      setResultado(await verificarIntegridad(idReporte));
    } catch (e) {
      setResultado(null);
      setError((e as Error).message);
    } finally {
      setVerificando(false);
    }
  }

  return (
    <section
      aria-labelledby="titulo-integridad"
      className="rounded-xl bg-white p-6 shadow dark:bg-gray-900 dark:shadow-none dark:ring-1 dark:ring-gray-800"
    >
      <h2
        id="titulo-integridad"
        className="mb-2 text-xl font-semibold text-gray-900 dark:text-gray-100"
      >
        Integridad del historial
      </h2>
      <p className="mb-3 text-sm text-gray-500 dark:text-gray-400">
        Comprueba que la cadena de hashes de auditoría de este reporte no fue alterada.
      </p>
      <button
        type="button"
        onClick={verificar}
        disabled={verificando}
        className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50 dark:bg-blue-700 dark:hover:bg-blue-600"
      >
        {verificando ? "Verificando..." : "Verificar integridad"}
      </button>

      {error && (
        <p role="alert" className="mt-3 text-sm text-red-700 dark:text-red-400">
          {error}
        </p>
      )}
      {resultado && (
        <p
          role="status"
          className={`mt-3 text-sm font-medium ${
            resultado.verified ? "text-green-700 dark:text-green-400" : "text-red-700 dark:text-red-400"
          }`}
        >
          {resultado.verified
            ? "Válida"
            : `Alterada${resultado.brokenSequence !== null ? ` (secuencia ${resultado.brokenSequence})` : ""}`}
          <span className="ml-2 font-normal text-gray-500 dark:text-gray-400">
            Verificado {formatearFecha(resultado.checkedAt)}
          </span>
        </p>
      )}
    </section>
  );
}
