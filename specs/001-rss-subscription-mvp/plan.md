# Implementation Plan: MVP RSS Reader - Subscription Management

**Branch**: `001-rss-subscription-mvp` | **Date**: 2026-09-28 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-rss-subscription-mvp/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Deliver the MVP subscription-management slice of the RSS reader: a user pastes a feed URL into an
input field and submits it, the URL is appended to an in-memory subscription list with no
validation or feed fetching, and the UI displays the current list immediately after each addition.
Per `StakeholderDocuments/TechStack.md`, the backend is an ASP.NET Core Web API exposing endpoints
to add and list subscriptions from an in-memory store, and the frontend is a Blazor WebAssembly app
with a single page (input + add control + list) that calls the API via `HttpClient`. No persistence,
authentication, feed parsing, or subscription removal is in scope for this phase (FR-006, FR-007,
FR-008, FR-009).

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# 12 / .NET 10 (latest LTS; see research.md for rationale)

**Primary Dependencies**: ASP.NET Core Web API (backend), Blazor WebAssembly + `HttpClient` (frontend). No `System.ServiceModel.Syndication`, EF Core, or other feed/persistence packages — those are Extended-MVP/Post-MVP per `StakeholderDocuments/TechStack.md`.

**Storage**: In-memory only (singleton-scoped `List<Subscription>` behind a backend service); no database, no file persistence (FR-006).

**Testing**: N/A for this phase. Constitution Principle IV exempts pure in-memory, non-networked MVP logic from mandatory automated tests; xUnit is introduced starting Extended-MVP when feed fetching/parsing is added.

**Target Platform**: Local developer machine (Windows/macOS/Linux), two localhost web processes (API + WASM app) per `StakeholderDocuments/ProjectGoals.md`.

**Project Type**: Web application (frontend + backend, two projects)

**Performance Goals**: Not a driver for this phase — single local user, no load requirements beyond the list rendering after each add (SC-001).

**Constraints**: Add-then-see-it-in-the-list must happen with no page reload and no more than two user actions (SC-001, SC-003); backend CORS must explicitly allowlist the frontend origin (Constitution Principle I).

**Scale/Scope**: Single local user, single session, 2 user stories (add subscription, view list), no upper bound on list size specified.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status |
|---|---|---|
| I. Security-First Development | Backend enforces explicit CORS origin allowlist (no wildcard) for the Blazor origin; the MVP's URL-validation exception is not extended to CORS or any other trust boundary. No HTML rendering of feed content occurs in this phase (nothing fetched), so sanitization is N/A. No secrets are needed. | **PASS** |
| II. Maintainability Through Phased Scope Discipline | Scope is limited to add + list subscriptions, in-memory, exactly as `spec.md` and `ProjectGoals.md`/`AppFeatures.md` MVP section define. No feed fetching, persistence, removal/edit, or auth is introduced. Backend/frontend responsibilities are separated (API vs. Blazor UI) so Extended-MVP can add feed fetching without rewriting this slice. | **PASS** |
| III. Code Quality & Consistency | Ports, API base URL, and CORS origins are read from configuration (`launchSettings.json`, `wwwroot/appsettings.json`), never hardcoded. Blazor template demo pages (`Home.razor`, `Counter.razor`, `Weather.razor`) MUST be removed and routing verified during Phase 2 foundational work before feature pages are added. | **PASS** |
| IV. Test-First for Networked & Parsing Logic | Not triggered: this phase has no network calls (backend↔external feed) or parsing logic — only the frontend↔backend subscription CRUD-lite calls, and the in-memory list logic itself has no network/parsing. Exempt per the principle's explicit carve-out. | **N/A (exempt)** |
| V. Simplicity & YAGNI | No persistence, background services, auth, or multi-user support is introduced. `List<Subscription>` in a singleton service is the simplest solution satisfying FR-001–FR-009. | **PASS** |

No violations requiring justification; Complexity Tracking table is left empty.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
backend/
└── RSSFeedReader.Api/
    ├── Program.cs                     # Minimal API endpoints, CORS policy, DI registration
    ├── Models/
    │   └── Subscription.cs            # record Subscription(string Url)
    ├── Services/
    │   ├── ISubscriptionService.cs
    │   └── InMemorySubscriptionService.cs   # singleton, backing List<Subscription>
    ├── Properties/
    │   └── launchSettings.json        # backend port (default http://localhost:5151)
    └── appsettings.json                # CORS allowed origins (frontend port)

frontend/
└── RSSFeedReader.UI/
    ├── Pages/
    │   └── Subscriptions.razor        # @page "/" — input, add control, subscription list
    ├── Layout/
    │   └── NavMenu.razor              # updated to remove demo links, point to Subscriptions
    ├── Services/
    │   └── SubscriptionApiClient.cs   # wraps HttpClient calls to the backend API
    ├── Properties/
    │   └── launchSettings.json        # frontend port (default http://localhost:5213)
    └── wwwroot/
        └── appsettings.json           # ApiBaseUrl pointing at backend port
```

**Structure Decision**: Web application structure (Option 2) — a `backend/RSSFeedReader.Api`
ASP.NET Core Web API project and a `frontend/RSSFeedReader.UI` Blazor WebAssembly project, per
`StakeholderDocuments/TechStack.md`. No `tests/` directories are created in this phase since
Constitution Principle IV exempts the pure in-memory MVP logic from mandatory automated tests;
test projects are introduced starting Extended-MVP.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
