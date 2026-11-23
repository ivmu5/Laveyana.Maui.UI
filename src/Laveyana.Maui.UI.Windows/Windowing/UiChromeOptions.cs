using Microsoft.Maui.Graphics;

namespace Laveyana.Maui.UI.Windows.Windowing;

/// <summary>Вкладка заголовка, не зависящая от Shell, LavOSC или LavPulse.</summary>
public sealed record UiChromeTab(string Title, Action Selected);

/// <summary>Конфигурация Windows-заголовка с F11 и авто-скрытием в полноэкранном режиме.</summary>
public sealed class UiChromeOptions
{
    /// <summary>Название приложения в заголовке.</summary>
    public string Title { get; set; } = "Laveyana";
    /// <summary>Путь к значку окна.</summary>
    public string? IconFilePath { get; set; }
    /// <summary>Высота заголовка в DIP.</summary>
    public int Height { get; set; } = 44;
    /// <summary>Номер первоначально выбранной вкладки.</summary>
    public int InitialSelectedTab { get; set; }
    /// <summary>Время анимации скрытия заголовка.</summary>
    public int AnimationMilliseconds { get; set; } = 180;
    /// <summary>Цвет фона заголовка.</summary>
    public Color Background { get; set; } = Color.FromArgb("#0B1020");
    /// <summary>Цвет панели кнопок.</summary>
    public Color Surface { get; set; } = Color.FromArgb("#263548");
    /// <summary>Цвет текста заголовка.</summary>
    public Color Text { get; set; } = Color.FromArgb("#F0F5FC");
    /// <summary>Акцентный цвет элементов.</summary>
    public Color Accent { get; set; } = Color.FromArgb("#58E5FC");
    /// <summary>Список вкладок заголовка.</summary>
    public IList<UiChromeTab> Tabs { get; } = new List<UiChromeTab>();

    /// <summary>Добавляет вкладку заголовка и обработчик её выбора.</summary>
    public UiChromeOptions Tab(string title, Action selected)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(selected);
        Tabs.Add(new UiChromeTab(title, selected));
        return this;
    }
}
