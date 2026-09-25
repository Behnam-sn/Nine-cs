# Nine — Backend Context

> Backend-only document. It describes how Nine is built on the server.  
> It references the Product context for behavior and does not redefine product rules.

---

## 1. Architecture Principles

- **Modular monolith.** One host today: `Nine.Api`. Each bounded context is a module with its own model and persistence. Extracting a context later is a hosting change, not a redesign.
- **Domain-Driven Design.** Bounded contexts, aggregates, domain events.
- **Hexagonal architecture / ports and adapters.** Domain and Application layers do not depend on frameworks. Infrastructure implements interfaces defined in Domain. Identity is the exception: its Application layer uses ASP.NET Core Identity `UserManager` because Identity is the write model for credentials.
- **CQRS.** Separate write and read models. Command repositories load streams and append events. Query repositories read projection documents.
- **Event sourcing for the social domain.** Profiles, Content, Interactions, SocialGraphs, Notifications, Stories, and Moderation persist as event streams. Feeds is read-only with no aggregate. Identity core is not event-sourced.
- **Narrow event-sourced Identity adjunct.** Administrator, moderator, and suspension are events on an Account stream, projected onto a table. The Identity tables remain the source of truth for credentials, lockout, confirmation, reset tokens, and the cookie principal.
- **No broker today.** Cross-context communication happens in the same Marten session and the same unit-of-work commit. A broker is a later extraction option.
- **Open-source licenses only.** MIT / Apache 2.0.

### Long-run goal

The long-run goal is a system that can split into microservices **without changing how clients authenticate** and without other modules depending on Identities' internals.

### Extraction rule

A module other than Identity must not inject Identity `UserManager`, OpenIddict stores, or Identity repositories. It may use SharedKernel claim type names, `UserId` / `ProfileId` values, `[Authorize]`, SharedKernel policy names, and integration events.

---

## 2. Solution and Modules

One Git repository. `Nine.sln` is the .NET solution. The Next.js app lives in `frontend/`.

The API is a modular monolith. Nine bounded contexts exist:

| Context       | Store                           | Aggregate(s)                                     |
| ------------- | ------------------------------- | ------------------------------------------------ |
| Identity      | EF Core + event-sourced adjunct | Account                                          |
| Profiles      | Event-sourced                   | Profile                                          |
| Content       | Event-sourced                   | Post                                             |
| Interactions  | Event-sourced                   | Comment, PostReaction, CommentReaction, Bookmark |
| SocialGraphs  | Event-sourced                   | Follow, Block                                    |
| Feeds         | Read-only projection            | none                                             |
| Notifications | Event-sourced                   | Notification                                     |
| Stories       | Event-sourced                   | Story                                            |
| Moderation    | Event-sourced                   | Report                                           |

Each context is four projects. `Nine.Api` references the Presentation and Infrastructure projects, maps the routes, opens one Marten session per request, and commits the unit of work. It does not contain product rules.

| Project                             | Role                                                                                                  |
| ----------------------------------- | ----------------------------------------------------------------------------------------------------- |
| `Nine.Identity.Domain`              | Account aggregate, account events, command and query interfaces. `IEmailSender` lives here            |
| `Nine.Identity.Application`         | Identity use cases                                                                                    |
| `Nine.Identity.Presentation`        | Identity HTTP routes                                                                                  |
| `Nine.Identity.Infrastructure`      | EF Core Identity tables, Marten account stream, flag projection, Postmark                             |
| `Nine.Profiles.Domain`              | Profile aggregate, events, command and query interfaces                                               |
| `Nine.Profiles.Application`         | Profile use cases                                                                                     |
| `Nine.Profiles.Presentation`        | Profile HTTP routes                                                                                   |
| `Nine.Profiles.Infrastructure`      | Marten profile stream and projections                                                                 |
| `Nine.Content.Domain`               | Post aggregate, events, command and query interfaces                                                  |
| `Nine.Content.Application`          | Post use cases                                                                                        |
| `Nine.Content.Presentation`         | Post HTTP routes                                                                                      |
| `Nine.Content.Infrastructure`       | Marten post stream and projections                                                                    |
| `Nine.Interactions.Domain`          | Comment, PostReaction, CommentReaction, and Bookmark aggregates, events, command and query interfaces |
| `Nine.Interactions.Application`     | Comment, reaction, and bookmark use cases                                                             |
| `Nine.Interactions.Presentation`    | Comment, reaction, and bookmark HTTP routes                                                           |
| `Nine.Interactions.Infrastructure`  | Marten comment, reaction, and bookmark streams and projections                                        |
| `Nine.SocialGraphs.Domain`          | Follow and Block aggregates, events, command and query interfaces                                     |
| `Nine.SocialGraphs.Application`     | Follow and block use cases                                                                            |
| `Nine.SocialGraphs.Presentation`    | Follow, block, and list routes                                                                        |
| `Nine.SocialGraphs.Infrastructure`  | Marten follow and block streams and projections                                                       |
| `Nine.Feeds.Domain`                 | Feed read model and `IFeedReader`. No aggregate                                                       |
| `Nine.Feeds.Application`            | Feed queries                                                                                          |
| `Nine.Feeds.Presentation`           | Feed HTTP routes                                                                                      |
| `Nine.Feeds.Infrastructure`         | Inline feed projection and the Marten query                                                           |
| `Nine.Notifications.Domain`         | Notification aggregate, events, command and query interfaces                                          |
| `Nine.Notifications.Application`    | Notification use cases                                                                                |
| `Nine.Notifications.Presentation`   | Notification HTTP routes                                                                              |
| `Nine.Notifications.Infrastructure` | Marten notification stream, projection, and event handlers                                            |
| `Nine.Stories.Domain`               | Story aggregate, events, command and query interfaces                                                 |
| `Nine.Stories.Application`          | Story use cases                                                                                       |
| `Nine.Stories.Presentation`         | Story HTTP routes                                                                                     |
| `Nine.Stories.Infrastructure`       | Marten story stream and projections, expiry sweep                                                     |
| `Nine.Moderation.Domain`            | Report aggregate, events, command and query interfaces                                                |
| `Nine.Moderation.Application`       | Report and moderator use cases                                                                        |
| `Nine.Moderation.Presentation`      | Report and moderation routes                                                                          |
| `Nine.Moderation.Infrastructure`    | Marten report stream, queue, and audit projection                                                     |
| `Nine.Shared.Domain`                | `IClock` only                                                                                         |
| `Nine.Shared.Application`           | Empty until two contexts need an application type                                                     |
| `Nine.Shared.Presentation`          | Problem Details mapping and route conventions                                                         |
| `Nine.Shared.Infrastructure`        | `SystemClock`                                                                                         |
| `Nine.Api`                          | Composition root, authentication middleware, session, unit of work                                    |
| `Nine.Api.Tests`                    | HTTP integration tests through the composed host                                                      |
| `Nine.Identity.Domain.Tests`        | Domain tests for Account                                                                              |
| `Nine.Profiles.Domain.Tests`        | Domain tests for Profile                                                                              |
| `Nine.Content.Domain.Tests`         | Domain tests for Post                                                                                 |
| `Nine.Interactions.Domain.Tests`    | Domain tests for Comment, PostReaction, CommentReaction, and Bookmark                                 |
| `Nine.SocialGraphs.Domain.Tests`    | Domain tests for Follow and Block                                                                     |
| `Nine.Notifications.Domain.Tests`   | Domain tests for Notification                                                                         |
| `Nine.Stories.Domain.Tests`         | Domain tests for Story                                                                                |
| `Nine.Moderation.Domain.Tests`      | Domain tests for Report                                                                               |

