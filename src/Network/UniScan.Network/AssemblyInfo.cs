using MessagePack;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Extensions.MessagePack.Formatter.Identity.Slug;

//https://github.com/MessagePack-CSharp/MessagePack-CSharp/issues/2133
[assembly: MessagePackKnownFormatter(typeof(SlugMessagePackFormatter<SnakeSlugFormatter>))]