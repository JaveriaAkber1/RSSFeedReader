---

description: "Task list for MVP RSS Reader - Subscription Management"
---

# Tasks: MVP RSS Reader - Subscription Management

**Input**: Design documents from `/specs/001-rss-subscription-mvp/`

**Prerequisites**: plan.md, spec.md, data-model.md, contracts/subscriptions-api.md, research.md, quickstart.md

**Tests**: Not requested for this phase. Constitution Principle IV explicitly exempts the pure
in-memory, non-networked MVP subscription-list logic from mandatory automated tests; xUnit tests
are introduced starting Extended-MVP when feed fetching/parsing is added. No test tasks are
included below.

**Organization**: Tasks are grouped by user story (from spec.md: US1 = Add a feed subscription by
URL [P1], US2 = View the current subscription list [P2]) to enable independent implementation and
testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2)
- Paths follow the Web application structure from plan.md: `backend/RSSFeedReader.Api/`,
  `frontend/RSSFeedReader.UI/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure per plan.md's Project Structure section

- [ ] T001 Create `backend/RSSFeedReader.Api` (ASP.NET Core Web API) and `frontend/RSSFeedReader.UI`
      (Blazor WebAssembly) projects per plan.md's Project Structure, and add both to a solution
      file at the repository root
- [ ] T002 [P] Configure backend port in `backend/RSSFeedReader.Api/Properties/launchSettings.json`
      to `http://localhost:5151` per research.md decision #4
- [ ] T003 [P] Configure frontend ports in
      `frontend/RSSFeedReader.UI/Properties/launchSettings.json` to `http://localhost:5213` /
      `https://localhost:7025` per research.md decision #4
- [ ] T004 [P] Configure `ApiBaseUrl` in `frontend/RSSFeedReader.UI/wwwroot/appsettings.json` to
      point at the backend's configured port (`http://localhost:5151`)
- [ ] T005 Remove Blazor template demo pages (`Home.razor`, `Counter.razor`, `Weather.razor`) from
      `frontend/RSSFeedReader.UI/Pages/` per Constitution Principle III (default scaffold code MUST
      be removed before feature work begins)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before either user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T006 [P] Create `Subscription` model in `backend/RSSFeedReader.Api/Models/Subscription.cs` as
      `public record Subscription(string Url);` per data-model.md
- [ ] T007 Create `ISubscriptionService` interface in
      `backend/RSSFeedReader.Api/Services/ISubscriptionService.cs` with methods to add a
      subscription by URL and to return all subscriptions in insertion order (depends on T006)
- [ ] T008 Implement `InMemorySubscriptionService` in
      `backend/RSSFeedReader.Api/Services/InMemorySubscriptionService.cs` backed by a singleton
      `List<Subscription>` guarded by a lock for thread safety; the add operation MUST reject a
      `Url` that "MUST NOT be null, empty, or whitespace-only" (data-model.md validation rules,
      FR-005) without adding an entry, and MUST NOT de-duplicate — "The same URL text MAY appear
      more than once in the list; each submission is added as a separate entry" (depends on T007)
- [ ] T009 Register `InMemorySubscriptionService` as a singleton and configure a CORS policy in
      `backend/RSSFeedReader.Api/Program.cs` that allow-lists exactly the frontend origins
      (`http://localhost:5213`, `https://localhost:7025`) read from `appsettings.json` — no
      wildcard (`*`) origins, per Constitution Principle I and contracts/subscriptions-api.md's CORS
      section (depends on T008)
- [ ] T010 [P] Create `SubscriptionApiClient` in
      `frontend/RSSFeedReader.UI/Services/SubscriptionApiClient.cs` wrapping `HttpClient`, and
      register it with its base address set to `ApiBaseUrl` in
      `frontend/RSSFeedReader.UI/Program.cs`
- [ ] T011 Update `frontend/RSSFeedReader.UI/Layout/NavMenu.razor` to remove demo links and verify
      exactly one page uses `@page "/"` (Constitution Principle III routing check)

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Add a feed subscription by URL (Priority: P1) 🎯 MVP

**Goal**: A user pastes a feed URL into an input field, submits it, and the URL is added to the
subscription list without any validation or feed fetching (FR-001, FR-004, FR-005, FR-007).

**Independent Test**: Open the app, enter any text into the URL field, submit it, and confirm the
value now appears in the subscription list.

### Implementation for User Story 1

- [ ] T012 [P] [US1] Add `POST /api/subscriptions` minimal API endpoint in
      `backend/RSSFeedReader.Api/Program.cs` that reads `{ "url": "string" }`, calls
      `InMemorySubscriptionService`, and returns `201 Created` with `{ "url": "string" }` when
      `url` is non-blank, or `400 Bad Request` with `{ "error": "url must not be blank" }` when
      `url` is "missing, empty, or whitespace-only (FR-005)" per contracts/subscriptions-api.md
      (depends on T009)
- [ ] T013 [US1] Add an `AddSubscriptionAsync(string url)` method to
      `frontend/RSSFeedReader.UI/Services/SubscriptionApiClient.cs` that POSTs `{ "url": url }` to
      `/api/subscriptions` and returns the created subscription on `201`, or indicates failure on
      `400` (depends on T010, T012)
