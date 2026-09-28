# Data Model: MVP RSS Reader - Subscription Management

**Phase**: MVP | **Source**: `spec.md` Key Entities section

## Subscription

Represents a single feed the user wants to follow. Per `spec.md`, the URL text is the only
tracked attribute in this phase — no title, status, ID, or fetch results are modeled.

| Field | Type     | Required | Notes |
|-------|----------|----------|-------|
| Url   | `string` | Yes      | Raw text as entered by the user. Not validated as a well-formed URI or reachable feed (FR-004, FR-007). Must be non-blank to be added (FR-005). |

### C# representation

```csharp
public record Subscription(string Url);
```

### Validation rules

- **Non-blank**: `Url` MUST NOT be null, empty, or whitespace-only. Blank submissions are
  rejected without adding an entry (FR-005) — this is the *only* validation performed (FR-004).
- **No de-duplication**: The same URL text MAY appear more than once in the list; each submission
  is added as a separate entry (per Edge Cases in `spec.md`).
- **No format/reachability checks**: No URI parsing, scheme checks, or network calls are performed
  against the value (FR-004, FR-007).

### Relationships

None. `Subscription` is a standalone, flat entity with no references to other entities in this
phase (no user/session entity is modeled, per FR-008 single-local-user assumption).

### State / lifecycle

- **Created**: When a user submits a non-blank URL (User Story 1).
- **Read**: Displayed in the subscription list in the order added (User Story 2, SC-002).
- **Updated / Deleted**: Not supported in this phase (FR-009) — no transitions beyond creation.
- **Persistence**: Held only in an in-memory collection for the lifetime of the running backend
  process; lost on restart or refresh (FR-006).

## Storage shape (backend)

A single collection, ordered by insertion, is sufficient to satisfy SC-002 (list reflects 100% of
submitted non-blank URLs in submission order):

```csharp
// Singleton service; List<T> access guarded by a lock for thread-safe concurrent requests.
private readonly List<Subscription> _subscriptions = new();
```

No additional indexes, keys, or identifiers are introduced, consistent with Constitution
Principle V (Simplicity & YAGNI) and the Key Entities note that "no other metadata is tracked in
this phase."
