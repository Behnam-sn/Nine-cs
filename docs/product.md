# Nine — Product Context

> Product-only document. It describes what Nine is and how it behaves for users.  
> Backend, Frontend, and Deployment documents should reference these rules, not redefine them.

---

## 1. Summary

Nine is a public, free social platform.

- An account can have up to three profiles.
- A profile follows profiles.
- The active profile has its own chronological feed.
- A post is plain text, up to 280 characters, and may include up to four images.
- Posts can be edited, reacted to, and commented on.
- Comments can be edited and reacted to.
- Profiles can bookmark posts.
- The product has notifications.
- The product has stories.

---

## 2. Product Characteristics

- Worldwide, English and Persian.
- Public registration.
- Age floor: 16, taken from a date of birth at signup.
- No children’s product and no parental account.
- An account can have up to three profiles. It may have zero.
- A profile follows profiles.
- The active profile has its own chronological feed.
- A post may include up to four images.
- A post may be text-only, image-only, or text plus images.
- Posts can be edited by their author.
- Posts can be reacted to by profiles.
- Profiles can comment on posts.
- Comments can be edited by their author.
- Comments can be reacted to by profiles.
- Profiles can bookmark posts.
- The product has notifications.
- The product has stories.
- No machine translation of posts.

---

## 3. Actors

| Actor             | Description                                                                                                                            |
| ----------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| Visitor           | Not signed in. Can view public profiles, posts, comments, and images.                                                                  |
| Account           | The authenticated user. Owns up to three profiles.                                                                                     |
| Profile           | The public identity that acts. Has a handle, display name, bio, and optional avatar.                                                   |
| Active profile    | The profile currently acting for the account. Used for feed, posting, commenting, reacting, bookmarking, follows, blocks, and reports. |
| Suspended profile | A profile whose account is suspended. Hidden from feeds, search, lists, and public views. Its posts return 404.                        |
| Moderator         | Can review reports, remove posts, suspend and unsuspend accounts.                                                                      |
| Administrator     | Has moderator powers and can grant or revoke moderator.                                                                                |
| Suspended account | Can still export and delete. Cannot post, follow, rename, or create a profile.                                                         |

**User is who you are. Profile is who you act as.**

---

## 4. Accounts

### 4.1 Signup

- Signup requires an email, a date of birth, and agreement to the terms.
- The age floor is 16.
- Public registration is available.
- An account can have zero profiles.
- Passwords are at least 8 characters, with no character-class rules.
- Five failed logins lock the account for 15 minutes.
- The session lasts 14 days.

### 4.2 Confirmation

Register sends a confirmation email.

An unconfirmed account can:

- Log in.
- Resend the confirmation mail.
- Change locale.
- Export its data.
- Delete itself.

An unconfirmed account cannot:

- Create a profile.
- Post.
- Comment.
- React.
- Bookmark.
- Follow.
- Block.
- Report.

### 4.3 Email and password

- Changing the email keeps the old address as the login until the new address is confirmed.
- Passwords can be changed through settings.

### 4.4 Deletion and export

- The account can export a JSON file of its data.
- Deleting the account requires typing the email.
- Deleting the account removes every remaining profile, post, comment, reaction, bookmark, follow, and block, and frees every handle.

### 4.5 Suspension

A suspended account:

- Can still export and delete.
- Cannot post, comment, react, bookmark, follow, rename, or create a profile.
- Has its profiles hidden from feeds, search, lists, and public views.
- Has its posts return 404.

A moderator cannot suspend or demote an administrator. An administrator cannot suspend their own account.

---

## 5. Profiles

### 5.1 Limits and creation

- A profile can be created only after the email is confirmed.
- An account may have zero profiles.
- The cap is three profiles.
- One profile is selected automatically when available.
- Zero profiles shows the create-profile prompt.

### 5.2 Profile fields

- Handle
- Display name
- Bio
- Optional avatar

### 5.3 Handle rules

- Lowercase ASCII.
- Pattern: `[a-z0-9_]{3,20}`.
- Must start with a letter.
- Handles must not conflict with reserved system words.

### 5.4 Display name and bio

- Display name: 1 to 50 characters. May be Persian.
- Bio: optional, up to 160 characters.

### 5.5 Rename

- Rename is allowed once every 7 days.
- The old handle returns 404.
- Nobody can register the old handle for 30 days.
- After 30 days, the old handle becomes free.
- The profile keeps the list of previous handles and when each becomes free.
- Register and rename reject a spelling that is still inside a window.

### 5.6 Avatar

