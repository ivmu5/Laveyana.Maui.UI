namespace Laveyana.Maui.UI.Themes;

/// <summary>Устанавливает динамические ресурсы и обновляет системную тему без перезапуска.</summary>
public sealed class UiThemeManager : IDisposable
{
    /// <summary>Префикс ключей ресурсов библиотеки.</summary>
    public const string ResourcePrefix = "Laveyana.Maui.UI";
    private Application? _application;
    private UiThemeSettings _settings = new();

    /// <summary>Последняя рассчитанная палитра.</summary>
    public UiTheme Current { get; private set; } = UiTheme.Create(new());
    /// <summary>Управляет динамическими ресурсами цветов приложения.</summary>
    public UiThemeManager(UiThemeSettings? initial = null)
    {
        _settings = initial ?? new UiThemeSettings();
        Current = UiTheme.Create(_settings);
    }
    /// <summary>Текущие параметры или панель настроек, в зависимости от типа.</summary>
    public UiThemeSettings Settings => _settings;
    /// <summary>Возникает при обновлении ресурсов темы.</summary>
    public event EventHandler? ThemeChanged;

    /// <summary>Возвращает ключ цветового ресурса.</summary>
    public static string ColorKey(UiColorRole role) => $"{ResourcePrefix}.Color.{role}";
    /// <summary>Возвращает ключ кисти соответствующего цвета.</summary>
    public static string BrushKey(UiColorRole role) => $"{ResourcePrefix}.Brush.{role}";

    /// <summary>Вызывать после создания Application; повторный вызов безопасен.</summary>
    public void Attach(Application application, UiThemeSettings? settings = null)
    {
        ArgumentNullException.ThrowIfNull(application);
        if (_application is not null)
            _application.RequestedThemeChanged -= OnRequestedThemeChanged;
        _application = application;
        _application.RequestedThemeChanged += OnRequestedThemeChanged;
        Apply(settings ?? _settings);
    }

    /// <summary>Пересчитывает цвета и применяет настройки ко всем динамическим ресурсам.</summary>
    public void Apply(UiThemeSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        var requested = _application?.RequestedTheme ?? AppTheme.Dark;
        Current = UiTheme.Create(settings, requested);
        if (_application is { } app)
        {
            foreach (UiColorRole role in Enum.GetValues<UiColorRole>())
            {
                app.Resources[ColorKey(role)] = Current[role];
                app.Resources[BrushKey(role)] = new SolidColorBrush(Current[role]);
            }
        }
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        if (_settings.Mode == UiThemeMode.System)
            Apply(_settings);
    }

    /// <summary>Освобождает подписки на системные события.</summary>
    public void Dispose()
    {
        if (_application is not null)
            _application.RequestedThemeChanged -= OnRequestedThemeChanged;
        _application = null;
    }
}
