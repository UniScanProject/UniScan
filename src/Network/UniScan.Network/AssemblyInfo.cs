using MessagePack;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Extensions.MessagePack.Formatter.Identity.Slug;
using Shiki.Extensions.MessagePack.Formatter.Polymorphic;
using Shiki.Internationalization;
using Shiki.Internationalization.Component;

//https://github.com/MessagePack-CSharp/MessagePack-CSharp/issues/2133
[assembly: MessagePackKnownFormatter(typeof(SlugMessagePackFormatter<SnakeSlugFormatter>))]
[assembly: MessagePackKnownFormatter(typeof(PolymorphicMessagePackFormatter<ITextComponent>))]