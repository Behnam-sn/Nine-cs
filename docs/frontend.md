# Nine — Frontend Context

> Frontend-only document. It describes how Nine is built in the browser and on the React server.  
> It references the Product context for behavior and the Backend context for API shape. It does not redefine either.

---

## 1. Stack

| Piece         | Choice                                  |
| ------------- | --------------------------------------- |
| Framework     | Next.js App Router                      |
| Language      | TypeScript                              |
| Styling       | Tailwind CSS v4                         |
| Components    | shadcn/ui on Base UI                    |
| i18n          | `next-intl` with `en` and `fa` catalogs |
| Forms         | React Hook Form                         |
| Validation    | Zod                                     |
| Container     | Docker, Next.js standalone output       |
| Public origin | Caddy                                   |

- No TanStack Query.
- No Redux.
- No client-side access tokens.
- No `localStorage` for session or profile.
- No separate SPA. The React server is the only UI. .NET serves no HTML.

---

## 2. Rendering Model

### 2.1 Server components

Two routes are server components. Each fetches from .NET and renders one client component. That client component is the product UI and hydrates in full.

| Route         | Server component          | Client component |
| ------------- | ------------------------- | ---------------- |
| `/[handle]`   | `app/[handle]/page.tsx`   | `ProfilePage`    |
| `/posts/[id]` | `app/posts/[id]/page.tsx` | `PostPage`       |

Server components forward the session cookie and the active-profile cookie to .NET over the private Docker network. They do not write the database.

### 2.2 Client routes

Every other product route is a client route in the same app. Its first HTML does not include the list data. After hydration, navigation is client-side and the data comes from .NET.

Client routes:

- `/feed`
- `/search`
- `/[handle]/followers`
- `/[handle]/following`
- `/bookmarks`
- `/notifications`
- `/stories`
- `/profiles/new`
- `/settings/account`
- `/settings/profile`
- `/settings/blocked`
- `/mod`

### 2.3 Static and auth routes

| Route              | Rendering                             |
| ------------------ | ------------------------------------- |
| `/`                | Server-rendered shell                 |
| `/login`           | Client route                          |
| `/register`        | Client route                          |
| `/forgot-password` | Client route                          |
| `/reset-password`  | Client route                          |
| `/verify-email`    | Server-rendered shell, client form    |
| `/terms`           | Static                                |
| `/privacy`         | Static                                |
| `/suspended`       | Server-rendered shell, client message |

---

## 3. Route Map

| Path                  | Who                             | What                                                          |
| --------------------- | ------------------------------- | ------------------------------------------------------------- |
| `/`                   | public                          | Home. Links to feed when an active profile exists.            |
| `/login`              | public                          | Sign in                                                       |
| `/register`           | public                          | Sign up                                                       |
| `/forgot-password`    | public                          | Request a reset mail                                          |
| `/reset-password`     | public                          | Set a new password from the mailed link                       |
| `/verify-email`       | public link                     | Confirms the address                                          |
| `/terms`              | public                          | Terms                                                         |
| `/privacy`            | public                          | Privacy                                                       |
| `/feed`               | active profile                  | For You tab                                                   |
| `/feed?tab=following` | active profile                  | Following tab                                                 |
| `/search`             | public                          | Prefix match on handles                                       |
| `/[handle]`           | public                          | Profile, counts, posts                                        |
| `/[handle]/followers` | public                          | Handle list, sort control                                     |
| `/[handle]/following` | public                          | Handle list, sort control                                     |
| `/posts/[id]`         | public                          | Post, comments, reactions                                     |
| `/profiles/new`       | confirmed account under the cap | Create a profile                                              |
| `/bookmarks`          | active profile                  | Private bookmarks                                             |
| `/notifications`      | active profile                  | In-app notifications                                          |
| `/stories`            | active profile                  | View and create stories                                       |
| `/settings/account`   | signed in                       | Locale, change email, change password, export, delete account |
| `/settings/profile`   | active profile                  | Display name, bio, rename, delete profile                     |
| `/settings/blocked`   | active profile                  | Blocked handles, Unblock                                      |
| `/mod`                | moderator or administrator      | Open reports, remove, suspend, unsuspend, grant, revoke       |
| `/suspended`          | suspended account               | The screen instead of the product                             |

