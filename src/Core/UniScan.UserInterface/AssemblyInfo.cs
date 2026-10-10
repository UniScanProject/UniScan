using MessagePack;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Serialization.Polymorphism;
using Shiki.Extensions.MessagePack.Formatter.Identity;
using Shiki.Extensions.MessagePack.Formatter.Identity.Slug;
using Shiki.Extensions.MessagePack.Formatter.Polymorphic;
using Shiki.Internationalization.Component;
using UniScan.UserInterface.Formatting;

//https://github.com/MessagePack-CSharp/MessagePack-CSharp/issues/2133
[assembly: MessagePackKnownFormatter(typeof(SlugMessagePackFormatter<SnakeSlugFormatter>))]
[assembly: MessagePackKnownFormatter(typeof(SlugMessagePackFormatter<DashSlugFormatter>))]
[assembly: MessagePackKnownFormatter(typeof(IdentifierMessagePackFormatter))]
[assembly: MessagePackKnownFormatter(typeof(ColorMessagePackFormatter))]
[assembly: MessagePackKnownFormatter(typeof(PolymorphicMessagePackFormatter<ITextComponent>))]