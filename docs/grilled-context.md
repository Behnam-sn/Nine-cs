# Production design record

This file records the production design for Nine. Nothing here is implemented.

Nine is a public, free, text-only social product. An account can have up to three profiles. A profile follows profiles. The active profile has its own chronological feed.

## Product

Worldwide, English and Persian. The age floor is 16, taken from a date of birth at signup. There is no children's product and no parental account. The app can be deployed before a backup exists. Public registration stays off until one does.

No ads, no subscriptions, no video, no likes, no replies, no reposts, and no notifications. A post may include up to four images.

A few thousand accounts in the first year, one region, one PostgreSQL 18 database. Writes that are event-sourced append to a Marten stream and update their projection documents in the same commit. The next read sees that write. There is no second database, no message broker, and no background projection loop.

## Accounts

ASP.NET Core Identity holds the password hash, lockout, confirmation and reset tokens, and the cookie principal. Those tables are EF Core, in the same PostgreSQL database as Marten. The key is a `Guid`. Passwords are at least 8 characters, with no character-class rules. Five failed logins lock the account for 15 minutes.

Admin, moderator, and suspension are events on an Account stream, projected onto a table. Each authenticated request loads those flags from that projection. The cookie proves who is signed in and does not carry the flags, so a revoke or a suspension applies on the next request.

Register sets the session cookie and sends a confirmation mail through Postmark. The route exists in the build. It refuses public callers until a backup exists. The seeded admin account is created on startup either way. An unconfirmed account can log in, resend the mail, change locale, export, and delete itself. It cannot create a profile, post, follow, block, or report.

Changing the email keeps the old address as the login until the new address is confirmed through Postmark. Password reset is an emailed link. There is no separate change-password form in settings.

The session cookie lasts 14 days. It is HttpOnly and `SameSite=Lax`. It is `Secure` once the site is on HTTPS. Browser mutations send the antiforgery header.

Signup requires the date of birth and agreement to the terms.

## Profiles

A profile can be created only after the email is confirmed. An account may have zero profiles. The cap is three.

The active profile is stored in an HttpOnly cookie. The client does not read it. The browser sends it on calls to `/api`, and the API uses it for the feed, the composer, follows, blocks, and reports. Switching profiles sets the cookie. The header learns the current handle from the API, not from the cookie. One profile is selected automatically. Zero profiles shows the create-profile prompt. The follower on a follow button is the active profile. The composer posts as the active profile.

A profile has a handle, a display name, a bio, and an optional avatar.

- The handle is lowercase ASCII, `[a-z0-9_]{3,20}`, and must start with a letter.
- The display name is 1 to 50 characters and may be Persian.
- The bio is optional, up to 160 characters.
- The public URL is `/{handle}`.

A handle cannot be a reserved path: `feed`, `search`, `login`, `register`, `forgot-password`, `reset-password`, `verify-email`, `terms`, `privacy`, `posts`, `profiles`, `settings`, `mod`, `suspended`, `api`, `health`.

Rename is allowed once every 7 days. The old path returns 404, and nobody can register that handle for 30 days. Then it becomes free. The profile document keeps the list of previous handles and the time each one becomes free. Register and rename scan those lists and reject a spelling that is still inside a window. A profile delete removes that document, so its current handle and its unexpired previous handles become free. Follows stay on the profile.

Deleting a profile deletes its posts and its follows, and frees the handle. The account remains, including at zero profiles.

Deleting the account requires typing the email. It removes every remaining profile, post, follow, and block, and frees every handle.

## Feed and follows

A profile may follow a sibling profile owned by the same account. It may not follow itself.

The active profile's feed is its own posts plus the posts of profiles it follows now, newest first. Following someone puts their existing posts on the feed. Unfollow takes them off again.

Blocking is one handle against one handle. Other profiles on both accounts are unaffected.

- Both follow rows between those two handles are removed.
- Each handle's posts are left out of the other's feed.
- Neither can follow the other until the block is lifted.
- Lifting the block does not recreate the follows.
- Logged-in or logged-out, the profile URL and the post URL still open. The posts stay public.

## Posts

Plain text, up to 280 characters, plus up to four images. Newlines count toward the limit and are kept. Leading and trailing whitespace is removed. Text is optional when at least one image is attached. A post with no text and no image is rejected. Images and a video are not combined, and video is not accepted yet.

The composer is on `/feed` and on the active profile's own profile page. Other people's profiles do not have it. A second click while a create is in flight does not send another request. The composer sends an idempotency key, and the same key stores one post and returns the same id.

The active profile can delete its own post. Delete asks for confirmation. There is no editing. The delete removes the post's objects from MinIO before the response returns.

