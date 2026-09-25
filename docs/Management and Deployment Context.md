# Nine — Management and Deployment Context

> Operations-only document. It describes how Nine is run, deployed, and managed.  
> It references the Product, Backend, and Frontend contexts. It does not redefine them.

---

## 1. Environment Overview

### 1.1 Production

One VM. Every app and every piece of infrastructure runs on that VM.

Compose runs:

| Service        | Role                                                                                 |
| -------------- | ------------------------------------------------------------------------------------ |
| Caddy          | Public origin. Terminates TLS. Routes `/api` to .NET and everything else to Next.js. |
| Next.js server | The React server. Standalone output.                                                 |
| .NET API       | `Nine.Api`. Composition root, Marten session, unit of work.                          |
| MinIO          | Media bucket `nine-media`.                                                           |
| PostgreSQL 18  | The one database. Identity tables via EF Core. Marten event store and projections.   |
| Log store      | Structured JSON logs written to disk on the VM. Retained 30 days.                    |

- Database, object store, log store, and every application process live on the same VM.
- There is no separate event-store process.
- There is no message broker.
- There is no managed database, no managed object store, no managed log service.
- MinIO is not published outside the VM.
- PostgreSQL is not published outside the VM.
- Only Caddy publishes ports to the internet.

### 1.2 Local

- Compose runs PostgreSQL.
- `dotnet watch` runs on the host.
- `next dev` runs on the host.
- Next proxies `/api` to the API.
- Local development stays on HTTP.
- Local MinIO is optional. A local bucket can stand in.

### 1.3 Test

- One PostgreSQL container per test run, started with Testcontainers.
- The test host does not seed the administrator account from production configuration.
- The test host does not serve Scalar.
- Each test starts from an empty database.

### 1.4 No staging

- There is no staging VM.
- There is no staging environment.
- Tests against a disposable PostgreSQL are the migration rehearsal.
- Production has no demo accounts.
- Tests create their own rows.

---

## 2. Repository

- One Git repository.
- `Nine.sln` is the .NET solution.
- `frontend/` holds the Next.js app.
- The git remote is GitHub.
- Secrets never live in git.
- Secrets live in an env file on the server.

### 2.1 Layout

```text
/
  Nine.sln
  Sources/
    Nine.Shared.*
    Nine.Identity.*
    Nine.Profiles.*
    Nine.Content.*
    Nine.Interactions.*
    Nine.SocialGraphs.*
    Nine.Feeds.*
    Nine.Notifications.*
    Nine.Stories.*
    Nine.Moderation.*
    Nine.Api/
    Nine.Api.Tests/
    ...
  frontend/
    app/
    components/
    lib/
    messages/
    ...
  deploy/
    compose.yaml
    Caddyfile
    .env.example
    scripts/
```

---

## 3. Containers

### 3.1 Images

| Image           | Source                       | Notes                                                        |
| --------------- | ---------------------------- | ------------------------------------------------------------ |
| `nine-api`      | `Sources/Nine.Api`           | .NET 10 runtime image. Built with the release configuration. |
| `nine-frontend` | `frontend/`                  | Next.js standalone output.                                   |
| `caddy`         | Upstream image               | Config mounted from `deploy/Caddyfile`.                      |
| `postgres`      | Upstream image, pinned to 18 | Data on a named volume.                                      |
| `minio`         | Upstream image               | Data on a named volume. Bucket `nine-media`.                 |

Images are built by the operator and loaded onto the VM, or built on the VM itself.

### 3.2 Compose

- One `compose.yaml` in `deploy/`.
- Named volumes for PostgreSQL and MinIO data.
- Named volume or host directory for logs.
- The `.NET` API and Next.js server share a private network.
- Only Caddy publishes ports to the host.
- MinIO, PostgreSQL, and the log store are reachable only on the private network or the VM's filesystem.

### 3.3 Environment

Secrets and configuration come from an env file on the server, not from git.

Expected keys:

- `POSTGRES_USER` — `nine`
- `POSTGRES_PASSWORD`
- `POSTGRES_DB` — `nine`
- `POSTGRES_HOST`
- `POSTGRES_PORT`
- `MARTEN_SCHEMA_PREFIX`
- `MINIO_ROOT_USER`
- `MINIO_ROOT_PASSWORD`
- `MINIO_ENDPOINT`
- `MINIO_BUCKET` — `nine-media`
- `POSTMARK_SERVER_TOKEN`
- `POSTMARK_SENDER`
- `ADMIN_EMAIL`
- `ADMIN_PASSWORD`
- `ADMIN_DATE_OF_BIRTH`
- `PUBLIC_ORIGIN`
- `PUBLIC_NAME` — `Nine`
- `LOG_LEVEL`
- `LOG_DIR`

