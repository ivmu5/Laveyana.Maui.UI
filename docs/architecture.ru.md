# Архитектура Laveyana.Maui.UI

## Принципы

1. **Библиотека не знает о предметной области:** названия «дек», «трек», «OSC», «vJoy» отсутствуют в публичном API.
2. **Одна основная сборка** `Laveyana.Maui.UI` на `net10.0`, Windows-зависимости в `Laveyana.Maui.UI.Windows`.
3. **Нативные MAUI элементы:** Fluent-расширения возвращают тот же тип; базовые `Label`, `Grid`, `Border` и т.п. остаются доступными.
4. **Семантические роли цвета**, а не жёстко зашитые HEX-коды на каждом экране.
5. **Без глобальных singleton-переменных для UI:** менеджер темы регистрируется в DI, связывается с `Application` через `Attach` и корректно удаляет подписку `Dispose`.
6. **Анимация только при необходимости:** не запускать непрерывные UI-циклы ради статичного экрана.
7. **Дизайн для расширения:** компоненты V1 не требуют наследования от «суперконтейнеров» и оставляют возможность сменить хранилище настроек.

## Публичные поверхности

| Область | Ключевые типы | Источник идеи |
|---|---|---|
| Тема | `UiTheme`, `UiThemeSettings`, `UiThemeManager`, `UiPaletteCatalog` | `LavPulse.Controls/Views/Theme/*`, `PlayerColors` |
| Fluent | `UiFluentExtensions`, `UiBindingExtensions` | `CustomMauiUI-old-` |
| Настройки | `UiSettingsPanel`, `UiNumberInput`, `UiAppearanceEditor`, `UiThemePreferences` | `PlayerSettingsPanel`, `NumericSettingInput` |
| Цвет | `UiColorPicker` | `PaletteColorPicker` |
| Разметка | `UiAdaptiveGrid` | `LavOSC/DashboardGrid` |
| Список | `UiVirtualList<TItem,TRow>`, `UiVirtualListMath` | `LavPulse/TrackListView` |
| Анимации | `UiAnimator`, `UiMotionSettings`, `UiFramePolicy` | Анимации переходов в LavPulse |
| Навигация | `UiTabHost` | `LavOSC/OscReceiverPage.Workspace` |
| Windows | `UiWindowsChrome`, `UiChromeOptions`, `UiChromeTab` | Заголовки LavPulse и LavOSC |

## Не переносить в UI

- `PlayerSettings` целиком: настройки аудиопотока, анализ треков, BPM, спектр, деки остаются в LavPulse.
- `QueueTrackRow`, `DeckView`, `WaveformDrawable`, `MasterSpectrumView` — специализированные компоненты LavPulse.
- OSC-клиенты, UDP-приёмник, маршрутизация и vJoy — LavOSC.
- Настройки безопасности, шифрование, хранилища или сетевые сервисы — отдельные библиотеки Laveyana.
- Материальные шрифты и значки из архивной CustomMauiUI; приложение продолжает само владеть ресурсами и лицензиями шрифтов.

## Эволюция API

- До `1.0` версия имеет суффикс `alpha`, чтобы не заявлять о стабильном контракте.
- Добавление новых компонентов не должно вынуждать существующие экраны наследоваться от нового базового класса.
- Сложные элементы с переменной высотой, drag-and-drop между рядами, swipe-to-delete и виртуализацией списков пока остаются на рассмотрение V2: правильное разделение жестов и виртуализации требует дополнительных UI-тестов.
- Отладочные fps-метрики относятся к отдельной диагностике, но `UiAnimator` уже принимает общий лимит обновлений.
