namespace Laveyana.Maui.UI.Controls;

/// <summary>Редактор цвета HSV, заимствованный по конструкции у LavPulse без аудиозависимостей.</summary>
public sealed class UiColorPicker : VerticalStackLayout
{
    private const float Inset = 8;
    private readonly GraphicsView _field;
    private readonly GraphicsView _hueStrip;
    private float _hue, _saturation, _brightness;
    private bool _notifying;

    /// <summary>Возникает после изменения выбранного цвета пользователем.</summary>
    public event EventHandler<Color>? ColorChanged;
    /// <summary>Текущий цвет.</summary>
    public Color SelectedColor { get; private set; } = Colors.White;

    /// <summary>Графический HSV-редактор цвета.</summary>
    public UiColorPicker()
    {
        Spacing = 4;
        _field = new GraphicsView { HeightRequest = 140, Drawable = new DelegateDrawable(DrawField) };
        _hueStrip = new GraphicsView { HeightRequest = 34, Drawable = new DelegateDrawable(DrawHue) };
        SemanticProperties.SetDescription(_field, "Насыщенность и яркость цвета");
        SemanticProperties.SetDescription(_hueStrip, "Цветовой оттенок");
        Children.Add(_field);
        Children.Add(_hueStrip);
        AttachInput(_field, SelectField);
        AttachInput(_hueStrip, SelectHue);
        SetColor(SelectedColor);
    }

    /// <summary>Устанавливает цвет без уведомления о пользовательском редактировании.</summary>
    public void SetColor(Color color)
    {
        ArgumentNullException.ThrowIfNull(color);
        SelectedColor = color;
        float max = Math.Max(color.Red, Math.Max(color.Green, color.Blue));
        float min = Math.Min(color.Red, Math.Min(color.Green, color.Blue));
        float chroma = max - min;
        if (chroma > 0) _hue = color.GetHue();
        _saturation = max > 0 ? chroma / max : 0;
        _brightness = max;
        _field.Invalidate();
        _hueStrip.Invalidate();
    }

    private static void AttachInput(GraphicsView view, Action<PointF> select)
    {
        view.StartInteraction += (_, e) => { if (e.Touches.Length > 0) select(e.Touches[0]); };
        view.DragInteraction += (_, e) => { if (e.Touches.Length > 0) select(e.Touches[0]); };
        view.EndInteraction += (_, e) => { if (e.Touches.Length > 0) select(e.Touches[0]); };
    }

    private void SelectField(PointF position)
    {
        _saturation = Fraction(position.X, (float)_field.Width);
        _brightness = 1 - Fraction(position.Y, (float)_field.Height);
        Publish();
    }

    private void SelectHue(PointF position)
    {
        _hue = Fraction(position.X, (float)_hueStrip.Width);
        Publish();
    }

    private void Publish()
    {
        if (_notifying) return;
        var color = Color.FromHsv(_hue % 1f, _saturation, _brightness);
        SetColor(Color.FromRgb((int)Math.Round(color.Red * 255),
            (int)Math.Round(color.Green * 255), (int)Math.Round(color.Blue * 255)));
        try
        {
            _notifying = true;
            ColorChanged?.Invoke(this, SelectedColor);
        }
        finally { _notifying = false; }
    }

    private static float Fraction(float coordinate, float size) =>
        Math.Clamp((coordinate - Inset) / Math.Max(1, size - 2 * Inset), 0f, 1f);

    private static RectF Inner(RectF rect) => new(rect.X + Inset, rect.Y + Inset,
        Math.Max(0, rect.Width - 2 * Inset), Math.Max(0, rect.Height - 2 * Inset));

    private void DrawField(ICanvas canvas, RectF dirty)
    {
        var rect = Inner(dirty);
        if (rect.Width <= 0 || rect.Height <= 0) return;
        canvas.SetFillPaint(new LinearGradientPaint(
            [new PaintGradientStop(0, Colors.White), new PaintGradientStop(1, Color.FromHsv(_hue % 1f, 1, 1))],
            new Point(0, 0), new Point(1, 0)), rect);
        canvas.FillRectangle(rect);
        canvas.SetFillPaint(new LinearGradientPaint(
            [new PaintGradientStop(0, Colors.Black.WithAlpha(0)), new PaintGradientStop(1, Colors.Black)],
            new Point(0, 0), new Point(0, 1)), rect);
        canvas.FillRectangle(rect);
        DrawMarker(canvas, rect.X + _saturation * rect.Width, rect.Y + (1 - _brightness) * rect.Height);
    }

    private void DrawHue(ICanvas canvas, RectF dirty)
    {
        var rect = Inner(dirty);
        if (rect.Width <= 0 || rect.Height <= 0) return;
        var steps = Enumerable.Range(0, 7)
            .Select(i => new PaintGradientStop(i / 6f, Color.FromHsv(i % 6 / 6f, 1f, 1f)))
            .ToArray();
        canvas.SetFillPaint(new LinearGradientPaint(steps, new Point(0, 0), new Point(1, 0)), rect);
        canvas.FillRoundedRectangle(rect, 5);
        DrawMarker(canvas, rect.X + _hue * rect.Width, rect.Center.Y);
    }

    private static void DrawMarker(ICanvas canvas, float x, float y)
    {
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 3.5f;
        canvas.DrawCircle(x, y, 5);
        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 1.5f;
        canvas.DrawCircle(x, y, 5);
    }

    private sealed class DelegateDrawable(Action<ICanvas, RectF> draw) : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect) => draw(canvas, dirtyRect);
    }
}
