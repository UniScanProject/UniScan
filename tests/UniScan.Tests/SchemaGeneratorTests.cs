using System.Text.Json;
using Serilog;
using Shiki.Common.Serialization.Polymorphism;
using Shiki.Common.Serialization.Polymorphism.Source.Sources;
using Shiki.Tests.Util;
using UniScan.Client.Core.Config.Remote;

namespace UniScan.Tests;

public class SchemaGeneratorTests
{
    private SchemaGenerator _schemaGenerator;
    
    [SetUp]
    public void Setup()
    {
        var types = AssembliesPolymorphicTypeSource.Load();
        foreach (var type in types)
        {
            Log.Debug("Registering polymorphic type '{Id}' ({Type}) for base '{BaseType}'", type.Value.Id, type.Key.Name, type.Value.BaseType);
        }

        JsonSerializerOptions opt = new()
        {
            TypeInfoResolver = new PolymorphicTypeInfoResolver(types)
        };
        
        this._schemaGenerator = new SchemaGenerator(opt);
    }
    
    [Test, Explicit($"View the output schema for {nameof(RemoteDto)}")]
    public async Task GenerateRemoteServerSchema() => Assert.Pass(await _schemaGenerator.GenerateSchema<RemoteDto>());
}