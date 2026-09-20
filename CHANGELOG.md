# Changelog

All notable changes to `FoPost.Sdk` are listed here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses
[Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added

- `Ads.AuthorizeAsync` takes a `Provider` on `AuthorizeAdsOptions`, so a connection can be
  started on any ad network the API lists, not only Meta. `AuthorizeMetaAsync` delegates to it
  and is obsolete.

- `client.Accounts.ListSlackChannelsAsync`, `ListSlackMembersAsync`, `GetSlackIdentityAsync` and
  `UpdateSlackIdentityAsync` (`UpdateSlackIdentityOptions`) for a Slack account. All four need the
  `accounts` scope; a webhook-connected account answers 409 `webhook_connection`.
- `client.Accounts.CreateTelegramConnectCodeAsync` mints a one-time code that connects a Telegram
  chat when sent to the bot, and `GetTelegramConnectStatusAsync` polls its outcome.
- `client.Accounts.GetTelegramBotCommandsAsync`, `SetTelegramBotCommandsAsync` and
  `DeleteTelegramBotCommandsAsync` manage the bot's command menu in a connected chat. All five
  need the `accounts` scope.

## [0.3.0] - 2026-09-19

### Added

- `client.Ads`: the campaign tree (`AccountTreeAsync`; get, create, update, delete and duplicate for
  campaigns, ad sets and network ads; `BulkSetStatusAsync`), creatives (`CreativesAsync`,
  `CreateCreativeAsync`, `GetCreativeAsync`, `DeleteCreativeAsync`), audience management
  (`GetAudienceAsync`, `UpdateAudienceAsync`, `DeleteAudienceAsync`, `AddAudienceUsersAsync`),
  `EstimateReachAsync`, date-range insights (`InsightsAsync`, `AdInsightsAsync`), and lead forms and
  the stored leads feed (`GetLeadFormAsync`, `ArchiveLeadFormAsync`, `LeadsFeedAsync`,
  `LeadPagesAsync`, `SubscribeLeadPageAsync`, `UnsubscribeLeadPageAsync`). Campaign, ad set and
  network ad writes and bulk status need the `publish` scope on top of `ads`.
- `CreateAdOptions.UrlTags` and `AdCreative.UrlTags` carry the query string appended to every link
  in an ad.

- `client.Inbox`: `LikeAsync`, `UnlikeAsync`, `PinAsync`, `UnpinAsync`, `ReactAsync`,
  `EditCommentAsync`, `StartConversationAsync` and `SetTypingAsync`, plus a `ReplyAsync` overload
  taking `ReplyInboxItemOptions` for media and quick replies. These need the `publish` scope on
  top of `inbox`.
- `InboxItem` gains `Liked`, `Pinned`, `Reaction`, `EditedAt` and the `CanLike`, `CanPin`,
  `CanEdit`, `CanReact`, `CanSendMedia`, `CanQuickReply` and `CanPrivateReply` flags;
  `InboxAccount` gains `CanStartConversation`.
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
