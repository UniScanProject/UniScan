using MessagePack;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Extensions.MessagePack.Formatter.Identity;
using Shiki.Extensions.MessagePack.Formatter.Identity.Slug;
using UniScan.UserInterface.Definitions;

//https://github.com/MessagePack-CSharp/MessagePack-CSharp/issues/2133
[assembly: MessagePackKnownFormatter(typeof(SlugMessagePackFormatter<SnakeSlugFormatter>))]
[assembly: MessagePackKnownFormatter(typeof(SlugMessagePackFormatter<DashSlugFormatter>))]
[assembly: MessagePackKnownFormatter(typeof(IdentifierMessagePackFormatter))]
