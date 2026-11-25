namespace Laveyana.Maui.UI.Themes;

/// <summary>Семантические цвета: приложение не привязывается к конкретной палитре.</summary>
public enum UiColorRole
{
    /// <summary>Фон приложения.</summary>
    Background,
    /// <summary>Фон панели.</summary>
    Surface,
    /// <summary>Фон карточки.</summary>
    Card,
    /// <summary>Фон перекрывающего слоя.</summary>
    Overlay,
    /// <summary>Фон кнопки.</summary>
    Button,
    /// <summary>Выделенный элемент.</summary>
    Selected,
    /// <summary>Контур.</summary>
    Border,
    /// <summary>Основной текст.</summary>
    Text,
    /// <summary>Приглушённый текст.</summary>
    Muted,
    /// <summary>Акцентный цвет.</summary>
    Accent,
    /// <summary>Основной интерактивный элемент.</summary>
    Primary,
    /// <summary>Текст на основном интерактивном элементе.</summary>
    OnPrimary,
    /// <summary>Успешная операция.</summary>
    Success,
    /// <summary>Предупреждение.</summary>
    Warning,
    /// <summary>Ошибка или опасное действие.</summary>
    Danger,
    /// <summary>Полупрозрачная подложка.</summary>
    Scrim,
    /// <summary>Полоса переключателя.</summary>
    SwitchTrack,
}

/// <summary>Режим выбора оформления.</summary>
public enum UiThemeMode
{
    /// <summary>Следовать системной теме.</summary>
    System,
    /// <summary>Тёмная тема.</summary>
    Dark,
    /// <summary>Светлая тема.</summary>
    Light,
}
/// <summary>Вариант тёмного фона.</summary>
public enum UiDarkBackground
{
    /// <summary>Тёмный фон с оттенком.</summary>
    Classic,
    /// <summary>Полностью чёрный фон.</summary>
    Amoled,
}
/// <summary>Предустановленная палитра интерфейса.</summary>
public enum UiPalettePreset
{
    /// <summary>Стандартная палитра Laveyana.</summary>
    Laveyana = 0,
    /// <summary>Холодные оттенки Ocean.</summary>
    Ocean = 1,
    /// <summary>Тёплая палитра.</summary>
    Warm = 2,
    /// <summary>Палитра Mint.</summary>
    Mint = 3,
    /// <summary>Пользовательская палитра.</summary>
    Custom = 4,
}

/// <summary>Четыре исходных цвета, из которых вычисляются поверхности и контрастный текст.</summary>
public sealed record UiPalette(Color Accent, Color Alternate, Color Low, Color Mid);

/// <summary>Каталог предустановленных палитр.</summary>
public static class UiPaletteCatalog
{
    /// <summary>Возвращает значения выбранного предустановленного оформления.</summary>
    public static UiPalette Get(UiPalettePreset preset) => preset switch
    {
        UiPalettePreset.Ocean => new(Color.FromArgb("#8CEBFE"), Color.FromArgb("#A5B0FF"), Color.FromArgb("#589BEC"), Color.FromArgb("#8F84EA")),
        UiPalettePreset.Warm => new(Color.FromArgb("#FFD39A"), Color.FromArgb("#FF9EAE"), Color.FromArgb("#E48275"), Color.FromArgb("#E88EBE")),
        UiPalettePreset.Mint => new(Color.FromArgb("#65DCF1"), Color.FromArgb("#86E7B7"), Color.FromArgb("#263548"), Color.FromArgb("#526780")),
        UiPalettePreset.Laveyana => new(Color.FromArgb("#58E5FC"), Color.FromArgb("#FF9B65"), Color.FromArgb("#419DFB"), Color.FromArgb("#BF89E7")),
        _ => throw new ArgumentOutOfRangeException(nameof(preset), "Для пользовательской палитры используйте UiThemeSettings.CustomPalette.")
    };
}

/// <summary>Общие настройки оформления. Настройки воспроизведения и OSC остаются в приложениях.</summary>
public sealed record UiThemeSettings
{
    /// <summary>Режим светлой и тёмной темы.</summary>
    public UiThemeMode Mode { get; init; } = UiThemeMode.Dark;
    /// <summary>Вариант тёмного оформления.</summary>
    public UiDarkBackground DarkBackground { get; init; }
    /// <summary>Выбранная палитра.</summary>
    public UiPalettePreset Preset { get; init; } = UiPalettePreset.Laveyana;
    /// <summary>Цвета пользовательской палитры.</summary>
    public UiPalette CustomPalette { get; init; } = UiPaletteCatalog.Get(UiPalettePreset.Laveyana);
    /// <summary>Получает цвета выбранной палитры.</summary>
    public UiPalette ResolvePalette() => Preset == UiPalettePreset.Custom ? CustomPalette : UiPaletteCatalog.Get(Preset);
}

/// <summary>Неизменяемая контрастная тема. </summary>
public sealed class UiTheme
{
    private static readonly Color DarkInk = Color.FromArgb("#0B2137");
    private static readonly Color LightInk = Color.FromArgb("#F4F6FF");
    private readonly IReadOnlyDictionary<UiColorRole, Color> _colors;