A post lives at `/posts/{id}`. The id does not change when a handle changes hands. The page shows the author's current handle and display name. A missing, removed, or suspended author's post returns 404.

## Images

MinIO runs in Compose on the same VM. The bucket is `nine-media`. Only the API holds the credentials. Caddy does not publish MinIO.

An upload requires a confirmed active profile. The browser sends one image to the API and receives a key. The API accepts JPEG, PNG, or WebP, at most 8 MB. It strips the metadata, including location, before the object is stored. A later create-post request cites up to four keys. Each key must have been uploaded by that profile and must not already belong to a post. A sweep in the API deletes an unattached object 24 hours after the upload.

`GET /api/media/{id}` streams the object when it is attached to a visible post or is the current avatar of a public profile. A missing, unattached, or suspended author's image is 404. No login is required for an image on a public post or a public profile. The response is the image, with the stored content type.

The post event and the feed copy store the key, the content type, and the byte size for each image. They do not store the pixels.

## Finding people

A search box matches the start of a handle. Results are alphabetical, 20 at a time, with a cursor. The profile URL is the exact lookup. There is no page that lists every profile, and post text is not searchable. An empty feed shows the search box. An account with zero profiles sees the create-profile prompt first.

Post count, follower count, and following count are public.

`/{handle}/followers` and `/{handle}/following` are pages of 20 handles. Recent is the default and is omitted from the URL. Name is `?sort=name`. Changing the sort drops the cursor and loads the first page in that order. The control is a segmented pair of links. The feed does not get this control. Handle search stays alphabetical.

## Pages

One centered column, about 40rem wide, on a phone and on a desktop.

Times are stored in UTC. The visible text is relative. The `<time>` element carries the absolute instant, and the tooltip shows that absolute time in the viewer's locale.

The locale is `en` or `fa`. For a signed-in visitor it is the account locale. Otherwise it is the switcher cookie, then the browser language, then English. The visitor can switch it. `fa` sets `dir="rtl"` on the page. Layout uses logical CSS properties, so English stays left-to-right. Verification, reset, and email-change messages use the account locale. Post text is stored and shown as the author wrote it. There is no machine translation of posts. The URL has no `/en` or `/fa` prefix.

The header shows Nine, the handle search, the profile switcher, and an account menu. The menu holds settings, the language switch, a light/dark toggle that follows the system until changed, and logout. A moderator also sees the queue. Signed-out visitors see Log in and Register. Nine links to `/feed` when there is an active profile, and to `/` otherwise.

A profile with no avatar shows the first character of the display name. A profile has at most one avatar. Setting a new one replaces the old, and the previous object is deleted. The same upload endpoint used for post images accepts the file. A confirmed active profile then sets the avatar by citing one unused key it uploaded. The profile event stores the key, the content type, and the byte size. JPEG, PNG, or WebP, at most 2 MB, metadata stripped. The server does not crop or resize. The UI draws the image in a circle. Deleting the profile or the account deletes the object before the response returns. `GET /api/media/{id}` serves it when the profile is public. A missing or suspended profile's avatar is 404. No login is required.

| Path | Who | What |
| --- | --- | --- |
| `/`, `/login`, `/register`, `/forgot-password`, `/reset-password` | public | Auth |
| `/verify-email` | public link | Confirms the address |
| `/terms`, `/privacy` | public | Counsel's text |
| `/feed` | active profile | Client route. First page from the API. Load more calls the feed API |
| `/search` | public | Prefix match on handles. Client route. The first HTML does not include the handles |
| `/{handle}` | public | Profile, counts, posts. Server-rendered on first visit. 404 when missing or suspended |
| `/{handle}/followers`, `/{handle}/following` | public | Handle lists, 20 per page, with the sort control. Client routes. The first HTML does not include the handles |
| `/posts/{id}` | public | Post. Server-rendered on first visit |
| `/profiles/new` | confirmed account under the cap | Create a profile |
| `/settings/account` | signed in | Locale, change email, export, delete account. Client route |
| `/settings/profile` | active profile | Display name, bio, rename, delete profile. Client route |
| `/settings/blocked` | active profile | Handles this profile has blocked, and Unblock. Client route |
| `/mod` | moderator or admin | Open reports. Remove a post, suspend, unsuspend. Admins also grant and revoke moderator |
| `/suspended` | suspended account | The screen they see instead of the product |

The host assembles a page that needs more than one context by calling those contexts' query interfaces. A profile page is Profiles, plus follower and following counts from SocialGraphs, plus that profile's posts from Content. A post page is Content, plus the current handle and display name from Profiles, plus a suspension check in Identity. The feed page is one call to Feeds.