A type moves into `Nine.Shared.*` only when two contexts already need it. Events never move there. Feature Domain, Application, and Presentation projects do not reference `Nine.Shared.Infrastructure`.

---

## 3. Authentication

### 3.1 Today

- ASP.NET Core Identity holds the password hash, lockout, confirmation and reset tokens, and the cookie principal.
- The cookie is `HttpOnly` and `SameSite=Lax`. It is `Secure` once the site is on HTTPS.
- The session cookie lasts 14 days.
- Browser mutations send the antiforgery header.
- The active profile is a separate server-side cookie. The client does not read it.
- The API resolves the authenticated account from the cookie principal. It resolves the active profile from the active-profile cookie.
- Social writes take the actor from the active-profile cookie, never from a client-supplied `profileId` in the JSON body.

### 3.2 Profile-scoped authorization

A mutating social action requires:

- A valid session cookie that resolves to an account.
- A confirmed email.
- An active profile cookie that resolves to a profile owned by that account.
- The profile is not suspended.

The policy classes read the Account projection for administrator, moderator, and suspension flags.

### 3.3 Long-run OIDC/BFF path

The long-run goal is a system that can split into microservices without changing how clients authenticate. The path is:

```text
React / Next.js
    │ same-origin, httpOnly cookie
    ▼
BFF / public origin
    │ OIDC Authorization Code + PKCE
    ▼
Identity module
    ASP.NET Core Identity
    OpenIddict
    │ JWT access token RS256, sub = UserId
    ▼
Resource APIs
```

- Access token: short-lived JWT, `at+jwt`, unencrypted, RS256. `sub` is `UserId`.
- Resource APIs validate locally via JWKS; they do not call Identity per request.
- Profile-scoped token: social writes. Identity issues it only after proving ownership. Claim: `profile_id`.
- Refresh/session: held by the BFF and OpenIddict stores, not by JavaScript.
- React never holds access or refresh tokens in `localStorage` or the SPA bundle.
- Profile switch is a BFF call that performs token exchange.
- Replacing Identity with another OIDC server is a host change: `AddJwtBearer` with the new `Authority`, and the same SharedKernel policy helper with the JwtBearer scheme. Profiles and other resource modules stay unchanged.

### 3.4 Extraction to microservices

1. Identity becomes its own host: Identity + OpenIddict + JWKS.
2. BFF/gateway: cookie in, JWT out; `Authority` = Identity URL.
3. Other hosts: `AddJwtBearer` with the same `Authority`.
4. `AccountSuspended` and similar events already travel on the broker; subscribers revoke sessions and reject that `sub`.

The React app and resource modules keep using OIDC/JWT. No shared HMAC secret. No rewrite of login. No OpenIddict types outside Identity.

---

## 4. Persistence

**One PostgreSQL 18 database.** Two styles:

| Area            | Store                                             | Schema       |
| --------------- | ------------------------------------------------- | ------------ |
| Identity        | EF Core (Identity tables) + Marten Account stream | `identities` |
| Social contexts | Marten event store                                | per context  |