Nine links to `/feed` when there is an active profile, and to `/` otherwise.

---

## 4. Data Fetching

### 4.1 Reads

- Server components fetch from .NET directly on first visit.
- Client routes fetch from `/api` after hydration.
- Every list page is `{ items, nextCursor }`.
- The cursor is opaque. The client stores it and sends it back. It does not read it.
- Page size is 20.

### 4.2 Cursor pagination

A list is a small state machine:

- `items` — accumulated results.
- `nextCursor` — the cursor for the next page, or null.
- `isLoading` — a request is in flight.
- `error` — the last error.

Load more appends. Changing a sort or a tab drops the cursor and replaces `items` with the first page of the new order.

### 4.3 Mutations

- Mutations go to `/api` on the same origin.
- The browser sends the session cookie and the active-profile cookie.
- The browser sends the antiforgery header.
- Repeating a follow, unfollow, block, unblock, react, unreact, bookmark, or unbookmark returns `204`.
- After a mutation, the open page refetches its read model. The change is already in the projection on the server.

### 4.4 Optimistic UI

Optimistic toggles are used where the result is visually obvious and cheap to roll back:

- Post reactions
- Comment reactions
- Bookmarks
- Follows

The optimistic state is reconciled with the response. On error, the previous state is restored and an error toast is shown.

Post creation, editing, and deletion are not optimistic. The composer waits for the `201` or `204`.

### 4.5 Polling

- Notifications are fetched when the menu opens and on route change.
- There is no long-poll, WebSocket, or SSE.
- Stories are fetched on entry to `/stories`.

### 4.6 No client cache

- No TanStack Query.
- No SWR.
- No global store.
- Each page owns its own list state.
- React Context carries only the session, the active profile, the locale, and the theme.

---

## 5. Auth and Session

### 5.1 Session

- The session cookie is set by .NET on login or register.
- It is `HttpOnly`, `SameSite=Lax`, and `Secure` on HTTPS.
- The browser never reads the cookie.
- Client routes call `GET /api/auth/session` to learn whether the account is signed in and whether the email is confirmed.
- The session response is cached in React Context for the tab session.
- On `401`, the app clears the context and redirects to `/login`.

### 5.2 Active profile

- The active profile cookie is set by .NET.
- The browser never reads the cookie.
- The header learns the current handle from the API.
- Switching profiles calls `POST /api/profiles/switch` and then refetches the session context.
- Zero profiles shows the create-profile prompt.
- The composer, follow buttons, reactions, bookmarks, comments, and reports act as the active profile.

### 5.3 Suspension

- A suspended account sees `/suspended` instead of the product.
- The screen offers export and delete.
- A signed-in suspended account cannot reach the product routes.

---

## 6. Layout

One centered column, about 40rem wide, on a phone and on a desktop.

### 6.1 Header

- Nine wordmark.
- Handle search box.
- Profile switcher.
- Account menu.
- A moderator or administrator also sees a link to `/mod`.

Signed-out visitors see Log in and Register.

### 6.2 Account menu

- Settings
- Notifications
- Bookmarks
- Language switch
- Light/dark toggle
- Logout

The light/dark toggle follows the system until the user changes it. The choice is stored in a cookie.

---

## 7. Internationalization

### 7.1 Locale

- `en` or `fa`.
- For a signed-in visitor, the account locale.
- Otherwise, the switcher cookie, then the browser language, then English.
- The visitor can switch it from the account menu.
- The URL has no `/en` or `/fa` prefix.

### 7.2 RTL

- `fa` sets `dir="rtl"` on the page.
- Layout uses logical CSS properties.
- English stays left-to-right.

### 7.3 Catalogs

- `next-intl` catalogs for `en` and `fa`.
- Server and client components share the same catalog.
- Verification, reset, and email-change messages use the account locale.

### 7.4 Post text

- Post text is stored and shown as the author wrote it.
- There is no machine translation of posts.

### 7.5 Time

- Times are stored in UTC.
- Visible text is relative.
- The `<time>` element carries the absolute instant.
- The tooltip shows the absolute time in the viewer's locale.

---

## 8. Theming

- Tailwind CSS v4.
- shadcn/ui on Base UI.
- Light and dark themes.
- The system preference is the default.
- The user's choice is stored in a cookie.
- The server reads the cookie and sets the initial theme class so there is no flash.

