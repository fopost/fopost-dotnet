# Changelog

All notable changes to `FoPost.Sdk` are listed here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses
[Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added
- Per-network extras on `Accounts`, all `accounts` scope: Pinterest boards
  (`ListPinterestBoardsAsync`, `CreatePinterestBoardAsync`), YouTube playlists and captions
  (`ListYouTubePlaylistsAsync`, `CreateYouTubePlaylistAsync`, `SetDefaultYouTubePlaylistAsync`,
  `ListYouTubeCaptionsAsync`, `UploadYouTubeCaptionsAsync`, `ReadYouTubeTranscriptAsync`),
  Bluesky post languages (`GetBlueskyLanguagesAsync`, `SetBlueskyLanguagesAsync`), TikTok
  creator info (`GetTikTokCreatorInfoAsync`), TikTok music and place search plus video
  lookup (`SearchTikTokMusicAsync`, `SearchTikTokLocationsAsync`, `LookupTikTokVideoAsync`),
  Instagram audio, publishing limit and stories
  (`SearchInstagramAudioAsync`, `GetInstagramPublishingLimitAsync`, `ListInstagramStoriesAsync`,
  `GetInstagramStoryInsightsAsync`) and LinkedIn mentions (`SearchLinkedInMentionsAsync`).

- `InboxItem.ModerationStatus` carries the platform's own state for a comment
  (`published`, `held`, `spam`, `rejected`), and `InboxAccount.ReconnectRequired`
  flags an account connected before the inbox asked for a permission it needs.
- `client.Broadcasts`: one message into every conversation the workspace already has with a
  segment of its contacts. `ListAsync`, `GetAsync`, `CreateAsync`, `UpdateAsync`,
  `DeleteAsync`, `SendAsync`, `CancelAsync` and `RecipientsAsync`. Reading needs the `inbox`
  scope; `SendAsync` and `CancelAsync` also need `publish`.
- `client.Sequences`: a series of messages on a delay. `ListAsync`, `GetAsync`, `CreateAsync`,
  `UpdateAsync`, `DeleteAsync`, `EnrollAsync`, `UnenrollAsync` and `EnrollmentsAsync`.
  `EnrollAsync` and `UnenrollAsync` need `publish`.
- Both honour each network's messaging window server-side. Messenger and Instagram take a
  business-initiated message only within 24 hours of the contact's last one, so recipients
  outside it come back skipped with `SkipReasons.WindowClosed` and nothing is attempted — the
  number sent is often lower than the audience.

- `client.Contacts`: the people behind the inbox. `ListAsync`, `GetAsync`, `CreateAsync`,
  `UpdateAsync`, `DeleteAsync`, `ConversationsAsync` (the threads one person appears in),
  `ImportAsync` (CSV), and `ListFieldsAsync`/`CreateFieldAsync`/`UpdateFieldAsync`/
  `DeleteFieldAsync` for the custom columns a workspace keeps. All need the `inbox` scope.
- `client.Contacts.ConversationAnalyticsAsync` reads `/v1/analytics/inbox/conversations`:
  volume and median reply time per thread. Needs the `analytics` scope.
- Meta messaging settings on `client.Accounts`: `GetIceBreakersAsync`, `SetIceBreakersAsync` and
  `DeleteIceBreakersAsync` (Facebook Pages and Instagram), plus `GetPersistentMenuAsync`,
  `SetPersistentMenuAsync`, `DeletePersistentMenuAsync`, `GetGreetingAsync`, `SetGreetingAsync`
  and `DeleteGreetingAsync` (Facebook Pages). A network without a field answers 400.
- `client.Accounts.GetWebhookSubscriptionAsync` reports whether the network is still delivering
  events for an account, and `ResubscribeWebhookAsync` puts a lapsed subscription back.
- `client.Inbox.HandoverAsync` passes a Messenger thread to another Meta app, or takes it back
  when no `appId` is given (`inbox` scope, plus `publish`).
- `client.Knowledge`: the workspace knowledge base — `ListAsync`, `CreateAsync`
  (`CreateKnowledgeSourceOptions`), `UpdateAsync` (`UpdateKnowledgeSourceOptions`),
  `DeleteAsync`, `SyncAsync` and `SearchAsync` (`SearchKnowledgeOptions`), with the
  `KnowledgeSource` and `KnowledgeMatch` models. A source is an FAQ, a note, a URL
  on your own site or a plain-text/CSV media item; `SearchAsync` returns the passages
  closest to a question, and is what grounds a drafted inbox reply in your own
  answers. Needs the `inbox` scope.

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
