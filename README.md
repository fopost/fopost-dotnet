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
| `Accounts`   | `ListAsync`, `GetAsync`, `RenameAsync`, `MoveAsync`, `HealthAsync`, `CreateTelegramConnectCodeAsync`, `GetTelegramConnectStatusAsync`, `GetTelegramBotCommandsAsync`, `SetTelegramBotCommandsAsync`, `DeleteTelegramBotCommandsAsync` |
| `AccountGroups` | `ListAsync`, `GetAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `SetMembersAsync`                                         |
| `Workspaces` | `ListAsync`, `GetAsync`                                                                                                            |
| `Labels`     | `ListAsync`                                                                                                                        |
| `Ai`         | `CreditsAsync`, `GenerateCaptionAsync`, `RewriteAsync`, `RepurposeUrlAsync`                                                        |
| `Inbox`      | `ListAsync`, `ThreadsAsync`, `ConversationsAsync`, `UnreadCountAsync`, `AccountsAsync`, `PlatformsAsync`, `MarkThreadReadAsync`, `RefreshAsync`, `UpdateAsync`, `EditCommentAsync`, `ReplyAsync`, `HideAsync`, `UnhideAsync`, `LikeAsync`, `UnlikeAsync`, `PinAsync`, `UnpinAsync`, `ReactAsync`, `DeleteAsync`, `StartConversationAsync`, `SetTypingAsync`, `ApprovalsAsync`, `ApproveReplyAsync`, `RejectReplyAsync` |
| `Validate`   | `PostAsync`, `LengthAsync`, `MediaAsync`                                                                                           |
| `Ads`        | `ListAsync`, `ExternalAsync`, `BoostableAsync`, `ConnectionsAsync`, `SourcesAsync`, `AuthorizeMetaAsync`, `DeleteConnectionAsync`, `BoostAsync`, `CreateAsync`, `RefreshAsync`, `SetStatusAsync`, `DeleteAsync`, `AudiencesAsync`, `CreateAudienceAsync`, `SearchTargetingAsync`, `LeadFormsAsync`, `CreateLeadFormAsync`, `LeadsAsync` |
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

`Inbox` needs an API key with the `inbox` scope; the calls that act on the platform as the account
(`EditCommentAsync`, `LikeAsync`, `UnlikeAsync`, `PinAsync`, `UnpinAsync`, `ReactAsync`,
`StartConversationAsync`, `SetTypingAsync`, a reply with media or quick replies, and deleting our
own reply) need `publish` as well. `Ads` needs the `ads` scope, and the four calls
that spend money (`BoostAsync`, `CreateAsync`, `SetStatusAsync`, `DeleteAsync`) need `publish` as
well. A boost or ad starts paused unless `Paused = false`, so nothing spends until it is resumed.

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
