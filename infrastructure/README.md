# Infraestructura local

`docker-compose.yml` levanta todos los servicios de UrbanAlert con sus bases de datos y RabbitMQ.

## Uso

Desde esta carpeta:

```bash
docker compose up -d --build     # construye las imágenes y levanta todo
docker compose ps                # estado de los servicios
docker compose logs -f reportes  # logs de un servicio
docker compose down              # detiene todo (conserva los datos)
docker compose down -v           # detiene todo y borra los volúmenes
```

Las credenciales tienen valores por defecto **solo para desarrollo**. Para cambiarlas: `cp .env.example .env` y editar `.env` (no se versiona).

## Servicios

| Servicio | URL en el host | Descripción |
|---|---|---|
| `frontend` | http://localhost:3000 | Formulario de reportes (Next.js) |
| `reportes` | http://localhost:5039 — Scalar: `/scalar/v1` | API de Reportes (.NET) |
| `auditoria` | http://localhost:5040 — Scalar: `/scalar/v1` | Auditoría de eventos (.NET) |
| `geoespacial-api` | http://localhost:8000 — Swagger: `/docs` | Ubicación de reportes (FastAPI) |
| `usuarios` | http://localhost:8081 — Swagger: `/swagger-ui.html` | Usuarios (Spring Boot) |
| `rabbitmq` | `localhost:5672` — panel: http://localhost:15672 | Bus de eventos |
| `postgres-reportes` | `localhost:5432` | Bases `urbanalert_reportes` y `urbanalert_auditoria` |
| `geoespacial-db` | `localhost:5433` | PostgreSQL + PostGIS (`geospatial`) |
| `postgres-usuarios` | `localhost:5434` | Base `urbanalert_users` |

## Cómo se conectan

```
frontend ──/api/reportes──► reportes ──HTTP──► geoespacial-api ──► geoespacial-db
                               │
                               ├──► postgres-reportes (urbanalert_reportes)
                               └──ReporteCreado──► rabbitmq ──► auditoria ──► postgres-reportes (urbanalert_auditoria)
usuarios ──► postgres-usuarios
```

- **Frontend → Reportes**: Next.js reenvía `/api/reportes/*` a `http://reportes:8080/api/v1/Reportes/*`. La URL se fija al construir la imagen (argumento `REPORTES_API_URL`).
- **Reportes y Auditoria** corren con `ASPNETCORE_ENVIRONMENT=Development`. Así:
  - Reportes aplica sus migraciones al arrancar y expone Scalar;
  - Auditoria arranca sin Cognito. Sus endpoints `/auditoria/*` siguen pidiendo un JWT; sin él responden 401.
- **`postgres-reportes/01-auditoria.sh`** se ejecuta solo con el volumen vacío:
  - crea la base de Auditoria y los roles `audit_writer`, `audit_reader` y `reportes_reader`;
  - aplica `backend/UrbanAlert.Auditoria/database/001_audit_store.sql`;
  - da a `reportes_reader` permiso de lectura sobre las tablas que creen después las migraciones de Reportes.

  Para volver a ejecutarlo: `docker compose down -v`.

## Notas

- Los puertos son los mismos que se usan al ejecutar cada servicio por separado: no se pueden levantar las dos cosas a la vez.
- Cada servicio conserva su propio compose (`backend/*/deploy/docker` o `compose.yaml`) para trabajar con él de forma aislada.
