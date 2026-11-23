namespace Laveyana.Maui.UI.Layout;

/// <summary>Адаптивное размещение карточек: 1..N столбцов по доступной ширине.</summary>
public sealed class UiAdaptiveGrid : Grid
{
    private readonly int _maximumColumns;
    private readonly double _minimumColumnWidth;
    private readonly List<View> _cards = [];
    private readonly Dictionary<View, double> _originalMinimums = new(ReferenceEqualityComparer.Instance);
    private int _columns;

    /// <summary>Текущее число колонок.</summary>
    public int Columns => _columns;
    private double _minimumSingleRowHeight;
    /// <summary>Минимальная высота карточки для сетки из одной строки.</summary>
    public double MinimumSingleRowHeight
    {
        get => _minimumSingleRowHeight;
        set
        {
            if (!double.IsFinite(value) || value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            _minimumSingleRowHeight = value;
            ApplyRowMinimums();
        }
    }

    /// <summary>Располагает карточки в адаптивной сетке.</summary>
    public UiAdaptiveGrid(int maximumColumns = 3, double minimumColumnWidth = 280, double spacing = 12)
    {
        if (maximumColumns < 1) throw new ArgumentOutOfRangeException(nameof(maximumColumns));
        if (!double.IsFinite(minimumColumnWidth) || minimumColumnWidth < 1)
            throw new ArgumentOutOfRangeException(nameof(minimumColumnWidth));
        if (!double.IsFinite(spacing) || spacing < 0)
            throw new ArgumentOutOfRangeException(nameof(spacing));
        _maximumColumns = maximumColumns;
        _minimumColumnWidth = minimumColumnWidth;
        ColumnSpacing = RowSpacing = spacing;
        SizeChanged += (_, _) => Reflow();
    }

    /// <summary>Задаёт набор карточек для отображения.</summary>
    public void SetCards(IEnumerable<View> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);
        var next = cards.ToArray();
        if (next.Any(x => x is null)) throw new ArgumentException("Карточки не могут быть null.", nameof(cards));
        if (new HashSet<View>(next, ReferenceEqualityComparer.Instance).Count != next.Length)
            throw new ArgumentException("Одна карточка не может находиться в двух ячейках.", nameof(cards));
        foreach (View oldCard in _cards)
            oldCard.MinimumHeightRequest = _originalMinimums[oldCard];
        Children.Clear();
        _cards.Clear();
        _originalMinimums.Clear();
        foreach (View card in next)
        {
            _cards.Add(card);
            _originalMinimums.Add(card, card.MinimumHeightRequest);
            Children.Add(card);
        }
        _columns = 0;
        Reflow();
    }

    /// <summary>Определяет число колонок из ширины контейнера.</summary>
    public static int CalculateColumns(double width, int maximum, double minimumWidth, double spacing = 12)
    {
        if (maximum < 1 || minimumWidth <= 0 || !double.IsFinite(minimumWidth))
            throw new ArgumentOutOfRangeException(nameof(maximum));
        if (!double.IsFinite(spacing) || spacing < 0) throw new ArgumentOutOfRangeException(nameof(spacing));
        if (!double.IsFinite(width) || width <= 0) return 1;
        return Math.Clamp((int)Math.Floor((width + spacing) / (minimumWidth + spacing)), 1, maximum);
    }

    private void Reflow()
    {
        int next = CalculateColumns(Width, _maximumColumns, _minimumColumnWidth, ColumnSpacing);
        if (_columns == next && ColumnDefinitions.Count == next)
        {
            ApplyRowMinimums();
            return;
        }
        _columns = next;
        ColumnDefinitions.Clear();
        RowDefinitions.Clear();
        for (int i = 0; i < next; i++)
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (int i = 0; i < _cards.Count; i += next)
            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        for (int i = 0; i < _cards.Count; i++)
        {
            Grid.SetColumn((BindableObject)_cards[i], i % next);
            Grid.SetRow((BindableObject)_cards[i], i / next);
        }
        ApplyRowMinimums();
    }

    private void ApplyRowMinimums()
    {
        // На нескольких строках карточки не обязаны иметь высоту всего окна.
        foreach (var card in _cards)
            card.MinimumHeightRequest = _cards.Count <= _columns && MinimumSingleRowHeight > 0
                ? Math.Max(_originalMinimums[card], MinimumSingleRowHeight)
                : _originalMinimums[card];
    }
}