- EF Core is used only for the ASP.NET Core Identity tables.
- Marten is the event store and the projection store.
- Both use the same database and the same connection string.
- There is no second database, no message broker, and no background projection loop.

Marten stays in Infrastructure. Social domains talk to `IEventStore<TAggregate>` so the store can be swapped later.

### 4.1 Identity tables

ASP.NET Core Identity holds password hash, lockout, confirmation and reset tokens, and the cookie principal. The key is a `Guid`.

### 4.2 Account flags projection

Administrator, moderator, and suspension are events on an Account stream, projected onto a table. Each authenticated request loads those flags from that projection.

### 4.3 Feed projection

`Feeds.Infrastructure` registers an inline projection on the session. It references `Nine.Identity.Domain`, `Nine.Profiles.Domain`, `Nine.Content.Domain`, `Nine.Interactions.Domain`, and `Nine.SocialGraphs.Domain` for their event types. No publisher references Feeds. Feeds does not read another context's tables.

The projection stores:

- A copy of each public post: id, author, handle, display name, avatar key, text, time, and each image key, content type, and byte size.
- Each follow edge.
- Each block edge.
- Each suspension.
- Post reaction counts per post.
- Comment reaction counts per comment.
- Comment counts per post.

An avatar change updates the avatar key on the copies of that author's posts. A handle change updates the handle on those copies. The feed query uses only those copies.

### 4.4 Notification event handlers

`Notifications.Infrastructure` registers handlers that listen to events from other contexts and append `NotificationCreated` events. It references `Nine.Content.Domain`, `Nine.Interactions.Domain`, and `Nine.SocialGraphs.Domain` for their event types. No publisher references Notifications.

Handlers are idempotent. The notification stream id is derived from the triggering event id so the same event does not create two notifications.

---

## 5. Request and Commit

`Nine.Api` opens one Marten session for the request and one unit of work around it.

- Command repositories only load a stream and append events.
- Query repositories only read projection documents.
- Repositories do not call `SaveChanges`.
- The unit of work calls it once, at the end of the request.
- That commit writes the new events and runs the inline projections, including the feed.

Every append carries the version the aggregate was loaded at. A mismatch aborts the commit and the API returns 409. The client retries against the new version. The winner's commit, including its inline projections, stands.

### 5.1 Cross-context in the same session

When one context's event must cause another context's event, the handling happens in the same session, before `SaveChanges`.

- Moderation raises `AccountSuspended`. Identity handles it in the same session and appends `AccountSuspensionFlagSet` to the Account stream.
- Content raises `PostDeleted`. Interactions handles it in the same session and appends `CommentDeleted`, `PostReactionRemoved`, `CommentReactionRemoved`, and `PostBookmarkRemoved` to the affected streams.
- Content raises `PostDeleted`. Notifications removes pending notifications for that post in the same session.
- Interactions raises `PostReacted`. Notifications handles it in the same session and appends `NotificationCreated`.
- Interactions raises `CommentReacted`. Notifications handles it in the same session and appends `NotificationCreated`.
- Interactions raises `CommentAdded`. Notifications handles it in the same session and appends `NotificationCreated`.

`Nine.Identity.Application` references `Nine.Moderation.Domain` so it can handle suspension events. `Nine.Interactions.Application` references `Nine.Content.Domain` so it can handle post deletion. `Nine.Notifications.Infrastructure` references the domains of the contexts whose events it consumes.

### 5.2 Delete and masking

A delete appends a tombstone and removes the projection documents in the unit-of-work commit. After that commit, and before the response, the use case applies Marten's event-data masking to the affected streams. Masking overwrites post text, comment text, display name, bio, and email-bearing fields. It does not overwrite handle strings. Masking is safe to run again. The moderation audit projection records the action and never receives the post body.

The HTTP response is sent after that commit. Creating a post returns `201` and the new id. Deleting a post, comment, follow, block, bookmark, reaction, and rename return `204`. The open page then refetches its read model. The change is already in the projection.

---

## 6. Bounded Contexts

### 6.1 Identity

**Purpose:** credentials, session, email confirmation, password reset, account lifecycle, and the narrow Account stream that carries administrator, moderator, and suspension flags.

**Store:** EF Core for credentials. Event-sourced adjunct for flags.

**Aggregate:** Account.

**Stream id:** `account-{accountId}`.

**Aggregate fields:**

- `AccountId`
- `Email`
- `DateOfBirth`
- `Locale`
- `IsEmailConfirmed`
- `IsAdministrator`
- `IsModerator`
- `IsSuspended`

**Value objects:**

- `AccountId` — wraps `Guid`.
- `EmailAddress` — normalized, unique.
- `PlainPassword` — transient, never stored.
- `DateOfBirth` — used only to enforce the age floor at signup.
- `Locale` — `en` or `fa`.

**Invariants:**

- Email is unique across accounts.
- Password is at least 8 characters. No character-class rules.
- Five failed logins lock the account for 15 minutes.
- The account must remain able to sign in.
- An administrator cannot suspend their own account.
- A moderator cannot suspend or demote an administrator.

**Events:**

- `AccountAdminFlagSet`
- `AccountAdminFlagRevoked`
- `AccountModeratorFlagSet`
- `AccountModeratorFlagRevoked`
- `AccountSuspensionFlagSet`
- `AccountSuspensionFlagCleared`

