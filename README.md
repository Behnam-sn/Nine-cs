# Social Media Platform (DDD + CQRS + Event Sourcing)

A **social media application** with a **C#/.NET** modular-monolith backend and a **React** SPA, designed using **Domain-Driven Design (DDD)**, **Hexagonal Architecture (Ports & Adapters)**, **CQRS**, and **Event Sourcing** (for the social domain).

The goal is a **scalable, maintainable, and evolvable** system that models a real-world social media domain, keeps business logic independent of infrastructure, and can be split into microservices later **without changing how clients authenticate**.

The stack is **open source only** (MIT / Apache 2.0). No commercial identity products.

---

## 🧠 Architectural Overview

This project follows these core architectural principles:

- **Modular monolith**  
  One deployable host today. Each bounded context is a module with its own model and persistence. Other modules depend on **contracts, claims, and integration events** — never on another module’s repositories or user store. Extracting a context later is a hosting change, not a redesign.

- **Domain-Driven Design (DDD)**  
  Clear bounded contexts, aggregates, and domain/integration events

- **Hexagonal Architecture (Ports & Adapters)**  
  Domain and application layers are independent of frameworks. Each context exposes Ports (interfaces) for commands and queries; Adapters provide HTTP, persistence, and brokers.

- **CQRS (Command Query Responsibility Segregation)**  
  Separate write (command) and read (query) models

- **Event Sourcing (social domain)**  
  Profiles, Contents, Interactions, SocialGraphs, Feeds, Notifications, and Moderation persist aggregates as event streams, not state snapshots. **Identities does not.** Authentication, sessions, and credentials are a poor fit for event streams (high-churn lookups, lockout, recovery, OAuth grants).

- **Event-Driven Architecture**  
  Integration events propagate changes across contexts (outbox + broker)

- **OIDC at the edge**  
  Identities is an OAuth 2.1 / OpenID Connect **authorization server**. Every other module is a **resource API** that validates JWTs. The React app talks to a **BFF**, not to tokens in the browser.

---

## 🧩 Bounded Contexts

| Bounded Context   | Write model                                              | Notes                                                                                                                              |
|-------------------|----------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------|
| **Identities**    | ASP.NET Core Identity user (`Account`)                   | Authentication, credentials, account settings. Not event-sourced. Issues tokens; publishes integration events.                     |
| **Profiles**      | `Profile` (event-sourced)                                | Public-facing content space; an account can own multiple profiles. Profile settings (bio, avatar, etc.) are part of the aggregate. |
| **Contents**      | `Post` (event-sourced)                                   | Posts belong to a profile. Visibility rules and media management are handled here.                                                 |
| **Interactions**  | `PostReaction`, `CommentReaction`, `Comment`, `Bookmark` | `PostReaction` targets posts; `CommentReaction` targets comments. High throughput, small aggregates.                               |
| **SocialGraphs**  | `FollowRelationship`, `Block`                            | Follows and blocks happen between profiles.                                                                                        |
| **Feeds**         | *(none)*                                                 | Projection-only; builds timelines and rankings from events.                                                                        |
| **Notifications** | `Notification`                                           | Preferences live on the account (defaults) and `Profile` (per-profile overrides).                                                  |
| **Moderation**    | `Report`, `ModerationCase`, `Appeal`                     | Actions reference profiles; the reporter’s account is recorded for anti-abuse.                                                     |

Each bounded context owns:
- Its write model
- Its domain and integration events
- Its read models

**Extraction rule:** a module other than Identities must not inject Identity `UserManager`, OpenIddict stores, or Identities repositories. It may use claim type names, `AccountId` / `ProfileId` values, and integration events only.

---

## 🔐 Authentication and Authorization

**Account is who you are. Profile is who you act as.** Authentication proves the account. Social writes (posts, follows, reactions) authorize as a profile owned by that account.

```text
React SPA
    │  same-origin, httpOnly cookie
    ▼
BFF  (today: Nine.WebApi; later: gateway)
    │  OIDC Authorization Code + PKCE
    ▼
Identities module
    ASP.NET Core Identity   (users, passwords, lockout, external logins)
    OpenIddict              (authorize, token, JWKS, revocation)
    │  JWT access token (RS256), `sub` = account id
    ▼
Resource APIs  (Profiles, Contents, … — same process today)
```

### Stack (all free)

| Piece | Choice | License |
|-------|--------|---------|
| Users, passwords, Google login | ASP.NET Core Identity | MIT |
| OIDC authorization server | OpenIddict | Apache 2.0 |
| Access tokens | JWT **RS256**, published via JWKS | — |
| React security | **BFF** — ASP.NET cookie session + OpenID Connect handler | MIT |
| Grants, consents, refresh | OpenIddict EF Core stores on PostgreSQL | — |
| Token revocation | OpenIddict (Redis optional later) | — |

