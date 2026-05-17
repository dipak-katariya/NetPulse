using Microsoft.Win32;

namespace NetPulse.Core;

public static class SystemTheme
{
    private const string PersonalizeKey =
        @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    public static bool IsDarkMode()
    {
        try
        {
            object? raw = Registry.GetValue(PersonalizeKey, "AppsUseLightTheme", defaultValue: 1);
            return raw is int i && i == 0;
        }
        catch (System.Security.SecurityException)
        {
            return true;
        }
        catch (IOException)
        {
            return true;
        }
    }
}