## Safety

A confirmed active profile can report a post or a profile. Reasons are spam, harassment, hate, illegal content, and other. A report may include a note of up to 500 characters. The reporter is the active profile. One open report per reporter and target.

Moderators and admins work in `/mod`.

- Remove a post. The unit of work appends a tombstone and removes the projection document. Before the response, Marten event-data masking overwrites the post text on that stream. Masking is safe to run again. The audit projection keeps the actor, action, target, reason, and time, without the post body.
- Suspend an account, with a reason. The account's profiles disappear from feeds, search, lists, and public URLs. Its posts return 404. Moderation raises an event it owns. Identity handles that event in the same session, before `SaveChanges`, and appends to the Account stream. There is no broker.
- Unsuspend an account, with a reason. The posts come back, because a suspension hides them rather than deleting them. The same in-session handoff appends the unsuspend event to the Account stream.

A suspended account can still export and delete. It cannot post, follow, rename, or create a profile.

A moderator cannot suspend or demote an admin. An admin cannot suspend their own account.

Signup is limited to 5 accounts per IP per hour. A profile may create 30 posts and 50 follows per hour. The limits live in the one API process and reset when that process restarts.

## Authorization

Admin and moderator are flags on the Account projection, not Identity roles. Each mutating action has its own policy class, run before the handler. The policy uses that context's query interface and reads those flags from the Account projection. If the document exists and this caller must not do it, the policy returns 403. If the document is missing, the policy passes and the handler returns 404, then loads the stream again to append the event. Public reads do not go through those policies. A missing or suspended post returns 404 from the read path.

## Admin

Two flags: admin and moderator.

Configuration supplies the admin email, password, and date of birth. On startup, if that email does not exist, the API creates the Identity user through EF Core and appends the event that sets the admin flag: email already confirmed, locale English, no profiles. If the account already exists and the projection does not show the flag, startup appends that event. The configured password is used for the first creation and is not written again on later deploys. The admin changes it with the same reset mail as everyone else.

From `/mod`, an admin grants or revokes moderator on an existing confirmed account, by handle. That person has already registered and chosen their own password. Moderators cannot change the admin or moderator flags. There is no screen for creating a second admin.

## Export and policy

The signed-in user can download a JSON file of the account (email, locale, date of birth), every profile (handle, display name, bio), every post those profiles still have, every follow those profiles still have, and every block those profiles have made. The file is built from the current projection documents.

`/terms` and `/privacy` exist before the first public signup. Counsel writes the prose. The privacy page says the running database is the only copy, and that a lost disk loses accounts and posts. Delete appends a tombstone, removes the projection documents, deletes the post's objects from MinIO, and then runs Marten's event-data masking on the affected streams so post text, display name, bio, and email-bearing fields are overwritten. The response is not sent until the objects are gone and that masking has finished. Handle strings are not masked. A tombstone remains, recording that the thing existed and was deleted, without that payload. The page does not describe a backup window.

## Solution

One Git repository. `Nine.sln` is the .NET solution. The Next.js app lives in `frontend/`.

The API is a modular monolith. Six bounded contexts exist. Interactions, Notifications, and Stories are not created until one of them has a stream. Each context is four projects. `Nine.Api` references the Presentation and Infrastructure projects, maps the routes, opens one Marten session per request, and commits the unit of work. It does not contain product rules. The first milestone is this solution, the Marten session, and an empty route per context. A post comes after that. The result is still one deployable.

