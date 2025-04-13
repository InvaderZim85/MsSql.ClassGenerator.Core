using System.Text.Json;

namespace MsSql.ClassGenerator.Core.Common;

/// <summary>
/// Provides several helper functions.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Checks if the value matches the desired filter.
    /// </summary>
    /// <param name="value">The value (for example the name of a table).</param>
    /// <param name="filter">The desired filter.</param>
    /// <returns><see langword="true"/> when the filter matches the value, otherwise <see langword="false"/></returns>
    public static bool MatchFilter(this string value, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return true; // No filter.

        var start = filter.StartsWith('*');
        var end = filter.EndsWith('*');

        // Remove the wildcard
        filter = filter.Replace("*", "");

        return start switch
        {
            true when end => value.Contains(filter, StringComparison.InvariantCultureIgnoreCase),
            true => value.EndsWith(filter, StringComparison.InvariantCultureIgnoreCase),
            false when end => value.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase),
            _ => value.Equals(filter, StringComparison.InvariantCultureIgnoreCase)
        };
    }

    /// <summary>
    /// Creates a clone of the provided data.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="data">The original data.</param>
    /// <returns>The clone of the data.</returns>
    public static async Task<T> CloneAsync<T>(this T data)
    {
        var stream = new MemoryStream();

        await JsonSerializer.SerializeAsync(stream, data);

        // Reset the position
        stream.Position = 0;

        var result = await JsonSerializer.DeserializeAsync<T>(stream);
        return result!;
    }

    /// <summary>
    /// Converts the first char of a string to lower case
    /// </summary>
    /// <param name="value">The original value</param>
    /// <returns>The converted string</returns>
    public static string FirstCharToLower(this string value)
    {
        return ChangeFirstChart(value, false);
    }

    /// <summary>
    /// Converts the first char of a string to upper case
    /// </summary>
    /// <param name="value">The original value</param>
    /// <returns>The converted string</returns>
    public static string FirstChatToUpper(this string value)
    {
        return ChangeFirstChart(value, true);
    }

    /// <summary>
    /// Changes the casing of the first char
    /// </summary>
    /// <param name="value">The original value</param>
    /// <param name="upper"><see langword="true"/> to convert the first char to upper, <see langword="false"/> to convert the first char to lower</param>
    /// <returns>The converted string</returns>
    private static string ChangeFirstChart(string value, bool upper)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return upper
            ? $"{value[..1].ToUpper()}{value[1..]}"
            : $"{value[..1].ToLower()}{value[1..]}";
    }

    /// <summary>
    /// Checks if the value starts with a number which is illegal in C#.
    /// </summary>
    /// <param name="value">The value which should be checked.</param>
    /// <returns><see langword="true"/> when the value starts with a number, otherwise <see langword="false"/>.</returns>
    public static bool StartsWithNumber(this string value)
    {
        return !string.IsNullOrWhiteSpace(value) && int.TryParse(value[0].ToString(), out _);
    }
}
