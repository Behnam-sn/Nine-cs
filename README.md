# Nine

A social platform. **.NET** modular monolith, **React** SPA.

One account, many profiles. You sign in as yourself; you post, follow, and react as a profile. Identities speaks **OIDC**. Everything else is a resource API that trusts a JWT. The social domain is **event-sourced**. Auth is not.

Hobby project. MIT / Apache stack only — no commercial identity products.

```text
React  --cookie-->  BFF  --OIDC-->  Identities (Identity + OpenIddict)
                       --JWT-->   Profiles · Contents · Graphs · Feeds · …
```

When a module grows up and moves out, the browser still logs in the same way. Other services still validate the same tokens. That is the whole point.

| | Social domain | Identities |
|---|---|---|
| Persistence | Marten event streams on PostgreSQL | ASP.NET Identity + OpenIddict (EF) |
| Truth | the event log | the current user row |
| Talks to others via | integration events | JWT + integration events |

**Contexts:** Identities · Profiles · Contents · Interactions · SocialGraphs · Feeds · Notifications · Moderation

Architecture, auth, streams, and extraction rules: **[context.md](context.md)**.
