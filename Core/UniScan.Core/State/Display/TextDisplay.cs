namespace UniScan.Core.State.Display;

public struct DisplaySlot
{
    public struct SlotStyle
    {
        public ConsoleColor Color { get; set; }

        public bool Underlined { get; set; }
    }
 
    public char Character { get; set; }

    public required SlotStyle Style { get; set; } 
}

public class TextDisplay(int width, int height) : IDisplay
{
    private DisplaySlot[] _slots = new DisplaySlot[width * height];
    public DisplaySlot[] Slots => _slots;

    public int Width { get; private set; } = width;
    public int Height { get; private set; } = height;

    public void Resize(int width, int height)
    {
        Array.Resize(ref _slots, width * height);

        this.Width = width;
        this.Height = height;
    }

    public void SetSlot(int x, int y, DisplaySlot slot) => _slots[y * Width + x] = slot;
    public DisplaySlot GetSlot(int x, int y) => _slots[y * Width + x];
}