**Services:**

- `IEmailSender` — Postmark. Confirmation, reset, email-change.
- `IPasswordHasher` — provided by ASP.NET Core Identity.
- `IClock` — from SharedKernel.

**Exceptions:**

- `EmailAlreadyInUseException`
- `WeakPasswordException`
- `AccountLockedOutException`
- `EmailNotConfirmedException`
- `CannotSuspendSelfException`
- `CannotSuspendAdministratorException`

**Read models:**

- Account projection: flags, email, locale, date of birth.
- Session projection for the signed-in account.

**Routes:**

| Method | Path                           | Who       |
| ------ | ------------------------------ | --------- |
| POST   | `/api/auth/register`           | public    |
| POST   | `/api/auth/login`              | public    |
| POST   | `/api/auth/logout`             | signed in |
| POST   | `/api/auth/forgot-password`    | public    |
| POST   | `/api/auth/reset-password`     | public    |
| POST   | `/api/auth/verify-email`       | public    |
| GET    | `/api/auth/session`            | signed in |
| GET    | `/api/account`                 | signed in |
| PATCH  | `/api/account/locale`          | signed in |
| POST   | `/api/account/change-email`    | signed in |
| POST   | `/api/account/change-password` | signed in |
| GET    | `/api/account/export`          | signed in |
| DELETE | `/api/account`                 | signed in |

---

### 6.2 Profiles

**Purpose:** the public identity that acts. One account may own up to three profiles.

**Store:** Event-sourced.

**Aggregate:** Profile.

**Stream id:** `profile-{profileId}`.

**Aggregate fields:**

- `ProfileId`
- `AccountId`
- `Handle`
- `DisplayName`
- `Bio`
- `AvatarKey`
- `PreviousHandles` — list of handle and release time
- `CreatedAt`

**Value objects:**

- `ProfileId` — wraps `Guid`.
- `Handle` — lowercase ASCII, `[a-z0-9_]{3,20}`, must start with a letter.
- `DisplayName` — 1 to 50 characters. May be Persian.
- `Bio` — optional, up to 160 characters.
- `AvatarKey` — optional object key in MinIO.

**Invariants:**

- Handle pattern `[a-z0-9_]{3,20}`, must start with a letter.
- Handle is not a reserved system word.
- Handle is not inside another profile's 30-day release window.
- Display name is 1 to 50 characters.
- Bio is at most 160 characters.
- Rename is allowed once every 7 days.
- An account can own at most three profiles.
- A profile can be created only after the email is confirmed.

**Events:**

- `ProfileCreated`
- `ProfileHandleChanged`
- `ProfileUpdated`
- `ProfileDeleted`

**Services:**

- `IHandleAvailabilityReader` — scans current handles and previous handles inside a release window.
- `IClock` — for rename windows.

**Exceptions:**

- `HandleTakenException`
- `HandleReservedException`
- `HandleInsideReleaseWindowException`
- `RenameTooSoonException`
- `ProfileCapReachedException`
- `EmailNotConfirmedException`
- `InvalidHandleException`

**Read models:**

- Profile document: handle, display name, bio, avatar key, previous handles with release times, creation time.

**Routes:**

| Method | Path                            | Who                             |
| ------ | ------------------------------- | ------------------------------- |
| POST   | `/api/profiles`                 | confirmed account under the cap |
| GET    | `/api/profiles/{handle}`        | public                          |
| PATCH  | `/api/profiles/{handle}`        | active profile                  |
| DELETE | `/api/profiles/{handle}`        | active profile                  |
| POST   | `/api/profiles/{handle}/rename` | active profile                  |
| POST   | `/api/profiles/switch`          | signed in                       |
| POST   | `/api/profiles/avatar`          | active profile                  |

---

### 6.3 Content

**Purpose:** posts. Plain text plus up to four images.

**Store:** Event-sourced.

**Aggregate:** Post.

**Stream id:** `post-{postId}`.

**Aggregate fields:**

- `PostId`
- `AuthorProfileId`
- `Text`
- `Images` — list of image key, content type, byte size
- `CreatedAt`
- `EditedAt`
- `IsDeleted`

**Value objects:**

- `PostId` — wraps `Guid`.
- `PostText` — up to 280 characters. Newlines are kept. Leading and trailing whitespace is removed.
- `ImageAttachment` — object key, content type, byte size.
- `ProfileId` — author.

**Invariants:**

- Text is at most 280 characters.
- At most four images.
- At least one of text or image is present.
- Only the author can edit or delete.
- Editing changes the text only.
- Images cannot be added or removed after publishing.
- A post belongs to exactly one profile.

**Events:**

- `PostPublished`
- `PostEdited`
- `PostDeleted`

**Services:**

- `IImageAttachmentValidator` — verifies each key was uploaded by the author, is not already attached, and is a JPEG, PNG, or WebP.
- `IIdempotencyStore` — dedupes composer submissions by key.
- `IClock`.

**Exceptions:**

- `PostTextTooLongException`
- `TooManyImagesException`
- `EmptyPostException`
- `NotPostAuthorException`
- `ImageNotOwnedByAuthorException`
- `ImageAlreadyAttachedException`

**Read models:**

