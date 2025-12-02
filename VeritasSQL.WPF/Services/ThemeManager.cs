using System.Windows;

namespace VeritasSQL.WPF.Services;

/// <summary>
/// Manages application themes (Light/Dark mode)
/// </summary>
public static class ThemeManager
{
    private static string _currentTheme = "Light";

    /// <summary>
    /// Applies the specified theme to the application
    /// </summary>
    public static void ApplyTheme(string themeName)
    {
        if (themeName == "System")
        {
            // Detect system theme
            themeName = IsSystemDarkMode() ? "Dark" : "Light";
        }

        _currentTheme = themeName;

        var app = Application.Current;
        if (app == null) return;

        // Remove existing theme dictionaries
        var toRemove = app.Resources.MergedDictionaries
            .Where(d => d.Source?.OriginalString.Contains("Theme") == true)
            .ToList();
        
        foreach (var dict in toRemove)
        {
            app.Resources.MergedDictionaries.Remove(dict);
        }

        // Add new theme dictionary
        var themeUri = new Uri($"pack://application:,,,/VeritasSQL.WPF;component/Themes/{themeName}Theme.xaml", UriKind.Absolute);
        
        try
        {
            var themeDictionary = new ResourceDictionary { Source = themeUri };
            app.Resources.MergedDictionaries.Add(themeDictionary);
        }
        catch
        {
            // Theme file not found, use default
        }
    }

    /// <summary>
    /// Gets the current theme name
    /// </summary>
    public static string CurrentTheme => _currentTheme;

    /// <summary>
    /// Detects if the system is in dark mode
    /// </summary>
    private static bool IsSystemDarkMode()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            
            if (key != null)
            {
                var value = key.GetValue("AppsUseLightTheme");
                if (value is int intValue)
                {
                    return intValue == 0; // 0 = Dark, 1 = Light
                }
            }
        }
        catch
        {
            // Registry access failed
        }

        return false; // Default to light mode
    }

    /// <summary>
    /// Toggles between light and dark mode
    /// </summary>
    public static void ToggleTheme()
    {
        var newTheme = _currentTheme == "Light" ? "Dark" : "Light";
        ApplyTheme(newTheme);
    }
}

