export function Cargando({ texto = "Cargando..." }: { texto?: string }) {
  return (
    <p role="status" className="py-8 text-center text-gray-500 dark:text-gray-400">
      {texto}
    </p>
  );
}

export function Vacio({ texto }: { texto: string }) {
  return <p className="py-8 text-center text-gray-500 dark:text-gray-400">{texto}</p>;
}

export function ErrorReintentar({
  mensaje,
  onReintentar,
}: {
  mensaje: string;
  onReintentar: () => void;
}) {
  return (
    <div
      role="alert"
      className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-300"
    >
      <p>{mensaje}</p>
      <button
        type="button"
        onClick={onReintentar}
        className="mt-2 rounded-md bg-red-600 px-3 py-1 text-sm font-medium text-white hover:bg-red-700 dark:bg-red-700 dark:hover:bg-red-600"
      >
        Reintentar
      </button>
    </div>
  );
}
