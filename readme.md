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

| Property | Description |
|---|---|
| Namespace | The desired namespace which should be used. If the value is empty (e. g. `string.Empty` or `""`) only the bare class is created (without namespace and usings). |
| SealedClass | If `true`, the `sealed` modifier is added. |
| Modifier | The desired modifier. Default = `private`. |
| DbModel | If `true`, a class is created which can be used with *EF Core*. |
| AddColumnAttribute | If `true`, the `[Column("Name")]` attribute is added to each property even if it's not needed. |
| WithBackingField | If `true`, a *backing field* is created. |
| AddSetProperty | If `true`, the `SetProperty` method and the using of the [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) is added. |
| AddSummary | If `true`, a summary is added to the class / property. |
| AddTableNameToClassSummary | If `true`, the name of the table is added to the class summary. Is added as `remarks`. |
| AdditionalInformatilFilepath | The path of the file, which contains the [additional information](#additional-information). |

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

> **Note**: The information for a class is indicated by the value `null` for the *Column*. For an example see here [Example](Demo/TableInformation.json)

## Templates / Type Conversion file

After you build your project, a folder named "Files" will be created. Inside you'll find the following files:

| File | Description |
|---|---|
| `ClassDefault.cgt` | The template for a default class without a namespace (the bare *class* without a namespace and without the usings). |
| `ClassDefaultWithNs.cgt` | The template for a class with a namespace (includes the usings). |
| `PropertyBackingField.cgt` | The template for a property with a backing field. |
| `PropertyBackingFieldSetProperty.cgt` | The template for a property with a backing field and the usage of the `SetProperty` method (for more information see [Example > *AddSetProperty*](#example)). |
| `PropertyDefault.cgt` | The template for property (without backing field). |
| `TypeConverstion.json` | The file containing the information for converting an MS SQL data type to the corresponding C# data type. |

> **Note**: The content of the files can/may be changed, but the file name must not be changed, otherwise they can no longer be found and an error will occur.