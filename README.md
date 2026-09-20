# FoPost.Sdk

[![NuGet](https://img.shields.io/nuget/v/FoPost.Sdk.svg)](https://www.nuget.org/packages/FoPost.Sdk)
[![license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/fopost/fopost-dotnet/blob/main/LICENSE)
[![CI](https://img.shields.io/github/actions/workflow/status/fopost/fopost-dotnet/ci.yml?label=ci)](https://github.com/fopost/fopost-dotnet/actions/workflows/ci.yml)

Official .NET SDK for the [FoPost](https://fopost.com) API. Schedule and publish to +30 social
platforms from your code.

```bash
dotnet add package FoPost.Sdk
```

Requires .NET 8 or newer. No third-party dependencies.

> **0.x release.** The public API is still settling and minor versions may contain breaking
> changes. Pin an exact version if that matters to you.

## Quick start

```csharp
using FoPost;

using var client = new FoPostClient(Environment.GetEnvironmentVariable("FOPOST_API_KEY")!);

// List your accounts
var accounts = await client.Accounts.ListAsync("9b2f6c1e-…");

// Create a post, then publish it immediately
var post = await client.Posts.CreateAsync(
    workspaceId: "9b2f6c1e-…",
    text: "Hello from the SDK",
    accounts: accounts.Select(account => account.Id));

await client.Posts.PublishAsync(post.Id);

// Schedule for later
await client.Posts.CreateAsync(new CreatePostOptions
{
    WorkspaceId = "9b2f6c1e-…",
    Status = PostStatuses.Scheduled,
    ScheduleAt = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero),
    Content = new List<PostContent> { new("Scheduled with the SDK") },
    Accounts = new List<string> { accounts[0].Id },
});
```

Create an API key in the dashboard under **Settings → API Keys**. A key is limited to the scopes
granted at creation — the calls above need `posts` and `accounts`.

## Configuration

```csharp
using var client = new FoPostClient(new FoPostClientOptions
{
    ApiKey = "fp_…",                        // defaults to $FOPOST_API_KEY
    BaseUrl = "https://api.fopost.com",     // override for staging or self-hosted
    Timeout = TimeSpan.FromSeconds(30),
    MaxRetries = 3,                         // total attempts on a 429, including the first
    HttpClient = httpClientFromFactory,     // optional; the SDK will not dispose it
});
```

| Env var          | Used for                                  |
| ---------------- | ----------------------------------------- |
| `FOPOST_API_KEY` | API key, when `ApiKey` is not passed |

`FoPostClient` is thread-safe and meant to be long-lived — register it as a singleton rather than
constructing one per request. With `IHttpClientFactory`:

```csharp
services.AddHttpClient("fopost");
services.AddSingleton(provider => new FoPostClient(new FoPostClientOptions
{
    HttpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("fopost"),
}));
```

## Paging

`ListAsync` returns one page plus its `Meta`. `ListAllAsync` walks every page for you:

```csharp
await foreach (var post in client.Posts.ListAllAsync(new ListPostsOptions
{
    WorkspaceId = "9b2f6c1e-…",
    Status = PostStatuses.Scheduled,
}))
{
    Console.WriteLine($"{post.ScheduleAt:u}  {post.Content.FirstOrDefault()?.Text}");
}
```

## Partial updates

Every field on `UpdatePostOptions` is an `Optional<T>`, so "leave this alone" and "clear this" stay
different things:

```csharp
await client.Posts.UpdateAsync(post.Id, new UpdatePostOptions
{
    Title = "New title",                    // sent
    Summary = Optional<string?>.Of(null),   // sent as null, clearing it
    // ScheduleAt is untouched — not sent at all
});
```

## AI features

```csharp
// Caption assist — accepts an API key carrying the `ai` scope
var caption = await client.Ai.GenerateCaptionAsync(new GenerateCaptionOptions
{
    CurrentCaption = "shipping a new feature",
    Platforms = new List<string> { Platforms.Twitter, Platforms.LinkedIn },
});

// Check your balance
var balance = await client.Ai.CreditsAsync();
Console.WriteLine($"{balance.CreditsRemaining} of {balance.CreditsTotal} credits left");
```

`RewriteAsync` and `RepurposeUrlAsync` are dashboard-session endpoints: they need a
`BearerToken` rather than an API key, and answer `401` to a key.

## Inbox and ads

```csharp
// Unread comments and mentions, then answer one
var unread = await client.Inbox.ListAsync(new ListInboxOptions
{
    WorkspaceId = "9b2f6c1e-…",
    State = InboxItemStates.Unread,
});
await client.Inbox.ReplyAsync(unread[0].Id, "Thanks for reaching out!");

// Boost a published post; it starts paused until you resume it
var ad = await client.Ads.BoostAsync(new BoostPostOptions
{
    WorkspaceId = "9b2f6c1e-…",
    ConnectionId = "c1d2e3f4-…",
    AdAccountId = "act_123",
    PostId = post.Id,
    AccountId = post.Accounts[0].Id,
    Name = "Launch boost",
    Goal = AdGoals.Engagement,
    Budget = new AdBudget(2000, AdBudgetTypes.Daily),
    Targeting = new AdTargeting { Countries = new List<string> { "US" } },
});
await client.Ads.SetStatusAsync(ad.Id, ad.WorkspaceId!, AdStatuses.Active);
```

## Contacts

The people behind the inbox: one person however many handles they write from. An inbound item files its author, a reply files whoever you answered, and both fold into whatever is already on file.

```csharp
var page = await client.Contacts.ListAsync(new ListContactsOptions
{
    WorkspaceId = workspaceId,
    Search = "ada",
});
foreach (var contact in page)
{
    Console.WriteLine($"{contact.DisplayName} — {contact.Channels.Count} handles");
}

// Folds into whoever already holds the first channel, so this cannot duplicate someone.
var contact = await client.Contacts.CreateAsync(new CreateContactOptions
{
    WorkspaceId = workspaceId,
    Channels = new[] { ContactChannel.Of("x", "ada_writes") },
    DisplayName = "Ada Okafor",
    Fields = new Dictionary<string, string> { ["plan_tier"] = "Pro" },
});

// A field set to null is cleared; everything unset is left alone.
await client.Contacts.UpdateAsync(contact.Id, new UpdateContactOptions
{
    Fields = new Dictionary<string, string?> { ["region"] = null },
});
await client.Contacts.DeleteAsync(contact.Id);   // the messages stay in the inbox

// The threads this person appears in, newest first.
foreach (var thread in await client.Contacts.ConversationsAsync(contact.Id))
{
    Console.WriteLine($"{thread.Platform} {thread.Messages} messages");
}

// platform and handle are required columns; any other column is a custom field key.
var result = await client.Contacts.ImportAsync(workspaceId, "platform,handle\nx,ada_writes");
Console.WriteLine($"{result.Created} created, {result.Merged} merged");

// The columns your workspace keeps.
var field = await client.Contacts.CreateFieldAsync(workspaceId, new CreateContactFieldOptions
{
    Key = "plan_tier",
    Name = "Plan Tier",
    Type = ContactFieldTypes.Select,
    Options = new[] { "Free", "Pro" },
});
await client.Contacts.DeleteFieldAsync(field.Id);   // removes every answer to it

// Volume and median reply time per thread. Needs the `analytics` scope.
var report = await client.Contacts.ConversationAnalyticsAsync(new ConversationAnalyticsOptions
{
    Days = 30,
    Sort = ConversationSorts.Slowest,
});
```

## Broadcasts and sequences

A broadcast is one message into every conversation you already have with a segment of your contacts; a sequence is a series of them on a delay. Neither opens a cold DM.

Nothing is sent into a closed messaging window: Messenger and Instagram take a business-initiated message only within 24 hours of the contact's last one, so recipients outside it come back skipped with `window_closed` rather than attempted. Telegram, Slack, Bluesky and Reddit have no window. The number sent is therefore often lower than the audience, and that is correct rather than a failure.

Reading needs the `inbox` scope; `SendAsync`, `CancelAsync`, `EnrollAsync` and `UnenrollAsync` also need `publish`.

```csharp
var broadcast = await client.Broadcasts.CreateAsync(new CreateBroadcastOptions
{
    WorkspaceId = workspaceId,
    AccountId = accountId,
    Name = "September check-in",
    Text = "New colours just landed. Want a look?",
    Audience = AudienceFilter.OnPlatforms("instagram"),
});

// Recipients is how many contacts matched, not how many will be messaged.
var sent = await client.Broadcasts.SendAsync(broadcast.Id);

// Who was skipped, and why.
var skipped = await client.Broadcasts.RecipientsAsync(
    broadcast.Id,
    new ListRecipientsOptions { Status = RecipientStatuses.Skipped });
foreach (var recipient in skipped)
{
    Console.WriteLine($"{recipient.DisplayName}: {recipient.SkipReason}");
}

var sequence = await client.Sequences.CreateAsync(new CreateSequenceOptions
{
    WorkspaceId = workspaceId,
    AccountId = accountId,
    Name = "Welcome",
    Steps = new[]
    {
        SequenceStep.Of(0, "Thanks for the follow — anything I can help with?"),
        SequenceStep.Of(48, "Here is what people usually ask us first."),
    },
});

// By id, or by the same audience filter a broadcast takes.
await client.Sequences.EnrollAsync(sequence.Id, new EnrollOptions { ContactIds = new[] { contactId } });

// Nothing further fires for them.
await client.Sequences.UnenrollAsync(sequence.Id, new[] { contactId });
```

## Error handling

Every non-2xx response raises a `FoPostException` carrying the API's status, error code, and body.

```csharp
try
{
    await client.Posts.PublishAsync(post.Id);
}
catch (FoPostRateLimitException error)
{
    Console.WriteLine($"Slow down for {error.RetryAfter}");
}
catch (FoPostPaymentRequiredException error)
{
    Console.WriteLine($"Upgrade at {error.UpgradeUrl}");
}
catch (FoPostException error)
{
    Console.WriteLine($"API {error.Status} ({error.Code}): {error.Message}");
}
```

| Status     | Exception                            |
| ---------- | ------------------------------------ |
| 400, 422   | `FoPostValidationException`          |
| 401        | `FoPostAuthenticationException`      |
| 402        | `FoPostPaymentRequiredException`     |
| 403        | `FoPostPermissionDeniedException`    |
| 404        | `FoPostNotFoundException`            |
| 429        | `FoPostRateLimitException`           |
| any other  | `FoPostException`                    |

A `429` is retried automatically, up to `MaxRetries` attempts, waiting for the interval the API asks
for in `Retry-After`. The exception is raised only once the retries are spent.

## Resources

| Namespace    | Methods                                                                                                                            |
| ------------ | ---------------------------------------------------------------------------------------------------------------------------------- |
| `Posts`      | `ListAsync`, `ListAllAsync`, `GetAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `PublishAsync`, `CancelAsync`, `RetryAsync`, `PreflightAsync`, `DuplicateAsync`, `DeliveriesAsync` |
| `Accounts`   | `ListAsync`, `GetAsync`, `RenameAsync`, `MoveAsync`, `HealthAsync`, `CreateTelegramConnectCodeAsync`, `GetTelegramConnectStatusAsync`, `GetTelegramBotCommandsAsync`, `SetTelegramBotCommandsAsync`, `DeleteTelegramBotCommandsAsync`, `ListSlackChannelsAsync`, `ListSlackMembersAsync`, `GetSlackIdentityAsync`, `UpdateSlackIdentityAsync`, `GetIceBreakersAsync`, `SetIceBreakersAsync`, `DeleteIceBreakersAsync`, `GetPersistentMenuAsync`, `SetPersistentMenuAsync`, `DeletePersistentMenuAsync`, `GetGreetingAsync`, `SetGreetingAsync`, `DeleteGreetingAsync`, `GetWebhookSubscriptionAsync`, `ResubscribeWebhookAsync`, `ListDiscordChannelsAsync`, `SwitchDiscordChannelAsync`, `GetDiscordIdentityAsync`, `UpdateDiscordIdentityAsync`, `ListDiscordPinsAsync`, `DeleteDiscordMessageAsync`, `PinDiscordMessageAsync`, `UnpinDiscordMessageAsync`, `CrosspostDiscordMessageAsync`, `CreateDiscordThreadAsync`, `SendDiscordDmAsync`, `ListDiscordEventsAsync`, `GetDiscordEventAsync`, `CreateDiscordEventAsync`, `UpdateDiscordEventAsync`, `DeleteDiscordEventAsync`, `ListDiscordMembersAsync`, `GetDiscordMemberAsync`, `ListDiscordRolesAsync`, `CreateDiscordRoleAsync`, `UpdateDiscordRoleAsync`, `DeleteDiscordRoleAsync`, `AddDiscordMemberRoleAsync`, `RemoveDiscordMemberRoleAsync` |
| `AccountGroups` | `ListAsync`, `GetAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `SetMembersAsync`                                         |
| `Workspaces` | `ListAsync`, `GetAsync`                                                                                                            |
| `Labels`     | `ListAsync`                                                                                                                        |
| `Ai`         | `CreditsAsync`, `GenerateCaptionAsync`, `RewriteAsync`, `RepurposeUrlAsync`                                                        |
| `Inbox`      | `ListAsync`, `ThreadsAsync`, `ConversationsAsync`, `UnreadCountAsync`, `AccountsAsync`, `PlatformsAsync`, `MarkThreadReadAsync`, `RefreshAsync`, `UpdateAsync`, `EditCommentAsync`, `ReplyAsync`, `HideAsync`, `UnhideAsync`, `LikeAsync`, `UnlikeAsync`, `PinAsync`, `UnpinAsync`, `ReactAsync`, `DeleteAsync`, `StartConversationAsync`, `SetTypingAsync`, `HandoverAsync`, `ApprovalsAsync`, `ApproveReplyAsync`, `RejectReplyAsync` |
| `Contacts`   | `ListAsync`, `GetAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `ConversationsAsync`, `ImportAsync`, `ListFieldsAsync`, `CreateFieldAsync`, `UpdateFieldAsync`, `DeleteFieldAsync`, `ConversationAnalyticsAsync` |
| `Knowledge`  | `ListAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `SyncAsync`, `SearchAsync`                                              |
| `Validate`   | `PostAsync`, `LengthAsync`, `MediaAsync`                                                                                           |
| `Ads`        | `ListAsync`, `ExternalAsync`, `BoostableAsync`, `ConnectionsAsync`, `SourcesAsync`, `AuthorizeMetaAsync`, `DeleteConnectionAsync`, `BoostAsync`, `CreateAsync`, `RefreshAsync`, `SetStatusAsync`, `DeleteAsync`, `AccountTreeAsync`, `CreateCampaignAsync`, `GetCampaignAsync`, `UpdateCampaignAsync`, `DeleteCampaignAsync`, `DuplicateCampaignAsync`, `CreateAdSetAsync`, `GetAdSetAsync`, `UpdateAdSetAsync`, `DeleteAdSetAsync`, `DuplicateAdSetAsync`, `CreateNetworkAdAsync`, `GetNetworkAdAsync`, `UpdateNetworkAdAsync`, `DeleteNetworkAdAsync`, `DuplicateNetworkAdAsync`, `BulkSetStatusAsync`, `CreativesAsync`, `CreateCreativeAsync`, `GetCreativeAsync`, `DeleteCreativeAsync`, `AudiencesAsync`, `CreateAudienceAsync`, `GetAudienceAsync`, `UpdateAudienceAsync`, `DeleteAudienceAsync`, `AddAudienceUsersAsync`, `SearchTargetingAsync`, `EstimateReachAsync`, `InsightsAsync`, `AdInsightsAsync`, `LeadFormsAsync`, `CreateLeadFormAsync`, `GetLeadFormAsync`, `ArchiveLeadFormAsync`, `LeadsAsync`, `LeadsFeedAsync`, `LeadPagesAsync`, `SubscribeLeadPageAsync`, `UnsubscribeLeadPageAsync`, `GoalsAsync`, `CatalogsAsync`, `CreateCatalogAsync`, `GetCatalogAsync`, `UpdateCatalogAsync`, `DeleteCatalogAsync`, `CatalogProductsAsync`, `WriteCatalogProductsAsync`, `ProductFeedsAsync`, `CreateProductFeedAsync`, `DeleteProductFeedAsync`, `FeedUploadsAsync`, `StartFeedUploadAsync`, `ProductSetsAsync`, `CreateProductSetAsync`, `UpdateProductSetAsync`, `DeleteProductSetAsync`, `ReachFrequencyAsync`, `CreateReachFrequencyAsync`, `GetReachFrequencyAsync`, `ReserveReachFrequencyAsync`, `CancelReachFrequencyAsync`, `LibraryAsync`, `PartnershipCreatorsAsync`, `RequestPartnershipAsync`, `RevokePartnershipAsync`, `AccountActivityAsync`, `LabelsAsync`, `CreateLabelAsync`, `UpdateLabelAsync`, `DeleteLabelAsync`, `ApplyLabelAsync`, `StudiesAsync`, `CreateStudyAsync`, `GetStudyAsync`, `DeleteStudyAsync`, `IosCampaignLimitsAsync`, `HighDemandPeriodsAsync`, `CreateHighDemandPeriodAsync`, `DeleteHighDemandPeriodAsync`, `ValueRuleSetsAsync`, `CreateValueRuleSetAsync`, `DeleteValueRuleSetAsync` |
| `Media`      | `PresignAsync`, `CompleteAsync`, `UploadDirectAsync`                                                                              |

`Validate` checks a draft, its length, or a media URL against platform rules without creating
anything; it needs the `posts` scope.

```csharp
var check = await client.Validate.PostAsync(new ValidatePostOptions
{
    Content = "Hello from the SDK",
    Platforms = new List<string> { Platforms.Twitter, Platforms.LinkedIn },
});
foreach (var platform in check.Platforms.Where(p => !p.Ready))
{
    Console.WriteLine($"{platform.Platform}: {string.Join(", ", platform.Issues)}");
}
```

`Contacts` needs the `inbox` scope too, except `ConversationAnalyticsAsync`, which needs `analytics`. `Inbox` needs an API key with the `inbox` scope; the calls that act on the platform as the account
(`EditCommentAsync`, `LikeAsync`, `UnlikeAsync`, `PinAsync`, `UnpinAsync`, `ReactAsync`,
`StartConversationAsync`, `SetTypingAsync`, `HandoverAsync`, a reply with media or quick replies, and deleting our
own reply) need `publish` as well. `Ads` needs the `ads` scope, and the calls
that spend money (`BoostAsync`, `CreateAsync`, `SetStatusAsync`, `DeleteAsync`, `BulkSetStatusAsync`,
and every create, update, delete and duplicate on campaigns, ad sets and network ads) need `publish`
as well. Anything created starts paused unless `Paused = false`, so nothing spends until it is
resumed. Campaigns, ad sets, network ads, creatives and audiences take the ad platform's own ids plus
a `connectionId`, and are read live rather than stored.

`Media` uploads a file straight to storage with the `posts` scope: `UploadDirectAsync` presigns,
PUTs the bytes, and completes in one call, or drive the three steps yourself.

```csharp
var bytes = await File.ReadAllBytesAsync("launch.png");
var media = await client.Media.UploadDirectAsync("9b2f6c1e-…", "launch.png", "image/png", bytes);
```

The API has more endpoints than the SDK wraps — analytics, webhooks, automations, and
communities among them. `RequestAsync` reaches any of them with the same auth, retries, and error
handling:

```csharp
var overview = await client.RequestAsync(
    HttpMethod.Get,
    "/v1/analytics/overview",
    query: new Dictionary<string, object?> { ["workspace_id"] = "9b2f6c1e-…" });
```

The full surface is documented in the
[API collection](https://github.com/fopost/fopost-api-collections).

## Contributing

Issues and pull requests are welcome at
[fopost/fopost-dotnet](https://github.com/fopost/fopost-dotnet).

```bash
dotnet build
dotnet test
dotnet pack src/FoPost/FoPost.csproj -c Release
```

## License

MIT
