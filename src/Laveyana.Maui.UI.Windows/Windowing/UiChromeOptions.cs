using Microsoft.Maui.Graphics;

namespace Laveyana.Maui.UI.Windows.Windowing;

/// <summary>Вкладка заголовка, не зависящая от навигации приложения.</summary>
public sealed record UiChromeTab(string Title, Action Selected);

/// <summary>Конфигурация Windows-заголовка с F11 и авто-скрытием в полноэкранном режиме.</summary>
public sealed class UiChromeOptions
{
    /// <summary>Название приложения в заголовке.</summary>
    public string Title { get; set; } = "Laveyana";
    /// <summary>Путь к значку окна.</summary>
    public string? IconFilePath { get; set; }
    /// <summary>URI логотипа, например ms-appx:///logo.png. Имеет приоритет над IconFilePath.</summary>
    public Uri? LogoUri { get; set; }
    /// <summary>Ширина логотипа в DIP.</summary>
    public double LogoWidth { get; set; } = 44;
    /// <summary>Высота логотипа в DIP.</summary>
    public double LogoHeight { get; set; } = 26;
    /// <summary>Непрозрачность фона заголовка (0–255).</summary>
    public byte CaptionOpacity { get; set; } = 146;
    /// <summary>Непрозрачность фона панели кнопок (0–255).</summary>
    public byte ToolbarOpacity { get; set; } = 230;
    /// <summary>Цвет рамки окна.</summary>
    public Color Border { get; set; } = Color.FromArgb("#263548");
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
