# MsSql.ClassGenerator.Core

This class library offers the possibility to generate a C# class from an SQL table.

The code originally comes from the [MsSqlToolBelt](https://github.com/InvaderZim85/MsSqlToolBelt) project, which was optimized for the [MsSql.ClassGenerator](https://github.com/InvaderZim85/MsSql.ClassGenerator) project. Since the functions are spread across two different projects, it was time to extract the code and provide it as a standalone library.

The class library is quite simple and contains the following two non-static classes:

1. `TableManager`: Provides the function to load table information.
2. `ClassManager`: Provides the functions to generate C# classes from the table information (point 1).

**ToDos**

1. Testing: Testing of all different combinations of the options.
2. Release: Release of the first version.

## Example

```csharp
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
```

For a detailed example, see the demo project.

## Additional information

It is possible to provide additional information for a class/property. This is done via a JSON file with the following structure:

```json
[
  {
    "Table": "Book",
    "Column": null,
    "Summary": "Represents a single book.",
    "Remarks": "Test remarks entry."
  }
]
```

| Key | Data type | Description |
|---|---|---|
| Table | `string` | The name of the desired table. |
| Column | `string?` (nullable) | The name of the column. |
| Summry | `string` | The desired text for the summary. |
| Remarks | `string` | The desired text for the remarks. |

> **Note**: The information for a class is indicated by the value `null` for the *Column*.