An `.env.example` in the repo lists the keys with placeholder values. The real file is never committed.

---

## 4. Public Origin, TLS, DNS

- The operator registers a domain and points it at the VM.
- The domain string is configuration. It is not chosen yet.
- Caddy obtains a Let's Encrypt certificate.
- Caddy redirects HTTP to HTTPS.
- The public origin is configuration.
- The Next.js server does not publish ports to the host.
- Caddy is the only service that speaks to the internet.

### 4.1 Routing

| Path            | Target             |
| --------------- | ------------------ |
| `/api/*`        | The .NET API       |
| `/health`       | The .NET API       |
| Everything else | The Next.js server |

Server-rendered reads forward the session cookie to .NET over the private Docker network.

---

## 5. Migrations and Schema

- The deploy runs EF Core migrations for the Identity tables.
- Then it runs Marten's schema changes.
- Both run before the API serves traffic.
- Migration failures abort the deploy.
- There is no separate migration service. The API image carries both.
- A migration that cannot run forward is a rollback to the previous image and the previous schema state.

---

## 6. Deployment

Deployment is manual. The operator runs it from the VM or over SSH.

### 6.1 Steps

1. Pull the new commit on the VM, or copy the built images to the VM.
2. Build the API image and the Next.js image, if they are not prebuilt.
3. Run EF Core migrations against the production database.
4. Run Marten's schema changes.
5. Recreate the containers with Compose.
6. Probe `GET /health`.
7. If the probe fails, stop and roll back.

### 6.2 SSH

- Each operator has their own SSH key.
- Password login is off.
- SSH is used by the operator for deploys and maintenance.
- No shared password.

### 6.3 Tagging

- Each image is tagged with the commit SHA.
- The previous tag is kept on the VM for rollback.
- The latest tag is a convenience alias, not the source of truth.

### 6.4 Rollback

- Rollback means pointing Compose at the previous tags and recreating.
- A forward-only migration that cannot be undone is documented in the pull request.
- There is no automated rollback. The operator decides.
- If the previous API version cannot tolerate the new schema, a restore is required. There is no backup today.

---

## 7. Secrets

- Secrets live in an env file on the server, not in git.
- The env file is readable only by the deploy user.
- Rotating a secret means editing the env file and recreating the affected container.
- The Postmark server token, MinIO root credentials, PostgreSQL password, and admin bootstrap password are the rotating set.

---

## 8. Administrator Bootstrap

- Configuration supplies the administrator email, password, and date of birth.
- On startup, if that email does not exist, the API creates the Identity user through EF Core and appends the event that sets the administrator flag.
- The account is created with email already confirmed, locale English, and no profiles.
- If the account already exists and the projection does not show the flag, startup appends that event.
- The configured password is used for the first creation only. It is not written again on later deploys.
- The administrator changes the password through settings.
- There is no screen for creating a second administrator.

The bootstrap password is in the env file. Once the administrator has changed their password through the product, the env value is unused. Removing it from the env file is safe.

---

## 9. Backups

**Status: not yet designed.**

Public registration stays off until an offsite backup exists.

- The running database is the only copy today.
- A lost disk loses accounts and posts.
- The MinIO bucket is also on the VM.
- The log store is also on the VM.
- The seeded administrator is on the VM.
- The backup design is not chosen yet.
- The privacy page says the running database is the only copy, and that a lost disk loses accounts and posts.
- The privacy page does not describe a backup window.

A backup design must cover:

- PostgreSQL data.
- The MinIO bucket.
- A restore rehearsal.
- An offsite copy.
- A retention policy.

Until then, only the seeded administrator exists in production.

---

## 10. Data Retention and Deletion

### 10.1 Logs

- The API and the Next.js server write structured JSON logs to the VM.
- Logs are kept for 30 days.
- After 30 days they are discarded.
- No log shipping today.
- The log store is on the same VM as everything else.

### 10.2 Media

