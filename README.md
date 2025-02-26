# AspNetCore Build Task Runner

This repository is for the AspNetCore build task runner package. The purpose of this package is to provide a simplified interface for executing tasks during the build that require access to the service's `IServiceProvider`.

## Installing the package

You can install the main package using the dotnet cli.
```
dotnet add package AspNetCoreBuildTaskRunner
```

The package can either be installed in the same project as the AspNetCore application or in a project that references it. When installed in a different project, `WebApplication` must be set to true in order to identify project that is the AspNetCore application.

```
    <ProjectReference Include="path/to/aspnetcore.csproj" WebApplication="true" />
```

## Creating plugins

See [AspNetCoreBuildTaskRunner.ExamplePlugin](test/AspNetCoreBuildTaskRunner.ExamplePlugin) for an example.

Each plugin project file should reference the AspNetCoreBuilderTaskRunner.Abstractions package with private assets set to all.

```xml
    <PackageReference Include="AspNetCoreBuildTaskRunner.Abstractions" Version="1.0.0" PrivateAssets="all" />
```

They should also implement the `IAspNetCoreBuildTaskRunnerPlugin` interface.

```csharp
public class PrintSharedClassPlugin : IAspNetCoreBuildTaskRunnerPlugin
{
    public Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Console.WriteLine("I did something");

        var cls = (SharedClass)serviceProvider.GetService(typeof(SharedClass))!;

        foreach (var c in cls.Lines)
        {
            Console.WriteLine(c);
        }

        return Task.CompletedTask;
    }
}
```

The build output assembly should be placed in a different output directory. This is to ensure that it is not referenced by the project consuming it.

```xml
  <PropertyGroup>
    <IncludeBuildOutput>false</IncludeBuildOutput>
  </PropertyGroup>

  <ItemGroup>
    <None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="aspnetcorebuildtaskrunnerplugin" Visible="false" />
  </ItemGroup>
```

If needing to access classes from AspNetCoreApp you should add a framework reference.

```xml
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
```

To access other packages also referenced by the AspNetCore app being targeted. Ensure that they are using the same version of the package and reference it with private assets set to all.

```xml
    <PackageReference Include="My.Shared.Package" Version="1.0.0" PrivateAssets="all" />
```

The plugin will require a targets file to add it to the list of plugins that need running. This should point to where the dll has been published to in the package.

```xml
<Project>
  <ItemGroup>
    <AspNetCoreBuildTaskRunnerPlugin Include="$(MSBuildThisFileDirectory)..\aspnetcorebuildtaskrunnerplugin\AspNetCoreBuildTaskRunner.ExamplePlugin.dll" />
  </ItemGroup>
</Project>
```
