using Microsoft.Maui.Devices;

namespace Laveyana.Maui.UI.Display;

/// <summary>Общие возможности экрана, предоставляемые MAUI Essentials.</summary>
public static class UiScreen
{
    /// <summary>Запрещает автоматическое выключение экрана или возвращает системное поведение.
    /// Возвращает false, если платформа не предоставляет эту возможность. Не снимает блокировку.</summary>
    public static Task<bool> SetKeepAwakeAsync(bool enabled) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            try
            {
                DeviceDisplay.Current.KeepScreenOn = enabled;
                return true;
            }
            catch (NotSupportedException) { return false; }
        });

    /// <summary>Получает сведения об основном экране на UI-потоке.</summary>
    public static Task<DisplayInfo> GetMainDisplayInfoAsync() =>
        MainThread.InvokeOnMainThreadAsync(() => DeviceDisplay.Current.MainDisplayInfo);
}
