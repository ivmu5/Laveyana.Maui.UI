using Android.App;
using Android.Views;
using AndroidX.Core.View;

namespace Laveyana.Maui.UI.Android.Windowing;

/// <summary>Настройки окна Android. Вызывать на UI-потоке после создания Activity.</summary>
public static class UiAndroidWindow
{
    /// <summary>Разрешает уже открытой Activity отображаться поверх keyguard.
    /// Не разблокирует устройство и не запускает Activity из фона.</summary>
    public static void SetShowWhenLocked(Activity activity, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(activity);
        if (OperatingSystem.IsAndroidVersionAtLeast(27))
            activity.SetShowWhenLocked(enabled);
        else
            SetLegacyShowWhenLocked(activity, enabled);
    }

#pragma warning disable CS0618
    private static void SetLegacyShowWhenLocked(Activity activity, bool enabled)
    {
        if (enabled) activity.Window?.AddFlags(WindowManagerFlags.ShowWhenLocked);
        else activity.Window?.ClearFlags(WindowManagerFlags.ShowWhenLocked);
    }
#pragma warning restore CS0618

    /// <summary>Настраивает контраст системных значков и видимость системных панелей.
    /// При изменении фокуса Activity настройки можно применить повторно.</summary>
    public static void ApplySystemBars(Activity activity, bool lightBackground,
        bool hideStatusBar = false, bool hideNavigationBar = false)
    {
        ArgumentNullException.ThrowIfNull(activity);
        if (activity.Window is not { } window) return;
        var controller = WindowCompat.GetInsetsController(window, window.DecorView);
        controller.AppearanceLightStatusBars = lightBackground;
        controller.AppearanceLightNavigationBars = lightBackground;
        controller.SystemBarsBehavior = hideStatusBar || hideNavigationBar
            ? WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe
            : WindowInsetsControllerCompat.BehaviorDefault;
        int status = WindowInsetsCompat.Type.StatusBars();
        int navigation = WindowInsetsCompat.Type.NavigationBars();
        if (hideStatusBar) controller.Hide(status); else controller.Show(status);
        if (hideNavigationBar) controller.Hide(navigation); else controller.Show(navigation);
    }
}
