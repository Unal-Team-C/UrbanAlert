#!/usr/bin/env bash
# Actualiza SOLO el frontend en el servidor, sin tocar los demás servicios ni el túnel.
#
# Qué cambió desde el último despliegue (merge de geoespacial-frontend + historial #15):
# solo código de frontend/reportes-front y next.config.ts. El único cambio de
# infrastructure/docker-compose.yml (montaje de geoespacial-db) solo aplica a volúmenes
# nuevos (los scripts de init no se repiten con datos ya existentes), así que no hace
# falta recrear ni tocar ese servicio aquí.
#
# Ejecutar en el servidor, dentro de infrastructure/ (donde está docker-compose.yml),
# con el repo en la rama que ya se usa en ese servidor.
set -euo pipefail

cd "$(dirname "$0")"

rama="$(git rev-parse --abbrev-ref HEAD)"
echo "Rama actual: $rama"

git fetch origin "$rama"
git merge --ff-only "origin/$rama"

# Reconstruye y reinicia solo el contenedor del frontend. --no-deps: no toca gateway,
# reportes, usuarios, geoespacial-api/db, rabbitmq ni el túnel de Cloudflare.
docker compose up -d --no-deps --build frontend

echo "Listo. Frontend actualizado; el resto de servicios y el túnel siguen como estaban."
docker compose ps frontend