| Project | Role |
| --- | --- |
| `Nine.Identity.Domain` | Account aggregate, account events, command and query interfaces. `IEmailSender` lives here |
| `Nine.Identity.Application` | Identity use cases |
| `Nine.Identity.Presentation` | Identity HTTP routes |
| `Nine.Identity.Infrastructure` | EF Core Identity tables, Marten account stream, flag projection, Postmark |
| `Nine.Profiles.Domain` | Profile aggregate, events, command and query interfaces |
| `Nine.Profiles.Application` | Profile use cases |
| `Nine.Profiles.Presentation` | Profile HTTP routes |
| `Nine.Profiles.Infrastructure` | Marten profile stream and projections |
| `Nine.Content.Domain` | Post aggregate, events, command and query interfaces |
| `Nine.Content.Application` | Post use cases |
| `Nine.Content.Presentation` | Post HTTP routes |
| `Nine.Content.Infrastructure` | Marten post stream and projections |
| `Nine.SocialGraphs.Domain` | Follow and Block aggregates, events, command and query interfaces |
| `Nine.SocialGraphs.Application` | Follow and block use cases |
| `Nine.SocialGraphs.Presentation` | Follow, block, and list routes |
| `Nine.SocialGraphs.Infrastructure` | Marten follow and block streams and projections |
| `Nine.Feeds.Domain` | Feed read model and `IFeedReader`. No aggregate and no `Decide` |
| `Nine.Feeds.Application` | Feed queries |
| `Nine.Feeds.Presentation` | Feed HTTP routes |
| `Nine.Feeds.Infrastructure` | Inline feed projection and the Marten query |
| `Nine.Moderation.Domain` | Report aggregate, events, command and query interfaces |
| `Nine.Moderation.Application` | Report and moderator use cases |
| `Nine.Moderation.Presentation` | Report and `/mod` routes |
| `Nine.Moderation.Infrastructure` | Marten report stream, queue, and audit projection |
| `Nine.Shared.Domain` | `IClock` only |
| `Nine.Shared.Application` | Empty until two contexts need an application type |
| `Nine.Shared.Presentation` | Problem Details mapping and the route conventions |
| `Nine.Shared.Infrastructure` | `SystemClock` |
| `Nine.Api` | Composition root, authentication middleware, session, unit of work |
| `Nine.Api.Tests` | HTTP integration tests through the composed host |
| `Nine.Identity.Domain.Tests` | `Decide` and `Apply` for Account |
| `Nine.Profiles.Domain.Tests` | `Decide` and `Apply` for Profile |
| `Nine.Content.Domain.Tests` | `Decide` and `Apply` for Post |
| `Nine.SocialGraphs.Domain.Tests` | `Decide` and `Apply` for Follow and Block |
| `Nine.Moderation.Domain.Tests` | `Decide` and `Apply` for Report |

A type moves into `Nine.Shared.*` only when two contexts already need it. Events never move there. Feature Domain, Application, and Presentation projects do not reference `Nine.Shared.Infrastructure`.

## Domain model

An aggregate does not reference Marten. `Decide` checks the invariant and returns the new events. `Apply` folds one event into the in-memory state and does not check rules. The session loads the past events, folds them with `Apply`, calls `Decide`, and appends the result.

| Aggregate | Stream id | Context |
| --- | --- | --- |
| Account | account id | Identity |
| Profile | profile id | Profiles |
| Post | post id | Content |
| Follow | follower profile + followed profile | SocialGraphs |
| Block | blocker profile + blocked profile | SocialGraphs |
| Report | report id | Moderation |

Feeds has no stream.

Command and query interfaces for an aggregate sit in that context's Domain, beside the aggregate. The feed query interface sits in `Nine.Feeds.Domain` beside the read model. Application and Presentation take those interfaces. Infrastructure implements them with Marten.

## Request and commit

`Nine.Api` opens one Marten session for the request and one unit of work around it. Command repositories only load a stream and append events. Query repositories only read projection documents. Repositories do not call `SaveChanges`. The unit of work calls it once, at the end of the request. That commit writes the new events and runs the inline projections, including the feed.

Every append carries the version the aggregate was loaded at. A mismatch aborts the commit and the API returns 409. The client retries against the new version. The winner's commit, including its inline projections, stands.

`Feeds.Infrastructure` registers that projection on the session. It references `Nine.Identity.Domain`, `Nine.Profiles.Domain`, `Nine.Content.Domain`, and `Nine.SocialGraphs.Domain` for their event types. No publisher references Feeds. Feeds does not read another context's tables.

`Nine.Identity.Application` references `Nine.Moderation.Domain` so it can handle the suspension and unsuspension events Moderation raises. Moderation does not reference Identity.

The projection stores, inside Feeds, a copy of each post (id, author, handle, display name, avatar key, text, time, and each image key, content type, and byte size), each follow edge, each block edge, and each suspension. An avatar change updates the avatar key on the copies of that author's posts. The feed query uses only those copies. A `PostPublished` event stores the post. A `Followed` event adds an edge, and posts already stored become visible. An `Unfollowed` event removes the edge, and those posts drop off. The follow transaction writes one edge. It does not copy the author's history.

A delete appends a tombstone and removes the projection documents in the unit-of-work commit. After that commit, and before the response, the use case applies Marten's event-data masking to the affected streams. Masking overwrites post text, display name, bio, and email-bearing fields. It does not overwrite handle strings. Masking is safe to run again. The moderation audit projection records the action and never receives the post text.

The HTTP response is sent after that commit. Creating a post returns `201` and the new id. Deleting a post, follow, unfollow, block, unblock, and rename return `204`. The open page then refetches its read model. The change is already in the projection.

