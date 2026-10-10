using MessagePack;

namespace UniScan.UserInterface.Properties.Styling;

[MessagePackObject]
public class UIControlPositionPreferences
{
    [Key(0)] public VerticalPosition VerticalPosition { get; set; } = VerticalPosition.Stretch;
    [Key(1)] public HorizontalPosition HorizontalPosition { get; set; } = HorizontalPosition.Stretch;
}