# Notes

## Bounded Contexts

| Bounded Context                    | Aggregate Roots                          | Notes                                                                                                            |
|------------------------------------|------------------------------------------|------------------------------------------------------------------------------------------------------------------|
| **Identities**                     | `User`, `Credential`, `Profile`          | Merges Authentication + Identity + Profiles. Manages account lifecycle, authentication, and public profile info. |
| **Contents**                       | `Post`                                   | Manages posts and media. Enforces visibility rules.                                                              |
| **Interactions**                   | `Reaction`, `Comment`, `Bookmark`        | Handles likes, comments, and bookmarks. High-throughput, small aggregates.                                       |
| **SocialGraphs**                   | `FollowRelationship`, `Block`            | Manages follow and block relationships. No mega-aggregate for the full social graph.                             |
| **Feeds**                          | *(none)*                                 | Projection-only. Builds timelines and rankings from events.                                                      |
| **Notifications**                  | `Notification`, `NotificationPreference` | Manages user notifications and preference settings.                                                              |
| **Moderation**  (optional / later) | `Report`, `ModerationCase`, `Appeal`     | Human-driven workflow for reporting, reviewing, and taking moderation actions.                                   |

---
