# Заголовок Windows через Laveyana.Maui.UI.Windows

Расширение не включается в Android/iOS/Mac Catalyst-сборки. Пакет используется только в Windows MAUI проекте.

## Windows MauiProgram

```csharp
#if WINDOWS
using Laveyana.Maui.UI.Windows.Windowing;
using Microsoft.Maui.LifecycleEvents;
#endif

var builder = MauiApp.CreateBuilder().UseMauiApp<App>().UseLaveyanaMauiUi();
#if WINDOWS
builder.ConfigureLifecycleEvents(events => events.AddWindows(windows => windows
    .OnPlatformWindowSubclassed((native, _) => UiWindowsChrome.Prepare(native))));
#endif
return builder.Build();
```

## Обработчик созданного Window

```csharp
#if WINDOWS
using Laveyana.Maui.UI.Windows.Windowing;
#endif

protected override Window CreateWindow(IActivationState? activationState)
{
    var window = new Window(new MainPage());
#if WINDOWS
    window.Created += (_, _) =>
    {
        var settings = new UiChromeOptions
        {
            Title = "Пример",
            IconFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "AppIcon", "appicon.png")
        };
        settings.Tab("Главная", () => NavigateToDashboard())
            .Tab("Модули", () => NavigateToModules())
            .Tab("Настройки", () => NavigateToSettings());
        _windowChrome = UiWindowsChrome.Attach(window, settings);
    };
#endif
    return window;
}
```

`_windowChrome` — поле типа `UiWindowsChrome?`, определённое только в Windows-сборке. Пример иллюстративный: `NavigateToDashboard/Modules/Settings` — методы **вашего приложения**, которые необходимо определить самостоятельно.

При изменении темы вызовите `chrome.ApplyTheme(themes.Current)` (с подпиской на `themes.ThemeChanged` на стороне приложения). При смене вкладки из приложения вызовите `_windowChrome?.SetActiveTab(index)`, чтобы синхронизировать подсветку. В полном экране заголовок скрывается и появляется при наведении курсора к верхней границе; выход — F11 или Esc. При закрытии окна подписки и анимационный обработчик освобождаются.

## Зона ответственности приложения

- Вертикальный отступ контента под 44px заголовок задаётся в самом приложении.
- Высота отступа доступна как `chrome.ContentInset`.
- События навигации не должны управлять жизненным циклом сервисов из библиотеки.
- Проверить на реальном Windows скрытие/появление, drag, maximize, восстановление размеров, DPI и F11 до выпуска NuGet.

## Заголовок с логотипом и собственными кнопками (0.26.11.3)

```csharp
var chrome = UiWindowsChrome.Attach(window, new UiChromeOptions
{
    Title = "Пример",
    LogoUri = new Uri("ms-appx:///appicon.png"),
    LogoWidth = 44,
    LogoHeight = 26,
    CaptionOpacity = 146,
    ToolbarOpacity = 230
});
```

LogoUri — ресурс приложения-потребителя; библиотека не включает логотип приложения. Абсолютный IconFilePath остаётся совместимым резервным API. Заголовок использует собственные четыре кнопки во всех режимах, системные кнопки скрываются через Collapsed. Разворачивание повторным нажатием восстанавливает размер. F11/Esc, вкладки, автоматическое появление и скрытие в fullscreen сохранены. Старые Windows без AppWindow title bar customization по-прежнему возвращают null из Attach; приложение может использовать существующий fallback. Проверяйте логотип, DPI, перетаскивание, все четыре кнопки и закрытие/открытие окна на реальном устройстве.
