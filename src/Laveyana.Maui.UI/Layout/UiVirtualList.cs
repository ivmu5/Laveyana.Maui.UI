using Microsoft.Maui.Layouts;

namespace Laveyana.Maui.UI.Layout;

/// <summary>
/// Виртуализированный список фиксированных строк (приём из LavPulse).
/// Для переменной высоты рекомендуется штатный CollectionView.
/// </summary>
public sealed class UiVirtualList<TItem, TRow> : ScrollView where TItem : class where TRow : View
{
    private readonly AbsoluteLayout _canvas = new();
    private readonly Func<TRow> _create;
    private readonly Action<TRow, TItem, int> _bind;
    private readonly Action<TRow> _release;
    private readonly Dictionary<TItem, TRow> _visible = new(ReferenceEqualityComparer.Instance);
    private readonly Stack<TRow> _pool = new();
    private IReadOnlyList<TItem> _items = [];
    private readonly double _rowHeight, _spacing;
    private readonly int _overscan;
    private bool _refreshing, _refreshAgain;

    /// <summary>Количество реально созданных и привязанных строк.</summary>
    public int RealizedCount => _visible.Count;
    /// <summary>Текущий снимок моделей строк.</summary>
    public IReadOnlyList<TItem> Items => _items;

    /// <summary>Виртуализирует список моделей с одинаковой высотой строк.</summary>
    public UiVirtualList(double rowHeight, Func<TRow> create, Action<TRow, TItem, int> bind,
        Action<TRow>? release = null, double spacing = 5, int overscan = 2)
    {
        if (!double.IsFinite(rowHeight) || rowHeight <= 0) throw new ArgumentOutOfRangeException(nameof(rowHeight));
        if (!double.IsFinite(spacing) || spacing < 0) throw new ArgumentOutOfRangeException(nameof(spacing));
        ArgumentOutOfRangeException.ThrowIfNegative(overscan);
        _create = create ?? throw new ArgumentNullException(nameof(create));
        _bind = bind ?? throw new ArgumentNullException(nameof(bind));
        _release = release ?? (_ => { });
        _rowHeight = rowHeight;
        _spacing = spacing;
        _overscan = overscan;
        _canvas.VerticalOptions = LayoutOptions.Start;
        Content = _canvas;
        Scrolled += (_, _) => Refresh();
        SizeChanged += (_, _) => Refresh();
        Unloaded += (_, _) => DetachAll();
        Loaded += (_, _) => Refresh();
    }

    /// <summary>Заменяет модели, пересоздавая текущие привязки.</summary>
    public void SetItems(IReadOnlyList<TItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Any(item => item is null)) throw new ArgumentException("Элементы не могут быть null.", nameof(items));
        // Reference identity is intentional; repeated instances would share a single row.
        if (new HashSet<TItem>(items, ReferenceEqualityComparer.Instance).Count != items.Count)
            throw new ArgumentException("Один и тот же объект не должен встречаться дважды; используйте уникальные модели строк.", nameof(items));
        DetachAll();
        _items = items.ToArray();
        _canvas.HeightRequest = UiVirtualListMath.Extent(items.Count, _rowHeight, _spacing);
        Refresh();
    }

    /// <summary>Обновляет видимую область списка.</summary>
    public void Refresh()
    {
        if (_refreshing) { _refreshAgain = true; return; }
        _refreshing = true;
        try
        {
            do
            {
                _refreshAgain = false;
                UpdateVisible();
            } while (_refreshAgain);
        }
        finally { _refreshing = false; }
    }

    private void UpdateVisible()
    {
        var (first, end) = UiVirtualListMath.Range(_items.Count, ScrollY, Height, _rowHeight, _spacing, _overscan);
        var needed = new HashSet<TItem>(ReferenceEqualityComparer.Instance);
        for (int index = first; index < end; index++) needed.Add(_items[index]);
        foreach (var pair in _visible.ToArray())
        {
            if (!needed.Contains(pair.Key))
            {
                _release(pair.Value);
                _canvas.Children.Remove(pair.Value);
                _pool.Push(pair.Value);
                _visible.Remove(pair.Key);
            }
        }
        for (int index = first; index < end; index++)
        {
            TItem item = _items[index];
            if (!_visible.TryGetValue(item, out var row))
            {
                row = _pool.TryPop(out var reused) ? reused : _create();
                row.HeightRequest = _rowHeight;
                AbsoluteLayout.SetLayoutFlags(row, AbsoluteLayoutFlags.WidthProportional);
                _canvas.Children.Add(row);
                _visible.Add(item, row);
            }
            AbsoluteLayout.SetLayoutBounds(row, new Rect(0, index * (_rowHeight + _spacing), 1, _rowHeight));
            _bind(row, item, index);
        }
    }

    private void DetachAll()
    {
        foreach (var row in _visible.Values)
        {
            _release(row);
            _canvas.Children.Remove(row);
            _pool.Push(row);
        }
        _visible.Clear();
    }
}
