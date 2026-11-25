# Laveyana.Maui.UI

Открытый набор переиспользуемых компонентов и Fluent API для **.NET MAUI 10**. Единый дизайн-фундамент приложений Laveyana на Windows, Android, iOS и Mac Catalyst.

**Статус: `0.26.11.1` (глобальная версия 0, предварительная разработка).** Windows-сборка библиотек и часть CI-проверок прошли, но полный Verify и тесты интерфейса ещё не подтверждены. До стабильной версии 1 API может изменяться.

## Зачем нужен пакет

- Единый внешний вид приложения без зависимости от его предметной области.
- Темы: системная / светлая / тёмная / AMOLED, палитры Laveyana, Ocean, Warm и Mint, свои цвета, контрастные роли.
- Fluent-расширения над нативными MAUI-элементами: размеры, отступы, цвета, сетки, обработчики, привязки.
- Числовое поле настроек (ввод, ползунок, сброс, нормализация, двусторонний binding).
- Цветовой редактор HSV, готовый редактор оформления, ленивые группы настроек и вкладки.
- Адаптивная сетка карточек и виртуализированный список **фиксированной** высоты строк.
- Анимации по запросу (без бесконечного таймера), настройки частоты кадров и отключения эффектов.
- Отдельное Windows-расширение: заголовок окна, интерактивные вкладки, F11, показ при наведении курсора сверху в полноэкранном режиме.

## Пакеты

| NuGet ID | Сборка | Целевая платформа | Назначение |
|---|---|---|---|
| `Laveyana.Maui.UI` | `Laveyana.Maui.UI.dll` | `net10.0` | Общие компоненты для MAUI Windows / Android / iOS / Mac Catalyst |
| `Laveyana.Maui.UI.Windows` | `Laveyana.Maui.UI.Windows.dll` | `net10.0-windows10.0.19041.0` | **Опциональный** нативный заголовок WinUI |

Платформенные пакеты ссылаются на основной; обратной зависимости нет. Пакеты не зависят от приложений-потребителей. В проекте нет собственной БД и обязательного хранилища настроек.

## Версионирование

Формат: **`v.YY.MM.vv`** — глобальная версия, две цифры года, месяц с ведущим нулём и номер опубликованного выпуска в этом месяце (начиная с 1). Например, `0.26.11.1` → `0.26.11.2` → `0.26.12.1`. Git-тег содержит префикс `v`: `v0.26.11.1`, а свойство `Version` и версия NuGet префикса не содержат. Оба пакета выпускаются с одной версией.

**Важно:** схема отличается от SemVer. NuGet поддерживает четыре числовых компонента, но удаляет ведущие нули: тег `v0.27.01.1` создаст пакет `0.27.1.1`. Версия `0.*` сама по себе не помечается NuGet как prerelease. Подробности — в [правилах версионирования](docs/versioning.ru.md).

## Установка

Пока пакеты ещё не выпущены, подключайте их через `ProjectReference`. После первого релиза:

```bash
dotnet add package Laveyana.Maui.UI --version 0.26.11.1
# Только для Windows MAUI-проекта:
dotnet add package Laveyana.Maui.UI.Windows --version 0.26.11.1
```

### Регистрация в MauiProgram.cs

```csharp
using Laveyana.Maui.UI;
using Laveyana.Maui.UI.Themes;

return MauiApp.CreateBuilder()
    .UseMauiApp<App>()
    .UseLaveyanaMauiUi(new UiThemeSettings
    {
        Mode = UiThemeMode.System,
        Preset = UiPalettePreset.Laveyana
    })
    .Build();
```

В `Application` подключите менеджер **после создания ресурсов**:

```csharp
using Laveyana.Maui.UI.Themes;
using Laveyana.Maui.UI.Settings;

public partial class App : Application
{
    public App(UiThemeManager themes)
    {
        InitializeComponent();
        themes.Attach(this, UiThemePreferences.Load("laveyana.ui"));
    }
}
```

Настройки сохраняются **явно**: `UiThemePreferences.Save(themes.Settings, "laveyana.ui")`. Можно использовать собственное хранилище через `UiThemeManager.Apply(...)`.

## Fluent API

```csharp
using Laveyana.Maui.UI.Controls;
using Laveyana.Maui.UI.Fluent;
using Laveyana.Maui.UI.Themes;

var heading = Ui.Text("Настройки", 22, UiColorRole.Text, bold: true)
    .WithMargin(0, 8)
    .OnTap((_, _) => System.Diagnostics.Debug.WriteLine("Нажатие"));

var port = Ui.Number("UDP-порт", 1, 65535, 9000, step: 1, showSlider: false);
port.ValueChanged += (_, value) => System.Diagnostics.Debug.WriteLine(value);

var panel = Ui.Card(Ui.Stack(10, heading, port));
var cards = Ui.Cards(maximumColumns: 3, minimumColumnWidth: 290);
cards.SetCards([panel, Ui.Card(Ui.Text("Сеть")), Ui.Card(Ui.Text("Модули"))]);
```

### Привязки

```csharp
// ViewModel реализует INotifyPropertyChanged.
var label = new Label().Bind(x => x.Text, viewModel, x => x.Title);
```

