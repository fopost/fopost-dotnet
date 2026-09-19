# CLAUDE.md

Guidance for Claude Code (claude.ai/code) when working in this repository.

## What This Is

`FoPost.Sdk` — the official .NET SDK for the FoPost REST API, destined for **NuGet**.
It wraps the HTTP API in a `FoPost.FoPostClient` with async resources, typed models, and an
exception per error status.

- **.NET 8** (`net8.0`), **C# 12**, nullable reference types enabled, `TreatWarningsAsErrors`.
- **No runtime package dependencies** — `System.Net.Http.HttpClient` and `System.Text.Json` from
  the BCL. Do not add one.
- Assembly and package id `FoPost.Sdk`; root namespace `FoPost`. Version `0.3.0` in
  `src/FoPost/FoPost.csproj` (the release workflow enforces the tag match).

## Downstream Packages

These repos wrap this SDK and must be updated in lockstep:

- `fopost-aspnet` — ASP.NET Core integration: DI registration, options binding, webhook receiving

**Whenever you change this SDK's public surface — a renamed method, a changed parameter,
a new or removed resource, a new error type, a bumped minimum language version — you must
open a matching PR in every repo listed above in the same session.** They are separate
git repos, checked out as siblings at `../fopost-<child>`. A parent release that silently
breaks a child is only discovered by the user who upgrades first.

Also bump the child's dependency constraint on this package and note the change in its
CHANGELOG when this package is released.

## Brand Rules

- The product is **FoPost** (`fopost.com`). Never write "OwlStack" — retired Aug 2026.
- Never write an email address anywhere: not in code, XML doc comments, README, or `.csproj`
  package metadata. Support is https://fopost.com/contact and the GitHub issues page.
- Never name AI providers or models, infrastructure vendors, hosting, or any person.
  `<Authors>` / `<Company>` name the brand and Porter Bridge, LLC — keep it that way.

## Architecture

```
FoPost.sln
src/FoPost/
  FoPostClient.cs          entry point (IDisposable); owns one instance per resource
  FoPostClientOptions.cs   credentials, base URL, timeout, retries, HttpClient injection
  Http/
    FoPostHttpClient.cs    headers, URI building, JSON coding, retry loop, decode, unwrap
    FoPostJson.cs          the shared JsonSerializerOptions + camelCase alias resolver
  Errors/
    FoPostException.cs     base + the sealed per-status subclasses
    ErrorFactory.cs        status + body -> exception
  Models/
    FoPostModel.cs         base; unknown keys land in AdditionalData
    Optional.cs            struct sentinel for partial updates
    Post.cs Account.cs Page.cs Media.cs Ai.cs Platforms.cs Inbox.cs Ads.cs Validate.cs
  Resources/
    PostsResource.cs AccountsResource.cs WorkspacesResource.cs LabelsResource.cs AiResource.cs
    InboxResource.cs AdsResource.cs ValidateResource.cs
    PostOptions.cs AiOptions.cs InboxOptions.cs AdsOptions.cs ValidateOptions.cs ResourceHelpers.cs
tests/FoPost.Tests/        xunit; TestServer.cs holds the stub handler and fixtures
examples/CreatePost/       runnable create-and-publish sample, part of the solution
```

**Request flow.** `await client.Posts.CreateAsync(...)` → `PostsResource` builds an options
object and calls `_http.PostAsync("/v1/posts", body)` → `FoPostHttpClient.RequestAsync` builds
the `Uri`, serialises with `FoPostJson.Options`, and enters the retry loop → `HttpClient.SendAsync`
→ `DecodeAsync` parses to a `JsonNode` and either throws via `ErrorFactory.FromResponse` or
returns it → the resource calls `FoPostHttpClient.Unwrap(...)` and `ResourceHelpers.Require<T>` /
`ToList<T>` to deserialise.

- **There is no `Transport` interface here** — unlike the PHP and Ruby SDKs. The seam is the
  standard .NET one: set `FoPostClientOptions.HttpClient` (supply your own, e.g. from
  `IHttpClientFactory` — the SDK then neither configures nor disposes it) or
  `FoPostClientOptions.HttpMessageHandler` (the SDK builds a client on it). Everything that would
  live behind a transport interface lives behind `HttpMessageHandler` instead.
- `FoPostHttpClient.Delay` is an `internal` `Func<TimeSpan, CancellationToken, Task>` test seam
  for the retry wait, reachable from the test assembly via `InternalsVisibleTo`.
- Resource methods pass the full versioned path (`"/v1/posts"`) — the base URL has no path
  segment. Keep that convention when adding an endpoint.
- `FoPostJson` registers a **read-only camelCase alias** beside every snake_case property, because
  the API is inconsistent (posts answer snake_case, accounts and deliveries camelCase). Unknown
  keys survive in `FoPostModel.AdditionalData` rather than being dropped.
