namespace UniScan.Client.App.UI.Theme;

public class ThemeManager(ISystemAccentSource systemAccentSource)
{
    public ISystemAccentSource AccentSource { get; private set; } = systemAccentSource;

    public void SetAccentSource(ISystemAccentSource accentSource)
    {
        AccentSource = accentSource;
    }
}