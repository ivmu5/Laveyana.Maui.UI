using Laveyana.Maui.UI.Themes;

namespace Laveyana.Maui.UI.Fluent;

/// <summary>Цепочки возвращают тот же MAUI-тип (не создают тяжёлых промежуточных обёрток).</summary>
public static class UiFluentExtensions
{
    /// <summary>Предоставляет открытый API View.</summary>
    public static T WithMargin<T>(this T view, double all) where T : View => view.WithMargin(new Thickness(all));
    /// <summary>Предоставляет открытый API View.</summary>
    public static T WithMargin<T>(this T view, double horizontal, double vertical) where T : View => view.WithMargin(new Thickness(horizontal, vertical));
    /// <summary>Предоставляет открытый API View.</summary>
    public static T WithMargin<T>(this T view, Thickness value) where T : View
    {
        ArgumentNullException.ThrowIfNull(view);
        view.Margin = value;
        return view;
    }

    /// <summary>Предоставляет открытый API Layout.</summary>
    public static T WithPadding<T>(this T view, double all) where T : Microsoft.Maui.Controls.Layout => view.WithPadding(new Thickness(all));
    /// <summary>Предоставляет открытый API Layout.</summary>
    public static T WithPadding<T>(this T view, Thickness value) where T : Microsoft.Maui.Controls.Layout
    {
        ArgumentNullException.ThrowIfNull(view);
        view.Padding = value;
        return view;
    }

    /// <summary>Предоставляет открытый API VisualElement.</summary>
    public static T WithSize<T>(this T view, double width, double height) where T : VisualElement
    {
        ArgumentNullException.ThrowIfNull(view);
        view.WidthRequest = width;
        view.HeightRequest = height;
        return view;
    }

    /// <summary>Предоставляет открытый API VisualElement.</summary>
    public static T WithBackground<T>(this T view, UiColorRole role) where T : VisualElement
    {
        ArgumentNullException.ThrowIfNull(view);
        view.SetDynamicResource(VisualElement.BackgroundColorProperty, UiThemeManager.ColorKey(role));
        return view;
    }

    /// <summary>Предоставляет открытый API Label.</summary>
    public static T WithTextColor<T>(this T view, UiColorRole role) where T : Label
    {
        ArgumentNullException.ThrowIfNull(view);
        view.SetDynamicResource(Label.TextColorProperty, UiThemeManager.ColorKey(role));
        return view;
    }

    /// <summary>Предоставляет открытый API family.</summary>
    public static T WithFont<T>(this T view, double size, string? family = null, bool bold = false) where T : Label
    {
        ArgumentNullException.ThrowIfNull(view);
        view.FontSize = size;
        if (family is not null) view.FontFamily = family;
        view.FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None;
        return view;
    }

    /// <summary>Предоставляет открытый API column.</summary>
    public static T At<T>(this T view, int row, int column = 0, int rowSpan = 1, int columnSpan = 1) where T : View
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentOutOfRangeException.ThrowIfLessThan(rowSpan, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(columnSpan, 1);
        Grid.SetRow((BindableObject)view, row);
        Grid.SetColumn((BindableObject)view, column);
        Grid.SetRowSpan((BindableObject)view, rowSpan);
        Grid.SetColumnSpan((BindableObject)view, columnSpan);
        return view;
    }

    /// <summary>Предоставляет открытый API Grid.</summary>
    public static T AddRow<T>(this T grid, GridLength length) where T : Grid
    {
        ArgumentNullException.ThrowIfNull(grid);
        grid.RowDefinitions.Add(new RowDefinition(length));
        return grid;
    }

    /// <summary>Предоставляет открытый API Grid.</summary>
    public static T AddColumn<T>(this T grid, GridLength length) where T : Grid
    {
        ArgumentNullException.ThrowIfNull(grid);
        grid.ColumnDefinitions.Add(new ColumnDefinition(length));
        return grid;
    }

    /// <summary>Предоставляет открытый API row.</summary>
    public static T AddChild<T>(this T grid, View child, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1) where T : Grid
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(child);
        grid.Add(child.At(row, column, rowSpan, columnSpan));
        return grid;
    }

    /// <summary>Предоставляет открытый API Layout.</summary>
    public static T AddChildren<T>(this T layout, params View[] children) where T : Microsoft.Maui.Controls.Layout
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(children);
        foreach (var child in children)
        {
            ArgumentNullException.ThrowIfNull(child);
            layout.Children.Add(child);
        }
        return layout;
    }

    /// <summary>Создаёт кнопку с семантической расцветкой.</summary>
    public static T OnClick<T>(this T button, EventHandler handler) where T : Button
    {
        ArgumentNullException.ThrowIfNull(button);
        ArgumentNullException.ThrowIfNull(handler);
        button.Clicked += handler;
        return button;
    }

    /// <summary>Предоставляет открытый API View.</summary>
    public static T OnTap<T>(this T view, EventHandler<TappedEventArgs> handler) where T : View
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(handler);
        var recognizer = new TapGestureRecognizer();
        recognizer.Tapped += handler;
        view.GestureRecognizers.Add(recognizer);
        return view;
    }

    /// <summary>Предоставляет открытый API color.</summary>
    public static T WithShadow<T>(this T view, Color? color = null, float radius = 12,
        float opacity = .25f, double x = 0, double y = 3) where T : View
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentOutOfRangeException.ThrowIfNegative(radius);
        if (opacity is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(opacity));
        view.Shadow = new Shadow { Brush = new SolidColorBrush(color ?? Colors.Black),
            Radius = radius, Opacity = opacity, Offset = new Point(x, y) };
        return view;
    }
}