- A profile has at most one avatar.
- With no avatar, the UI shows the first character of the display name.
- Setting a new avatar replaces the old one.
- JPEG, PNG, or WebP, at most 2 MB.
- Metadata is stripped.
- The server does not crop or resize.
- The UI draws the avatar in a circle.
- Deleting the profile or account deletes the avatar.

### 5.7 Profile deletion

- Deleting a profile deletes its posts, comments, reactions, bookmarks, and follows, and frees the handle.
- The account remains, including at zero profiles.

---

## 6. Active Profile

- The active profile is stored server-side and sent with API calls.
- The client does not read it directly.
- The browser uses it for the feed, composer, comments, reactions, bookmarks, follows, blocks, and reports.
- Switching profiles changes the active profile.
- The header learns the current handle from the API.
- The follower on a follow button is the active profile.
- The composer posts as the active profile.

---

## 7. Posts

### 7.1 Content rules

- Plain text, up to 280 characters.
- Plus up to four images.
- Newlines count toward the limit and are kept.
- Leading and trailing whitespace is removed.
- Text is optional when at least one image is attached.
- A post with no text and no image is rejected.
- Images and video are not combined.
- Video is not accepted yet.
- Posts can be edited by their author.

### 7.2 Composer

- A second click while a create is in flight does not send another request.
- The composer sends an idempotency key.
- The same key stores one post and returns the same id.

### 7.3 Post deletion

- The active profile can delete its own post.
- Delete asks for confirmation.
- Deleting removes the post permanently, along with its comments, reactions, and bookmarks.

### 7.4 Post editing

- The author can edit a post.
- Editing changes the text only.
- Images cannot be added or removed after posting.
- Edits are visible to readers.

### 7.5 Post URL

- A post has a stable id.
- The id does not change when a handle changes hands.
- The page shows the author’s current handle and display name.
- A missing, removed, or suspended author’s post returns 404.

### 7.6 Images

- JPEG, PNG, or WebP.
- At most 8 MB per post image.
- Metadata, including location, is stripped.
- A post may cite up to four uploaded images.
- Each image must have been uploaded by that profile and must not already belong to a post.
- An unattached image is deleted 24 hours after upload.
- No login is required for an image on a public post or public profile.

---

## 8. Comments

- A confirmed active profile can comment on a post.
- Comments are plain text, up to 280 characters.
- Comments cannot include images.
- Comments can be edited by their author.
- Comments can be deleted by their author.
- Comments are public.
- A post shows its comments in chronological order.
- Comment counts are public.

---

## 9. Post Reactions

- A confirmed active profile can react to a post.
- The only reaction type available now is a like.
- A profile can have at most one reaction per post.
- A profile can remove its reaction.
- Reaction counts are public.
- Reactions do not create a separate feed.
- Reactions trigger notifications for the post author.

Future reaction types may be added. The product will support multiple reaction types, but only likes are enabled for now.

---

## 10. Comment Reactions

- A confirmed active profile can react to a comment.
- The only reaction type available now is a like.
- A profile can have at most one reaction per comment.
- A profile can remove its reaction.
- Reaction counts are public.
- Reactions trigger notifications for the comment author.

Future reaction types may be added. The product will support multiple reaction types, but only likes are enabled for now.

---

## 11. Bookmarks

- A confirmed active profile can bookmark a post.
- Bookmarks are private to the profile that made them.
- A profile can bookmark a post at most once.
- A profile can remove a bookmark.
- Bookmarked posts are shown in a separate bookmarks area for the profile.
- Bookmarks do not trigger notifications.
- Bookmark counts are not public.

---

## 12. Stories

- Stories are short-lived posts.
- A story disappears after 24 hours.
- A story may contain text and/or images.
- Stories appear in a separate stories area.
- Profiles can view stories from profiles they follow.
- Stories do not appear in the main feed.
- Stories cannot be reacted to or commented on.
- Stories can be deleted by their author before they expire.

---

## 13. Notifications

- The product has in-app notifications.
- Notifications are generated for:
  - Post reactions on your posts.
  - Comment reactions on your comments.
  - Comments on your posts.
  - New followers.
- Notifications are delivered to the profile.
- Notifications can be marked as read.
- Notifications can be deleted.
- No email notifications are sent for these events.

---

## 14. Feed

- The feed has two tabs: For You and Following.
- The Following tab shows only the posts of profiles the active profile follows.
- The For You tab shows a mix of posts from followed profiles and other public posts.
- The Following tab is chronological, newest first.
- An empty feed shows the search box.
- An account with zero profiles sees the create-profile prompt first.

---

## 15. Follows

