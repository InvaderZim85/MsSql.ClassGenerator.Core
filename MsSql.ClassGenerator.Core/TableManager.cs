using MsSql.ClassGenerator.Core.Data;
using MsSql.ClassGenerator.Core.Model.Database;
using MsSql.ClassGenerator.Core.Model.Internal;

namespace MsSql.ClassGenerator.Core;

/// <summary>
/// Provides the functions for the interaction with the tables.
/// </summary>
/// <param name="server">The name / path of the MS SQL server.</param>
/// <param name="database">The name of the database which should be used.</param>
/// <param name="user">The name of the user. <b>Note</b>: If no username is specified, <c>IntegratedSecurity</c> is used.</param>
/// <param name="password">The password of the user.</param>
public sealed class TableManager(string server, string database, string user = "", string password = "")
{
    /// <summary>
    /// Occurs when progress was made.
    /// </summary>
    public event EventHandler<string>? ProgressEvent;

    /// <summary>
    /// Contains the desired connection.
    /// </summary>
    private readonly ConnectionEntry _connectionEntry = new(server, database, user, password);

    /// <summary>
    /// Loads all available user tables (with its columns and PK information).
    /// </summary>
    /// <param name="filter">The desired filter.</param>
    /// <returns>The list with the tables.</returns>
    public async Task<List<TableEntry>> LoadTablesAsync(string filter = "")
    {
        var repo = new TableRepo(_connectionEntry);

        ProgressEvent?.Invoke(this, "Load tables.");

        var tables = await repo.LoadTablesAsync(filter);

        var count = 1;
        foreach (var table in tables)
        {
            var message = $"{count++} of {tables.Count} > Load PK information for table '{table.Name}'.";

            ProgressEvent?.Invoke(this, message);
            await repo.LoadPrimaryKeyInfoAsync(table);
        }

        return tables;
    }
}