- Post document: id, author profile id, text, image keys, content types, byte sizes, creation time, edit time.

**Routes:**

| Method | Path                           | Who            |
| ------ | ------------------------------ | -------------- |
| POST   | `/api/posts`                   | active profile |
| GET    | `/api/posts/{id}`              | public         |
| PATCH  | `/api/posts/{id}`              | author         |
| DELETE | `/api/posts/{id}`              | author         |
| GET    | `/api/profiles/{handle}/posts` | public         |

---

### 6.4 Interactions

**Purpose:** comments, post reactions, comment reactions, and bookmarks.

**Store:** Event-sourced.

**Aggregates:** Comment, PostReaction, CommentReaction, Bookmark.

**Value objects:**

- `CommentId` — wraps `Guid`.
- `CommentText` — up to 280 characters. Newlines are kept. Leading and trailing whitespace is removed.
- `ReactionType` — enum. Only `Like` is enabled now.
- `ProfileId` — actor.
- `PostId` — target of a post reaction or bookmark.
- `CommentId` — target of a comment reaction.

#### 6.4.1 Comment

**Stream id:** `comment-{commentId}`.

**Aggregate fields:**

- `CommentId`
- `PostId`
- `AuthorProfileId`
- `Text`
- `CreatedAt`
- `EditedAt`
- `IsDeleted`

**Invariants:**

- Text is at most 280 characters.
- No images.
- Only the author can edit or delete.
- A comment belongs to exactly one post and one author profile.

**Events:**

- `CommentAdded`
- `CommentEdited`
- `CommentDeleted`

**Services:** `IClock`.

**Exceptions:**

- `CommentTextTooLongException`
- `EmptyCommentException`
- `NotCommentAuthorException`

#### 6.4.2 PostReaction

**Stream id:** `postreaction-{postId}-{profileId}`.

**Aggregate fields:**

- `PostId`
- `ProfileId`
- `ReactionType`
- `CreatedAt`
- `IsActive`

**Invariants:**

- A profile has at most one reaction per post.
- The reaction type must be one of the enabled types. Only `Like` is enabled now.
- Add only if none exists; remove only if one exists.

**Events:**

- `PostReacted`
- `PostReactionRemoved`

**Services:** `IReactionTypePolicy` — the enabled set.

**Exceptions:**

- `ReactionAlreadyExistsException`
- `ReactionDoesNotExistException`
- `UnsupportedReactionTypeException`

#### 6.4.3 CommentReaction

**Stream id:** `commentreaction-{commentId}-{profileId}`.

**Aggregate fields:**

- `CommentId`
- `ProfileId`
- `ReactionType`
- `CreatedAt`
- `IsActive`

**Invariants:**

- A profile has at most one reaction per comment.
- The reaction type must be one of the enabled types. Only `Like` is enabled now.
- Add only if none exists; remove only if one exists.

**Events:**

- `CommentReacted`
- `CommentReactionRemoved`

**Services:** `IReactionTypePolicy`.

**Exceptions:**

- `ReactionAlreadyExistsException`
- `ReactionDoesNotExistException`
- `UnsupportedReactionTypeException`

#### 6.4.4 Bookmark

**Stream id:** `bookmark-{profileId}-{postId}`.

**Aggregate fields:**

- `ProfileId`
- `PostId`
- `CreatedAt`
- `IsActive`

**Invariants:**

- A profile has at most one bookmark per post.
- Bookmarks are private to the profile.
- Add only if none exists; remove only if one exists.

**Events:**

- `PostBookmarked`
- `PostBookmarkRemoved`

**Services:** `IClock`.

**Exceptions:**

- `BookmarkAlreadyExistsException`
- `BookmarkDoesNotExistException`

**Read models:**

- Comments by post, chronological.
- Comment reaction count by comment.
- Whether a profile reacted to a comment.
- Post reaction count by post.
- Whether a profile reacted to a post.
- Bookmarks by profile, newest first.

**Routes:**

| Method | Path                           | Who            |
| ------ | ------------------------------ | -------------- |
| POST   | `/api/posts/{id}/comments`     | active profile |
| GET    | `/api/posts/{id}/comments`     | public         |
| PATCH  | `/api/comments/{id}`           | author         |
| DELETE | `/api/comments/{id}`           | author         |
| POST   | `/api/posts/{id}/reactions`    | active profile |
| DELETE | `/api/posts/{id}/reactions`    | active profile |
| GET    | `/api/posts/{id}/reactions`    | public         |
| POST   | `/api/comments/{id}/reactions` | active profile |
| DELETE | `/api/comments/{id}/reactions` | active profile |
| GET    | `/api/comments/{id}/reactions` | public         |
| POST   | `/api/posts/{id}/bookmark`     | active profile |
| DELETE | `/api/posts/{id}/bookmark`     | active profile |
| GET    | `/api/bookmarks`               | active profile |

The POST body for a reaction carries the reaction type. Only `like` is accepted now.

---

### 6.5 SocialGraphs

**Purpose:** follows and blocks between profiles.

**Store:** Event-sourced.

**Aggregates:** Follow, Block.

**Value objects:**

- `ProfileId` — follower, followed, blocker, or blocked.
- `BlockPair` — one handle against one handle.

#### 6.5.1 Follow

**Stream id:** `follow-{followerProfileId}-{followedProfileId}`.

