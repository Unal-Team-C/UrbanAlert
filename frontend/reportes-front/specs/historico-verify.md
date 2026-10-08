# Verification — Report History

## Automated (run from `frontend/reportes-front`)

| Check | Command | Result |
|---|---|---|
| Unit + component tests | `npm test` | 34 passed (9 files) |
| Lint | `npx eslint app` | clean |
| Production build | `npm run build` | compiles; `/historial` and `/historial/[id]` generated |

## Manual against the compose gateway

Stack: `cd infrastructure && GATEWAY_PORT=8088 AUDITORIA_MODO_DESARROLLO=true docker compose up -d --build`, then http://localhost:8088/historial.

Data: 3 reports created through the gateway (2 users), one moved to `VERIFICADO`, one `RECHAZADO` with reason, one set to level `ALTA`. Checked with `curl` and headless Chrome screenshots.

| Check | Result |
|---|---|
| `/historial` renders list, totals, filters | OK |
| Totals match the backend (`REPORTADO 1, VERIFICADO 1, RECHAZADO 1, EN_INTERVENCION 0`; `DEFAULT 2, ALTA 1`) | OK, exact |
| UPPER_SNAKE codes accepted by `?estado=` and `?nivelEmergencia=` | OK |
| `/historial/[id]`: detail, user name resolved, location `4.60970, -74.08170` from Geoespacial | OK |
| Timeline through gateway (`X-Urban-Gateway-Key` added by nginx, dev-mode admin identity) | OK, 1 event as expected |
| `GET .../integridad` → `verified: true` | OK (API; button behaviour covered by component tests) |
| Event type emitted by Auditoría is `reporte.creado`, not `ReporteCreado` | **Bug found and fixed** in `LineaDeTiempo` (label map + test) |

Known and expected: the timeline has only the creation event (the `VERIFICADO` and `RECHAZADO` changes are not audited by the backend, see functional spec §7.1).

Not exercised in a browser: clicking filters/pager, "Cargar más" with real data, the "Alterada" result. These are covered by component tests only.
