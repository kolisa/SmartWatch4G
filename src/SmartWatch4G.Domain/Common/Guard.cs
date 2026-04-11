namespace SmartWatch4G.Domain.Common;

/// <summary>
/// Lightweight argument validation helpers for domain entities.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Throws <see cref="ArgumentException"/> when <paramref name="value"/> is
    /// <c>null</c> or whitespace.
    /// </summary>
    internal static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"'{paramName}' must not be null or empty.", paramName);
        }

        return value;
    }
}