- An upload requires a confirmed active profile.
- An unattached image is deleted 24 hours after upload.
- A post delete removes the post's objects before the response returns.
- An avatar replace deletes the previous object.
- A profile delete deletes its posts' objects and its avatar.
- An account delete deletes every remaining profile's objects.

### 10.3 Event-data masking

- Delete appends a tombstone.
- Projection documents are removed.
- Marten event-data masking overwrites post text, comment text, display name, bio, and email-bearing fields.
- Masking does not overwrite handle strings.
- Masking is safe to run again.
- The response is not sent until the objects are gone and masking has finished.

---

## 11. Monitoring and Health

### 11.1 Health

- `GET /health` checks PostgreSQL and returns 503 when it does not answer.
- The deploy script uses it.
- `health` is not a handle.
- It is not a product page.

### 11.2 Logs

- Structured JSON.
- Written to the VM.
- Kept 30 days.
- No log aggregation today.
- No log shipping today.

### 11.3 Metrics

- No metrics stack today.
- No dashboards.
- No alerts beyond the manual health probe during deploy.

### 11.4 Uptime

- No external uptime check today.
- The operator notices from the product.

A metrics and alerting stack is a later addition.

---

## 12. Rate Limits and Abuse Controls

- Signup: 5 accounts per IP per hour.
- Profile: 30 posts per hour.
- Profile: 50 follows per hour.
- Profile: 100 post reactions per hour.
- Profile: 100 comment reactions per hour.
- Profile: 100 bookmarks per hour.
- Profile: 50 comments per hour.

The limits live in the one API process and reset when that process restarts.

- No distributed rate limiter today.
- No WAF today.
- Caddy does not rate limit.

---

## 13. Operational Runbooks

### 13.1 Deploy

1. Pull the new commit on the VM or copy the images.
2. Build the images, if needed.
3. Apply EF Core migrations.
4. Apply Marten schema changes.
5. Recreate the containers.
6. `GET /health` returns 200.

### 13.2 Rollback

1. Find the previous image tags.
2. Point Compose at them.
3. Recreate the containers.
4. If a migration ran forward, verify the previous API version tolerates the schema.
5. If not, restore from the last backup. There is no backup today.

### 13.3 Rotate a secret

1. Edit the env file on the server.
2. Recreate the affected container.
3. Verify `GET /health`.
4. For Postmark, verify a test mail.
5. For MinIO, verify upload and fetch.

### 13.4 Grant moderator

The administrator does this from `/mod` by handle.

### 13.5 Suspend an account

A moderator or administrator does this from `/mod` with a reason.

### 13.6 Restore the database

There is no backup today. This runbook is a placeholder.

### 13.7 Domain cutover

1. Register a domain.
2. Point it at the VM.
3. Update `PUBLIC_ORIGIN` in the env file.
4. Recreate Caddy and the API.
5. Verify the certificate was obtained.
6. Verify signup, login, and the composer.

---

## 14. Launch Checklist

Before public registration is turned on:

- A backup exists, offsite, with a tested restore.
- Terms and Privacy exist before the first public signup.
- The domain is chosen and pointing at the VM.
- TLS is working.
- The administrator account is seeded.
- The administrator has changed the bootstrap password.
- `GET /health` is green.
- The manual deploy has run once.
- The rate limits are in place.
- Postmark is sending confirmation and reset mail.

Until the backup exists, public registration stays off. The seeded administrator is the only account.

---

## 15. Cost and Capacity

- One VM.
- One PostgreSQL database.
- One MinIO bucket.
- One log store.
- One Caddy.
- One Next.js server.
- One .NET API.

The first year target is a few thousand accounts in one region. The single VM is sized for that.

Growth beyond the single VM is a later concern. The bounded contexts are designed so a context can be extracted to its own host when that day comes.

---

## 16. Left for Later

- Backups. Offsite copy, retention, restore rehearsal.
- The domain name.
- The legal prose for Terms and Privacy.
- A metrics and alerting stack.
- Log shipping and search.
- An external uptime check.
- A distributed rate limiter.
- A WAF.
- A staging environment.
- Automated rollback.
- Container image signing and provenance.
- Secret management beyond the server env file.
- Multi-region deployment.
- Horizontal scaling of the API.
- A second database for read models if scale requires it.
- A broker and outbox for cross-context integration events.
- OIDC/BFF extraction with Identity as its own host.
