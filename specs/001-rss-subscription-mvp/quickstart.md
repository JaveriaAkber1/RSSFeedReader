# Quickstart: MVP RSS Reader - Subscription Management

**Phase**: MVP | **Feature**: `001-rss-subscription-mvp`

This guide validates the feature end-to-end once implemented. It assumes the projects described in
`plan.md`'s Project Structure section (`backend/RSSFeedReader.Api`, `frontend/RSSFeedReader.UI`)
exist. See `data-model.md` for the `Subscription` shape and `contracts/subscriptions-api.md` for
the API contract.

## Prerequisites

- .NET SDK installed (see `research.md` decision #1 for target version); verify with:

  ```powershell
  dotnet --version
  ```

- Repository cloned locally; no external services, database, or network access required (MVP has
  no persistence and no feed fetching).

## Pre-test checklist (per `StakeholderDocuments/TechStack.md`)

Before running, confirm:

- [ ] Blazor template demo pages removed (`Home.razor`, `Counter.razor`, `Weather.razor` gone from
      `frontend/RSSFeedReader.UI/Pages/`)
- [ ] Only one page uses `@page "/"`
- [ ] Backend `launchSettings.json` port, frontend `wwwroot/appsettings.json` `ApiBaseUrl`, and
      backend CORS allowed origins all agree (default: backend `http://localhost:5151`, frontend
      `http://localhost:5213` / `https://localhost:7025`)

## Run the backend

```powershell
dotnet run --project backend/RSSFeedReader.Api
```

Expected: process starts with no errors and listens on the configured port
(`http://localhost:5151` by default).

## Run the frontend

In a second terminal:

```powershell
dotnet run --project frontend/RSSFeedReader.UI
```

Expected: Blazor WebAssembly app builds and serves with no errors; navigating to the frontend URL
in a browser loads the subscriptions page with no console errors in DevTools (F12).

## Validation scenarios

Run these against the running app to confirm the feature works end-to-end (maps to `spec.md`
Acceptance Scenarios and Success Criteria):

1. **Empty state** (User Story 2, Acceptance Scenario 1)
   - Load the app for the first time.
   - **Expected**: subscription list area is visible and empty — no error is shown.

2. **Add first subscription** (User Story 1, Acceptance Scenario 1; SC-001, SC-003, SC-004)
   - Enter any non-blank text (e.g., `https://devblogs.microsoft.com/dotnet/feed/`) into the URL
     field and submit.
   - **Expected**: the URL appears in the list immediately, with no page reload, using only the
     input field and one submit action.

3. **Add additional subscriptions without losing existing ones** (User Story 1, Acceptance
   Scenario 2; SC-002)
   - Add a second, different URL.
   - **Expected**: both URLs are visible in the list, in the order they were added; the first entry
     is not removed or reordered.

4. **Accept unvalidated text** (User Story 1, Acceptance Scenario 3; FR-004, FR-007)
   - Submit clearly non-URL text (e.g., `not a real feed`).
   - **Expected**: the text is accepted and added to the list — no validation error, no attempt to
     fetch/parse it.

5. **Reject blank submission** (Edge Cases; FR-005)
   - Submit an empty or whitespace-only value.
   - **Expected**: no new entry is added to the list.

6. **Allow duplicate URLs** (Edge Cases)
   - Submit the same URL twice.
   - **Expected**: two separate entries appear in the list (no de-duplication).

7. **In-memory-only storage** (Edge Cases; FR-006)
   - After adding one or more subscriptions, refresh the browser tab or restart the backend
     process.
   - **Expected**: the subscription list is empty again — nothing persisted across restarts.

## Out of scope for this quickstart

- Feed fetching, parsing, or item display — Extended-MVP (see `AppFeatures.md`).
- Removing or editing subscriptions — deferred (FR-009).
- Automated test execution — not required for this phase per Constitution Principle IV; these
  scenarios are manual/exploratory validation only.
