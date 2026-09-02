namespace UniScan.Tests;

public static class ArrayExtensions
{
    extension(byte[] arr)
    {
        public string ToHexViewString() => string.Join(Environment.NewLine, arr.Chunk(16)
                                                          .Select(chunk => string.Join(" ", Convert.ToHexString(chunk)
                                                                              .Chunk(2)
                                                                              .Select(c => new string(c)))));
    }
}