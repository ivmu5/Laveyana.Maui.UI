# Миграция LavPulse и LavOSC

Текущая версия библиотеки **не меняет сами репозитории**. Миграцию выполнить после успешной сборки и GitHub Actions.

## Шаг 1: LavPulse

Исходники в репозитории: `src/LavPulse.Controls`.

1. Добавить в проект приложения и проект `LavPulse.Controls` ссылку на `Laveyana.Maui.UI` (локальный `ProjectReference`, после выпуска — `PackageReference`).
2. Установить `UiThemeManager` в `MauiProgram` и `App`; восстановить старые сохранённые настройки `PlayerPreferences`, преобразовав только интерфейсные поля в `UiThemeSettings`.
3. Перевести применение цветов в `MixerStyle` с `LavPulse.Color.*` на семантические ключи `Laveyana.Maui.UI.Color.*`. Не удалять старые ключи, пока все контролы их используют.
4. В `NumericSettingInput` заменить инкрементальные части на `UiNumberInput`, **сохранив** поведение прокрутки колеса там, где оно важно. При переносе проверить жесты Android и отсутствие конфликта со scroll-контейнером.
5. Вывести `PaletteColorPicker` на общий `UiColorPicker`, а `PlayerSettingsPanel` оставить как композицию специализированных секций + `UiSettingsPanel`.
6. После этого отдельно рассмотреть `TrackListView`: новая `UiVirtualList` пока поддерживает только равновысокие строки, а текущий список LavPulse содержит жесты и жизненный цикл переходов. **Не заменять без тестов!**
7. `WindowsWindowChrome` менять только после проверки восстановления окна, переключения F11 и drag-regions; новая Windows-реализация предоставляет независимые вкладки, но не переносит специфические размеры/переходы дек.

## Шаг 2: LavOSC

Исходники в репозитории: `src/LavOSC`.

1. Подключить `Laveyana.Maui.UI` в MAUI проект LavOSC.
2. Заменить статические цвета `PulseVisuals` на тему `UiPalettePreset.LavOsc`, при этом оставить семантические цвета статусов (`Success`, `Warning`, `Danger`).
3. Заменить `DashboardGrid` на `UiAdaptiveGrid`, адаптировав пороги ширины; при необходимости выбрать `minimumColumnWidth` для текущих карточек.
4. Перенести фабрики `Caption`, `Heading`, `Card`, `MakeButton` в `Ui.Text`, `Ui.Card`, `Ui.Button`.
5. Использовать `UiTabHost` **только** там, где не требуется разделяемое состояние содержимого. Состояние OSC и жизненный цикл активных модулей остаются в приложении.
6. Перенести заголовок окна на `Laveyana.Maui.UI.Windows` с явным списком вкладок; обязательно восстановить `SetCaptionInset` в самом LavOSC.

## Предохранители от регрессий

- Отказ от наследования `LavPulse.Controls` → `Laveyana.Maui.UI` должен ликвидировать зависимость UI-библиотеки от `Laveyana.Audio.Playback`.
- Не менять идентификаторы приложения, `ApplicationId`, папки хранения данных, ключи Preferences и существующую сериализацию на первой итерации.
- Сравнить скриншоты до/после для Windows, Android, светлой/тёмной/AMOLED тем, нестандартного масштабирования и изменяемого DPI.
- Проверить остановку UI-анимаций при сворачивании и отсутствии активной страницы, а также восстановление F11.
- Для OSC проверить активность модулей и доставку сообщений при навигации; для LavPulse — очереди, визуализацию и переключение дек.

## Установка до публикации

В проектах приложений:

```xml
<ProjectReference Include="../../Laveyana.Maui.UI/src/Laveyana.Maui.UI/Laveyana.Maui.UI.csproj" />
```

Для Windows-specific функционала (путь относительно конкретного проекта уточнить):

```xml
<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">
  <ProjectReference Include="../../Laveyana.Maui.UI/src/Laveyana.Maui.UI.Windows/Laveyana.Maui.UI.Windows.csproj" />
</ItemGroup>
```

После выпуска заменить `ProjectReference` на `PackageReference` с версией из релиза.
