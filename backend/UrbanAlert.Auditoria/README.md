# UrbanAlert.Auditoria

Servicio .NET 10 para auditar mensajes MassTransit de `UrbanAlert.Reportes`. Guarda eventos en PostgreSQL append-only, calcula una cadena SHA-256 global e idempotente por `eventId`, y ofrece API para consultar el historial e integridad.

## Flujo auditado

El consumidor usa el tipo .NET `Application.Reportes.Eventos.ReporteCreadoEvent`, publicado actualmente por `UrbanAlert.Reportes`. La auditoría normaliza los campos disponibles (`IdEvento`, `Id`, `IdUsuario`, `IdCoordenada`, `Estado` y `Fecha`). El mensaje actual no trae `CorrelationId`, por lo que queda nulo en el registro. Hoy solo se audita la creación: Reportes todavía no publica eventos para sus cambios de estado, asignación, nivel de emergencia, rechazo o eliminación.

La API expone:

- `GET /auditoria/reportes/{reportId}?limit=100&cursor=0`
- `GET /auditoria/reportes/{reportId}/integridad` (solo gestor/admin)
- `GET /health`

La API valida JWT de Cognito y consulta la tabla `"Reportes"` de PostgreSQL. Ciudadanos solo consultan reportes cuyo `IdUsuario` coincide con su `sub`; gestores, aquellos cuyo `IdResponsable` coincide; admin puede consultar cualquier reporte.

## Preparación

1. Crear en la base de auditoría los roles `audit_writer` y `audit_reader` con credenciales gestionadas fuera del repositorio. Aplicar `database/001_audit_store.sql` como administrador. El escritor solo inserta eventos y actualiza la cabeza de la cadena; el lector solo consulta. La tabla de eventos rechaza `UPDATE`, `DELETE` y `TRUNCATE` mediante triggers.
2. Configurar las variables de entorno:

```text
ConnectionStrings__AuditWriter=<PostgreSQL con rol audit_writer>
ConnectionStrings__AuditReader=<PostgreSQL con rol audit_reader>
ConnectionStrings__CorePostgres=<PostgreSQL del servicio UrbanAlert.Reportes, solo lectura>
Cognito__Issuer=<issuer del User Pool>
Cognito__AppClientId=<App Client ID>
Gateway__SharedSecret=<secreto compartido con API Gateway>
RabbitMq__Enabled=true
RabbitMq__Host=<host RabbitMQ>
RabbitMq__VirtualHost=/
RabbitMq__Username=<usuario>
RabbitMq__Password=<secreto>
RabbitMq__Queue=q_auditoria_dotnet
AuditArchive__Bucket=<bucket Object Lock, opcional>
AuditArchive__ObjectLockRetentionDays=2555
AuditArchive__ServiceUrl=<endpoint S3 compatible, opcional>
AWS_REGION=us-east-1
```

Si `AuditArchive__Bucket` está vacío, el archivado queda desactivado. Para activarlo, el bucket debe tener S3 Object Lock habilitado y el rol de ejecución necesita permisos de lectura de metadatos y escritura con retención (`s3:GetObject`, `s3:PutObject` y `s3:PutObjectRetention`). El SDK obtiene credenciales desde la cadena estándar de AWS; en despliegue se recomienda un rol de ejecución en lugar de claves estáticas.

Por cada evento, el servicio consulta primero `events/{yyyy-MM-dd}/{eventId}.json`; si no existe, archiva el sobre completo, el hash anterior y el hash calculado con modo `COMPLIANCE`. La fecha de retención se calcula desde el momento del archivado. Si S3 falla después del commit de PostgreSQL, el mensaje se reencola; la clave idempotente evita añadir otro evento y el reintento vuelve a intentar el archivo.

3. Ejecutar desde esta carpeta:

```powershell
dotnet run --project UrbanAlert.Auditoria.csproj
dotnet test tests/UrbanAlert.Auditoria.Tests.csproj
```

La suite incluye tests unitarios y de integración. Los tests de persistencia/autorización usan Testcontainers y requieren Docker disponible; se inicia un PostgreSQL temporal y se aplica `database/001_audit_store.sql`.

La imagen se construye con el `Dockerfile` de esta carpeta. MassTransit crea y enlaza la cola `q_auditoria_dotnet` al exchange del tipo `ReporteCreadoEvent`; los errores persistentes siguen la política de reintentos/error queue de MassTransit.

## Integración y alcance

El consumidor se conecta únicamente al tipo MassTransit de Reportes .NET. En el workspace no existe un productor Geoespacial .NET que publique eventos para esta auditoría.

La cobertura seguirá incompleta hasta que los handlers .NET publiquen eventos para transiciones, asignación, nivel de emergencia, rechazo y eliminación. El contrato actual no incluye `CorrelationId`, municipio ni coordenadas geográficas; el consumidor no fabrica estos datos. Además, `IdUsuario` se genera aleatoriamente, así que el filtro de propietario no coincidirá con el `sub` real hasta incorporar autenticación en Reportes.

El archivo de auditoría, la consulta de integridad y el archivado opcional S3 Object Lock están portados. La política de retención y el bucket deben configurarse en AWS; este servicio no crea ni configura el bucket.

El detalle del flujo, el contenido normalizado del evento y las brechas actuales está en [docs/funcionamiento-dotnet.md](docs/funcionamiento-dotnet.md).
