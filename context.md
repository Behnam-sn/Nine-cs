# Nine — Architecture Context

This is the source of truth for how Nine is designed. `README.md` is the short introduction; this file is the detail.

Nine is a social platform: a **C#/.NET** modular-monolith backend and a **React** SPA. It is a hobby project and uses **open-source licenses only** (MIT / Apache 2.0). No commercial identity products (no Duende, Auth0, Cognito, etc.).

The long-run goal is a system that can split into microservices **without changing how clients authenticate** and without other modules depending on Identities’ internals.

---

## Principles

- **Modular monolith.** One host today (`Nine.WebApi`). Each bounded context is a module with its own model and persistence. Extracting a context later is a hosting change, not a redesign.
- **Domain-Driven Design.** Bounded contexts, aggregates, domain events, integration events.
- **Hexagonal architecture (ports & adapters).** Domain and application layers do not depend on frameworks. Adapters provide HTTP, persistence, and brokers.
- **CQRS.** Separate write (command) and read (query) models.
- **Event sourcing for the social domain only.** Profiles, Contents, Interactions, SocialGraphs, Feeds, Notifications, and Moderation persist as event streams. **Identities does not.**
- **Event-driven integration.** Cross-context communication is integration events (outbox + broker), not database queries into another module.
- **OIDC at the edge.** Identities is an OAuth 2.1 / OpenID Connect authorization server. Every other module is a resource API that validates JWTs. React talks to a BFF, not to tokens in the browser.

### Extraction rule

A module other than Identities must **not** inject Identity `UserManager`, OpenIddict stores, or Identities repositories. It may use:

- claim type names
- `AccountId` / `ProfileId` values
- integration events

If a module today calls Identities services, it will not extract cleanly.

---

## Actors: Account vs Profile

**Account is who you are. Profile is who you act as.**

| | Account | Profile |
|---|---|---|
| Question | Who signed in? | Which public face is acting? |
| Cardinality | One per login | Many per account |
| Examples | Password, email, suspension, notification defaults | Handle, posts, follows, reactions |
| Token | `sub` = account id | `profile_id` claim, only if this account owns it |

Authentication proves the account. Social writes (posts, follows, reactions) authorize as a **profile owned by that account**. Command handlers take the actor from the token, never from a client-supplied `profileId` in the JSON body.

---

## Authentication and authorization

### Target flow

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

**Rejected (and why):**

- Homemade JWT + shared HMAC — fails as soon as a second process must validate tokens.
- ASP.NET Identity as the *only* HTTP auth story (cookies, no OIDC) — React + future services need a protocol, not a membership cookie.
- Duende IdentityServer / Duende BFF — not free if the hobby ever becomes a product.
- Auth0 / Keycloak as the user store — Identities would stop being a bounded context we own; Keycloak is extra Java ops for a .NET hobby monolith.

### Tokens

- **Access token:** short-lived JWT. `sub` is the Identity user id (`AccountId`). Resource APIs validate locally via JWKS — they do not call Identities per request.
- **Account-scoped token:** settings, credentials, email/phone, logout.
- **Profile-scoped token:** social writes. Identities issues it only after proving ownership (token exchange or a dedicated OpenIddict grant). Claim: `profile_id`.
- **Refresh / session:** held by the BFF and OpenIddict stores, **not** by JavaScript.

### React

- Dev: Vite proxy to the BFF (same origin).
- Prod: the browser’s only origin is the BFF (or later the gateway).
- Login is a redirect to OpenIddict (`/connect/authorize`), not a JSON endpoint that returns tokens.
- No access or refresh token in `localStorage` or the SPA bundle.
- Profile switch is a BFF call that performs token exchange; the cookie session then carries the new access token.

### Authorization layers

- **Edge (ASP.NET policies):** authenticated; optional email-verified; `MustHaveActiveProfile` for social writes; `MustBeModerator` for Moderation. Policies read **claims only**.
- **Domain:** ownership, blocks, visibility — on aggregates. “Can this profile delete this post?” is not a JWT role.

Coarse roles belong in the token (member / moderator). Resource rules belong in the aggregate.

### Extraction to microservices

1. Identities becomes its own host (Identity + OpenIddict + JWKS).
2. BFF/gateway: cookie in, JWT out; `Authority` = Identities URL.
3. Other hosts: `AddJwtBearer` with the same `Authority`.
4. `AccountSuspended` (and similar) already travel on the broker; subscribers revoke sessions and reject that `sub`.

The React app and resource modules keep using OIDC/JWT. No shared HMAC secret. No rewrite of login.

---

## Persistence

**One PostgreSQL instance**, two styles:

| Area | Store | Schema |
|------|--------|--------|
| Identities | EF Core (Identity + OpenIddict) | `identities` |
| Social contexts | Marten event store | per context (`profiles`, `contents`, …) |

**Why Identities is not event-sourced.** Passwords, security stamps, lockout, recovery, 2FA, OAuth grants, and refresh tokens are high-churn, need indexed lookups, and are already solved by Identity + OpenIddict. Event streams are the wrong source of truth for a security principal. Lifecycle facts other contexts care about (`AccountSuspended`) are **integration events**, not an account stream.

