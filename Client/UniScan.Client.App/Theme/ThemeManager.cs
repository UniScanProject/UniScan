namespace UniScan.Client.App.Theme;

public class ThemeManager(ISystemAccentSource systemAccentSource)
{
    public ISystemAccentSource AccentSource { get; private set; } = systemAccentSource;

    public void SetAccentSource(ISystemAccentSource accentSource)
    {
        AccentSource = accentSource;
    }
}