**Aggregate fields:**

- `FollowerProfileId`
- `FollowedProfileId`
- `FollowedAt`
- `IsActive`

**Invariants:**

- A profile cannot follow itself.
- A profile cannot follow a profile it has blocked, or that has blocked it.
- Add only if not already following; remove only if following.
- A profile may follow a sibling profile owned by the same account.

**Events:**

- `Followed`
- `Unfollowed`

**Services:** `IBlockReader` — checks whether either side has blocked the other.

**Exceptions:**

- `SelfFollowException`
- `FollowAlreadyExistsException`
- `FollowDoesNotExistException`
- `FollowBlockedException`

#### 6.5.2 Block

**Stream id:** `block-{blockerProfileId}-{blockedProfileId}`.

**Aggregate fields:**

- `BlockerProfileId`
- `BlockedProfileId`
- `BlockedAt`
- `IsActive`

**Invariants:**

- A profile cannot block itself.
- One block per blocker–blocked pair.
- On `Blocked`, both follow streams between the two profiles are closed with `Unfollowed`.

**Events:**

- `Blocked`
- `Unblocked`

**Services:** `IFollowCloser` — closes both follow streams in the same session.

**Exceptions:**

- `SelfBlockException`
- `BlockAlreadyExistsException`
- `BlockDoesNotExistException`

**Read models:**

- Followers by followed.
- Following by follower.
- Blocks by blocker.

**Routes:**

| Method | Path                               | Who            |
| ------ | ---------------------------------- | -------------- |
| POST   | `/api/profiles/{handle}/follow`    | active profile |
| DELETE | `/api/profiles/{handle}/follow`    | active profile |
| GET    | `/api/profiles/{handle}/followers` | public         |
| GET    | `/api/profiles/{handle}/following` | public         |
| POST   | `/api/profiles/{handle}/block`     | active profile |
| DELETE | `/api/profiles/{handle}/block`     | active profile |
| GET    | `/api/settings/blocked`            | active profile |

---

### 6.6 Feeds

**Purpose:** the For You and Following timelines.

**Store:** Read-only projection.

**Aggregate:** none.

**Stream id:** none.

**Read model:** one feed entry per visible post, plus follow edges, block edges, suspensions, post reaction counts, comment reaction counts, and comment counts.

**Fields of a feed entry:**

- `PostId`
- `AuthorProfileId`
- `AuthorHandle`
- `AuthorDisplayName`
- `AuthorAvatarKey`
- `Text`
- `Images` — key, content type, byte size
- `CreatedAt`
- `PostReactionCount`
- `CommentCount`

**Invariants:** none. Feeds is derived, not authoritative.

**Services:**

- `IFeedReader` — following tab, For You tab.
- `IFeedRanker` — the For You strategy. The first implementation selects recent public posts, boosting followed profiles.

**Exceptions:** none. Read path returns empty pages.

**Projection sources:**

- `Nine.Profiles.Domain` — for handle, display name, and avatar key.
- `Nine.Content.Domain` — for post publish, edit, and delete.
- `Nine.Interactions.Domain` — for reaction counts and comment counts.
- `Nine.SocialGraphs.Domain` — for follow and block edges.
- `Nine.Identity.Domain` — for suspension flags.

No publisher references Feeds. Feeds does not read another context's tables.

**Routes:**

| Method | Path                      | Who            |
| ------ | ------------------------- | -------------- |
| GET    | `/api/feed?tab=following` | active profile |
| GET    | `/api/feed?tab=for-you`   | active profile |

---

### 6.7 Notifications

**Purpose:** in-app notifications per profile.

**Store:** Event-sourced.

**Aggregate:** Notification.

**Stream id:** `notification-{notificationId}`.

The stream id is derived from the triggering event id so a replayed event does not create a second notification.

**Aggregate fields:**

- `NotificationId`
- `RecipientProfileId`
- `Kind` — `PostReacted`, `CommentReacted`, `CommentAdded`, `Followed`
- `TargetRef` — post id, comment id, or profile id
- `CreatedAt`
- `IsRead`
- `IsDeleted`

**Value objects:**

- `NotificationId` — wraps `Guid`.
- `NotificationKind` — enum.
- `ProfileId` — recipient.
- `TargetRef` — discriminated reference.
- `ReadState` — unread or read.

**Invariants:**

- A notification belongs to exactly one profile.
- One notification per triggering event.
- Marking as read is idempotent.
- A profile does not receive a notification for its own action.

**Events:**

- `NotificationCreated`
- `NotificationMarkedAsRead`
- `NotificationDeleted`

**Services:**

- `INotificationFactory` — maps an incoming event to a `NotificationCreated`.
- `IClock`.

**Exceptions:**

- `NotificationNotFoundException`
- `NotNotificationOwnerException`

**Read models:**

- Notifications by profile, newest first, with read state.

**Routes:**

| Method | Path                           | Who            |
| ------ | ------------------------------ | -------------- |
| GET    | `/api/notifications`           | active profile |
| POST   | `/api/notifications/{id}/read` | active profile |
| DELETE | `/api/notifications/{id}`      | active profile |

---

### 6.8 Stories

**Purpose:** short-lived posts that disappear after 24 hours.

**Store:** Event-sourced.

**Aggregate:** Story.

**Stream id:** `story-{storyId}`.

**Aggregate fields:**

