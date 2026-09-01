#!/usr/bin/env dotnet
#:property JsonSerializerIsReflectionEnabledByDefault=true
#:property DisableMSBuildAssemblyCopyCheck=true
#:package SmartFormat.NET@3.6.1
#:package Microsoft.Build@18.9.6
#:package Microsoft.Build.Locator@1.11.2

#:package Serilog@4.4.1-dev-02443
#:package Serilog.Sinks.Console@6.1.1

using System.Collections.Immutable;
using SmartFormat;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Serilog;

MSBuildLocator.RegisterDefaults();

Log.Logger = new LoggerConfiguration().WriteTo
                                      .Console(outputTemplate:
                                               "{Timestamp:HH:mm:ss} [{Level:u3} | {SourceContext}] {Message:lj}{NewLine}{Exception}")
                                      .CreateLogger()
                                      .ForContext("SourceContext", Path.GetFileName(Environment.ProcessPath));

await RunAsync();
return;

async Task RunAsync()
{
    //get csprojs
    DirectoryInfo src = new(Path.Combine(Environment.CurrentDirectory, "src"));
    IEnumerable<FileInfo> files = src.EnumerateFiles("*.csproj", SearchOption.AllDirectories);

    ImmutableArray<Project> projects = [.. files.Select(f => new Project(f.FullName))];
    foreach (Project project in projects)
    {
        Log.Information("Found project {Project}", project.GetPropertyValue("MSBuildProjectName"));
    }

    //read template
    await using FileStream stream2 = new("template/ProjectTableRow.md.template", FileMode.OpenOrCreate, FileAccess.Read);
    using StreamReader reader2 = new(stream2);

    string rowTemplate = await reader2.ReadToEndAsync();

    //generate rows
    ImmutableArray<string> rows =
    [
        .. projects.Select(p => Smart.Format(rowTemplate,
                                             new
                                             {
                                                 Name = p.GetPropertyValue("MSBuildProjectName"),
                                                 Version = p.GetPropertyValue("AssemblyVersion") ??
                                                           "Unknown",
                                                 Description =
                                                     p.GetPropertyValue("Description") ??
                                                     string.Empty,
                                                 Path =
                                                     Path.GetRelativePath(Environment
                                                                             .CurrentDirectory, p.DirectoryPath)
                                             }
                                            ))
    ];
    
    Log.Information("got {num} rows", rows.Length);

    //read readme template
    await using FileStream stream = new("template/README.md.template", FileMode.OpenOrCreate, FileAccess.Read);
    using StreamReader reader = new(stream);

    string s = await reader.ReadToEndAsync();

    //write to readme
    string formatted = Smart.Format(s, new
    {
        ProjectRows = string.Join('\n', rows)
    });

    await File.WriteAllTextAsync("README.md", formatted);
    
    Log.Information("done");
}