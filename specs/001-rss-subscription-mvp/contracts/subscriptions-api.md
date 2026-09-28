# API Contract: Subscriptions

**Phase**: MVP | **Base path**: `/api/subscriptions` (configured via `ApiBaseUrl` in the frontend's
`wwwroot/appsettings.json`, matching the backend's configured port per `TechStack.md`)

This is the contract the Blazor WebAssembly frontend consumes from the ASP.NET Core Web API
backend. It covers only the two MVP operations (FR-001, FR-002); no update/delete endpoints exist
in this phase (FR-009).

## POST /api/subscriptions

Adds a new subscription from a submitted URL.

**Request body**:

```json
{
  "url": "string"
}
```

**Responses**:

| Status | Condition | Body |
|--------|-----------|------|
| `201 Created` | `url` is non-blank | `{ "url": "string" }` (the created subscription) |
| `400 Bad Request` | `url` is missing, empty, or whitespace-only (FR-005) | `{ "error": "url must not be blank" }` |

**Behavior notes**:

- No validation is performed on `url` beyond the blank check (FR-004, FR-007) — any non-blank text
  is accepted, including malformed URLs or non-feed text.
- Duplicate `url` values are accepted and stored as separate entries (no de-duplication).
- The new subscription is appended to the end of the in-memory list (insertion order preserved).

## GET /api/subscriptions

Returns the full current subscription list for the running session.

**Responses**:

| Status | Condition | Body |
|--------|-----------|------|
| `200 OK` | Always | `[{ "url": "string" }, ...]` — zero or more entries, in insertion order |

**Behavior notes**:

- Returns an empty array (`[]`) when no subscriptions have been added yet (empty state, per Edge
  Cases in `spec.md`) — never an error for the empty case.
- Order MUST match the order subscriptions were added in (SC-002).
- Response reflects only the current in-memory state; nothing is fetched, parsed, or persisted.

## CORS

The backend MUST restrict `Access-Control-Allow-Origin` to the configured frontend origin(s) only
(e.g., `http://localhost:5213`, `https://localhost:7025`) — no wildcard (`*`) origins, per
Constitution Principle I and `TechStack.md`'s CORS configuration guidance.

## Out of scope for this contract

- `PUT`/`PATCH`/`DELETE` on subscriptions (removal/editing) — deferred per FR-009.
- Any feed-fetching or item-listing endpoints — deferred to Extended-MVP per `AppFeatures.md`.
- Authentication/authorization headers — not applicable (FR-008, single local user, no auth).
