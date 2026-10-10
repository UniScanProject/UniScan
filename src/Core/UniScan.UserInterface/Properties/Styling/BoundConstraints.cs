using System.Numerics;
using MessagePack;

namespace UniScan.UserInterface.Properties.Styling;

[MessagePackObject]
public class BoundConstraints
{
    public static readonly BoundConstraints NaN = new();
    
    [Key(1)] public Vector2 MinBounds { get; set; } = Vector2.NaN; //NOTE: if any element is NaN, treated as null.
    [Key(2)] public Vector2 MaxBounds { get; set; } = Vector2.NaN; //NOTE: if any element is NaN, treated as null.
}