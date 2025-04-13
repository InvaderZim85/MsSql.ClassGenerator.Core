namespace MsSql.ClassGenerator.Core.Model.Internal;

/// <summary>
/// Provides the type conversion information.
/// </summary>
/// <param name="SqlType">The name of the SQL data type.</param>
/// <param name="CsharpType">The name of the C# data type.</param>
public sealed record TypeConversionDto(string SqlType, string CsharpType);