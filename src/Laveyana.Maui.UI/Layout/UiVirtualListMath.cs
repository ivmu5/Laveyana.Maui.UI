namespace Laveyana.Maui.UI.Layout;

/// <summary>Чистая геометрия виртуального списка, легко тестируемая без GUI.</summary>
public static class UiVirtualListMath
{
    /// <summary>Высота всех строк с промежутками без лишнего промежутка в конце.</summary>
    public static double Extent(int count, double rowHeight, double spacing)
    {
        Validate(rowHeight, spacing);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return count == 0 ? 0 : count * rowHeight + (count - 1) * spacing;
    }

    /// <summary>Предоставляет открытый API static.</summary>
    public static (int First, int End) Range(int count, double offset, double viewport,
        double rowHeight, double spacing, int overscan = 2)
    {
        Validate(rowHeight, spacing);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(overscan);
        if (count == 0 || !double.IsFinite(viewport) || viewport <= 0) return (0, 0);
        double stride = rowHeight + spacing;
        double normalizedOffset = double.IsFinite(offset) ? Math.Max(0, offset) : 0;
        int first = (int)Math.Clamp(Math.Floor(normalizedOffset / stride) - overscan, 0, count);
        int end = (int)Math.Clamp(Math.Ceiling((normalizedOffset + viewport) / stride) + overscan, first, count);
        return (first, end);
    }

    private static void Validate(double rowHeight, double spacing)
    {
        if (!double.IsFinite(rowHeight) || rowHeight <= 0) throw new ArgumentOutOfRangeException(nameof(rowHeight));
        if (!double.IsFinite(spacing) || spacing < 0) throw new ArgumentOutOfRangeException(nameof(spacing));
    }
}
