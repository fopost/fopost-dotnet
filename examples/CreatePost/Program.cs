// Create a post and publish it, end to end.
//
//   export FOPOST_API_KEY=fp_...
//   dotnet run --project examples/CreatePost
//
// The key needs the `posts` and `accounts` scopes.

using FoPost;

using var client = new FoPostClient();

var workspaces = await client.Workspaces.ListAsync();
if (workspaces.Count == 0)
{
    Console.Error.WriteLine("No workspaces on this key.");
    return 1;
}

var workspace = workspaces[0];
Console.WriteLine($"Workspace: {workspace.Name} ({workspace.Id})");

var accounts = await client.Accounts.ListAsync(workspace.Id);
if (accounts.Count == 0)
{
    Console.Error.WriteLine("Connect an account in the dashboard first.");
    return 1;
}

foreach (var account in accounts)
{
    Console.WriteLine($"  {account.Platform}: @{account.Username} ({account.Id})");
}

var post = await client.Posts.CreateAsync(new CreatePostOptions
{
    WorkspaceId = workspace.Id,
    Content = new List<PostContent> { new("Hello from the FoPost .NET SDK") },
    Accounts = accounts.Select(account => account.Id).ToList(),
});

Console.WriteLine($"Created draft {post.Id}");

// Preflight reports per-account blockers without sending anything.
var preflight = await client.Posts.PreflightAsync(post.Id);
Console.WriteLine($"Preflight: {preflight}");

// Uncomment to actually publish.
// await client.Posts.PublishAsync(post.Id);
// Console.WriteLine("Published.");

return 0;
