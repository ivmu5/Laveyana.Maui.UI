using Android.App;
using Laveyana.Maui.UI.Android.Display;
using Laveyana.Maui.UI.Android.Windowing;

namespace Laveyana.Maui.UI.Smoke;

/// <summary>Проверяет публичные Android API при компиляции.</summary>
public static class AndroidSmokeConsumer
{
    public static UiAndroidScreenMonitor CreateMonitor(Activity activity) => new(activity);
    public static void Configure(Activity activity)
    {
        UiAndroidWindow.SetShowWhenLocked(activity, false);
        UiAndroidWindow.ApplySystemBars(activity, false);
    }
}
