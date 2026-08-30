using System.Collections;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>Pagination detail returned beside a list of results.</summary>
public sealed class PageMeta : FoPostModel
{
    [JsonPropertyName("current_page")]
    public int? CurrentPage { get; set; }

    [JsonPropertyName("per_page")]
    public int? PerPage { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("last_page")]
    public int? LastPage { get; set; }

    [JsonPropertyName("from")]
    public int? From { get; set; }

    [JsonPropertyName("to")]
    public int? To { get; set; }
}

/// <summary>One page of a list endpoint: its items plus the pagination meta.</summary>
public sealed class Page<T> : IReadOnlyList<T>
{
    public Page(IReadOnlyList<T> items, PageMeta meta)
    {
        Items = items;
        Meta = meta;
    }

    public IReadOnlyList<T> Items { get; }

    public PageMeta Meta { get; }

    public int Count => Items.Count;

    public T this[int index] => Items[index];

    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
