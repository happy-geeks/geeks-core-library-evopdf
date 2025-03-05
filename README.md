# GeeksCoreLibrary.Modules.GclConverters.EvoPdf
The Geeks Core Library has functionality for converting HTML to PDF files. This functionality can be used by injecting `IHtmlToPdfConverterService` into your class.

The default implementation in the GCL (`HtmlToPdfConverterService`) always returns `null` from the `ConvertHtmlStringToPdfAsync` method. This is because the GCL doesn't include any PDF converter dependencies. The default implementation is there to show how to use the `IHtmlToPdfConverterService` interface and to load HTML to PDF conversion settings from the database. The current library is an implementation of the `IHtmlToPdfConverterService` interface that uses the EvoPdf library to convert HTML to PDF.

## Installation
This package depends on `EvoPdf.Chromium` package. This is a paid library, so it's not included in the NuGet package of the GCL. You have to add a reference to it in your own project. If you don't, you'll get an exception when you try to use the `IHtmlToPdfConverterService`. If you don't want to use Evo PDF, you can create your own implementation of `IHtmlToPdfConverterService` and use a different library there.

If you do want to use Evo PDF, you need to add a reference to `EvoPdf.Chromium.Windows` and/or `EvoPdf.Chromium.Linux`, depending on which OS your application will run. It's also possible to load the correct package dynamically, based on the OS. That can be done by adding the following to your `.csproj` file:
```xml
<!-- Default PackageReference when no RuntimeIdentifier is specified -->
<ItemGroup Condition="'$(RuntimeIdentifier)' == '' AND $([MSBuild]::IsOsPlatform('Windows'))">
    <PackageReference Include="EvoPdf.Chromium.Windows" Version="11.4.5" />
</ItemGroup>

<ItemGroup Condition="'$(RuntimeIdentifier)' == '' AND $([MSBuild]::IsOsPlatform('Linux'))">
    <PackageReference Include="EvoPdf.Chromium.Linux" Version="11.4.1" />
</ItemGroup>

<!-- Windows-specific dependency -->
<ItemGroup Condition="'$(RuntimeIdentifier)' == 'win-x64'">
    <PackageReference Include="EvoPdf.Chromium.Windows" Version="11.4.5" />
</ItemGroup>

<!-- Linux-specific dependency -->
<ItemGroup Condition="'$(RuntimeIdentifier)' == 'linux-x64'">
    <PackageReference Include="EvoPdf.Chromium.Linux" Version="11.4.1" />
</ItemGroup>
```

To add this implementation to the dependency injection container, add the following line just below the call to `AddGclServices`:
```c#
builder.Services.AddEvoPdfHtmlToPdfConverterService();
```
