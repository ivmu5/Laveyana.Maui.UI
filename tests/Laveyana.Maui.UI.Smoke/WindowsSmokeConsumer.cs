using Laveyana.Maui.UI.Windows.Windowing;

namespace Laveyana.Maui.UI.Smoke;

/// <summary>Компиляционная проверка публичных WinUI API в приложении-потребителе.</summary>
public static class WindowsSmokeConsumer
{
    /// <summary>Создаёт конфигурацию пользовательского заголовка.</summary>
    public static UiChromeOptions CreateOptions() => new UiChromeOptions
    {
        Title = "Пример",
        Height = 44
    }.Tab("Главная", () => { });
}
