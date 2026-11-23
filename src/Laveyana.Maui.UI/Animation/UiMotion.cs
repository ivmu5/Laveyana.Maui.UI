namespace Laveyana.Maui.UI.Animation;

/// <summary>Запрашиваемая частота кадров анимаций. 0 = выбор платформы.</summary>
public enum UiFrameRate
{
    /// <summary>Автоматический режим с приблизительным интервалом 60 FPS.</summary>
    Automatic = 0,
    /// <summary>Лимит 30 кадров в секунду.</summary>
    Fps30 = 30,
    /// <summary>Лимит 60 кадров в секунду.</summary>
    Fps60 = 60,
    /// <summary>Лимит 90 кадров в секунду.</summary>
    Fps90 = 90,
    /// <summary>Лимит 120 кадров в секунду.</summary>
    Fps120 = 120,
    /// <summary>Лимит 144 кадров в секунду.</summary>
    Fps144 = 144,
    /// <summary>Лимит 165 кадров в секунду.</summary>
    Fps165 = 165,
    /// <summary>Лимит 240 кадров в секунду.</summary>
    Fps240 = 240,
    /// <summary>Лимит 360 кадров в секунду.</summary>
    Fps360 = 360,
}

/// <summary>Глобальные предпочтения для плавных переходов.</summary>
public sealed record UiMotionSettings
{
    /// <summary>Разрешает анимации.</summary>
    public bool Enabled { get; init; } = true;
    /// <summary>Запрашиваемый лимит частоты кадров.</summary>
    public UiFrameRate FrameRate { get; init; } = UiFrameRate.Automatic;
    /// <summary>Продолжительность анимации по умолчанию в миллисекундах.</summary>
    public int DefaultDurationMilliseconds { get; init; } = 220;
}

/// <summary>Ограничивает число запрашиваемых шагов; не гарантирует частоту обновления дисплея.</summary>
public static class UiFramePolicy
{
    /// <summary>Возвращает примерный интервал между кадрами в миллисекундах.</summary>
    public static uint FrameIntervalMilliseconds(UiFrameRate requested)
    {
        int fps = (int)requested;
        if (fps == 0) return 16; // Консервативное приближение ~60 FPS; реальная частота зависит от платформы
        if (fps < 1 || fps > 360) throw new ArgumentOutOfRangeException(nameof(requested));
        return (uint)Math.Max(1, Math.Round(1000d / fps));
    }

    /// <summary>Вычисляет кубическую кривую замедления.</summary>
    public static double EaseOutCubic(double progress)
    {
        progress = Math.Clamp(progress, 0, 1);
        return 1 - Math.Pow(1 - progress, 3);
    }
}

/// <summary>Одна анимация работает по запросу и завершается: постоянных background-циклов нет.</summary>
public sealed class UiAnimator
{
    private long _sequence;
    /// <summary>Настройки анимаций экземпляра.</summary>
    public UiMotionSettings Settings { get; private set; }

    /// <summary>Запускает кратковременные MAUI-анимации по запросу.</summary>
    public UiAnimator(UiMotionSettings? settings = null) => Settings = settings ?? new UiMotionSettings();
    /// <summary>Меняет предпочтения будущих анимаций.</summary>
    public void Configure(UiMotionSettings settings) => Settings = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>Анимирует прозрачность элемента.</summary>
    public Task FadeAsync(VisualElement view, double opacity, TimeSpan? duration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(view);
        if (!double.IsFinite(opacity) || opacity is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(opacity));
        double initial = view.Opacity;
        return AnimateAsync(view, progress => view.Opacity = initial + (opacity - initial) * progress,
            duration, cancellationToken: cancellationToken);
    }

    /// <summary>Анимирует сдвиг элемента по двум осям.</summary>
    public Task SlideAsync(VisualElement view, double x, double y, TimeSpan? duration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(view);
        if (!double.IsFinite(x) || !double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(x));
        double initialX = view.TranslationX, initialY = view.TranslationY;
        return AnimateAsync(view, progress =>
        {
            view.TranslationX = initialX + (x - initialX) * progress;
            view.TranslationY = initialY + (y - initialY) * progress;
        }, duration, cancellationToken: cancellationToken);
    }

    /// <summary>Вызывать из UI-потока. Отмена останавливает собственную анимацию без подавления других.</summary>
    public async Task AnimateAsync(VisualElement view, Action<double> draw, TimeSpan? duration = null,
        Easing? easing = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(draw);
        cancellationToken.ThrowIfCancellationRequested();
        var actual = duration ?? TimeSpan.FromMilliseconds(Settings.DefaultDurationMilliseconds);
        if (actual < TimeSpan.Zero || actual.TotalMilliseconds > uint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(duration));
        if (!Settings.Enabled || !view.IsVisible || actual == TimeSpan.Zero)
        {
            draw(1);
            return;
        }

        string key = $"Laveyana.Maui.UI.{Interlocked.Increment(ref _sequence)}";
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = cancellationToken.Register(() =>
        {
            // CancellationToken может сработать не на UI-потоке.
            if (view.Dispatcher is { } dispatcher)
                dispatcher.Dispatch(() => { view.AbortAnimation(key); tcs.TrySetCanceled(cancellationToken); });
            else tcs.TrySetCanceled(cancellationToken);
        });
        var animation = new Microsoft.Maui.Controls.Animation(
            value => draw(value), 0d, 1d, easing ?? Easing.CubicOut);
        animation.Commit(view, key, rate: UiFramePolicy.FrameIntervalMilliseconds(Settings.FrameRate),
            length: (uint)Math.Max(1, Math.Round(actual.TotalMilliseconds)),
            finished: (_, canceled) => tcs.TrySetResult(!canceled));
        await tcs.Task.ConfigureAwait(false);
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);
        // MAUI уже установил конечное значение в callback анимации на UI-потоке.
    }
}