    /// <summary>Исходные цвета текущей темы.</summary>
    public UiPalette Palette { get; }
    /// <summary>Показывает, включено ли светлое оформление.</summary>
    public bool IsLight { get; }
    /// <summary>Показывает, включён ли полностью чёрный фон.</summary>
    public bool IsAmoled { get; }
    /// <summary>Предоставляет открытый API None.</summary>
    public Color this[UiColorRole role] => _colors[role];

    private UiTheme(UiThemeSettings settings, bool light)
    {
        IsLight = light;
        IsAmoled = !light && settings.DarkBackground == UiDarkBackground.Amoled;
        Palette = settings.ResolvePalette();
        Color tint = Blend(Palette.Low, Palette.Mid, .5f);
        Color basis = light ? Color.FromArgb("#F7F9FC") : Color.FromArgb("#0B1020");
        Color background = IsAmoled ? Colors.Black : Blend(basis, tint, light ? .04f : .025f);
        Color surface = Blend(background, tint, .07f);
        Color card = Blend(background, tint, .12f);
        Color button = Blend(background, tint, .16f);
        Color overlay = Blend(background, tint, .10f);
        Color selected = Blend(background, Palette.Accent, light ? .18f : .22f);
        Color[] surfaces = [background, surface, card, button, overlay, selected];
        Color reference = light ? surfaces.MinBy(Luminance)! : surfaces.MaxBy(Luminance)!;
        Color toward = light ? Colors.Black : Colors.White;
        Color ink = light ? LightInk : DarkInk;
        Color primary = EnsureContrast(EnsureContrast(Palette.Accent, ink, 4.5, toward), reference, 3, toward);
        Color accent = EnsureContrast(Palette.Accent, reference, 4.5, toward);
        Color text = EnsureContrast(Blend(light ? DarkInk : LightInk, tint, .06f), reference, 7, toward);
        _colors = new Dictionary<UiColorRole, Color>
        {
            [UiColorRole.Background] = background,
            [UiColorRole.Surface] = surface,
            [UiColorRole.Card] = card,
            [UiColorRole.Overlay] = overlay,
            [UiColorRole.Button] = button,
            [UiColorRole.Selected] = selected,
            [UiColorRole.Border] = EnsureContrast(Blend(background, tint, .45f), reference, 3, toward),
            [UiColorRole.Text] = text,
            [UiColorRole.Muted] = EnsureContrast(Blend(text, reference, .32f), reference, 4.5, toward),
            [UiColorRole.Accent] = accent,
            [UiColorRole.Primary] = primary,
            [UiColorRole.OnPrimary] = ink,
            [UiColorRole.Success] = EnsureContrast(Color.FromArgb("#86E7B7"), reference, 4.5, toward),
            [UiColorRole.Warning] = EnsureContrast(Color.FromArgb("#FFC287"), reference, 4.5, toward),
            [UiColorRole.Danger] = EnsureContrast(Color.FromArgb("#E87992"), reference, 4.5, toward),
            [UiColorRole.Scrim] = Colors.Black.WithAlpha(light ? .42f : .76f),
            [UiColorRole.SwitchTrack] = EnsureContrast(reference, reference, 2, accent)
        };
    }

    /// <summary>Создаёт тему с учётом системных настроек.</summary>
    public static UiTheme Create(UiThemeSettings settings, AppTheme platformTheme = AppTheme.Dark)
    {
        ArgumentNullException.ThrowIfNull(settings);
        bool light = settings.Mode == UiThemeMode.Light ||
            settings.Mode == UiThemeMode.System && platformTheme == AppTheme.Light;
        return new UiTheme(settings, light);
    }

    /// <summary>Смешивает два цвета в заданной пропорции.</summary>
    public static Color Blend(Color a, Color b, float proportion)
    {
        proportion = Math.Clamp(proportion, 0, 1);
        return new Color(a.Red + (b.Red - a.Red) * proportion,
            a.Green + (b.Green - a.Green) * proportion,
            a.Blue + (b.Blue - a.Blue) * proportion);
    }

    /// <summary>Повышает контраст по WCAG; не заменяет проверку графических элементов приложения.</summary>
    public static Color EnsureContrast(Color color, Color background, double ratio, Color toward)
    {
        if (ratio is < 1 or > 21 || !double.IsFinite(ratio))
            throw new ArgumentOutOfRangeException(nameof(ratio));
        double current = Contrast(color, background);
        if (current >= ratio) return color;
        if (Contrast(toward, background) < ratio) return toward;
        float lo = 0, hi = 1;
        for (int i = 0; i < 16; i++)
        {
            float mid = (lo + hi) * .5f;
            if (Contrast(Blend(color, toward, mid), background) >= ratio) hi = mid;
            else lo = mid;
        }
        return Blend(color, toward, hi);
    }

    /// <summary>Вычисляет контраст двух цветов по WCAG.</summary>
    public static double Contrast(Color first, Color second)
    {
        double a = Luminance(first), b = Luminance(second);
        return (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
    }

    private static double Luminance(Color color)
    {
        static double Linear(double x) => x <= .04045 ? x / 12.92 : Math.Pow((x + .055) / 1.055, 2.4);
        return .2126 * Linear(color.Red) + .7152 * Linear(color.Green) + .0722 * Linear(color.Blue);
    }
}
