using Laveyana.Maui.UI.Controls;
using Laveyana.Maui.UI.Themes;

namespace Laveyana.Maui.UI.Navigation;

/// <summary>Навигация по вкладкам без привязки к конкретной предметной области.</summary>
public sealed class UiTabHost : ContentView
{
    private readonly HorizontalStackLayout _tabs = new() { Spacing = 6 };
    private readonly ContentView _body = new();
    private readonly List<(string Title, Func<View> Create, Button Button, View? View)> _items = [];
    private int _selected = -1;

    /// <summary>Индекс выбранной вкладки.</summary>
    public int SelectedIndex => _selected;
    /// <summary>Возникает после переключения вкладки.</summary>
    public event EventHandler<int>? SelectedIndexChanged;

    /// <summary>Простой хост вкладок с созданием страниц по требованию.</summary>
    public UiTabHost()
    {
        Content = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) },
            Children = { _tabs, _body }
        };
        Grid.SetRow((BindableObject)_body, 1);
    }

    /// <summary>Добавляет вкладку с отложенным созданием содержимого.</summary>
    public UiTabHost Tab(string title, Func<View> content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(content);
        int index = _items.Count;
        var button = Ui.Button(title);
        button.Clicked += (_, _) => Select(index);
        _items.Add((title, content, button, null));
        _tabs.Children.Add(button);
        if (_selected < 0) Select(0);
        return this;
    }

    /// <summary>Выбирает вкладку по индексу.</summary>
    public void Select(int index)
    {
        if (index < 0 || index >= _items.Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == _selected) return;
        _body.Content = null;
        _selected = index;
        var tab = _items[index];
        if (tab.View is null)
        {
            View content = tab.Create() ?? throw new InvalidOperationException("Фабрика вкладки вернула null.");
            _items[index] = (tab.Title, tab.Create, tab.Button, content);
        }
        _body.Content = _items[index].View;
        for (int i = 0; i < _items.Count; i++)
            _items[i].Button.SetDynamicResource(VisualElement.BackgroundColorProperty,
                UiThemeManager.ColorKey(i == index ? UiColorRole.Selected : UiColorRole.Button));
        SelectedIndexChanged?.Invoke(this, index);
    }
}
