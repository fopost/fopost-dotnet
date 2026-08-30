namespace FoPost;

/// <summary>
/// "The caller did not pass this", so a partial update sends only the fields
/// that were named. Distinct from null, which clears a field.
/// </summary>
/// <example>
/// <code>
/// // Retitles the post and clears its summary; everything else is untouched.
/// await client.Posts.UpdateAsync(id, new UpdatePostOptions
/// {
///     Title = "New title",
///     Summary = Optional&lt;string?&gt;.Of(null),
/// });
/// </code>
/// </example>
public readonly struct Optional<T> : IEquatable<Optional<T>>
{
    private readonly T? _value;

    private Optional(T? value)
    {
        _value = value;
        IsSet = true;
    }

    /// <summary>True when the caller named this field, even to set it to null.</summary>
    public bool IsSet { get; }

    /// <summary>The value the caller passed. Meaningless unless <see cref="IsSet"/>.</summary>
    public T? Value => _value;

    /// <summary>Wrap a value, including null, as "the caller passed this".</summary>
    public static Optional<T> Of(T? value) => new(value);

    /// <summary>The absent value — the field is left out of the request entirely.</summary>
    public static Optional<T> Unset => default;

    public static implicit operator Optional<T>(T? value) => new(value);

    public bool Equals(Optional<T> other) =>
        IsSet == other.IsSet && EqualityComparer<T?>.Default.Equals(_value, other._value);

    public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(IsSet, _value is null ? 0 : EqualityComparer<T?>.Default.GetHashCode(_value));

    public override string ToString() => IsSet ? _value?.ToString() ?? "null" : "unset";

    public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);

    public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);
}
