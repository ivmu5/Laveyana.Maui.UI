using Laveyana.Maui.UI.Themes;

namespace Laveyana.Maui.UI.Controls;

/// <summary>Общий редактор тем и палитр, без настроек конкретного приложения.</summary>
public sealed class UiAppearanceEditor : ContentView
{
    private readonly Picker _mode = new() { Title = "Тема" };
    private readonly Picker _preset = new() { Title = "Палитра" };
    private readonly Picker _colorRole = new() { Title = "Изменяемый цвет" };
    private readonly Switch _amoled = Ui.Switch();
    private readonly UiColorPicker _picker = new();
    private readonly Entry _hex = Ui.Entry("#RRGGBB");
    private readonly Label _validation = Ui.Text("", 12, UiColorRole.Warning);
    private readonly VerticalStackLayout _custom = new() { Spacing = 8 };
    private bool _synchronizing;

    /// <summary>Текущие параметры или панель настроек, в зависимости от типа.</summary>
    public UiThemeSettings Settings { get; private set; }
    /// <summary>Уведомляет об изменении параметров оформления.</summary>
    public event EventHandler<UiThemeSettings>? SettingsChanged;

    /// <summary>Редактор светлой, тёмной и AMOLED тем, а также цветовых палитр.</summary>
    public UiAppearanceEditor(UiThemeSettings? initial = null)
    {
        Settings = initial ?? new UiThemeSettings();
        foreach (var text in new[] { "Системная", "Тёмная", "Светлая" }) _mode.Items.Add(text);
        foreach (var text in new[] { "LavPulse", "Ocean", "Warm", "LavOSC", "Своя" }) _preset.Items.Add(text);
        foreach (var text in new[] { "Основной", "Дополнительный", "Градиент 1", "Градиент 2" })
            _colorRole.Items.Add(text);
        _colorRole.SelectedIndex = 0;
        _mode.SelectedIndexChanged += (_, _) =>
        {
            if (!_synchronizing && _mode.SelectedIndex >= 0)
                Update(Settings with { Mode = _mode.SelectedIndex switch
                {
                    0 => UiThemeMode.System,
                    2 => UiThemeMode.Light,
                    _ => UiThemeMode.Dark
                } });
        };
        _preset.SelectedIndexChanged += (_, _) =>
        {
            if (_synchronizing || _preset.SelectedIndex < 0) return;
            var selected = (UiPalettePreset)_preset.SelectedIndex;
            Update(Settings with
            {
                CustomPalette = selected == UiPalettePreset.Custom && Settings.Preset != UiPalettePreset.Custom
                    ? Settings.ResolvePalette() : Settings.CustomPalette,
                Preset = selected
            });
        };
        _amoled.Toggled += (_, e) =>
        {
            if (!_synchronizing) Update(Settings with { DarkBackground = e.Value
                ? UiDarkBackground.Amoled : UiDarkBackground.Classic });
        };
        _colorRole.SelectedIndexChanged += (_, _) => SyncPicker();
        _picker.ColorChanged += (_, color) =>
        {
            if (_synchronizing || _colorRole.SelectedIndex < 0) return;
            UiPalette palette = Settings.CustomPalette;
            palette = _colorRole.SelectedIndex switch
            {
                0 => palette with { Accent = color },
                1 => palette with { Alternate = color },
                2 => palette with { Low = color },
                _ => palette with { Mid = color }
            };
            Update(Settings with { Preset = UiPalettePreset.Custom, CustomPalette = palette });
        };
        _hex.Completed += (_, _) => CommitHex();
        _hex.Unfocused += (_, _) => CommitHex();
        _custom.Children.Add(Ui.Text("Цвет палитры", 13, UiColorRole.Accent, true));
        _custom.Children.Add(_colorRole);
        _custom.Children.Add(_picker);
        _custom.Children.Add(_hex);
        _custom.Children.Add(_validation);
        Content = Ui.Stack(10,
            Ui.Text("Оформление", 19, bold: true),
            Ui.SettingRow("Тема", _mode),
            Ui.SettingRow("Палитра", _preset),
            Ui.SettingRow("Чёрный фон (AMOLED)", _amoled),
            _custom);
        SetSettings(Settings);
    }

    /// <summary>Обновить настройки извне без обратной отправки события.</summary>
    public void SetSettings(UiThemeSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Settings = settings;
        _synchronizing = true;
        try
        {
            _mode.SelectedIndex = settings.Mode switch
            {
                UiThemeMode.System => 0, UiThemeMode.Light => 2, _ => 1
            };
            _preset.SelectedIndex = (int)settings.Preset;
            _amoled.IsToggled = settings.DarkBackground == UiDarkBackground.Amoled;
            _custom.IsVisible = settings.Preset == UiPalettePreset.Custom;
            SyncPicker();
        }
        finally { _synchronizing = false; }
    }

    private void Update(UiThemeSettings updated)
    {
        if (Settings == updated) return;
        SetSettings(updated);
        SettingsChanged?.Invoke(this, updated);
    }

    private void SyncPicker()
    {
        if (_colorRole.SelectedIndex < 0) return;
        var colors = Settings.CustomPalette;
        Color color = _colorRole.SelectedIndex switch
        {
            0 => colors.Accent, 1 => colors.Alternate, 2 => colors.Low, _ => colors.Mid
        };
        bool previouslySynchronizing = _synchronizing;
        _synchronizing = true;
        try
        {
            _picker.SetColor(color);
            color.ToRgb(out byte r, out byte g, out byte b);
            _hex.Text = $"#{r:X2}{g:X2}{b:X2}";
            _validation.Text = "";
        }
        finally { _synchronizing = previouslySynchronizing; }
    }

    private void CommitHex()
    {
        if (_synchronizing || !_custom.IsVisible) return;
        string text = _hex.Text?.Trim() ?? "";
        if (text.Length != 7 || text[0] != '#' || !text.AsSpan(1).ToString().All(Uri.IsHexDigit))
        {
            _validation.Text = "Укажите цвет в формате #RRGGBB.";
            return;
        }
        _validation.Text = "";
        _picker.SetColor(Color.FromArgb(text));
        var color = _picker.SelectedColor;
        UiPalette palette = Settings.CustomPalette;
        palette = _colorRole.SelectedIndex switch
        {
            0 => palette with { Accent = color }, 1 => palette with { Alternate = color },
            2 => palette with { Low = color }, _ => palette with { Mid = color }
        };
        Update(Settings with { Preset = UiPalettePreset.Custom, CustomPalette = palette });
    }
}