## HTTP contract

.NET 10. Minimal APIs. JSON, camelCase, UTC timestamps, errors as Problem Details. A page is `{ items, nextCursor }`. The cursor is an opaque string. The client sends it back and does not read it. Time-ordered pages encode the creation time and the id. Alphabetical pages, handle search and `?sort=name`, encode the handle. The page size is 20 for both.

Writes go to `/api` on the same origin. Repeating a follow, unfollow, block, or unblock returns 204. A second report of the same target by the same profile is safe to repeat. Following yourself, following while blocked, a fourth profile, a handle that is taken, reserved, or reserved as a path, a rename inside 7 days, and a stream version conflict return 409.

OpenAPI and Scalar are on in Development and off in production.

`GET /health` checks PostgreSQL and returns 503 when it does not answer. The deploy script uses it. It is not a product page, and `health` is not a handle.

The database and its user are `nine`. Keys are UUIDv7 `Guid`s, generated in the application. EF Core is used only for the ASP.NET Core Identity tables. Marten is the event store and the projection store. Both use this database.

## Web

Next.js App Router, Dockerized with the standalone output. The React server is the only UI. .NET serves no HTML. The React server calls .NET and does not write the database.

`app/[handle]/page.tsx` and `app/posts/[id]/page.tsx` are server components. Each fetches from .NET and renders one client component. That client component is the product UI and hydrates in full. No other product UI is a server component. `/feed`, `/search`, `/[handle]/followers`, `/[handle]/following`, `/profiles/new`, and the `/settings/*` routes are client routes in the same app. Their first HTML does not include the list. After hydration, navigation is client-side and the data comes from .NET.

Tailwind CSS v4. shadcn/ui on Base UI. `next-intl` for the `en` and `fa` catalogs. React Hook Form and Zod validate signup, login, create profile, edit profile, and the composer. The API remains the authority on those rules.

Caddy is the public origin. It sends `/api` to the .NET API and every other path to the Next.js server. Server-rendered reads forward the session cookie to .NET over the private Docker network. Mutations from the browser go to `/api`.

`/feed` loads the first 20 posts from the API. Load more fetches the next cursor and appends those posts. There is no TanStack Query.

## Tests

`Nine.Api.Tests` is the HTTP suite. A `WebApplicationFactory` points at one PostgreSQL container per test run, started with Testcontainers, with Marten. Each test starts from an empty database. The test host does not seed the admin account from the production configuration and does not serve Scalar.

Those HTTP tests are the suite that decides a change is done. `Decide` and `Apply` get unit tests in the Domain test project of Identity, Profiles, Content, SocialGraphs, and Moderation. Feeds has no unit-test project until it has a pure rule. The deploy gate has no BDD framework and no browser suite.

GitHub Actions runs the API integration tests and the Next.js build on every pull request and on `main`. The deploy job runs only after that succeeds on `main`.

## Runtime

One VM in Hetzner Falkenstein (`fsn1`). No staging VM.

Compose runs Caddy, the Next.js server, the .NET API, and MinIO. PostgreSQL 18 runs as the database. Marten uses that database. There is no separate event-store process. MinIO is not published outside the VM.

The git remote is GitHub. GitHub Actions, on `main`, builds the images, pushes them to the GitHub container registry, and connects over SSH to pull and recreate the containers. The deploy applies EF Core migrations for the Identity tables, then Marten's schema changes, before the API serves. Each operator has their own SSH key. Password login is off. Secrets live in an env file on the server, not in git.

Locally, Compose runs PostgreSQL, and `dotnet watch` and `next dev` run on the host. Next proxies `/api` to the API. Local development stays on HTTP. Production has no demo accounts. Tests create their own rows. CI against a disposable PostgreSQL is the migration rehearsal.

The API and the Next.js server write structured JSON logs to the VM. The logs are kept for 30 days, then discarded.

The public name Nine is one configuration value, used by the header, the browser title, and the Postmark sender.

The operator registers a domain and points it at the VM. Caddy obtains a Let's Encrypt certificate and redirects HTTP to HTTPS. The public origin is configuration. The domain string itself is not chosen yet.

## Left for later

- Backups. Public registration stays off until an offsite copy exists. The backup design itself is not chosen yet. A lost disk still takes the only copy of whatever is already on the VM, including the seeded admin and the MinIO bucket.
- The domain name.
- The legal prose for `/terms` and `/privacy`.
- Interactions, Notifications, and Stories. Each one is added as four projects on the day it has a stream.
- Video. The upload-then-attach path is the one a video will use. No video type is accepted yet.
