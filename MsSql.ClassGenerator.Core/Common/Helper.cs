using System.Text.Json;

namespace MsSql.ClassGenerator.Core.Common;

/// <summary>
/// Provides several helper functions.
/// </summary>
internal static class Helper
{
    /// <summary>
    /// The options for the JSON serializer.
    /// </summary>
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Load the content of a JSON formatted file.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="filepath">The path of the file which contains the data.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Will be thrown when the specified filepath is empty.</exception>
    /// <exception cref="FileNotFoundException">Will be thrown when the specified file doesn't exist.</exception>
    public static async Task<T> LoadJsonAsync<T>(string filepath) where T : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filepath);

        if (!File.Exists(filepath))
            throw new FileNotFoundException($"The specified file '{filepath}' doesn't exist.", filepath);

        await using var stream = File.OpenRead(filepath);
        return await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions) ?? new T();
    }
}