No Duende IdentityServer, Duende BFF, Auth0, or other commercial identity products.

### Tokens and actors

- **Access token:** short-lived JWT. `sub` is the Identity user id (`AccountId`). Resource APIs validate locally via the issuer’s JWKS — they do not call Identities per request.
- **Account-scoped token:** settings, credentials, email/phone, logout.
- **Profile-scoped token:** social writes. Identities issues it only after proving the account owns that profile (token exchange or a dedicated OpenIddict grant). `profile_id` is a claim. Command handlers take the actor from the token, never from a client-supplied `profileId` in the body.
- **Refresh / session:** held by the BFF (and OpenIddict stores), not by JavaScript.

### React

- Dev: Vite proxy to the BFF (same origin).
- Prod: the browser’s only origin is the BFF (or later the gateway).
- Login is a redirect to OpenIddict (`/connect/authorize`), not a JSON endpoint that returns tokens.
- No access or refresh token in `localStorage` or the SPA bundle.

### Authorization layers

- **Edge (ASP.NET policies):** authenticated, optional email-verified, `MustHaveActiveProfile` for social writes, `MustBeModerator` for Moderation. Policies read claims only.
- **Domain:** ownership, blocks, visibility — enforced on aggregates. “Can this profile delete this post?” is not a JWT role.

### Extraction to microservices

1. Identities becomes its own host (Identity + OpenIddict + JWKS).
2. BFF/gateway: cookie in, JWT out; `Authority` = Identities URL.
3. Other hosts: `AddJwtBearer` with the same `Authority`.
4. `AccountSuspended` (and similar) already travel on the broker; subscribers revoke sessions and reject that `sub`.

The React app and resource modules keep using OIDC/JWT. No shared HMAC secret, no rewrite of login.

---

## 🗃️ Persistence

**One PostgreSQL instance**, two styles:

- **Identities:** EF Core in an `identities` schema — Identity users/roles/logins and OpenIddict applications, authorizations, tokens.
- **Social contexts:** **Marten** event store, schema per context (`profiles`, `contents`, …).

**Why Marten for the social domain:** stream management, optimistic concurrency (`ExpectedVersion`), async projections, snapshots, and transactional outbox over PostgreSQL.

**Hexagonal alignment:** Marten stays in the Infrastructure/Adapter layer. Social domains talk to `IEventStore<TAggregate>` ports so the store can be swapped later.

**Migration path to EventStoreDB** (social streams only):
1. Replace the Marten adapter with an `EventStore.Client.Grpc` adapter.
2. Map Marten's `stream_id`/`version` → EventStoreDB's `StreamName`/`ExpectedVersion`.
3. Migrate historical events via a background sync worker (idempotent replay to the new stream format).
4. Switch traffic via feature flag. Zero domain logic changes required.

---

## Overall write/read flow

- **Identities write side:** Identity `UserManager` / OpenIddict stores. State is the current row, not a replayed stream. After a significant change (registered, suspended, reactivated), Identities publishes an **integration event** via outbox.
- **Social write side:** each aggregate’s lifecycle is a stream of events in Marten. State is rebuilt by replaying those events. Invariants are checked by loading the aggregate, applying the command, and appending events atomically (optimistic concurrency).
- **Read side:** projections (Marten Async Daemon or external consumers) maintain denormalised models. Eventually consistent.
- **Cross-context:** integration events on a broker (e.g. Kafka/Redpanda). At-least-once delivery via transactional outbox. Resource modules never query Identities tables.

---

## 1. Identities

**Not event-sourced.** The write model is ASP.NET Core Identity.

**User (`Account`):** id (`AccountId`), email, phone, password hash, lockout, email/phone confirmation, notification defaults. Google (and later other providers) are Identity **external logins**, not a parallel credential store.

**Protocol:** OpenIddict in this module — `/connect/authorize`, `/connect/token`, `/connect/revocation`, discovery, JWKS. Register, change password, and verify email are account-management APIs on top of Identity, not custom JWT-minting endpoints.

**Invariants:**
- Email uniqueness (Identity normalized email + unique index).
- Phone uniqueness when a phone is set (unique index; null/absent is allowed).
- An account must remain able to sign in (at least one login: password and/or external).

**Persistence:** PostgreSQL via EF Core, schema `identities`.

**Integration events** (for other contexts, not an account event stream):
- `AccountRegistered`
- `AccountSuspended` / `AccountReactivated`
- `AccountEmailAddressChanged`
- `NotificationPreferencesUpdated`

