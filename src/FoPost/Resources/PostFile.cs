namespace FoPost;

/// <summary>A file to attach to a post: streamed as multipart when small, uploaded directly when large.</summary>
public sealed class PostFile : IDisposable
{
    public PostFile(string fileName, string mimeType, Stream content, long length)
    {
        ArgumentNullException.ThrowIfNull(content);

        FileName = fileName;
        MimeType = mimeType;
        Content = content;
        Length = length;
    }

    public string FileName { get; }

    public string MimeType { get; }

    public Stream Content { get; }

    /// <summary>Exact byte length of <see cref="Content"/>.</summary>
    public long Length { get; }

    /// <summary>Open a file on disk; dispose the <see cref="PostFile"/> once the post is sent.</summary>
    public static PostFile FromPath(string path, string mimeType)
    {
        var stream = File.OpenRead(path);
        return new PostFile(Path.GetFileName(path), mimeType, stream, stream.Length);
    }

    public void Dispose() => Content.Dispose();

    /// <summary>The media item type the API infers for this file.</summary>
    internal string MediaType
    {
        get
        {
            if (MimeType.Equals("image/gif", StringComparison.OrdinalIgnoreCase))
            {
                return "gif";
            }

            return MimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ? "video" : "image";
        }
    }
}
