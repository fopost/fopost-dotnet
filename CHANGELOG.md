# Changelog

All notable changes to `FoPost.Sdk` are listed here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses
[Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added

- `client.Media`: direct uploads through a presigned URL (`PresignAsync`, `CompleteAsync`, and
  `UploadDirectAsync`, which runs all three steps). Needs the `posts` scope.
- `client.Validate`: `PostAsync`, `LengthAsync` and `MediaAsync` wrap `/v1/validate/*` to check
  a draft, its length, or a media URL against platform rules without creating anything. Needs
  the `posts` scope.
- `client.AccountGroups`: `ListAsync`, `GetAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` and
  `SetMembersAsync` wrap `/v1/account-groups`. Needs the `accounts` scope.
- `client.Accounts.RenameAsync` sets or clears an account's display name, and `MoveAsync` moves an
  account to another workspace you own. `ListAsync(workspaceId, groupId)` filters by account group,
  and `SocialAccount.PlatformName` carries the name from the platform.
- `CreatePostOptions.AccountGroupId` targets every account in a group; `Accounts` may then be left
  empty.

## [0.2.0] - 2026-09-19

### Added

- `client.Inbox`: comments, mentions and DMs (`ListAsync`, `ThreadsAsync`, `ConversationsAsync`,
  `UnreadCountAsync`, `AccountsAsync`, `PlatformsAsync`, `MarkThreadReadAsync`, `RefreshAsync`,
  `UpdateAsync`, `ReplyAsync`, `HideAsync`, `UnhideAsync`, `DeleteAsync`) and drafted-reply
  approvals (`ApprovalsAsync`, `ApproveReplyAsync`, `RejectReplyAsync`). Needs the `inbox` scope.
- `client.Ads`: boosts and ads, ad connections and sources, audiences, targeting search, lead
  forms and leads. Needs the `ads` scope; `BoostAsync`, `CreateAsync`, `SetStatusAsync` and
  `DeleteAsync` need `publish` as well. A boost or ad starts paused unless `Paused = false`.
- Typed models for both resources, plus the `InboxItemTypes`, `InboxItemStates`, `AdGoals`,
  `AdBudgetTypes`, `AdStatuses`, `TargetingSearchTypes`, `AudienceSubtypes` and
  `LeadFormQuestions` constants.

## [0.1.0] - 2026-08-31

### Added

- Initial release: `Posts`, `Accounts`, `Workspaces`, `Labels` and `Ai` resources, typed models,
  an exception per error status, 429 retries honouring `Retry-After`, and `RequestAsync` for
  endpoints the SDK does not wrap.