**Revocation:** password change, logout, and `AccountSuspended` invalidate OpenIddict tokens/sessions. Redis can sit in front later; Postgres is enough at the start.

Other modules consume those events and the JWT. They do not load Identity users.

---

## 2. Profiles

**Aggregate:** `Profile` (event-sourced)  
**Stream:** `profile-{profileId}`

**Core events:**
- `ProfileCreated` (handle, name, linked **account id** = Identity user id)
- `ProfileHandleChanged` (must enforce uniqueness – see below)
- `ProfileUpdated` (bio, avatar, visibility, notification preferences)
- `ProfileDeleted`

**Handle uniqueness:**  
A dedicated projection `unique_handles` (database table with a unique constraint) is maintained by consuming `ProfileCreated` and `ProfileHandleChanged`. To avoid race conditions, a **handle reservation saga** can be used:
1. The command to set a new handle first tries to append `HandleReservationRequested` to a **separate stream** (`handles-{handle}`).
2. On success, the `Profile` aggregate appends `HandleChanged` and the saga appends `HandleReservationConfirmed`.
3. If the profile operation fails or the reservation times out, a `HandleReservationCancelled` event cleans up.

This ensures strict uniqueness while keeping aggregates small.

Ownership of a profile is checked in Identities when issuing a profile-scoped token. Contents and other APIs trust `profile_id` on the token.

**Event store:** PostgreSQL via Marten. Context-level schema separation (`mt_events` in `profiles` schema or partitioned tables).

**Read models / projections:**
- **MongoDB** or **PostgreSQL** for profile details; heavily cached in **Redis** for session/profile page reads.

---

## 3. Contents

**Aggregate:** `Post` (event-sourced)  
**Stream:** `post-{postId}`

**Core events:**
- `PostCreated` (profileId, body, media refs, visibility rules)
- `PostEdited` (new body/media)
- `PostVisibilityChanged` (e.g., from public to followers)
- `PostDeleted`

**Invariants:**
- Cannot edit a deleted post (enforced by current state loaded from events).
- The authoring actor is the `profile_id` claim, not a body field.

**Event store:** PostgreSQL via Marten. Snapshots configured via Marten's `SnapshotLifecycle` for long-lived posts with frequent edits.

**Read models / projections:**
- **Cassandra** or **MongoDB** for fast post retrieval by ID or profile.
- **Elasticsearch** for full-text search on post bodies (projection updates from events).

---

## 4. Interactions

All interaction aggregates are event-sourced, stream per composite ID. The profile in each stream id is the **active profile** from the access token.

### PostReaction
**Stream:** `postreaction-{postId}-{profileId}`  
**Events:** `PostReacted`, `PostReactionRemoved`  
**Invariant:** A reaction can only be added if none exists, and removed only if one exists. The composite stream ID guarantees one stream per profile‑post pair.

### CommentReaction
**Stream:** `commentreaction-{commentId}-{profileId}`  
**Events:** `CommentReacted`, `CommentReactionRemoved`  
**Invariant:** Same pattern as `PostReaction`.

### Comment
**Stream:** `comment-{commentId}`  
**Events:** `CommentAdded` (postId, authorProfileId, body, optional parentCommentId), `CommentDeleted`.  
**Invariant:** A comment can be deleted only by its author or a moderator (via appropriate event). No editing or threading within the aggregate keeps it small; replies are separate comments with a parent reference.

### Bookmark
**Stream:** `bookmark-{profileId}-{postId}`  
**Events:** `PostBookmarked`, `PostUnbookmarked`.  
**Invariant:** A post can be bookmarked only once per profile.

**Event store for interactions:**
PostgreSQL via Marten. Aggregates are tiny (2–3 events max), so snapshotting is disabled. Marten's `AppendEventsAsync` with `ExpectedVersion.Any` provides low-latency writes. Streams are lightweight and partitioned by context/month if volume exceeds thresholds.

**Read models / projections:**
- **Cassandra** tables for `reactions_by_post`, `reactions_by_user`, `comments_by_post`, `bookmarks_by_user`.
- Counts of reactions/comments are maintained in a separate fast projection (**Redis** counters) for display.

---

## 5. SocialGraphs

### FollowRelationship
**Stream:** `follow-{followerProfileId}-{followedProfileId}`  
**Events:** `Followed`, `Unfollowed`.  
**Invariant:** Cannot follow if already following or if blocked. The aggregate loads both the follow stream and the block stream (or checks a read model of blocks) before appending. Self‑follow is rejected.

### Block
**Stream:** `block-{blockerProfileId}-{blockedProfileId}`  
**Events:** `Blocked`, `Unblocked`.  
**Side effect:** When `Blocked` is appended, a saga/policy emits a command to unfollow if a follow relationship exists (via event‑driven process). The follow aggregate will then append `Unfollowed`.

