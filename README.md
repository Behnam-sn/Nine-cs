# Social Media Platform (DDD + CQRS + Event Sourcing)

A **social media application backend** built with **C#/.NET**, designed using **Domain-Driven Design (DDD)**, **Hexagonal Architecture (Ports & Adapters)**, **CQRS**, and **Event Sourcing**.

The goal is to create a **scalable, maintainable, and evolvable system** that models a real-world social media domain while keeping business logic cleanly separated from infrastructure concerns.

---

## 🧠 Architectural Overview

This project follows these core architectural principles:

- **Domain-Driven Design (DDD)**  
  Clear bounded contexts, aggregates, and domain events

- **Hexagonal Architecture (Ports & Adapters)**  
  Domain and application layers are independent of frameworks. Each context exposes Ports (interfaces) for commands and queries; Adapters provide concrete implementations (HTTP, gRPC, message brokers).

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

## Overall Architecture with Event Sourcing

- **Write side:** Each aggregate’s lifecycle is a stream of events in an append-only store. State is rebuilt by replaying those events. Invariants are checked by loading the aggregate from its stream, applying the new command, and appending new events atomically (optimistic concurrency).
- **Read side:** Projections subscribe to streams and maintain denormalised, query-optimised models in separate databases. These are eventually consistent.
- **Cross-context:** Events are the integration mechanism. Each bounded context owns its own event store, but shares events through a broker (e.g., Kafka) for other contexts to consume.

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

**Event store:**
- **Relational DB as simple event store** (e.g., `events` table with `stream_id`, `version`, `event_type`, `data`) – strong ACID for concurrent credential changes. Or **EventStoreDB** if a dedicated store is preferred.

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
A dedicated projection `unique_handles` (e.g., a database table with a unique constraint) is maintained by consuming `ProfileCreated` and `ProfileHandleChanged`. To avoid race conditions, a **handle reservation saga** can be used:
1. The command to set a new handle first tries to append `HandleReservationRequested` to a **separate stream** (`handles-{handle}`).
2. On success, the `Profile` aggregate appends `HandleChanged` and the saga appends `HandleReservationConfirmed`.
3. If the profile operation fails or the reservation times out, a `HandleReservationCancelled` event cleans up.

This ensures strict uniqueness while keeping aggregates small.

**Event store:**
- Same relational event store or EventStoreDB, within the Profiles context.

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

**Event store:**
- Relational or event store; high append volume, consider **EventStoreDB** with snapshots for long-lived posts with many edits.

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
- **EventStoreDB** or a dedicated **relational DB** for high‑speed writes. Aggregates are tiny (2–3 events max), so snapshotting is unnecessary. Streams may be archived after the last reaction/bookmark is removed.

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

**Event store:** Same as interactions; streams are short.

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

All handling is idempotent (using event IDs as deduplication keys).

---

## 7. Notifications (Read Side & Aggregate Mix)

**Aggregate:** `Notification` (event-sourced)  
**Stream:** `notification-{notificationId}`  
**Events:** `NotificationCreated`, `NotificationMarkedAsRead`, `NotificationDeleted`.

- The service creates notifications by consuming events from other contexts (e.g., `PostReacted` → `NotificationCreated`).
- The aggregate exists to manage the notification’s lifecycle (read/unread).

**Event store:** Simple event store for notification streams; can be lightweight.

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

**Event store:** Relational event store (audit trail is natural).

**Read models:** Relational DB for moderator dashboards (list of open cases, reports by status).

---

## Key Event-Sourcing Practices Applied

1. **Stream per aggregate instance** – concurrency control via expected version; no conflicts across aggregates.
2. **All state is ephemeral** – the event store is the source of truth; read models can be rebuilt from scratch.
3. **Snapshots** – recommended for long-lived aggregates with many events (e.g., `Post` with hundreds of edits). Store snapshot periodically in the event store or a separate blob.
4. **Event upcasting** – plan for event schema evolution (e.g., new fields) by transforming old events when loading.
5. **Cross-context events** – use a broker (Kafka) for distribution; each context publishes its core events to topics, others consume.
6. **Idempotent projections** – all event handlers track processed event IDs to avoid duplicates.