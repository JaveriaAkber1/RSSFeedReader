# Research: MVP RSS Reader - Subscription Management

**Phase**: MVP (per `StakeholderDocuments/ProjectGoals.md`)

All unknowns below were resolved from `StakeholderDocuments/ProjectGoals.md`,
`StakeholderDocuments/TechStack.md`, `StakeholderDocuments/AppFeatures.md`, and
`.specify/memory/constitution.md`. No open `NEEDS CLARIFICATION` items remain.

## 1. Target .NET version

- **Decision**: .NET 10 (current LTS release).
- **Rationale**: `TechStack.md` mandates ASP.NET Core Web API + Blazor WebAssembly but does not
  pin a specific .NET version. With no existing `global.json` or `.csproj` in the repo, the
  greenfield project should target the current LTS SDK available to the developer at scaffold
  time to maximize support lifetime.
- **Alternatives considered**: Pin to .NET 8 (previous LTS) — rejected because there is no
  stakeholder requirement to support an older SDK and a new project should start on the current
  LTS; the exact SDK is verified at scaffold time (`dotnet --version`) rather than hardcoded here.

## 2. API style: Minimal APIs vs. Controllers

- **Decision**: ASP.NET Core Minimal APIs (`app.MapGet` / `app.MapPost` in `Program.cs`).
- **Rationale**: Only two operations are needed (`add subscription`, `list subscriptions`).
  Constitution Principle V (Simplicity & YAGNI) favors the simplest solution; Minimal APIs avoid
  controller/route-attribute boilerplate for a two-endpoint surface.
- **Alternatives considered**: MVC Controllers — rejected for MVP as unnecessary structure for two
  endpoints; can be introduced later if the API surface grows in Extended-MVP/Post-MVP without
  affecting the frontend contract.

## 3. In-memory storage mechanism

- **Decision**: A singleton-scoped `InMemorySubscriptionService` backed by a `List<Subscription>`,
  guarded by a simple lock for thread safety across concurrent HTTP requests.
- **Rationale**: FR-006 requires in-memory-only storage with no persistence across restarts.
  `AppFeatures.md` explicitly allows "List in C#" as the storage mechanism for MVP.
- **Alternatives considered**: `ConcurrentBag`/`ConcurrentQueue` — rejected because order must be
  preserved exactly as added (SC-002), which a `List<T>` with a lock guarantees more simply than
  lock-free concurrent collections whose enumeration order is not contract-guaranteed.

## 4. Port and CORS configuration

- **Decision**: Backend defaults to `http://localhost:5151`, frontend defaults to
  `http://localhost:5213` (and its HTTPS equivalent `https://localhost:7025`), matching
  `TechStack.md`. CORS policy in `Program.cs` allow-lists exactly these frontend origins, read
  from `appsettings.json`, not hardcoded per environment.
- **Rationale**: Directly specified in `StakeholderDocuments/TechStack.md`'s port configuration
  section; Constitution Principle I forbids wildcard CORS origins and mandates configuration-driven
  values.
- **Alternatives considered**: Wildcard CORS (`AllowAnyOrigin`) — explicitly prohibited by the
  constitution.

## 5. Validation of submitted URLs

- **Decision**: Only reject blank/whitespace-only input (FR-005); accept any other non-blank text
  as-is without format or reachability checks (FR-004, FR-007).
- **Rationale**: Explicit MVP simplification in `spec.md` and `AppFeatures.md` ("Accept any URL
  without validation").
- **Alternatives considered**: Regex/URI.TryCreate validation — rejected as explicitly out of scope
  for MVP; would also risk conflicting with FR-004's "MUST NOT validate" requirement.

## 6. Frontend/backend communication contract shape

- **Decision**: JSON over HTTP; `POST /api/subscriptions` with `{ "url": string }` body, and
  `GET /api/subscriptions` returning `{ "url": string }[]` in insertion order.
- **Rationale**: Matches the two required capabilities (FR-001, FR-002) and keeps the contract
  minimal per the Key Entities section of `spec.md` (URL is the only tracked attribute).
- **Alternatives considered**: Returning a wrapped envelope object (e.g., `{ items: [...] }`) —
  rejected as unnecessary indirection for a flat list with no pagination/metadata needs in MVP.

## Output

All Technical Context fields in `plan.md` are resolved; no `NEEDS CLARIFICATION` markers remain.
Proceeding to Phase 1 design.
