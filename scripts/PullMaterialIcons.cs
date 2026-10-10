#!/usr/bin/env dotnet

#:package Serilog@4.4.1-dev-02443
#:package Serilog.Sinks.Console@6.1.1

using Serilog;

string path = Environment.CurrentDirectory;

if (args.Length > 0)
{
    path = args[0];
}

Log.Logger = new LoggerConfiguration().WriteTo
                                      .Console(outputTemplate:
                                               "{Timestamp:HH:mm:ss} [{Level:u3} | {SourceContext}] {Message:lj}{NewLine}{Exception}")
                                      .CreateLogger()
                                      .ForContext("SourceContext", Path.GetFileName(Environment.ProcessPath));

string dst = Path.Combine(path, "external", "MaterialSymbols");
string f = Path.Combine(dst, "MaterialSymbolsRounded.ttf");

if (Directory.Exists(dst) && File.Exists(f))
{
    Log.Information("Material Symbols already fetched");
    return;
}

Directory.CreateDirectory(dst);

HttpClient httpClient = new();

Log.Information("Downloading material symbols...");
await using Stream s = await httpClient.GetStreamAsync("https://raw.githubusercontent.com/google/material-design-icons/refs/heads/master/variablefont/MaterialSymbolsRounded%5BFILL%2CGRAD%2Copsz%2Cwght%5D.ttf");
await using FileStream fs = new(f, FileMode.Create);

await s.CopyToAsync(fs);