- [ ] T014 [US1] Create `Subscriptions.razor` page in
      `frontend/RSSFeedReader.UI/Pages/Subscriptions.razor` with `@page "/"`, a URL text input, and
      an add control that calls `AddSubscriptionAsync`; on success, append the returned
      subscription to the displayed list immediately with no page reload ("update the displayed
      subscription list immediately after a subscription is successfully added, without requiring
      a page reload", FR-003) and clear the input; blank/whitespace-only input MUST NOT be
      submitted client-side (FR-005) so that adding a subscription requires no more than two user
      actions — enter text, then submit (SC-001, SC-003, SC-004) (depends on T005, T011, T013)

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently —
adding one or more subscriptions (including duplicates and non-URL text) shows them in the list
with no page reload.

---

## Phase 4: User Story 2 - View the current subscription list (Priority: P2)

**Goal**: A user viewing the app can see all feed subscriptions added during the current session,
including a clear empty state when none have been added (FR-002).

**Independent Test**: Add zero, one, and multiple subscriptions and confirm the displayed list
always matches what has been added so far in the session.

### Implementation for User Story 2

- [ ] T015 [P] [US2] Add `GET /api/subscriptions` minimal API endpoint in
      `backend/RSSFeedReader.Api/Program.cs` that returns `200 OK` with
      `[{ "url": "string" }, ...]` — "Returns an empty array (`[]`) when no subscriptions have been
      added yet" — always in insertion order (SC-002), per contracts/subscriptions-api.md (depends
      on T009)
- [ ] T016 [US2] Add a `GetSubscriptionsAsync()` method to
      `frontend/RSSFeedReader.UI/Services/SubscriptionApiClient.cs` that GETs
      `/api/subscriptions` and returns the list of subscriptions (depends on T010, T015)
- [ ] T017 [US2] In `Subscriptions.razor`'s `OnInitializedAsync`, call `GetSubscriptionsAsync` to
      populate the list on load: render "a clear empty list state rather than an error" when the
      result is empty, and render every returned URL "in the order they were added" when populated
      (FR-002, SC-002) (depends on T014, T016)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently — a fresh app
load shows the empty state, and previously-added-this-session subscriptions display in insertion
order.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final verification across both user stories

- [ ] T018 [P] Run all validation scenarios in quickstart.md end-to-end (empty state, add first,
      add second without losing first, accept unvalidated text, reject blank, allow duplicates,
      in-memory-only across restart) against the running backend and frontend
- [ ] T019 [P] Verify the port/CORS/configuration consistency checklist from `TechStack.md`
      (backend port, frontend port, `ApiBaseUrl`, CORS allowed origins all agree) per Constitution's
      Development Workflow & Quality Gates section

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS both user stories
- **User Story 1 (Phase 3)**: Depends on Foundational phase completion - no dependency on User
  Story 2
- **User Story 2 (Phase 4)**: Depends on Foundational phase completion; T017 builds on the same
  `Subscriptions.razor` file as T014 (US1) but is independently testable (viewing zero/one/many
  subscriptions) once US1's page exists
- **Polish (Phase 5)**: Depends on both user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - no dependency on User Story 2
- **User Story 2 (P2)**: Can start its backend endpoint (T015) in parallel with US1; its frontend
  task (T017) touches `Subscriptions.razor` after T014 (US1) creates that file, but the story
  remains independently testable (empty/populated list rendering) per its own acceptance scenarios

### Within Each User Story

- Backend endpoint before frontend API client method before page wiring
- Story complete before moving to the next priority (or in parallel, per team capacity)

### Parallel Opportunities

- T002, T003, T004 (Setup) can run in parallel
- T006 (Foundational model) and T010 (frontend API client scaffold) can run in parallel
- T012 (US1 backend endpoint) and T015 (US2 backend endpoint) can run in parallel once Foundational
  is complete, since they are independent minimal-API route registrations
- T018 and T019 (Polish) can run in parallel

---

## Parallel Example: Foundational Phase

```bash
Task: "Create Subscription model in backend/RSSFeedReader.Api/Models/Subscription.cs"
Task: "Create SubscriptionApiClient in frontend/RSSFeedReader.UI/Services/SubscriptionApiClient.cs"
```

## Parallel Example: User Stories 1 & 2 Backend Endpoints

```bash
Task: "Add POST /api/subscriptions minimal API endpoint in backend/RSSFeedReader.Api/Program.cs"
Task: "Add GET /api/subscriptions minimal API endpoint in backend/RSSFeedReader.Api/Program.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks both stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently (add subscriptions, confirm they appear)
   — this alone satisfies the "single core capability the MVP exists to demonstrate"

### Incremental Delivery

1. Setup + Foundational → backend and frontend scaffolding compiles and runs
2. Add User Story 1 → test independently → MVP demonstrable (add-only, list visible from local
   append)
3. Add User Story 2 → test independently → list is now backed by `GET /api/subscriptions` on load,
   covering the empty-state and multi-entry viewing scenarios
4. Polish phase → run full quickstart.md validation and configuration checklist
