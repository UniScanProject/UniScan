using System.Text;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace UniScan.Network.CodeGenerator.Generator;

[Generator]
public class FactoryConstructableGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            StringBuilder sb = new();

            sb.AppendLine("//GENERATED")
              .AppendLine()
              .AppendLine("namespace UniScan.Network.Request.Factory;")
              .AppendLine()
              .AppendLine("using UniScan.Network.Packet.PayloadPart;")
              .AppendLine();

            for (int i = 0; i < 16 + 1; i++)
            {
                var p = Enumerable.Range(1, i).ToDictionary(n => n, n => $"T{n}");

                sb.AppendLine("/// <summary>")
                  .AppendLine("/// Used by RequestConstructorAttribute for creating a Request with an automatic request ID")
                  .AppendLine("/// </summary>")
                  .AppendLine("/// <typeparam name=\"TSelf\">The request</typeparam>")
                  .AppendLine("/// <typeparam name=\"TResponse\">The response type</typeparam>");

                foreach (var tp in p)
                {
                    sb.AppendLine($"/// <typeparam name=\"{tp.Value}\">The {tp.Key.ToOrdinalWords()} param of your constructor</typeparam>");
                }

                string ps = string.Join(", ", p.Values.Select(s => $"in {s}").Prepend("out TSelf, TResponse"));
                string fp = string.Join(", ", p.Select(n => $"{n.Value} {n.Key.ToOrdinalWords().Replace(" ", "-")}"));
                sb.AppendLine($"public interface IRequestFactoryConstructable<{ps}>")
                    .AppendLine("    where TResponse : IPacket, IResponsePayloadPart")
                    .AppendLine($"    where TSelf : IRequestFactoryConstructable<{string.Join(", ", p.Values.Prepend("TSelf, TResponse"))}>, IRequestPayloadPart<TResponse>, allows ref struct");
                foreach (var tp in p)
                {
                    sb.AppendLine($"    where {tp.Value} : allows ref struct");
                }

                sb.AppendLine("{")
                  .AppendLine("    /// <summary>")
                  .AppendLine("    /// Creates an instance of TSelf")
                  .AppendLine("    /// </summary>")
                  .AppendLine("    /// <returns>The created TSelf instance</returns>")
                  .AppendLine($"    public static abstract TSelf CreateRequest({fp});")
                  .AppendLine("}")
                  .AppendLine()
                  .AppendLine();
            }

            ctx.AddSource("IRequestFactoryConstructable.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        });
    }
}