- `Optional<T>` distinguishes "the caller did not pass this" from `null`, so a partial update
  sends only the named fields. `Optional<T>.Of(null)` explicitly clears; `Optional<T>.Unset` omits.
- Every resource method is async and takes a trailing `CancellationToken`.

**Resources wired today:** `Posts`, `Accounts`, `Workspaces`, `Labels`, `Ai`, `Inbox`, `Ads`,
`Media`, `Validate` (scope `posts`, wraps the three stateless `/v1/validate/*` checks).
`Inbox` (scope `inbox`) skips `/v1/inbox/chat/*` (browser-encrypted X Chat) and the binary
`/v1/inbox/{id}/attachments/{index}` stream. `Ads` (scope `ads`) wraps every `/v1/ads` route;
boost, create, set status, delete, bulk status and the campaign/ad set/network ad writes also need
`publish`, and anything created starts paused unless `Paused = false`. Inbox lists carry `{ page, perPage, total }` meta, read into
`InboxPage<T>`/`InboxPageMeta` rather than `Page<T>`. Ads request bodies are camelCase and are
serialised straight from their options objects; the inbox `read`/`refresh` bodies are
snake_case and `PATCH /v1/inbox/{id}` is camelCase, so those are built by hand. `Media` (scope
`posts`) is only the direct-upload flow: `PresignAsync` → `FoPostHttpClient.PutBytesAsync` (raw
bytes to the presigned URL, no credential header, only the headers the API returned) →
`CompleteAsync`, bundled as `UploadDirectAsync`. There is no `Communities`, `Webhooks`,
`Analytics`, or `Automations` resource here — reach those
through the escape hatch `FoPostClient.RequestAsync(...)`, which returns the raw `JsonNode?`
envelope and all.

## API Contract

- **Base URL:** `FoPostClientOptions.DefaultBaseUrl` = `https://api.fopost.com` — **host only, no
  version path.** Every resource path therefore starts `/v1/...`. Only trailing slashes are
  stripped from an override. There is **no `FOPOST_BASE_URL` env read**; set `Options.BaseUrl`.
- **Auth:** header `X-API-Key: <key>`. `FoPostClientOptions.ApiKey` defaults to the
  `FOPOST_API_KEY` environment variable, read at options construction.
  **Divergence worth knowing:** the options also expose `BearerToken`, sent as
  `Authorization: Bearer` for the handful of endpoints that do not accept an API key, and a set
  bearer token **wins over** the API key — precisely because the key may have arrived from the
  environment unintentionally. With neither set, the constructor throws `ArgumentException`
  before any request.
- **Headers on every request:** `Accept: application/json`, the credential header,
  `User-Agent: fopost-dotnet` (note: **no version suffix**), and `Content-Type: application/json`
  only when there is a body.
- **Timeout:** `TimeSpan.FromSeconds(30)` default. Ignored when `Options.HttpClient` is supplied —
  that client's own timeout wins.
- **Retries — as implemented here:** `MaxRetries` (default 3) is the **total attempt count**, and
  only **HTTP 429** is retried. 5xx is not retried, and an `HttpRequestException` from the handler
  propagates on the first attempt. There is **no exponential backoff**: the wait is `Retry-After`
  (`Delta` or `Date`, floored at zero) when present, otherwise a flat 1 second, capped at
  `MaxRetryWait` = 60 seconds. The `CancellationToken` is passed to the delay, so a cancelled
  request is never retried.
- **Success envelope:** `FoPostHttpClient.Unwrap` peels `{"data": ...}` only when the key is
  present, because some endpoints answer bare. Paginated lists carry a sibling `meta`
  (`current_page`, `per_page`, `total`, `last_page`, `from`, `to`) read by
  `ResourceHelpers.ReadMeta` into `PageMeta`.
- **Error envelope:** `{"error": "<code>", "message": "<text>"}` maps onto `FoPostException.Code`
  and `.Message`; the decoded `JsonNode` stays on `.Body`, so extra fields stay reachable.
  `FoPostPaymentRequiredException.UpgradeUrl` reads `upgrade_url` off it. `ToString()` renders
  `[<status> (<code>)] <message>`.
- **Exception map** (`ErrorFactory.FromResponse`): 400/422 `FoPostValidationException` ·
  401 `FoPostAuthenticationException` · 402 `FoPostPaymentRequiredException` ·
  403 `FoPostPermissionDeniedException` · 404 `FoPostNotFoundException` ·
  429 `FoPostRateLimitException` (carries `TimeSpan? RetryAfter`) · **everything else, 5xx
  included, falls back to `FoPostException`** — there is no dedicated server-error class. The
  subclasses are `sealed`; only the base is extensible.
