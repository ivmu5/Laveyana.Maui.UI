using Android.App;
using Android.Content;
using Android.OS;
using Laveyana.Maui.UI.Display;

namespace Laveyana.Maui.UI.Android.Display;

/// <summary>Следит за включением экрана и состоянием Activity. Создавать и освобождать на UI-потоке.</summary>
public sealed class UiAndroidScreenMonitor : IDisposable
{
    private readonly Activity _activity;
    private Receiver? _receiver;
    private bool _disposed;

    /// <summary>Общее состояние доступности отрисовки.</summary>
    public UiVisibilityState State { get; } = new();

    /// <summary>Создаёт монитор; подписка начинается при Start.</summary>
    public UiAndroidScreenMonitor(Activity activity) =>
        _activity = activity ?? throw new ArgumentNullException(nameof(activity));

    /// <summary>Вызывать из OnStart.</summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_receiver is not null) return;
        using var filter = new IntentFilter(Intent.ActionScreenOn);
        filter.AddAction(Intent.ActionScreenOff);
        var receiver = new Receiver(interactive => State.Update(screenInteractive: interactive));
        try
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
                _activity.RegisterReceiver(receiver, filter, ReceiverFlags.NotExported);
            else
                _activity.RegisterReceiver(receiver, filter);
            _receiver = receiver;
        }
        catch { receiver.Dispose(); throw; }
        State.Update(visible: true, screenInteractive: ReadInteractive());
    }

    /// <summary>Вызывать из OnResume или OnPause.</summary>
    public void SetResumed(bool resumed)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        State.Update(active: resumed, screenInteractive: ReadInteractive());
    }

    /// <summary>Вызывать из OnStop.</summary>
    public void Stop()
    {
        var receiver = _receiver;
        _receiver = null;
        try
        {
            if (receiver is not null) _activity.UnregisterReceiver(receiver);
        }
        finally
        {
            receiver?.Dispose();
            State.Update(active: false, visible: false);
        }
    }

    private bool ReadInteractive() =>
        _activity.GetSystemService(Context.PowerService) is not PowerManager { IsInteractive: false };

    /// <summary>Удаляет системную подписку и прекращает обновления.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
    }

    private sealed class Receiver(Action<bool> changed) : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent?.Action == Intent.ActionScreenOn) changed(true);
            else if (intent?.Action == Intent.ActionScreenOff) changed(false);
        }
    }
}
