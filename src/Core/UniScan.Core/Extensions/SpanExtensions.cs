using System.Text;

namespace UniScan.Core.Extensions;

public static class SpanExtensions
{
    extension(ReadOnlySpan<byte> span)
    {
        public string ToHexViewString()
        {
            StringBuilder sb = new();
            const int bytes = 16;

            int off = 0;

            for (int i = 0; i < span.Length; i += bytes)
            {
                int c = Math.Min(bytes, span.Length - i);
                ReadOnlySpan<byte> chunk = span.Slice(i, c);

                //partially based from https://stackoverflow.com/a/14333437
                sb.Append($"\n{off.ToString("X4")}  {string.Create(bytes * 3, chunk, (res, sp) => {
                    int resPos = 0;

                    for (int i2 = 0; i2 < sp.Length; i2++) {
                        //should format into the res?
                        sp[i2].TryFormat(res[resPos..], out int o1, "X2");
                        resPos += o1;

                        //if not last element in span
                        if (i2 < sp.Length - 1) {
                            res[resPos] = ' ';
                            resPos += 1;
                        }
                    }
                })}");
                off += bytes;
            }

            return sb.ToString();
        }
    }
}