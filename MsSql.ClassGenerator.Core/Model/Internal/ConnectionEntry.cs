namespace MsSql.ClassGenerator.Core.Model.Internal;

/// <summary>
/// Contains the data of the desired connection.
/// </summary>
/// <remarks>
/// <b>Note</b>: If the username is empty, <c>IntegratedSecurity</c> will be used.
/// </remarks>
/// <param name="Server">The name / path of the MS SQL server.</param>
/// <param name="Database">The name of the database.</param>
/// <param name="User">The name of the user.</param>
/// <param name="Password">The password of the user.</param>
internal sealed record ConnectionEntry(string Server, string Database, string User, string Password);
