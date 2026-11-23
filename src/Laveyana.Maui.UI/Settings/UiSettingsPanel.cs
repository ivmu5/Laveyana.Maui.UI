using Laveyana.Maui.UI.Controls;
using Laveyana.Maui.UI.Themes;

namespace Laveyana.Maui.UI.Settings;

/// <summary>Ленивые разделы настроек. Содержимое создаётся только после первого открытия.</summary>
public sealed class UiSettingsPanel : ContentView
{
    private readonly VerticalStackLayout _menu = new() { Spacing = 6 };
    private readonly ContentView _host = new();
    private readonly List<(string Name, Func<View> Build, Button Button, View? Content)> _sections = [];
    private int _selected = -1;

    /// <summary>Индекс текущего раздела.</summary>
    public int SelectedSection => _selected;
    /// <summary>Возникает после открытия другого раздела.</summary>
    public event EventHandler<int>? SectionChanged;

    /// <summary>Создаёт ленивые разделы для произвольных настроек.</summary>
    public UiSettingsPanel()
    {
        Content = new VerticalStackLayout { Spacing = 12, Children = { _menu, _host } };
    }

    /// <summary>Создаёт группу настроек в карточке.</summary>
    public UiSettingsPanel Section(string name, Func<View> createContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(createContent);
        int index = _sections.Count;
        var button = Ui.Button(name);
        button.HorizontalOptions = LayoutOptions.Fill;
        button.Clicked += (_, _) => Show(index);
        _sections.Add((name, createContent, button, null));
        _menu.Children.Add(button);
        if (_selected < 0) Show(0);
        return this;
    }

    /// <summary>Открывает раздел по индексу.</summary>
    public void Show(int index)
    {
        if (index < 0 || index >= _sections.Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == _selected) return;
        // Если текущий элемент уже был размещён в другом родителе, освободить хост.
        _host.Content = null;
        _selected = index;
        var section = _sections[index];
        if (section.Content is null)
        {
            var content = section.Build() ?? throw new InvalidOperationException("Фабрика раздела вернула null.");
            _sections[index] = (section.Name, section.Build, section.Button, content);
        }
        _host.Content = _sections[index].Content;
        for (int i = 0; i < _sections.Count; i++)
            _sections[i].Button.SetDynamicResource(VisualElement.BackgroundColorProperty,
                UiThemeManager.ColorKey(i == index ? UiColorRole.Selected : UiColorRole.Button));
        SectionChanged?.Invoke(this, index);
    }
}
