using MessagePack;
using Shiki.Common.Extensions;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Core.Extensions;
using UniScan.UserInterface;
using UniScan.UserInterface.Definitions;

namespace UniScan.Tests;

public class UserInterfaceTests
{
    public readonly IUINode node = new ContainerUIControl(
        new TextBlockUIControl("Hello, world 1!")
        {
          FontSize = 16
        },
        new TextBlockUIControl("Hello, world 2!"),
        new TextBlockUIControl("Hello, world 3!"),
        new TextBlockUIControl("Hello, world 4!")
    )
    {
        Id = "parent".ToSlug<DashSlugFormatter>()
    };

    [Test]
    public void PrintUI()
    {
        Console.WriteLine(node.ToString());
    }
    
    [Test]
    public async Task SerializeUI()
    {
        MemoryStream ms = new MemoryStream();
        await MessagePackSerializer.SerializeAsync(ms, node);

        byte[] arr = ms.ToArray();
        Console.WriteLine(arr.ToHexViewString());
    }
    
    [Test]
    public async Task DeserializeUI()
    {
        MemoryStream ms = new MemoryStream();
        await MessagePackSerializer.SerializeAsync(ms, node);
        ms.Seek(0, SeekOrigin.Begin);
        
        IUINode n = MessagePackSerializer.Deserialize<IUINode>(ms);
        Console.WriteLine(n.ToString());
    }
}