- `StoryId`
- `AuthorProfileId`
- `Text`
- `Images` — key, content type, byte size
- `CreatedAt`
- `ExpiresAt`
- `IsDeleted`

**Value objects:**

- `StoryId` — wraps `Guid`.
- `StoryText` — up to 280 characters. Optional.
- `ImageAttachment` — object key, content type, byte size.
- `ExpiresAt` — creation time plus 24 hours.

**Invariants:**

- At least one of text or image is present.
- A story disappears 24 hours after creation.
- A story cannot be reacted to or commented on.
- Only the author can delete.
- Stories do not appear in the main feed.

**Events:**

- `StoryCreated`
- `StoryDeleted`

**Services:**

- `IStoryExpirySweep` — appends `StoryDeleted` for expired stories whose stream is still open, then removes the projection document.
- `IImageAttachmentValidator`.
- `IClock`.

**Exceptions:**

- `EmptyStoryException`
- `StoryTextTooLongException`
- `NotStoryAuthorException`

**Read models:**

- Stories by profile, filtered to the last 24 hours.

**Routes:**

| Method | Path                | Who            |
| ------ | ------------------- | -------------- |
| POST   | `/api/stories`      | active profile |
| GET    | `/api/stories`      | active profile |
| DELETE | `/api/stories/{id}` | author         |

---

### 6.9 Moderation

**Purpose:** reports, removals, suspensions, and the administrator and moderator flags workflow.

**Store:** Event-sourced.

**Aggregate:** Report.

**Stream id:** `report-{reportId}`.

**Aggregate fields:**

- `ReportId`
- `ReporterProfileId`
- `Target` — post id, comment id, or profile id
- `Reason`
- `Note`
- `CreatedAt`
- `IsResolved`
- `IsDismissed`

**Value objects:**

- `ReportId` — wraps `Guid`.
- `ReportReason` — `Spam`, `Harassment`, `Hate`, `IllegalContent`, `Other`.
- `ReportNote` — optional, up to 500 characters.
- `ReportTarget` — post id, comment id, or profile id.
- `ReporterProfileId`.
- `ModerationAction` — remove post, remove comment, suspend, unsuspend, grant moderator, revoke moderator.

**Invariants:**

- One open report per reporter and target.
- Note is at most 500 characters.
- A moderator cannot suspend or demote an administrator.
- An administrator cannot suspend their own account.
- A report is filed by a confirmed active profile.

**Events:**

- `ReportFiled`
- `ReportResolved`
- `ReportDismissed`
- `AccountSuspended`
- `AccountUnsuspended`
- `ModeratorGranted`
- `ModeratorRevoked`

**Services:**

- `IReportQueue` — the open reports queue.
- `IModerationAudit` — records actor, action, target, reason, and time. Never records the post body or comment body.
- `IClock`.

**Exceptions:**

- `DuplicateOpenReportException`
- `ReportNoteTooLongException`
- `CannotModerateAdministratorException`
- `CannotSuspendSelfException`
- `NotModeratorException`
- `NotAdministratorException`

**Read models:**

- Open reports queue for moderators and administrators.
- Moderation audit projection: actor, action, target, reason, time. No post body or comment body.

**Cross-context actions:**

- Remove a post. Content appends a tombstone and removes the projection document. Marten event-data masking overwrites the post text.
- Remove a comment. Same pattern.
- Suspend an account. Moderation raises `AccountSuspended`. Identity handles it in the same session and appends to the Account stream.
- Unsuspend an account. Same in-session handoff.

**Routes:**

| Method | Path                                          | Who                        |
| ------ | --------------------------------------------- | -------------------------- |
| POST   | `/api/reports`                                | active profile             |
| GET    | `/api/mod/reports`                            | moderator or administrator |
| POST   | `/api/mod/posts/{id}/remove`                  | moderator or administrator |
| POST   | `/api/mod/comments/{id}/remove`               | moderator or administrator |
| POST   | `/api/mod/accounts/{handle}/suspend`          | moderator or administrator |
| POST   | `/api/mod/accounts/{handle}/unsuspend`        | moderator or administrator |
| POST   | `/api/mod/accounts/{handle}/grant-moderator`  | administrator              |
| POST   | `/api/mod/accounts/{handle}/revoke-moderator` | administrator              |

---

## 7. HTTP Contract

- .NET 10.
- Minimal APIs.
- JSON, camelCase, UTC timestamps, errors as Problem Details.
- A page is `{ items, nextCursor }`. The cursor is an opaque string. The client sends it back and does not read it.
- Time-ordered pages encode the creation time and the id.
- Alphabetical pages encode the handle.
- The page size is 20.

Writes go to `/api` on the same origin.

Repeating a follow, unfollow, block, unblock, react, unreact, bookmark, or unbookmark returns `204`. A second report of the same target by the same profile is safe to repeat.

The following return `409`:

- Following yourself
- Following while blocked
- A fourth profile
- A handle that is taken, reserved, or reserved as a path
- A rename inside 7 days
- A stream version conflict
- Reacting to a post twice without removing first
- Unreacting to a post that is not reacted
- Reacting to a comment twice without removing first
- Unreacting to a comment that is not reacted
- Bookmarking a post twice without removing first
- Unbookmarking a post that is not bookmarked

### 7.1 Search

Search belongs to Profiles.