**Why Marten for the social domain.** Stream management, optimistic concurrency (`ExpectedVersion`), async projections, snapshots, transactional outbox — all on PostgreSQL.

**Hexagonal alignment.** Marten stays in Infrastructure. Social domains talk to `IEventStore<TAggregate>` so the store can be swapped.

**EventStoreDB later (social streams only):**

1. Replace the Marten adapter with `EventStore.Client.Grpc`.
2. Map Marten `stream_id`/`version` → EventStoreDB `StreamName`/`ExpectedVersion`.
3. Replay history with an idempotent background worker.
4. Switch traffic with a feature flag. No domain logic changes.

---

## Write / read flow

- **Identities writes:** Identity `UserManager` / OpenIddict stores. Current row is the truth. After register / suspend / reactivate / email change, publish an integration event via outbox.
- **Social writes:** load aggregate from its stream, apply command, append events atomically (`ExpectedVersion`).
- **Reads:** projections (Marten Async Daemon or external consumers). Eventually consistent.
- **Cross-context:** broker (Kafka/Redpanda). At-least-once via transactional outbox. Resource modules never query Identities tables.

---

## Bounded contexts

Each context owns its write model, its events, and its read models.

### 1. Identities

Not event-sourced. Write model: ASP.NET Core Identity.

**User (`Account`):** id (`AccountId`), email, phone, password hash, lockout, email/phone confirmation, notification defaults. Google (and later other providers) are Identity **external logins**, not a parallel credential table.

**Protocol (OpenIddict, this module):**

- `/connect/authorize`
- `/connect/token`
- `/connect/revocation`
- discovery + JWKS

Register, change password, and verify email are account-management APIs on Identity — not custom JWT-minting endpoints.

**Invariants:**

- Email uniqueness (normalized email + unique index).
- Phone uniqueness when set (unique index; absent/null allowed).
- The account must remain able to sign in (password and/or at least one external login).

**Persistence:** PostgreSQL / EF Core, schema `identities`.

**Integration events** (not an account stream):

- `AccountRegistered`
- `AccountSuspended` / `AccountReactivated`
- `AccountEmailAddressChanged`
- `NotificationPreferencesUpdated`

**Revocation:** password change, logout, and `AccountSuspended` invalidate OpenIddict tokens/sessions. Postgres first; Redis later if needed.

Other modules consume these events and the JWT. They do not load Identity users.

---

### 2. Profiles

**Aggregate:** `Profile` (event-sourced)  
**Stream:** `profile-{profileId}`

**Events:**

- `ProfileCreated` (handle, name, linked account id = Identity user id)
- `ProfileHandleChanged`
- `ProfileUpdated` (bio, avatar, visibility, notification preferences)
- `ProfileDeleted`

**Handle uniqueness:** projection `unique_handles` with a unique constraint, fed by `ProfileCreated` and `ProfileHandleChanged`. To avoid races, a **handle reservation saga**:

1. Command tries to append `HandleReservationRequested` to stream `handles-{handle}`.
2. On success, `Profile` appends `HandleChanged`; saga appends `HandleReservationConfirmed`.
3. On failure or timeout, `HandleReservationCancelled` cleans up.

Ownership is checked in Identities when issuing a profile-scoped token. Other APIs trust `profile_id` on the token.

**Store:** Marten, `profiles` schema (or partitioned tables).

**Read models:** PostgreSQL or MongoDB for profile details; Redis cache for hot reads.

---

### 3. Contents

**Aggregate:** `Post` (event-sourced)  
**Stream:** `post-{postId}`

**Events:**

- `PostCreated` (profileId, body, media refs, visibility)
- `PostEdited`
- `PostVisibilityChanged`
- `PostDeleted`

**Invariants:**

- Cannot edit a deleted post (current state from events).
- Authoring actor is the `profile_id` claim, not a body field.

**Store:** Marten. Snapshots via `SnapshotLifecycle` for long-lived, frequently edited posts.

**Read models:** Cassandra or MongoDB for get-by-id / by-profile; Elasticsearch for full-text search on bodies.

---

### 4. Interactions

Event-sourced, stream per composite id. The profile in the stream id is the **active profile** from the access token. Aggregates stay tiny (typically 2–3 events); no snapshots.

**PostReaction**  
Stream: `postreaction-{postId}-{profileId}`  
Events: `PostReacted`, `PostReactionRemoved`  
Invariant: add only if none exists; remove only if one exists. Composite id ⇒ one stream per profile–post pair.

**CommentReaction**  
Stream: `commentreaction-{commentId}-{profileId}`  
Events: `CommentReacted`, `CommentReactionRemoved`  
Same pattern as post reactions.

**Comment**  
Stream: `comment-{commentId}`  
Events: `CommentAdded` (postId, authorProfileId, body, optional parentCommentId), `CommentDeleted`  
Invariant: delete only by author or moderator. No edit/thread inside the aggregate; replies are separate comments with a parent reference.

