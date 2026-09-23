<!--
Sync Impact Report
==================
Version change: (none) → 1.0.0 (initial ratification)
Modified principles: N/A (initial creation)
Added sections:
  - Core Principles: I. Security-First Development (NON-NEGOTIABLE), II. Maintainability
    Through Phased Scope Discipline, III. Code Quality & Consistency, IV. Test-First for
    Networked & Parsing Logic (NON-NEGOTIABLE from Extended-MVP onward), V. Simplicity & YAGNI
  - Security & Technology Constraints
  - Development Workflow & Quality Gates
  - Governance
Removed sections: none (placeholders only)
Templates requiring updates:
  - .specify/templates/plan-template.md: ⚠ pending manual review (verify Constitution Check
    gate references these principle names)
  - .specify/templates/spec-template.md: ✅ no principle-specific references found
  - .specify/templates/tasks-template.md: ✅ no principle-specific references found
Follow-up TODOs: none
-->

# RSSFeedReader Constitution

## Core Principles

### I. Security-First Development (NON-NEGOTIABLE)
All external input (feed URLs, HTTP responses, syndication payloads, rendered content) MUST be
treated as untrusted. The MVP's explicit exception of skipping feed-URL validation MUST NOT be
extended to any other trust boundary. The backend MUST enforce an explicit CORS origin allowlist
(no wildcard `*` origins) sourced from configuration, not hardcoded per environment. Any HTML or
rich content rendered from feed sources (post-MVP) MUST be sanitized (e.g., via an HTML sanitizer)
before rendering to prevent XSS. No secrets, connection strings, or API keys MAY be committed to
source control; environment-specific values MUST use configuration files, user-secrets, or
environment variables.
Rationale: This app consumes untrusted external content by design (arbitrary feed URLs and XML
payloads). Security must be designed in from the MVP forward so later phases do not retrofit it
as an afterthought.

### II. Maintainability Through Phased Scope Discipline
Implementation MUST match the current phase (MVP, Extended-MVP, or Post-MVP) as defined in
`StakeholderDocuments/ProjectGoals.md` and `StakeholderDocuments/AppFeatures.md`. Features from a
later phase MUST NOT be implemented early, and MVP simplifications (in-memory storage, no URL
validation, no persistence) MUST NOT be silently expanded without an explicit, documented phase
transition. Code MUST separate backend and frontend responsibilities so later phases (persistence,
background polling, advanced features) can be added incrementally without rewriting existing MVP
code. Any deviation from the phased plan MUST be justified in the relevant spec or plan document.
Rationale: The stakeholder documents define an intentional MVP → Extended-MVP → Post-MVP
progression; skipping ahead increases complexity risk and undermines the stated goal of a fast,
minimal MVP.

### III. Code Quality & Consistency
Code MUST follow standard .NET/C# conventions (naming, nullable reference types, `async`/`await`
for I/O) and idiomatic Blazor component patterns. Environment-specific values (API base URL,
ports, CORS origins) MUST be read from configuration, never hardcoded, consistent with
`StakeholderDocuments/TechStack.md`. Default template/demo scaffold code (e.g., Blazor's
`Home.razor`, `Counter.razor`, `Weather.razor`) MUST be removed before feature work begins, and
routing MUST be verified conflict-free before implementation continues.
Rationale: Consistency reduces onboarding friction and prevents the runtime routing and
configuration bugs already identified as recurring pitfalls in this stack.

### IV. Test-First for Networked & Parsing Logic (NON-NEGOTIABLE from Extended-MVP onward)
Once feed fetching, parsing, or any network/error-handling logic is introduced (Extended-MVP and
later), automated tests (xUnit) MUST be written for that logic before or alongside
implementation. The pure in-memory MVP subscription-list logic (no network, no parsing) is exempt.
Known-good test feeds (e.g., the .NET blog RSS feed referenced in the stakeholder docs) MUST be
used to validate parsing behavior before merging feed-fetching features.
Rationale: Network and parsing code carries the highest risk of runtime failures and security
issues; testing is deferred only where the stakeholder docs explicitly deem it unnecessary
(pure in-memory MVP with no network calls).

### V. Simplicity & YAGNI
Persistence, background services, authentication, or multi-user support MUST NOT be introduced
until the corresponding phase (Post-MVP) is explicitly started; each phase's stated MUST/MAY/MUST
NOT constraints in `AppFeatures.md` are binding. The simplest solution that satisfies the current
phase's explicit requirements MUST be preferred over speculative generalization.
Rationale: This project is explicitly a fast-moving proof-of-concept; premature complexity
contradicts the stated delivery approach and risks effort spent on features that may never be
needed.

## Security & Technology Constraints

The backend MUST be an ASP.NET Core Web API and the frontend MUST be Blazor WebAssembly, per
`TechStack.md`. CORS MUST use an explicit origin allowlist matching the frontend's configured
ports; wildcard origins are prohibited. Dependencies MUST be added only when required by the
current phase (e.g., `System.ServiceModel.Syndication` and an `HttpClient` registration are
introduced only at Extended-MVP, not earlier). Any future HTML rendering of feed content MUST go
through a sanitization library before being shown to users. No user-supplied data may be
interpolated into logs, headers, or responses without appropriate encoding.

## Development Workflow & Quality Gates

Before starting feature implementation for a phase, the Blazor template cleanup checklist in
`TechStack.md` (removal of demo pages, single root route, clean build) MUST be verified complete.
Before testing any phase, the port/CORS/configuration consistency checklist in `TechStack.md`
(backend port, frontend port, `ApiBaseUrl`, CORS origins) MUST be verified. Every spec, plan, or
task produced by Spec Kit commands MUST state which phase (MVP, Extended-MVP, or Post-MVP) it
targets, and MUST NOT introduce work belonging to a later phase without explicit justification.

## Governance

This constitution supersedes ad hoc practices for this project. Amendments MUST update this file
and its Sync Impact Report, and MUST bump the version according to semantic versioning: MAJOR for
backward-incompatible principle removals or redefinitions, MINOR for new principles or materially
expanded guidance, PATCH for clarifications and wording fixes. All specs, plans, and tasks
generated by Spec Kit commands MUST verify compliance with these principles before implementation
proceeds; any complexity that goes beyond the current phase MUST be explicitly justified in the
relevant plan.

**Version**: 1.0.0 | **Ratified**: 2026-09-23 | **Last Amended**: 2026-09-23
