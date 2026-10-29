namespace UniScan.Network;

public class Constants
{
    public const int ProtocolVersion = 10;
    public const ushort DefaultPort = 'U' | ('S' << 2) | ('C' << 4) | ('N' << 6);//This packing really means nothing other than being an attempt at getting a nice port number
}