**Bookmark**  
Stream: `bookmark-{profileId}-{postId}`  
Events: `PostBookmarked`, `PostUnbookmarked`  
Invariant: one bookmark per profile per post.

**Store:** Marten. `ExpectedVersion.Any` is acceptable for these tiny streams. Partition by context/month if volume requires it.

**Read models:** Cassandra `reactions_by_post`, `reactions_by_user`, `comments_by_post`, `bookmarks_by_user`. Redis counters for display counts.

---

### 5. SocialGraphs

**FollowRelationship**  
Stream: `follow-{followerProfileId}-{followedProfileId}`  
Events: `Followed`, `Unfollowed`  
Invariant: cannot follow if already following or if blocked; no self-follow. Load follow stream and block stream (or a block read model) before append.

**Block**  
Stream: `block-{blockerProfileId}-{blockedProfileId}`  
Events: `Blocked`, `Unblocked`  
Side effect: on `Blocked`, a policy/saga commands unfollow if a follow exists; the follow aggregate appends `Unfollowed`.

**Store:** Marten. Short/static streams; optimistic concurrency prevents duplicate follow/block.

**Read models:** Cassandra `followers_by_followed`, `following_by_follower`, `blocks_by_blocker`.

---

### 6. Feeds (read side only)

No aggregates. Projections from Contents, Interactions, and SocialGraphs.

- `PostCreated` → fan-out to followers’ feeds.
- `Followed` → backfill recent posts from that profile.
- `Blocked` → remove items from the blocker’s feed.
- Celebrity accounts (huge follower counts): fan-out on write for normal profiles; fan-out on **read** for celebrities (merge recent posts from followed profiles, short-term cache).

**Storage:** Redis sorted sets `feed:{profileId}` for hot timelines; Cassandra if durable long-term timelines are needed.

Handlers are idempotent (`EventId` as dedup key). Driven by Marten Async Daemon or Kafka consumers.

---

### 7. Notifications

**Aggregate:** `Notification` (event-sourced)  
**Stream:** `notification-{notificationId}`  
**Events:** `NotificationCreated`, `NotificationMarkedAsRead`, `NotificationDeleted`

Created by consuming other contexts (e.g. `PostReacted` → `NotificationCreated`). The aggregate owns read/unread/delete.

**Store:** Marten. Outbox keeps creation and publishing in one transaction.

**Read models:** MongoDB `notifications_current` (paginated inbox). Preferences from Identities (account defaults) and Profile projections (overrides), cached. Eventual consistency: a short window after a preference change may still send a notification — acceptable here.

---

### 8. Moderation

**Aggregates:** `Report`, `ModerationCase`, `Appeal` (event-sourced)

**Streams:**

- `report-{reportId}` → `ReportFiled`, `ReportResolved`, optional `ReportDismissed`
- `case-{caseId}` → `CaseOpened`, `ActionAdded`, `CaseResolved`
- `appeal-{appealId}` → `AppealFiled`, `AppealResolved`

**Invariants:** a case resolves only if open; an appeal is filed only on a resolved case; an invalid report can be dismissed without a case.

**Store:** Marten. Metadata (`CorrelationId`, `CausationId`, `UserId`) for audit.

**Read models:** relational DB for moderator dashboards.

Moderator capability is a coarse JWT claim/policy at the edge; workflow stays in these aggregates. Reporter **account** is recorded for anti-abuse; actions reference **profiles**.

---

## Key practices

### Identities

1. Identity is the source of truth for the user; OpenIddict is the protocol.
2. Other modules authenticate via JWT + JWKS, never via Identities services.
3. Lifecycle changes other contexts must see are integration events, not queries into the Identity database.

### Social event sourcing

1. **Stream per aggregate instance** — concurrency via Marten `ExpectedVersion`.
2. **Social state is ephemeral** — the event store is the source of truth; read models rebuild from scratch.
3. **Snapshots** — selective (`SnapshotLifecycle`, e.g. every 50–100 events for `Post`).
4. **Upcasting** — Marten `IUpcaster` on replay.
5. **Cross-context events** — outbox + Kafka/Redpanda, at-least-once.
6. **Idempotent projections** — track `EventId` or use Marten projection tracking.
7. **`IEventStore<T>` port** — swap Marten for EventStoreDB in infrastructure only.

---

## Suggested build order

1. Replace event-sourced `Account` with Identity + EF on the same PostgreSQL instance as Marten.
2. OpenIddict: authorization code + PKCE, RS256, discovery, JWKS.
3. BFF cookie session on `Nine.WebApi`; resource APIs JWT bearer.
4. Register / change password / verify email on Identity; publish integration events.
5. When Profiles exists: profile-scoped token exchange.
6. Google as Identity external login.
7. Split Identities to its own host only when a second process exists.

---

## Host layout (today)

```text
Sources/
  Nine.SharedKernel          claims, messaging ports, ES abstractions
  Identities/                Identity + OpenIddict + account APIs
  Hosts/Nine.WebApi          composition root: BFF + JWT bearer + module wiring
  (Profiles, Contents, …)    later modules; Marten; JWT only
```

React is a separate SPA. In development it proxies to the BFF. In production the BFF is the browser origin.
