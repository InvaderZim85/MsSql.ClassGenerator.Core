using System.Text.Json;
using MsSql.ClassGenerator.Core;
using MsSql.ClassGenerator.Core.Common.Enums;
using MsSql.ClassGenerator.Core.Model;
using MsSql.ClassGenerator.Core.Model.Database;

namespace Demo;

internal static class Program
{
    private static async Task Main()
    {
        // NOTE:
        // The following code demonstrates how to load tables from a database. To keep the demo as simple as possible,
        // it includes a JSON file with a few sample tables.

        // Load the tables
        //var tableManager = new TableManager("(localdb)\\MsSqlLocalDb", "YourDatabase");
        //tableManager.ProgressEvent += (_, message) => Console.WriteLine(message);
        //var tables = await tableManager.LoadTablesAsync();

        // Load the dummy tables
        var tables = await LoadDummyTablesAsync();

        // Create the code for the classes
        var classManager = new ClassManager();
        classManager.ProgressEvent += (_, message) => Console.WriteLine(message);
        var generatorResult = await classManager.GenerateClassesAsync(tables, new ClassGeneratorOptions
        {
            Namespace = "TestNamespace",
            SealedClass = false,
            Modifier = ClassModifier.Internal,
            DbModel = true,
            AddColumnAttribute = false,
            WithBackingField = false,
            AddSetProperty = false,
            AddSummary = true,
            AddTableNameToClassSummary = true,
            AdditionalInformationFilepath = Path.Combine(AppContext.BaseDirectory, "TableInformation.json")
        });

        Console.WriteLine();
        Console.WriteLine("Result");
        Console.WriteLine("======");
        foreach (var classEntry in generatorResult.Classes)
        {
            Console.WriteLine($"Class: {classEntry.Name}, Table: {classEntry.TableName}");
            Console.WriteLine("Code:");
            Console.WriteLine("-----");
            Console.WriteLine(classEntry.Code);
            Console.WriteLine();
        }
    }

    private static async Task<List<TableEntry>> LoadDummyTablesAsync()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "DummyTables.json");
        var stream = File.OpenRead(path);
        var result = await JsonSerializer.DeserializeAsync<List<TableEntry>>(stream);
        return result!;
    }
}