| Method | Path                     | Who    |
| ------ | ------------------------ | ------ |
| GET    | `/api/search?q=&cursor=` | public |

### 7.2 Media

Media is served by the API but not owned by a bounded context.

| Method | Path              | Who                       |
| ------ | ----------------- | ------------------------- |
| POST   | `/api/media`      | confirmed active profile  |
| GET    | `/api/media/{id}` | public for attached media |

### 7.3 Health

`GET /health` checks PostgreSQL and returns 503 when it does not answer. The deploy script uses it. It is not a product page, and `health` is not a handle.

---

## 8. Authorization

Administrator and moderator are flags on the Account projection, not Identity roles. Each mutating action has its own policy class, run before the handler. The policy uses that context's query interface and reads those flags from the Account projection.

- If the document exists and this caller must not do it, the policy returns 403.
- If the document is missing, the policy passes and the handler returns 404, then loads the stream again to append the event.
- Public reads do not go through those policies.
- A missing or suspended post returns 404 from the read path.

A moderator cannot suspend or demote an administrator. An administrator cannot suspend their own account.

### 8.1 Profile-scoped authorization

Social writes authorize as the active profile. The active profile is stored server-side in an HttpOnly cookie. The browser sends it on calls to `/api`. The API uses it for the feed, composer, comments, reactions, bookmarks, follows, blocks, and reports. The actor is never taken from a client-supplied `profileId` in the JSON body.

---

## 9. Cross-Context Communication

Today there is no broker. Cross-context communication happens in the same Marten session and the same unit-of-work commit.

| Publisher    | Event                    | Consumer      | Action                                                                                              |
| ------------ | ------------------------ | ------------- | --------------------------------------------------------------------------------------------------- |
| Content      | `PostDeleted`            | Interactions  | Append `CommentDeleted`, `PostReactionRemoved`, `CommentReactionRemoved`, and `PostBookmarkRemoved` |
| Content      | `PostDeleted`            | Notifications | Remove pending notifications for that post                                                          |
| Content      | `PostEdited`             | Feeds         | Update the post copy                                                                                |
| Interactions | `PostReacted`            | Feeds         | Increment post reaction count                                                                       |
| Interactions | `PostReactionRemoved`    | Feeds         | Decrement post reaction count                                                                       |
| Interactions | `PostReacted`            | Notifications | Append `NotificationCreated`                                                                        |
| Interactions | `CommentAdded`           | Feeds         | Increment comment count                                                                             |
| Interactions | `CommentAdded`           | Notifications | Append `NotificationCreated`                                                                        |
| Interactions | `CommentReacted`         | Feeds         | Increment comment reaction count                                                                    |
| Interactions | `CommentReactionRemoved` | Feeds         | Decrement comment reaction count                                                                    |
| Interactions | `CommentReacted`         | Notifications | Append `NotificationCreated`                                                                        |
| SocialGraphs | `Followed`               | Feeds         | Add edge, backfill posts                                                                            |
| SocialGraphs | `Unfollowed`             | Feeds         | Remove edge                                                                                         |
| SocialGraphs | `Followed`               | Notifications | Append `NotificationCreated`                                                                        |
| SocialGraphs | `Blocked`                | SocialGraphs  | Close both follow streams                                                                           |
| Moderation   | `AccountSuspended`       | Identity      | Append `AccountSuspensionFlagSet`                                                                   |
| Moderation   | `AccountUnsuspended`     | Identity      | Append `AccountSuspensionFlagCleared`                                                               |
| Profiles     | `ProfileHandleChanged`   | Feeds         | Update handle on post copies                                                                        |
| Profiles     | `ProfileUpdated`         | Feeds         | Update display name and avatar on post copies                                                       |

A consumer references the publisher's Domain for event types. A publisher never references a consumer.

### 9.1 Later extraction

When a broker is introduced, these in-session handoffs become integration events through an outbox. The consuming context gets its own handler driven by the broker. The unit of work stays the same for the publisher.

---

## 10. Tests

`Nine.Api.Tests` is the HTTP suite. A `WebApplicationFactory` points at one PostgreSQL container per test run, started with Testcontainers, with Marten. Each test starts from an empty database. The test host does not seed the administrator account from the production configuration and does not serve Scalar.

Those HTTP tests are the suite that decides a change is done. The Domain test project of each context holds aggregate and value-object tests. Feeds has no unit-test project until it has a pure rule. The deploy gate has no BDD framework and no browser suite.

GitHub Actions runs the API integration tests and the Next.js build on every pull request and on `main`. The deploy job runs only after that succeeds on `main`.

---

## 11. Left for Later

- Broker and outbox for cross-context integration events.
- OIDC/BFF extraction. Identity becomes its own host, BFF/gateway issues JWT internally, resource APIs validate via JWKS.
- Additional read stores (Redis, Cassandra, MongoDB, Elasticsearch) if scale requires.
- Handle reservation saga, if handle uniqueness races become a problem.
- ModerationCase and Appeal, if moderation workflow grows beyond reports and direct actions.
- EventStoreDB adapter, if Marten is swapped for the social streams.
- For You feed ranking algorithm.
- Additional reaction types beyond like for posts and comments.
- Reaction change event, if replace-in-one-step is preferred over remove-then-add.
- Bookmark collections or folders, if the flat list is not enough.
