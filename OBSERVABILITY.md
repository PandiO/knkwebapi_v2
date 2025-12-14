# Observability & Client Registry

## Header Contract
- **X-Knk-Client-Id**: unique app instance id (GUID/string)
- **X-Knk-Client-Type**: `paper-plugin` | `web-admin` | `web-player` | `worker` | `unknown`
- **X-Knk-Client-Version**: semver/build string
- **X-Knk-Client-Name**: optional human label

Rules:
- Missing headers → grouped under `unknown`
- Application-level identity only (no player/user identity)
- Do not log secrets (auth tokens/api keys)
- Do not store request bodies

## Endpoints
- Health:
  - GET `/health/live` → liveness
  - GET `/health/ready` → readiness + dependency statuses
  - GET `/api/health` → simple `{"status":"ok"}` for baseUrl ending with `/api`
- Admin (RequireAdmin policy):
  - GET `/api/admin/clients?activeWithinMinutes=60` → list of active clients
  - GET `/api/admin/clients/{clientType}/{clientId}` → client detail snapshot
  - GET `/api/admin/clients/all` → all known clients
  - POST `/api/admin/clients/cleanup?inactiveForHours=24` → remove inactive clients

## Telemetry (OpenTelemetry)
- Config via `Telemetry` in appsettings
  - `Enabled`: true/false
  - `Exporter`: `otlp` (current default)
  - `Otlp.Endpoint`: e.g. `http://localhost:4317`
- Tracing: HTTP server spans
- Metrics: HTTP request counts/durations
- Logging: structured logs with trace correlation (future)
- Prometheus: TODO add scraping endpoint when package version is selected

## Privacy Rules
- No user/player identifiers in labels or logs
- Labels allowed: clientType, clientId, route template
- Avoid high-cardinality labels (no query values)

## Client Registry
- In-memory store with rolling 60-minute buckets
- Data per client:
  - `lastSeenUtc`
  - `lastRequest` summary (method, route template, status code, duration, timestamp)
  - Counters: total/success/error, durations
- Extensible interface: allows future Redis implementation

## Enable OTLP locally
Run an OTLP collector (e.g., OpenTelemetry Collector) listening on `http://localhost:4317`.

## Future
- Add Prometheus scraping endpoint (`/metrics`) with stable package
- Wire `RequireAdmin` to your real auth setup