Это стандартная MAUI Binding, а не собственная система наблюдения за состоянием. Для NativeAOT/trim следует проверить привязки отражения и рассмотреть скомпилированные XAML bindings.

### Полный редактор оформления

```csharp
var appearance = Ui.Appearance(themes.Settings);
appearance.SettingsChanged += (_, next) =>
{
    themes.Apply(next); // предпросмотр сразу
    UiThemePreferences.Save(next, "laveyana.ui");
};
```

Редактор позволяет выбирать тему, палитру, AMOLED и пользовательские цвета через HSV + HEX. Его можно встроить в любой экран приложения; он не сохраняет настройки сам по себе.

### Настройки и навигация

```csharp
var settings = Ui.Settings()
    .Section("Интерфейс", () => Ui.Stack(8, Ui.Text("Цвета и тема")))
    .Section("О программе", () => Ui.Text("Версия 1"));

var tabs = new Laveyana.Maui.UI.Navigation.UiTabHost()
    .Tab("Главная", () => Ui.Text("Панель управления"))
    .Tab("Настройки", () => settings);
```

### Виртуализация списка

```csharp
var list = new UiVirtualList<MyItem, Label>(
    rowHeight: 54,
    create: () => Ui.Text(""),
    bind: (row, item, index) => row.Text = item.Title);
list.SetItems(myUniqueItems);
```

**Ограничения:** одна и та же ссылка на модель не должна дублироваться; высота строк постоянна; для переменной высоты предпочтителен штатный `CollectionView`. Подписки, добавленные к повторно используемым строкам, следует снимать в `release`.

### Анимации

```csharp
var animator = new Laveyana.Maui.UI.Animation.UiAnimator(
    new() { FrameRate = Laveyana.Maui.UI.Animation.UiFrameRate.Fps120 });
await animator.FadeAsync(panel, 0.5, TimeSpan.FromMilliseconds(200));
await animator.SlideAsync(panel, 0, 0, TimeSpan.FromMilliseconds(260));
```

Частота кадров — ограничение **запрашиваемого интервала**, не гарантия реальных 120 FPS. Вызывайте анимации на UI-потоке; обновление содержимого, которое не видно, лучше останавливать на уровне приложения.

## Проверка и публикация

- `.github/workflows/verify.yml`: PR + push в `master` + ручной запуск, NUnit под Linux/Windows с проверкой TRX, Windows + пользовательский smoke-проект, тестовая упаковка.
- `.github/workflows/extended-regression.yml`: **1-го числа каждого месяца**, вручную, пять повторений NUnit, Android и Apple smoke-компиляция.
- `.github/workflows/automatic-release.yml`: после успешного Verify на итоговом коммите объединённого PR в `master` создаёт GitHub Release новой версии.
- `.github/workflows/publish-nuget.yml`: выпуск **только после публикации GitHub Release** (вызывается напрямую после автоматического Release или по событию ручной публикации), проверка схемы `v.YY.MM.vv`/версии/принадлежности к `master`, тесты, упаковка, OIDC NuGet Trusted Publishing.

Прежде чем выпускать пакет, проверьте [инструкции по публикации](docs/publishing.ru.md) и [повторный аудит с перечнем исправленных ошибок](docs/verification.ru.md). При обычном коммите без объединения PR выпуск не начинается. После успешного объединённого PR в `master` может автоматически создаться GitHub Release и начаться публикация NuGet, **если версия в `Directory.Build.props` новая**. Каждому новому выпуску необходим новый `vv`; обновляйте версию в PR. См. [политику выпуска](docs/publishing.ru.md).

## Совместимость с приложениями

Рекомендации для интеграции: [docs/migration.ru.md](docs/migration.ru.md).

## Документация

- [Архитектура и границы ответственности](docs/architecture.ru.md)
- [Миграция приложений на общую сборку](docs/migration.ru.md)
- [GitHub Actions и NuGet](docs/publishing.ru.md)
- [Правила версионирования v.YY.MM.vv](docs/versioning.ru.md)
- [Заголовок WinUI](docs/windowing.ru.md)
- [Статус проверки](docs/verification.ru.md)

Лицензия: **MIT**, © 2026 ivmu5.

## Общие параметры оформления и состояние экрана

`UiDesignTokens` — базовые размеры текста, скруглений, отступов и интерактивных элементов. `Ui` использует эти значения по умолчанию. `UiScreen.SetKeepAwakeAsync` управляет запретом автоматического выключения экрана в поддерживаемых MAUI окружениях (не разблокирует устройство). `UiVisibilityState` объединяет состояние жизненного цикла и доступность экрана для остановки дорогостоящих обновлений.

### Android (необязательный пакет)

`Laveyana.Maui.UI.Android` подключается только на net10.0-android. `UiAndroidWindow.SetShowWhenLocked` разрешает открытой Activity оставаться поверх keyguard; `ApplySystemBars` управляет системными панелями. `UiAndroidScreenMonitor` отслеживает состояние экрана и Activity. Вызывайте Start/SetResumed/Stop/Dispose из жизненного цикла Activity. См. [платформенные API](docs/platforms.ru.md).
