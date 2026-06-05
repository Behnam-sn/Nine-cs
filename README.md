# Social Media Platform (DDD + CQRS + Event Sourcing)

A **social media application backend** built with **C#/.NET**, designed using **Domain-Driven Design (DDD)**, **Hexagonal Architecture (Ports & Adapters)**, **CQRS**, and **Event Sourcing**.

The goal is to create a **scalable, maintainable, and evolvable system** that models a real-world social media domain while keeping business logic cleanly separated from infrastructure concerns.

---

## 🧠 Architectural Overview

This project follows these core architectural principles:

- **Domain-Driven Design (DDD)**  
  Clear bounded contexts, aggregates, and domain events

- **Hexagonal Architecture (Ports & Adapters)**  
  Domain and application layers are independent of frameworks. Each context exposes Ports (interfaces) for commands and queries; Adapters provide concrete implementations (HTTP, gRPC, message brokers). **The event store adapter is abstracted behind `IEventStore` to allow future swaps.**

- **CQRS (Command Query Responsibility Segregation)**  
  Separate write (command) and read (query) models

- **Event Sourcing**  
  Aggregates are persisted as event streams, not state snapshots

- **Event-Driven Architecture**  
  Domain and integration events propagate changes across contexts

---

## 🧩 Bounded Contexts

| Bounded Context   | Aggregate Roots                                        | Notes                                                                                                                              |
|-------------------|--------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------|
| **Identities**    | `Account`                                              | Manages authentication, identity, credentials (as entities), and account-level settings.                                           |
| **Profiles**      | `Profile`                                              | Public-facing content space; an account can own multiple profiles. Profile settings (bio, avatar, etc.) are part of the aggregate. |
| **Contents**      | `Post`                                                 | Posts belong to a profile. Visibility rules and media management are handled here.                                                 |
| **Interactions**  | `PostReaction`, `CommentReaction`, `Comment`, `Bookmark` | `PostReaction` targets posts; `CommentReaction` targets comments. High throughput, small aggregates.                                 |
| **SocialGraphs**  | `FollowRelationship`, `Block`                          | Follows and blocks happen between profiles.                                                                                        |
| **Feeds**         | *(none)*                                               | Projection-only; builds timelines and rankings from events.                                                                        |
| **Notifications** | `Notification`                                         | Preferences live inside `Account` (defaults) and `Profile` (per-profile overrides).                                                |
| **Moderation**    | `Report`, `ModerationCase`, `Appeal`                   | Actions reference profiles; the reporter’s account is recorded for anti-abuse.                                                     |

Each bounded context owns:
- Its aggregates
- Its domain events
- Its read models

---

## 🗃️ Event Store Strategy: PostgreSQL + Marten

- **Primary Choice:** PostgreSQL paired with the **Marten** library for .NET
- **Why:** Marten provides a mature, production-ready event sourcing abstraction over PostgreSQL. It handles stream management, optimistic concurrency (`ExpectedVersion`), async projections, snapshots, and the transactional outbox pattern out-of-the-box.
- **Hexagonal Alignment:** Marten lives strictly in the **Infrastructure/Adapter layer**. The domain interacts only with `IEventStore<TAggregate>` ports. Swapping to another store later requires only a new adapter implementation.
- **Storage Model:** Events are stored in Marten's `mt_events` and `mt_streams` tables (customizable per context via schema segregation or table partitioning). Streams are append-only, versioned, and fully ACID-compliant.
- **Migration Path to EventStoreDB:** If write throughput, server-side projections, or strict log-compact semantics become necessary, the architecture supports a phased migration:
  1. Replace the Marten adapter with an `EventStore.Client.Grpc` adapter.
  2. Map Marten's `stream_id`/`version` → EventStoreDB's `StreamName`/`ExpectedVersion`.
  3. Migrate historical events via a background sync worker (idempotent replay to new stream format).
  4. Switch traffic via feature flag. Zero domain logic changes required.

---

## Overall Architecture with Event Sourcing

