namespace Laveyana.Maui.UI.Display;

/// <summary>Объединяет состояние окна и экрана. Обновлять из одного UI-потока.</summary>
public sealed class UiVisibilityState
{
    /// <summary>Окно активно.</summary>
    public bool IsActive { get; private set; }
    /// <summary>Окно отображается.</summary>
    public bool IsVisible { get; private set; }
    /// <summary>Экран интерактивен.</summary>
    public bool IsScreenInteractive { get; private set; } = true;
    /// <summary>Разрешено выполнять визуальные обновления.</summary>
    public bool CanRender => IsActive && IsVisible && IsScreenInteractive;
    /// <summary>Изменение итогового разрешения на отрисовку.</summary>
    public event Action<bool>? RenderAvailabilityChanged;

    /// <summary>Атомарно обновляет известные состояния; null оставляет прежнее значение.</summary>
    public void Update(bool? active = null, bool? visible = null, bool? screenInteractive = null)
    {
        bool previous = CanRender;
        IsActive = active ?? IsActive;
        IsVisible = visible ?? IsVisible;
        IsScreenInteractive = screenInteractive ?? IsScreenInteractive;
        if (previous != CanRender) RenderAvailabilityChanged?.Invoke(CanRender);
    }
}
