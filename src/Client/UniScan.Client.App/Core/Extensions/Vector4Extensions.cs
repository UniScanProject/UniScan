using System.Numerics;
using Avalonia;

namespace UniScan.Client.App.Core.Extensions;

public static class Vector4Extensions
{
    extension(Vector4 v)
    {
        public Thickness AsThickness() => new(v.X, v.Y, v.Z, v.W);
        public CornerRadius AsCornerRadius() => new(v.X, v.Y, v.Z, v.W);

    }
}