using Laveyana.Maui.UI.Design;
using Laveyana.Maui.UI.Fluent;
using Laveyana.Maui.UI.Layout;
using Laveyana.Maui.UI.Settings;
using Laveyana.Maui.UI.Themes;

namespace Laveyana.Maui.UI.Controls;

/// <summary>Создание готовых элементов с возможностью дальнейшей fluent-настройки.</summary>
public static class Ui
{
    /// <summary>Создаёт стилизованную надпись.</summary>
    public static Label Text(string text, double size = UiDesignTokens.BodyFontSize, UiColorRole role = UiColorRole.Text,
        bool bold = false, string? font = null)
        => new Label { Text = text }.WithFont(size, font, bold).WithTextColor(role);

    /// <summary>Создаёт кнопку с семантической расцветкой.</summary>
    public static Button Button(string text, bool primary = false, string? font = null)
    {
        var button = new Button
        {
            Text = text,
            FontSize = UiDesignTokens.ButtonFontSize,
            FontFamily = font,
            CornerRadius = UiDesignTokens.ButtonRadius,
            MinimumHeightRequest = UiDesignTokens.ButtonHeight,
            Padding = new Thickness(UiDesignTokens.ButtonHorizontalPadding, UiDesignTokens.ButtonVerticalPadding),
            FontAttributes = primary ? FontAttributes.Bold : FontAttributes.None
        };
        button.WithBackground(primary ? UiColorRole.Primary : UiColorRole.Button);
        button.SetDynamicResource(Microsoft.Maui.Controls.Button.TextColorProperty, UiThemeManager.ColorKey(primary ? UiColorRole.OnPrimary : UiColorRole.Text));
        return button;
    }

    /// <summary>Создаёт карточку с темой, скруглением и контуром.</summary>
    public static Border Card(View content, double radius = UiDesignTokens.CardRadius, double padding = UiDesignTokens.CardPadding)
    {
        ArgumentNullException.ThrowIfNull(content);
        var border = new Border
        {
            Content = content,
            Padding = new Thickness(padding),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(radius) }
        };
        border.SetDynamicResource(Border.BackgroundColorProperty, UiThemeManager.ColorKey(UiColorRole.Card));
        border.SetDynamicResource(Border.StrokeProperty, UiThemeManager.BrushKey(UiColorRole.Border));
        return border;
    }

    /// <summary>Создаёт вертикальный контейнер и добавляет его элементы.</summary>
    public static VerticalStackLayout Stack(double spacing = UiDesignTokens.Spacing, params View[] children)
        => new VerticalStackLayout { Spacing = spacing }.AddChildren(children);

    /// <summary>Создаёт текстовое поле в цветах темы.</summary>
    public static Entry Entry(string placeholder = "", Keyboard? keyboard = null)
    {
        var entry = new Entry { Placeholder = placeholder, Keyboard = keyboard ?? Keyboard.Default };
        entry.SetDynamicResource(Microsoft.Maui.Controls.Entry.TextColorProperty,
            UiThemeManager.ColorKey(UiColorRole.Text));
        entry.SetDynamicResource(Microsoft.Maui.Controls.Entry.PlaceholderColorProperty,
            UiThemeManager.ColorKey(UiColorRole.Muted));
        entry.WithBackground(UiColorRole.Overlay);
        return entry;
    }

    /// <summary>Создаёт переключатель в цветах темы.</summary>
    public static Switch Switch(bool enabled = false)
    {
        var control = new Switch { IsToggled = enabled };
        control.SetDynamicResource(Microsoft.Maui.Controls.Switch.OnColorProperty,
            UiThemeManager.ColorKey(UiColorRole.Accent));
        return control;
    }

    /// <summary>Создаёт слайдер с заданными границами.</summary>
    public static Slider Slider(double minimum, double maximum, double value)
    {
        if (!double.IsFinite(minimum) || !double.IsFinite(maximum) || minimum >= maximum)
            throw new ArgumentException("Нужен конечный диапазон Minimum < Maximum.");
        var slider = new Slider { Minimum = minimum, Maximum = maximum,
            Value = Math.Clamp(double.IsFinite(value) ? value : minimum, minimum, maximum) };
        slider.SetDynamicResource(Microsoft.Maui.Controls.Slider.MinimumTrackColorProperty,
            UiThemeManager.ColorKey(UiColorRole.Accent));
        slider.SetDynamicResource(Microsoft.Maui.Controls.Slider.MaximumTrackColorProperty,
            UiThemeManager.ColorKey(UiColorRole.Border));
        slider.SetDynamicResource(Microsoft.Maui.Controls.Slider.ThumbColorProperty,
            UiThemeManager.ColorKey(UiColorRole.Accent));
        return slider;
    }

    /// <summary>Размещает подпись и элемент настройки в одной строке.</summary>
    public static View SettingRow(string title, View control)
    {
        ArgumentNullException.ThrowIfNull(control);
        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 10
        };
        var label = Text(title, 13);
        label.VerticalOptions = LayoutOptions.Center;
        row.Add(label);
        Grid.SetColumn((BindableObject)control, 1);
        row.Children.Add(control);
        return row;
    }

    /// <summary>Создаёт группу настроек в карточке.</summary>
    public static Border Section(string title, params View[] content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        return Card(Stack(8, [Text(title, 16, UiColorRole.Accent, bold: true), .. content]));
    }

    /// <summary>Создаёт адаптивную сетку карточек.</summary>
    public static UiAdaptiveGrid Cards(int maximumColumns = 3, double minimumColumnWidth = 280)
        => new(maximumColumns, minimumColumnWidth);

    /// <summary>Создаёт панель настроек с разделами.</summary>
    public static UiSettingsPanel Settings() => new();

    /// <summary>Создаёт редактор оформления.</summary>
    public static UiAppearanceEditor Appearance(UiThemeSettings? initial = null) => new(initial);

    /// <summary>Создаёт числовую настройку с дополнительным слайдером.</summary>
    public static UiNumberInput Number(string title, double minimum, double maximum, double value,
        double step = 1, string unit = "", int decimals = 2, bool showSlider = true)
        => new(title, unit, new UiNumberRange(minimum, maximum, value, step, decimals), showSlider);
}
