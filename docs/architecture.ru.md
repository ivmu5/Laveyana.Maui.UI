# Архитектура

Библиотека — независимый UI-фундамент. Потребители зависят от неё, но она не зависит от их бизнес-логики, моделей, ресурсов и хранилищ.

| Область | Общие API |
|---|---|
| Тема | UiTheme, UiThemeSettings, UiThemeManager, UiPaletteCatalog |
| Дизайн | UiDesignTokens, Ui, fluent extensions |
| Разметка | UiAdaptiveGrid |
| Списки | UiVirtualList, UiVirtualListMath |
| Настройки | UiSettingsPanel, UiAppearanceEditor, UiThemePreferences |
| Анимации | UiAnimator, UiMotionSettings, UiFramePolicy |
| Навигация | UiTabHost |
| Экран и жизненный цикл | UiScreen, UiVisibilityState |
| Windows | UiWindowsChrome, UiChromeOptions, UiChromeTab |

В приложении остаются предметные настройки, специфическая графика, высокочастотный рендеринг, хранилища, разрешения, обработка фоновых задач и системные интеграции конкретного назначения. Не заменяйте оптимизированные контролы без измерения производительности.

Общее ядро portable net10.0; платформенные адаптеры ссылаются на ядро, не наоборот. Все новые API должны иметь нейтральные имена, явный жизненный цикл и документированные ограничения платформ.