- A profile follows profiles.
- A profile may follow a sibling profile owned by the same account.
- A profile may not follow itself.
- Following someone puts their existing posts on the feed.
- Unfollow takes them off again.
- The active profile’s following feed is only the posts of profiles it follows.

---

## 16. Blocking

Blocking is one handle against one handle. Other profiles on both accounts are unaffected.

When a block exists:

- Both follow records between those two handles are removed.
- Each handle’s posts are left out of the other’s feed.
- Neither can follow the other until the block is lifted.
- Lifting the block does not recreate the follows.
- Logged-in or logged-out, the profile and post still open.
- The posts stay public.

---

## 17. Search and Discovery

- A search box matches the start of a handle.
- Results are alphabetical.
- 20 results at a time, with a cursor.
- The profile is the exact lookup.
- There is no page that lists every profile.
- Post text is not searchable.
- Handle search stays alphabetical.

Post count, follower count, and following count are public.

Followers and following lists are pages of 20 handles.

- Recent is the default.
- Name sorting is available.
- Changing the sort drops the cursor and loads the first page in that order.
- The control is a segmented pair of links.

---

## 18. Safety and Moderation

### 18.1 Reporting

A confirmed active profile can report a post, a comment, or a profile.

Reasons:

- Spam
- Harassment
- Hate
- Illegal content
- Other

A report may include a note of up to 500 characters.

- The reporter is the active profile.
- One open report per reporter and target.

### 18.2 Moderation actions

Moderators and administrators work in the moderation area.

They can:

- Remove a post.
- Remove a comment.
- Suspend an account, with a reason.
- Unsuspend an account, with a reason.

Administrators can also:

- Grant moderator.
- Revoke moderator.

A moderator cannot suspend or demote an administrator.  
An administrator cannot suspend their own account.

### 18.3 Suspension behavior

- The account’s profiles disappear from feeds, search, lists, and public views.
- Its posts and comments return 404.
- Unsuspending brings the posts and comments back.
- Suspension hides content rather than deleting it.

### 18.4 Rate limits

- Signup: 5 accounts per IP per hour.
- Profile: 30 posts per hour.
- Profile: 50 follows per hour.
- Profile: 100 post reactions per hour.
- Profile: 100 comment reactions per hour.
- Profile: 100 bookmarks per hour.
- Profile: 50 comments per hour.

---

## 19. Administrator

- There are two flags: administrator and moderator.
- Configuration supplies the administrator email, password, and date of birth.
- On startup, the administrator account is created if it does not exist.
- The configured password is used for the first creation and is not written again on later deploys.
- The administrator changes the password through settings.
- An administrator grants or revokes moderator on an existing confirmed account, by handle.
- There is no screen for creating a second administrator.

---

## 20. Language and Presentation

- Locale is `en` or `fa`.
- For a signed-in visitor, it is the account locale.
- Otherwise it is the switcher cookie, then the browser language, then English.
- The visitor can switch it.
- `fa` sets `dir="rtl"` on the page.
- Layout uses logical CSS properties, so English stays left-to-right.
- Verification, reset, and email-change messages use the account locale.
- Post text is stored and shown as the author wrote it.
- There is no machine translation of posts.
- There is no language prefix in the URL.

Times are stored in UTC. The visible text is relative. The `<time>` element carries the absolute instant, and the tooltip shows that absolute time in the viewer’s locale.

The header shows Nine, the handle search, the profile switcher, and an account menu.

The account menu holds:

- Settings
- Notifications
- Bookmarks
- Language switch
- Light/dark toggle that follows the system until changed
- Logout

A moderator also sees the queue. Signed-out visitors see Log in and Register.

Nine links to the feed when there is an active profile, and to the home page otherwise.

---

## 21. Export and Privacy

The signed-in user can download a JSON file of:

- The account: email, locale, date of birth.
- Every profile: handle, display name, bio.
- Every post those profiles still have.
- Every comment those profiles still have.
- Every post reaction those profiles have made.
- Every comment reaction those profiles have made.
- Every bookmark those profiles have made.
- Every follow those profiles still have.
- Every block those profiles have made.

The file is built from the current projection documents.

Terms and Privacy exist before the first public signup. Counsel writes the prose.

The privacy page says the running database is the only copy, and that a lost disk loses accounts and posts.

Delete:

- Appends a tombstone.
- Removes the projection documents.
- Deletes the post’s objects.
- Overwrites post text, comment text, display name, bio, and email-bearing fields.
- Does not overwrite handle strings.
- Leaves a tombstone recording that the thing existed and was deleted, without that payload.
