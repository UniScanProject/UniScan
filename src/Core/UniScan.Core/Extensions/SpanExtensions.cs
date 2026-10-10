using System.Text;

namespace UniScan.Core.Extensions;

public readonly record struct HexViewStringArgs
{
    public readonly record struct OffsetOptions
    {
        public int OffsetBytes { get; init; }
        public bool Capitalize { get; init; }

        public char CapitalizationChar => Capitalize ? 'X' : 'x';
    }

    public bool Capitalize { get; init; }
    public char CapitalizationChar => Capitalize ? 'X' : 'x';
    public OffsetOptions Offset { get; init; }

    public bool Colored { get; init; }
    public bool IncludeHeader { get; init; }
    public bool IncludeAscii { get; init; }
    public bool UseSpacing { get; init; }
    
    public static readonly HexViewStringArgs Default = new()
    {
        Capitalize = false,
        Offset = new()
        {
            OffsetBytes = 4,
            Capitalize = false,
        },
        Colored = false,
        IncludeHeader = true,
        IncludeAscii = true,
        UseSpacing = true,
    };
    
    public static readonly HexViewStringArgs Basic = new()
    {
        Capitalize = false,
        Offset = new()
        {
            OffsetBytes = 0,
            Capitalize = false,
        },
        Colored = false,
        IncludeHeader = false,
        IncludeAscii = false,
        UseSpacing = false,
    };
    
    public static readonly HexViewStringArgs DefaultConsole = Default with
    {
        Colored = true
    };
}

public static class SpanExtensions
{
    extension(ReadOnlySpan<byte> span)
    {
        public string ToHexViewString() => span.ToHexViewString(HexViewStringArgs.DefaultConsole);

        public string ToHexViewString(HexViewStringArgs args)
        {
            if (span.Length == 0)
                return string.Empty;

            StringBuilder sb = new();
            const int bytes = 16;

            int off = 0;
            
            string colorReset = args.Colored ? "\e[0m" : string.Empty;
            string colorGray = args.Colored ? "\e[90m" : string.Empty;
            string colorCyan = args.Colored ? "\e[36m" : string.Empty;

            if (args.IncludeHeader)
            {
                sb.Append('\n');

                if (args.Offset.OffsetBytes > 0)
                {
                    sb.Append(' ', args.Offset.OffsetBytes * 2)
                      .Append("  ");
                }

                sb.Append(colorCyan)
                  .Append(string.Join(' ', Enumerable.Range(0, 8).Select(i => i.ToString($"{args.CapitalizationChar}2"))))
                  .Append(args.UseSpacing ? "  " : ' ')
                  .Append(string.Join(' ', Enumerable.Range(8, 8).Select(i => i.ToString($"{args.CapitalizationChar}2"))))
                  .Append(colorReset);
            }

            for (int i = 0; i < span.Length; i += bytes)
            {
                sb.Append('\n');
                int c = Math.Min(bytes, span.Length - i);
                ReadOnlySpan<byte> chunk = span.Slice(i, c);

                if (args.Offset.OffsetBytes > 0)
                {
                    sb.Append(colorCyan).Append(off.ToString($"{args.Offset.CapitalizationChar}{args.Offset.OffsetBytes * 2}")).Append(colorReset)
                      .Append("  ");
                }

                for (int i2 = 0; i2 < bytes; i2++)
                {
                    if (i2 < chunk.Length)
                    {
                        byte b = chunk[i2];
                        sb.Append(b == 0x00 ? colorGray : colorReset).Append(chunk[i2].ToString($"{args.CapitalizationChar}2"))
                          .Append(colorReset);
                    }
                    else
                    {
                        sb.Append("  ");
                    }

                    sb.Append(' ');
                    if (args.UseSpacing)
                    {
                        if (i2 == (bytes / 2) - 1)
                        {
                            sb.Append(' ');
                        }
                    }
                }

                if (args.IncludeAscii)
                {
                    sb.Append(colorCyan).Append(" |").Append(colorReset);

                    for (int i2 = 0; i2 < bytes; i2++)
                    {
                        if (i2 < chunk.Length)
                        {
                            byte b = chunk[i2];

                            bool within = b is >= 32 and <= sbyte.MaxValue - 1;
                            char ch = within ? (char)b : '·';

                            sb.Append(!within ? colorGray : string.Empty).Append(ch).Append(colorReset);
                        }
                        else
                        {
                            sb.Append(' ');
                        }
                    }

                    sb.Append(colorCyan).Append('|').Append(colorReset);
                }

                off += bytes;
            }

            return sb.ToString();
        }
    }
}