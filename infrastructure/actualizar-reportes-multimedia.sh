#!/usr/bin/env bash
# Actualiza SOLO Reportes y Multimedia (y su minio/mongo/multimedia-init) en el servidor,
# sin recrear ni reiniciar gateway, frontend, auditoria, usuarios, geoespacial, rabbitmq,
# las bases de datos de los demás servicios, ni el túnel de Cloudflare.
#
# Qué cambió: Reportes ahora sube la imagen del reporte al servicio Multimedia por HTTP y
# guarda la URL que este devuelve (antes solo guardaba el nombre del archivo). Multimedia es
# nuevo en este compose (minio + mongo:7.0 + el proceso de init + la función).
#
# No se usa --no-deps: Reportes depende de que Multimedia exista y esté arriba, y Multimedia
# depende de minio/mongo sanos y de que multimedia-init termine bien. Se listan explícitamente
# los servicios a tocar; cualquier otra dependencia de Reportes (postgres-reportes, rabbitmq,
# geoespacial-api) ya está arriba y sana, así que compose solo la verifica, no la reinicia.
#
# Ejecutar en el servidor, dentro de infrastructure/ (donde está docker-compose.yml),
# con el repo en la rama que ya se usa en ese servidor.
set -euo pipefail

cd "$(dirname "$0")"

rama="$(git rev-parse --abbrev-ref HEAD)"
echo "Rama actual: $rama"

git fetch origin "$rama"
git merge --ff-only "origin/$rama"

docker compose up -d --build minio mongo multimedia-init multimedia reportes

echo "Listo. Reportes y Multimedia actualizados; el resto de servicios y el túnel siguen como estaban."
docker compose ps minio mongo multimedia-init multimedia reportes