**Event store:** PostgreSQL via Marten. Streams are short-lived or static. Optimistic concurrency prevents duplicate follow/block operations.

**Read models / projections:**
- **Cassandra** tables `followers_by_followed`, `following_by_follower`, `blocks_by_blocker` for instant graph queries.

---

## 6. Feeds (Read Side Only)

No aggregates; purely a set of projections consuming events from **Contents**, **Interactions**, and **SocialGraphs**.

- **Projection logic:**
    - On `PostCreated`: fan‑out to followers’ feed projections.
    - On `Followed`: backfill recent posts from that profile.
    - On `Blocked`: remove items from the blocker’s feed.
    - For **profiles with very large follower counts** (celebrities), a hybrid approach is used: fan‑out on write for normal profiles, but switch to fan‑out on read for celebrities — the feed is assembled at query time by merging recent posts from followed profiles, with short‑term caching to maintain performance.

- **Storage for feed read models:**
    - **Redis** sorted sets (`feed:{profileId}`) for quick timeline loading, idempotently updated by event handlers.
    - **Cassandra** for durable, long‑term timeline storage if needed.

All handling is idempotent (using event IDs as deduplication keys). Marten's `Async Daemon` or external Kafka consumers drive these projections.

---

## 7. Notifications (Read Side & Aggregate Mix)

**Aggregate:** `Notification` (event-sourced)  
**Stream:** `notification-{notificationId}`  
**Events:** `NotificationCreated`, `NotificationMarkedAsRead`, `NotificationDeleted`.

- The service creates notifications by consuming events from other contexts (e.g., `PostReacted` → `NotificationCreated`).
- The aggregate exists to manage the notification’s lifecycle (read/unread).

**Event store:** PostgreSQL via Marten. Lightweight streams; Marten's outbox pattern ensures notification creation and event publishing stay transactionally consistent.

**Read models / projections:**
- **MongoDB** collection `notifications_current` rebuilt from events for user inbox queries (paginated, sorted).
- **Notification preferences** are read from Identities (account defaults) and Profile projections (cached). Because this is eventually consistent, a brief window exists after a preference change where a notification may still be sent — an acceptable trade‑off in social applications.

---

## 8. Moderation

**Aggregates:** `Report`, `ModerationCase`, `Appeal` (all event-sourced)

**Streams:**
- `report-{reportId}` → `ReportFiled`, `ReportResolved` (optionally `ReportDismissed` if a report is closed without creating a case)
- `case-{caseId}` → `CaseOpened`, `ActionAdded`, `CaseResolved`
- `appeal-{appealId}` → `AppealFiled`, `AppealResolved`

**Invariants:**
- A case can only be resolved if open. An appeal can only be filed on a resolved case. A report that is deemed invalid can be dismissed directly (`ReportDismissed`) without escalating.

**Event store:** PostgreSQL via Marten. Audit-trail friendly; Marten's metadata support captures `CorrelationId`, `CausationId`, and `UserId` automatically for compliance.

**Read models:** Relational DB for moderator dashboards (list of open cases, reports by status).

Moderator capability is a coarse JWT claim / policy at the edge; case workflow stays in these aggregates.

---

## Key Practices

**Identities**
1. Identity is the source of truth for the user; OpenIddict is the protocol.
2. Other modules authenticate via JWT + JWKS, never via Identities services.
3. Lifecycle changes that other contexts must see are integration events, not queries into the Identity database.

**Social event sourcing**
1. **Stream per aggregate instance** – concurrency control via Marten's `ExpectedVersion`; no conflicts across aggregates.
2. **All social state is ephemeral** – the event store is the source of truth; read models can be rebuilt from scratch.
3. **Snapshots** – configured selectively via Marten's `SnapshotLifecycle` (e.g., every 50–100 events for `Post`).
4. **Event upcasting** – handled via Marten's `IUpcaster` interface to transform legacy event payloads during stream replay.
5. **Cross-context events** – transactional outbox + Kafka/Redpanda for at-least-once distribution.
6. **Idempotent projections** – event handlers track `EventId` or use Marten's projection tracking to avoid duplicates.
7. **Adapter abstraction** – `IEventStore<T>` isolates social domain logic. Swapping Marten for EventStoreDB requires only infrastructure-layer changes.

---

> **Note:** Social domains are storage-agnostic at the domain layer. PostgreSQL + Marten is chosen for operational simplicity, ACID guarantees, and development velocity, with a path to EventStoreDB if streams outgrow relational capabilities. Identities is intentionally **not** event-sourced: it is an OIDC server on ASP.NET Core Identity so a React client and future microservices share one standard authentication path with no commercial licenses.
