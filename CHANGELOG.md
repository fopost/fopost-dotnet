# Changelog

All notable changes to `FoPost.Sdk` are listed here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses
[Semantic Versioning](https://semver.org/).

## [Unreleased]

### Changed
- **Breaking:** `Ads.UploadConversionsAsync` returns `ConversionUploadResult` (`Id`, `Accepted`,
  `EventCount`, `Replayed`) instead of the accepted count, and takes an optional `idempotencyKey`
  sent as the `Idempotency-Key` header.
- `ConversionEvent` gained `EventId`, `ActionSource` (`ConversionActionSources`), `SourceUrl`,
  `ExternalId`, `ClientIpAddress`, `ClientUserAgent`, `ClickId`, `BrowserId` and `ContentIds`.
- An X account connected before this release answers 409 `reconnect_required` to repost,
  bookmark and follow until it is reconnected, because the grant now asks for the bookmark and
  follow permissions. Every one of these calls is an X API call metered to the workspace.

### Added
- `Ads.SendPixelConversionsAsync` (`POST /v1/ads/conversions/events`): the Conversions API send
  with a required `Idempotency-Key`; a replay answers the stored batch with `Replayed` true.
- `Ads.ConversionBatchesAsync` (`GET /v1/ads/conversions/events`) lists stored batches as
  `ConversionBatch` with `HashedConversionEvent` rows.
- Pixels: `PixelsAsync`, `CreatePixelAsync`, `PixelAsync`, `UpdatePixelAsync`, `SharePixelAsync`,
  `PixelStatsAsync` and `EventMatchQualityAsync`, with `AdPixelDetail`, `PixelDataUseSettings`,
  `AdPixelStats` and `EventMatchQuality`.
- Custom conversions: `CustomConversionsAsync`, `CreateCustomConversionAsync`,
  `CustomConversionAsync`, `UpdateCustomConversionAsync` and `DeleteCustomConversionAsync`, with
  `CustomConversion`, `CustomConversionRule` and `CustomConversionEventTypes`.
- `FoPostHttpClient.RequestAsync` accepts per-request headers.
- `client.Inbox.RepostAsync`, `UnrepostAsync`, `BookmarkAsync` and `UnbookmarkAsync` act on an X
  item where `CanRepost` or `CanBookmark` is true (`inbox` and `publish`), answering the item with
  its new `Reposted` and `Bookmarked` state. A 402 `platform_balance_spent` means X's own
  pay-per-use balance is spent.
- `client.Accounts.FollowXUserAsync` and `UnfollowXUserAsync` follow or unfollow a numeric X user
  id from a connected X account into `XFollowResult` (`inbox` and `publish`).
- `client.Accounts.GetXPostAsync` reads one X post by id and `SearchXPostsAsync`
  (`XSearchOptions`) runs an X recent search, both with the named account's token (`inbox`).
  Results are `XPost`; a search page carries `NextToken` for the next one.
- The inbox item's `Reposted`, `Bookmarked`, `CanRepost` and `CanBookmark`.

## [0.4.0] - 2026-10-18

### Added
- `client.Queue` (scope `posts`): the posting queue. `ListSlotsAsync`, `CreateSlotAsync`,
  `UpdateSlotAsync`, `DeleteSlotAsync`, `NextAsync` and `PreviewAsync`, with the `QueueSlot`,
  `QueueMove`, `QueueSlotDeletion` and `QueueNext` models. `CreatePostOptions.Schedule` and
  `UpdatePostOptions.Schedule` take `PostSchedules.Queue` to drop a post into the next free slot.
- `Posts.UnpublishAsync` (`POST /v1/posts/{id}/unpublish`, scope `publish`) takes a published post
  down everywhere and answers an `UnpublishResult`. `PostStatuses` gains `PartiallyFailed` and
  `Unpublished`.
- `client.ConnectSessions` (scope `accounts`): hosted connect links. `CreateAsync` and `GetAsync`,
  with `ConnectSession`, `ConnectSessionBranding` and `ConnectSessionStatuses`.
- `client.Webhooks` (scope `webhooks`): `ListAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`,
  `TestAsync`, `DeliveriesAsync`, `GetDeliveryAsync` and `RedeliverAsync`, with the `Webhook`,
  `WebhookDelivery` models and the `WebhookEvents` constants.
- `client.Automations` (scope `automations`): `ListAsync`, `CreateAsync`, `StatsAsync`, `GetAsync`,
  `UpdateAsync`, `DeleteAsync`, `ToggleAsync`, `ListVersionsAsync`, `GetVersionAsync`,
  `RestoreVersionAsync`, `ListRunsAsync` and `GetRunAsync`, with the `Automation`, `AutomationGraph`,
  `AutomationVersion`, `AutomationRestore`, `AutomationRun`, `AutomationStats` models and the
  `AutomationTriggerTypes` and `AutomationNodeKinds` constants.
- `client.Logs` (scope `workspaces`): `ListAsync` reads request logs into a `RequestLogPage` with a
  `before` cursor and the window's `RequestLogSummary`.
- `Analytics` (scope `analytics`) gains the dashboard reports: `OverviewAsync`, `TimeSeriesAsync`,
  `TopPostsAsync`, `LabelsAsync`, `PostsTableAsync`, `PostingStreakAsync`, `DemographicsAsync`,
  `InboxAsync` and `CollectAsync`.
- `Workspaces.CreateAsync`, `UpdateAsync` and `DeleteAsync`; `Workspace` reads `RequireApproval`,
  `AiAltTextEnabled`, `BrandColor`, `ConnectRedirectDomains` and `BypassAbilities`.
- `Labels.GetAsync`, `CreateAsync`, `UpdateAsync` and `DeleteAsync`.
- `Accounts.ListCommunitiesAsync`, `SyncCommunitiesAsync`, `SearchCommunitiesAsync`,
  `AddCommunityAsync` and `RemoveCommunityAsync` for X communities.
- `OffsetPage<T>` and `OffsetPageMeta` for lists footed with `{ page, per_page, total, total_pages }`.

### Changed
- The Discord, Google Business and WhatsApp methods name the API operation they wrap in their
  doc comments, so the collections checker can see them.

## [0.3.0] - 2026-10-18

### Removed
- **Breaking:** `BlogSettings.AutoPublish` and `UpdateBlogSettingsOptions.AutoPublish` are removed;
  the API no longer auto-publishes. An article publishes only after `SubmitArticleAsync` moves it
  to review.

### Changed
- The blog resource follows the live wire format: `workspace_id` on the query string, snake_case
  answers (bare objects, `{data: [...]}` lists), and `website_secret` beside the settings on
  update. `BlogWebsiteTest.Status` is now `HttpStatus`, read from `http_status`; a failed test
  carries the API's message in `Error`.

### Added
- `Blog.SuggestSeedsAsync` (`POST /v1/blog/keywords/suggest`, scope `ai`) suggests about 10 search
  terms from the blog goal and topics into `BlogSeedSuggestions`; pass `Seeds` to refine and
  `Prompt` to steer.
- `Blog.SubmitArticleAsync` (`POST /v1/blog/articles/{id}/submit`, scope `posts`) moves a draft,
  failed or rejected article to the new `BlogArticleStatuses.InReview`. Publishing and setting
  `PublishAt` now need an article in review; a draft answers 409.
- `BlogSettings.Goal` (`BlogGoals`), `FocusTopics`, `AvoidTopics` and `PlanRun` (`BlogPlanRun`,
  `BlogPlanRunStatuses`), with `Goal`, `FocusTopics` and `AvoidTopics` on
  `UpdateBlogSettingsOptions`. `BlogKeyword.FitReason` says why a keyword serves the goal.
  `PlanKeywordsAsync` no longer fills the calendar: results land as `suggested`, and setting a
  keyword to `planned` picks it. `ReviewHoldDays` is now the days before its planned date that a
  picked keyword starts being written.
- `client.Blog`: articles for a website. Settings (`GetSettingsAsync`, `UpdateSettingsAsync`,
  `RotateWebsiteSecretAsync`, `TestWebsiteAsync`), the keyword plan (`ListKeywordsAsync`,
  `PlanKeywordsAsync`, `AddKeywordAsync`, `UpdateKeywordAsync`, `DeleteKeywordAsync`) and the
  articles (`ListArticlesAsync`, `RequestArticleAsync`, `GetArticleAsync`, `UpdateArticleAsync`,
  `RegenerateArticleAsync`, `PublishArticleAsync`, `RejectArticleAsync`, `DeleteArticleAsync`),
  with the `BlogSettings`, `BlogKeyword`, `BlogArticle`, `BlogArticleFull`, `BlogArticleImage`
  models and the `BlogArticleStatuses`, `BlogKeywordStatuses` and `BlogTargetKinds` constants.
  Reads need `posts`, planning and writing need `ai`, publishing needs `publish`.
- `FoPostWebhooks.Verify`: checks a webhook delivery's raw bytes against the FoPost signature
  headers in constant time, preferring the timestamped signature with a 300 second tolerance,
  and returns the `FoPostWebhookEnvelope` (`Event`, `Data`, `Timestamp`, `DeliveryId`).
  `BlogArticlePublishedPayload` types the `article.published` event.
- `Media.LimitsAsync` reads `GET /v1/media/limits` into `UploadLimits`: the per-file ceiling by
  any route, the multipart ceiling, the next connected-account tier and the allowed types.
- `PresignOptions` carries the optional `FolderId`, `Origin`, `BrandRole`, `BrandVoiceId` and
  `PersonConsent` on presign; `PresignedUpload.MaxFileBytes` reads the ceiling the API returns.
  `UploadDirectAsync` takes a `Stream` of known length and streams it to storage instead of
  buffering; sizes are `long` and go up to 5 GB.
- `CreatePostOptions.Files` and `UpdatePostOptions.Files` attach up to five `PostFile`s to the
  first content block. Files at or under the multipart ceiling are sent as one
  `multipart/form-data` request to the post endpoint; larger files are uploaded directly and
  attached as media before the JSON request.
- An oversize file is a `413` with code `file_too_large`; the `FoPostException` exposes the
  status and the body (`max_file_bytes`, `file_bytes`, `upgrade_url`).
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

- `Platforms.Snapchat`, included in `Platforms.All`.

- `Whatsapp` for a WhatsApp Business connection: the business profile
  (`GetProfileAsync`, `UpdateProfileAsync`, `RequestDisplayNameAsync`,
  `SetUsernameAsync`), message templates including the platform's own library
  (`ListTemplatesAsync`, `CreateTemplateAsync`, `ImportTemplateAsync`, …), groups,
  blocking, commerce settings and flows (`CreateFlowAsync`, `UploadFlowJsonAsync`,
  `PublishFlowAsync`, `ListFlowResponsesAsync`, …), plus `GetAccountEventsAsync`.
  All need the `accounts` scope.
- `Whatsapp.CreateSandboxSessionAsync` and `ListSandboxSessionsAsync` invite a
  tester to the platform-owned WhatsApp test number. Inviting sends a template, so
  it needs the `publish` scope.
- `Platforms.WhatsApp`.

- `client.Blogs` for content that already lives on a connected site: `ListBlogsAsync`,
  `ListArticlesAsync`, `GetArticleAsync`, `CreateArticleAsync`, `UpdateArticleAsync`,
  `DeleteArticleAsync`, `ListProductsAsync` and `UpdateProductAsync`, with `RemoteBlog`,
  `RemoteArticle` and `RemoteProduct`. Reads need the `posts` scope; the writes need
  `posts` and `publish`. Updating an article changes the live one in place and never
  creates a duplicate.
- `client.Accounts.ListSlackChannelsAsync`, `ListSlackMembersAsync`, `GetSlackIdentityAsync` and
  `UpdateSlackIdentityAsync` (`UpdateSlackIdentityOptions`) for a Slack account. All four need the
  `accounts` scope; a webhook-connected account answers 409 `webhook_connection`.
- `client.Accounts.ListRedditSubredditsAsync`, `ListRedditSubredditRulesAsync`,
  `ListRedditFlairsAsync` and `SetRedditDefaultSubredditAsync` cover a Reddit account's
  destinations: the subreddits it is in, a subreddit's rules and post flairs, and where posts go
  when one names no subreddit. All four need the `accounts` scope; an account connected before
  those permissions were asked for answers 409 `reconnect_required`.
- `client.Validate.SubredditAsync` asks whether a subreddit exists and takes a post from an
  account, before a post exists (`posts` scope).
- `client.Inbox.VoteAsync` votes an item up or down where the network ranks by votes, or takes an
  earlier vote back with `none` (`inbox` and `publish`).
- `SocialAccount.ReconnectRequired`, and the inbox item's `Vote` and `CanVote`.
- `client.Accounts.CreateTelegramConnectCodeAsync` mints a one-time code that connects a Telegram
  chat when sent to the bot, and `GetTelegramConnectStatusAsync` polls its outcome.
- `client.Accounts.GetTelegramBotCommandsAsync`, `SetTelegramBotCommandsAsync` and
  `DeleteTelegramBotCommandsAsync` manage the bot's command menu in a connected chat. All five
  need the `accounts` scope.

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
