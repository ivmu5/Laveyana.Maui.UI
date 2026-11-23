using Laveyana.Maui.UI.Themes;
using Microsoft.Maui.Storage;

namespace Laveyana.Maui.UI.Settings;

/// <summary>Необязательная настройка хранения UI-темы в стандартном MAUI Preferences.</summary>
public static class UiThemePreferences
{
    /// <summary>Восстанавливает оформление или значения по умолчанию.</summary>
    public static UiThemeSettings Load(string prefix = "laveyana.ui")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        var defaults = new UiThemeSettings();
        UiThemeMode mode = ReadEnum($"{prefix}.mode", defaults.Mode);
        UiDarkBackground dark = ReadEnum($"{prefix}.background", defaults.DarkBackground);
        UiPalettePreset preset = ReadEnum($"{prefix}.palette", defaults.Preset);
        var palette = defaults.CustomPalette;
        return new UiThemeSettings
        {
            Mode = mode, DarkBackground = dark, Preset = preset,
            CustomPalette = new UiPalette(
                ReadColor($"{prefix}.accent", palette.Accent),
                ReadColor($"{prefix}.alternate", palette.Alternate),
                ReadColor($"{prefix}.low", palette.Low),
                ReadColor($"{prefix}.mid", palette.Mid))
        };
    }

    /// <summary>Сохраняет пользовательское оформление.</summary>
    public static void Save(UiThemeSettings settings, string prefix = "laveyana.ui")
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        Preferences.Default.Set($"{prefix}.mode", (int)settings.Mode);
        Preferences.Default.Set($"{prefix}.background", (int)settings.DarkBackground);
        Preferences.Default.Set($"{prefix}.palette", (int)settings.Preset);
        WriteColor($"{prefix}.accent", settings.CustomPalette.Accent);
        WriteColor($"{prefix}.alternate", settings.CustomPalette.Alternate);
        WriteColor($"{prefix}.low", settings.CustomPalette.Low);
        WriteColor($"{prefix}.mid", settings.CustomPalette.Mid);
    }

    private static T ReadEnum<T>(string key, T fallback) where T : struct, Enum
    {
        int value = Preferences.Default.Get(key, Convert.ToInt32(fallback));
        return Enum.IsDefined(typeof(T), value) ? (T)Enum.ToObject(typeof(T), value) : fallback;
    }

    private static Color ReadColor(string key, Color fallback)
    {
        try { return Color.FromArgb(Preferences.Default.Get(key, Hex(fallback))); }
        catch (ArgumentException) { return fallback; }
        catch (FormatException) { return fallback; }
    }

    private static void WriteColor(string key, Color value) => Preferences.Default.Set(key, Hex(value));
    private static string Hex(Color color)
    {
        color.ToRgb(out byte r, out byte g, out byte b);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
