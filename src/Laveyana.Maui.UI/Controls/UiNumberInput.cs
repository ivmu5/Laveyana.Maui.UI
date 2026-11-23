using System.Globalization;
using Laveyana.Maui.UI.Themes;

namespace Laveyana.Maui.UI.Controls;

/// <summary>Границы числа, исходное значение и величина инкремента.</summary>
public readonly record struct UiNumberRange(double Minimum, double Maximum, double DefaultValue, double Step = 1, int Decimals = 2)
{
    /// <summary>Проверяет корректность границ, шага и точности.</summary>
    public void Validate()
    {
        if (!double.IsFinite(Minimum) || !double.IsFinite(Maximum) || Minimum >= Maximum)
            throw new ArgumentException("Диапазон должен быть конечным и иметь Minimum < Maximum.");
        if (!double.IsFinite(Step) || Step <= 0)
            throw new ArgumentOutOfRangeException(nameof(Step));
        if (Decimals is < 0 or > 8)
            throw new ArgumentOutOfRangeException(nameof(Decimals));
    }
    /// <summary>Ограничивает значение диапазоном и округляет до выбранной точности.</summary>
    public double Normalize(double number)
    {
        if (!double.IsFinite(number)) number = DefaultValue;
        if (!double.IsFinite(number)) number = Minimum;
        return Math.Round(Math.Clamp(number, Minimum, Maximum), Decimals, MidpointRounding.AwayFromZero);
    }
}

/// <summary>Числовое поле с возможностью редактирования и опциональным слайдером.</summary>
public sealed class UiNumberInput : Grid
{
    /// <summary>Двусторонне привязываемое числовое значение.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value),
        typeof(double), typeof(UiNumberInput), 0d, BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) =>
            ((UiNumberInput)bindable).OnValuePropertyChanged((double)oldValue, (double)newValue));

    private readonly Entry _entry;
    private readonly Slider? _slider;
    private bool _updating;

    /// <summary>Диапазон допустимых значений.</summary>
    public UiNumberRange Range { get; }
    /// <summary>Подпись числового поля или заголовка.</summary>
    public string Title { get; }
    /// <summary>Единица измерения.</summary>
    public string Unit { get; }
    /// <summary>Текущее нормализованное значение.</summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, Range.Normalize(value));
    }
    /// <summary>Возникает при фактическом изменении числового значения.</summary>
    public event EventHandler<double>? ValueChanged;

    /// <summary>Числовое поле с возможностью редактирования и опциональным слайдером.</summary>
    public UiNumberInput(string title, string unit, UiNumberRange range, bool showSlider = true)
    {
        range.Validate();
        Range = range;
        Title = title;
        Unit = unit;
        ColumnSpacing = 6;
        if (showSlider) ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        ColumnDefinitions.Add(new ColumnDefinition(new GridLength(74)));
        ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        if (showSlider) ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        _entry = new Entry
        {
            Keyboard = Keyboard.Numeric, ReturnType = ReturnType.Done,
            FontSize = 13, HorizontalTextAlignment = TextAlignment.End
        };
        _entry.SetDynamicResource(Entry.TextColorProperty, UiThemeManager.ColorKey(UiColorRole.Text));
        _entry.SetDynamicResource(Entry.BackgroundColorProperty, UiThemeManager.ColorKey(UiColorRole.Overlay));
        SemanticProperties.SetDescription(_entry, $"{title}, {unit}. От {range.Minimum} до {range.Maximum}");
        Grid.SetColumn((BindableObject)_entry, showSlider ? 1 : 0);
        Children.Add(_entry);
        var suffix = new Label { Text = unit, VerticalOptions = LayoutOptions.Center, FontSize = 12 };
        suffix.SetDynamicResource(Label.TextColorProperty, UiThemeManager.ColorKey(UiColorRole.Muted));
        Grid.SetColumn((BindableObject)suffix, showSlider ? 2 : 1);
        Children.Add(suffix);
        if (showSlider)
        {
            _slider = new Slider { Minimum = range.Minimum, Maximum = range.Maximum };
            _slider.SetDynamicResource(Slider.MinimumTrackColorProperty, UiThemeManager.ColorKey(UiColorRole.Accent));
            _slider.SetDynamicResource(Slider.MaximumTrackColorProperty, UiThemeManager.ColorKey(UiColorRole.Border));
            _slider.SetDynamicResource(Slider.ThumbColorProperty, UiThemeManager.ColorKey(UiColorRole.Accent));
            _slider.ValueChanged += (_, args) =>
            {
                if (!_updating && IsEnabled) Value = args.NewValue;
            };
            Grid.SetColumn((BindableObject)_slider, 0);
            Children.Add(_slider);
            var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            doubleTap.Tapped += (_, _) => Reset();
            _slider.GestureRecognizers.Add(doubleTap);
            var reset = new Button { Text = "↺", WidthRequest = 36, HeightRequest = 36, Padding = 0 };
            reset.SetDynamicResource(VisualElement.BackgroundColorProperty, UiThemeManager.ColorKey(UiColorRole.Button));
            reset.Clicked += (_, _) => Reset();
            SemanticProperties.SetDescription(reset, $"Сбросить: {title}");
            Grid.SetColumn((BindableObject)reset, 3);
            Children.Add(reset);
        }
        _entry.Completed += (_, _) => Commit();
        _entry.Unfocused += (_, _) => Commit();
        Value = range.DefaultValue;
        RefreshControls();
    }

    /// <summary>Сбрасывает значение к исходному.</summary>
    public void Reset()
    {
        if (IsEnabled) Value = Range.Normalize(Range.DefaultValue);
    }
    /// <summary>Изменяет значение на указанное число шагов.</summary>
    public void Increment(double steps = 1)
    {
        if (IsEnabled && double.IsFinite(steps)) Value = Range.Normalize(Value + Range.Step * steps);
    }

    /// <summary>Подтверждает ввод и принимает точку или запятую в качестве разделителя.</summary>
    public void Commit()
    {
        if (!IsEnabled) return;
        string source = (_entry.Text ?? "").Trim().Replace(',', '.');
        if (double.TryParse(source, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            Value = parsed;
        RefreshControls();
    }

    private void OnValuePropertyChanged(double previous, double requested)
    {
        if (_entry is null) return;
        double normalized = Range.Normalize(requested);
        if (normalized != requested)
        {
            SetValue(ValueProperty, normalized);
            return;
        }
        RefreshControls();
        if (!_updating && normalized != previous) ValueChanged?.Invoke(this, normalized);
    }

    private void RefreshControls()
    {
        _updating = true;
        try
        {
            _entry.Text = Value.ToString($"0.{new string('#', Range.Decimals)}", CultureInfo.CurrentCulture).TrimEnd('.', ',');
            if (_slider is not null) _slider.Value = Value;
        }
        finally { _updating = false; }
    }
}
