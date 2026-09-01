#!/usr/bin/env dotnet
#:property JsonSerializerIsReflectionEnabledByDefault=true
#:property DisableMSBuildAssemblyCopyCheck=true
#:package Microsoft.Build@18.9.6
#:package Microsoft.Build.Locator@1.11.2
#:package LibGit2Sharp@0.32.0
#:package Serilog@4.4.1-dev-02443
#:package Serilog.Sinks.Console@6.1.1

using System.Collections.Immutable;
using LibGit2Sharp;
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

    string pwd = Environment.CurrentDirectory;
    using (Repository repo = new(Path.Combine(pwd, ".git")))
    {
        foreach (Project project in projects)
        {
            string name = project.GetPropertyValue("MSBuildProjectName");
            string ver = project.GetPropertyValue("AssemblyVersion");
            if (string.IsNullOrWhiteSpace(ver))
            {
                Log.Error("Project {Project} has no AssemblyVersion", name);
                continue;
            }
            
            string tag = $"{name}/{ver}";
            if (repo.Tags[tag] == null)
            {
                repo.Tags.Add(tag, repo.Commits.First());
                Log.Information("Added tag {tag}", tag);
            }
        }
    }
}