- **Rate-limit headers** (`X-RateLimit-Limit`/`-Remaining`/`-Reset`) are not surfaced anywhere —
  the `HttpResponseMessage` is disposed inside `RequestAsync`. Read them from a custom
  `HttpMessageHandler` if you need them.

## Commands

```bash
dotnet restore
dotnet build --configuration Release
dotnet test                                     # xunit, tests/FoPost.Tests
dotnet test --filter FullyQualifiedName~RetryTest
dotnet pack src/FoPost/FoPost.csproj --configuration Release --output artifacts
dotnet run --project examples/CreatePost        # needs FOPOST_API_KEY
dotnet format                                   # applies .editorconfig; not wired into CI
```

CI (`.github/workflows/ci.yml`) runs restore → `build -c Release` → `test -c Release` →
`pack` on .NET **8.0.x**, and uploads the `.nupkg` as an artifact so a pack-time break surfaces
on every push rather than at tag time.

## Conventions

- **`.editorconfig` is the style source.** UTF-8, LF, 4-space indent for C# (2 for json/yml/md),
  final newline, no trailing whitespace, `csharp_style_namespace_declarations = file_scoped`
  (as a **warning**, and warnings are errors), `dotnet_sort_system_directives_first = true`.
  There is no analyzer package and no StyleCop — the compiler plus `.editorconfig` is the whole
  enforcement story, so keep the build clean.
- File-scoped namespaces everywhere. Nullable annotations are meaningful, not decorative.
- `GenerateDocumentationFile` is on with `CS1591` suppressed: XML doc comments are expected on
  the public API (with `<example><code>` on the client and main resources) but not demanded on
  every member. No narrated docs on obvious code; short "why" comments only.
- Response types derive from `FoPostModel`; request shapes are option objects in
  `Resources/*Options.cs` using `Optional<T>` for patchable fields.
- Async methods end in `Async` and take a trailing `CancellationToken cancellationToken = default`.

## Testing

- **xunit** (2.9.2) in `tests/FoPost.Tests`, assembly `FoPost.Sdk.Tests`, granted access to
  internals via `InternalsVisibleTo` in the main `.csproj`.
- **`StubHandler : HttpMessageHandler`** in `tests/FoPost.Tests/TestServer.cs` is the stub. It
  serves canned responses from a queue (`.Json(...)`, `.TooManyRequests(retryAfter)`), records
  every request and its body, and throws `InvalidOperationException` when nothing is queued — so
  an unintended call fails the test rather than reaching the network.
- `TestClient` wires a `FoPostClient` to that handler with base URL `https://api.test.fopost.com`
  and an API key of `fp_test_key`, and replaces `Client.Transport.Delay` with a recorder, so
  `HttpTests` asserts exact retry waits with no clock involved.
- `Fixtures.Post` (snake_case) and `Fixtures.Account` (camelCase) deliberately differ in casing
  to pin the alias resolver.
- **Tests never hit the live API.** No network call in the suite, ever, in CI or locally. If a
  change cannot be tested through `StubHandler`, the change is in the wrong layer.

## Releasing

**`FoPost.Sdk` is NOT yet on nuget.org.** `.github/workflows/release.yml` exists and is ready:
it triggers on a `v*` tag (or manual dispatch), runs in the `nuget` GitHub environment, verifies
the tag matches `<Version>` in `src/FoPost/FoPost.csproj`, builds, tests, packs, then runs
`dotnet nuget push "artifacts/*.nupkg" --source https://api.nuget.org/v3/index.json --skip-duplicate`.

**Repo secret referenced by `release.yml`** (on the `nuget` environment):

- `NUGET_API_KEY`

First publish also requires, outside GitHub:

1. A nuget.org account, and the `FoPost.Sdk` package id available or already reserved (an ID
   prefix reservation for `FoPost.*` is worth doing before the first push).
2. An **API key** created on nuget.org scoped to *Push* for that id, stored as `NUGET_API_KEY`.
   Keys expire — a failed release with a 403 is usually an expired key, not a bad package.
3. The `nuget` environment created in GitHub repo settings (it gates who can trigger a release).

The package publishes symbols too (`IncludeSymbols` + `snupkg`), so the `.snupkg` beside the
`.nupkg` is expected in `artifacts/` and is pushed by the same glob.

`CHANGELOG.md` gets an entry per release; the version bump and the entry land in the same commit.

## Git

- **Conventional Commits** `<type>(<scope>): <description>` — one logical change per commit.
- Branch `feature/<description>` off a fresh `main`; merge to `main` via PR.
- **Never run `gh pr create`.** Push the branch and hand over the compare link:
  `https://github.com/fopost/fopost-dotnet/compare/main...<branch>`
- Never `git stash` — use a worktree for parallel work.