---

## 9. Forms and Validation

React Hook Form and Zod validate on the client. The API remains the authority.

Forms:

- Signup
- Login
- Forgot password
- Reset password
- Change email
- Change password
- Create profile
- Edit profile
- Rename profile
- Composer (post, story)
- Comment composer
- Report
- Moderation actions

Zod schemas mirror the backend rules: password length, handle pattern, display name length, bio length, post text length, comment text length, report note length, image count, image size.

Server `409` and `403` responses map to field errors or a form-level error.

---

## 10. Components

### 10.1 Layout

- `AppShell`
- `Header`
- `AccountMenu`
- `ProfileSwitcher`
- `LocaleSwitcher`
- `ThemeToggle`

### 10.2 Feed and posts

- `Feed`
- `FeedTabs`
- `PostCard`
- `PostList`
- `PostComposer`
- `PostActions`
- `PostReactionButton`
- `BookmarkButton`
- `PostEditor`
- `DeletePostDialog`

### 10.3 Comments

- `CommentList`
- `CommentItem`
- `CommentComposer`
- `CommentReactionButton`
- `CommentEditor`
- `DeleteCommentDialog`

### 10.4 Profiles

- `ProfileHeader`
- `ProfileStats`
- `FollowButton`
- `BlockButton`
- `HandleList`
- `HandleSearch`
- `Avatar`
- `AvatarUploader`
- `ProfileEditor`
- `CreateProfileForm`

### 10.5 Stories

- `StoryStrip`
- `StoryViewer`
- `StoryComposer`

### 10.6 Notifications

- `NotificationList`
- `NotificationItem`
- `NotificationBadge`

### 10.7 Moderation

- `ReportDialog`
- `ReportQueue`
- `ModerationActionBar`
- `SuspendDialog`
- `GrantModeratorDialog`

### 10.8 Shared

- `CursorList` — generic `{ items, nextCursor }` list with Load more.
- `EmptyState`
- `ErrorState`
- `Toast`
- `Dialog`
- `ConfirmDialog`
- `Pagination` — cursor-based, not page numbers.

---

## 11. API Client

- One typed fetch wrapper in `frontend/lib/api`.
- Reads the origin from configuration.
- Sends cookies by default.
- Sends the antiforgery header on mutations.
- Throws a typed `ApiError` carrying the Problem Details payload.
- Maps `401` to a session reset.
- Maps `403` to a permission error.
- Maps `404` to a not-found state.
- Maps `409` to a conflict error with the server's message.
- Maps `503` to a retryable error.

Types are hand-written for now. Generated types from the OpenAPI document are a later option.

---

## 12. Media

- The composer uploads one image at a time to `POST /api/media`.
- The response carries a key.
- The composer cites up to four keys when creating a post.
- The avatar uploader uses the same endpoint.
- A preview is shown before the post is published.
- The server strips metadata; the client does not.
- The client does not resize, crop, or rotate.
- `GET /api/media/{id}` is used directly in `<img>` for attached media.
- Avatars are drawn in a circle by CSS.

---

## 13. Build and Deploy Integration

- Next.js is built with the standalone output.
- Caddy is the public origin.
- Caddy sends `/api` to .NET and every other path to the Next.js server.
- Server-rendered reads forward the session cookie to .NET over the private Docker network.
- Mutations from the browser go to `/api`.
- The public name Nine is one configuration value used by the header and the browser title.
- The Next.js server writes structured JSON logs to the VM. The logs are kept for 30 days, then discarded.
- The Next.js server does not write the database.

---

## 14. Tests

- The Next.js build runs in CI on every pull request and on `main`.
- The deploy job runs only after the build and the API integration tests succeed on `main`.
- There is no browser suite and no BDD framework.
- Component tests are added when a component carries a pure rule.
- Zod schemas are tested beside the form.

---

## 15. Left for Later

- Generated API types from the OpenAPI document.
- A component test runner.
- A browser suite for the auth flow and the composer.
- Service worker or offline support.
- Push notifications. The product has in-app notifications only.
- Story viewer animations.
- Bookmark collections or folders.
- Additional reaction types beyond like.
- Video in the composer.
- The For You ranking preview in the UI.
