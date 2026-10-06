#!/bin/bash
# Se ejecuta una sola vez, con el volumen vacío (docker-entrypoint-initdb.d).
# Prepara en el mismo PostgreSQL la base de Auditoria y sus roles, y deja a
# Auditoria leer la tabla "Reportes" que crearán después las migraciones de Reportes.
set -euo pipefail

psql_admin() {
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" "$@"
}

psql_admin --dbname "$POSTGRES_DB" \
    -v audit_writer_password="$AUDIT_WRITER_PASSWORD" \
    -v audit_reader_password="$AUDIT_READER_PASSWORD" \
    -v reportes_reader_password="$REPORTES_READER_PASSWORD" \
    -v propietario="$POSTGRES_USER" <<'SQL'
CREATE DATABASE urbanalert_auditoria;
CREATE ROLE audit_writer LOGIN PASSWORD :'audit_writer_password';
CREATE ROLE audit_reader LOGIN PASSWORD :'audit_reader_password';
CREATE ROLE reportes_reader LOGIN PASSWORD :'reportes_reader_password';

-- Lectura de "Reportes" para la autorización de Auditoria. La tabla aún no existe:
-- los privilegios por defecto se aplican a las tablas que cree el dueño más adelante.
GRANT USAGE ON SCHEMA public TO reportes_reader;
ALTER DEFAULT PRIVILEGES FOR ROLE :"propietario" IN SCHEMA public GRANT SELECT ON TABLES TO reportes_reader;
SQL

psql_admin --dbname urbanalert_auditoria -f /auditoria-sql/001_audit_store.sql
psql_admin --dbname urbanalert_auditoria -c "GRANT USAGE ON SCHEMA public TO audit_writer, audit_reader;"