- **Write side:** Each aggregate’s lifecycle is a stream of events in an append-only store (PostgreSQL via Marten). State is rebuilt by replaying those events. Invariants are checked by loading the aggregate from its stream, applying the new command, and appending new events atomically (optimistic concurrency via `version` column + DB constraints).
- **Read side:** Projections subscribe to streams via Marten's `Async Daemon` or external consumers, maintaining denormalised, query-optimised models in separate databases. These are eventually consistent.
- **Cross-context:** Events are the integration mechanism. Each bounded context owns its own event streams, but shares domain events through a broker (e.g., Kafka) for other contexts to consume. The transactional outbox pattern guarantees at-least-once delivery.

---

## 1. Identities

**Aggregate:** `Account` (event-sourced)  
**Stream:** `account-{accountId}`

**Core events:**
- `AccountRegistered` (email, initial credential)
- `CredentialAdded` / `CredentialRemoved` / `CredentialChanged`
- `AccountSuspended` / `AccountReactivated`
- `NotificationPreferencesUpdated`

**Invariants enforced at append:**
- Email uniqueness requires a reservation pattern or a read model of taken emails (idempotent projection). The aggregate checks against that read model before emitting `AccountRegistered`.
- An account must always have at least one credential; this is validated before appending `CredentialRemoved`.

**Event store:** PostgreSQL via Marten. Marten's `ExpectedVersion` guarantees ACID-safe concurrent credential updates. Stream schema isolated per context.

**Read models / projections:**
- **PostgreSQL** table `accounts_current` rebuilt from events for login lookups (hashed password, status).
- **Redis** for token revocation lists or session state.

---

## 2. Profiles

**Aggregate:** `Profile` (event-sourced)  
**Stream:** `profile-{profileId}`

**Core events:**
- `ProfileCreated` (handle, name, linked account ID)
- `ProfileHandleChanged` (must enforce uniqueness – see below)
- `ProfileUpdated` (bio, avatar, visibility, notification preferences)
- `ProfileDeleted`

**Handle uniqueness:**  
A dedicated projection `unique_handles` (database table with a unique constraint) is maintained by consuming `ProfileCreated` and `ProfileHandleChanged`. To avoid race conditions, a **handle reservation saga** can be used:
1. The command to set a new handle first tries to append `HandleReservationRequested` to a **separate stream** (`handles-{handle}`).
2. On success, the `Profile` aggregate appends `HandleChanged` and the saga appends `HandleReservationConfirmed`.
3. If the profile operation fails or the reservation times out, a `HandleReservationCancelled` event cleans up.

This ensures strict uniqueness while keeping aggregates small.

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

**Event store:** PostgreSQL via Marten. Snapshots configured via Marten's `SnapshotLifecycle` for long-lived posts with frequent edits.

**Read models / projections:**
- **Cassandra** or **MongoDB** for fast post retrieval by ID or profile.
- **Elasticsearch** for full-text search on post bodies (projection updates from events).

---

## 4. Interactions

All interaction aggregates are event-sourced, stream per composite ID.

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
- **Notification preferences** are read from the Account/Profile projections (cached). Because this is eventually consistent, a brief window exists after a preference change where a notification may still be sent — an acceptable trade‑off in social applications.

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

---

## Key Event-Sourcing Practices Applied

1. **Stream per aggregate instance** – concurrency control via Marten's `ExpectedVersion`; no conflicts across aggregates.
2. **All state is ephemeral** – the event store is the source of truth; read models can be rebuilt from scratch.
3. **Snapshots** – configured selectively via Marten's `SnapshotLifecycle` (e.g., every 50–100 events for `Post` or `Account`).
4. **Event upcasting** – handled via Marten's `IUpcaster` interface to transform legacy event payloads during stream replay.
5. **Cross-context events** – Marten's transactional outbox + Kafka/Redpanda ensures reliable, at-least-once distribution.
6. **Idempotent projections** – all event handlers track `EventId` in a `processed_events` table or use Marten's built-in projection tracking to avoid duplicates.
7. **Adapter Abstraction** – `IEventStore<T>` port isolates domain logic. Swapping Marten for EventStoreDB requires only infrastructure-layer changes.

---
> 💡 **Note:** This architecture is intentionally storage-agnostic at the domain layer. PostgreSQL + Marten is chosen for operational simplicity, ACID guarantees, and rapid development velocity. The system is designed to migrate to EventStoreDB or another log-structured store transparently if throughput or projection requirements outgrow relational capabilities.