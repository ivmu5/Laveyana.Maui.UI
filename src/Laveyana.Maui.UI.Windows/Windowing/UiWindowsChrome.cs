using Microsoft.Maui.Graphics;
using Laveyana.Maui.UI.Themes;
using System.Diagnostics;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics;
using Windows.System;
using Windows.UI.ViewManagement;
using NativeWindow = Microsoft.UI.Xaml.Window;
using NativeKeyboardAccelerator = Microsoft.UI.Xaml.Input.KeyboardAccelerator;
using NativeKeyboardAcceleratorInvokedEventArgs = Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs;
using NativeVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
using NativeHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using NativeImage = Microsoft.UI.Xaml.Controls.Image;
using NativeStretch = Microsoft.UI.Xaml.Media.Stretch;
using NativeCanvas = Microsoft.UI.Xaml.Controls.Canvas;
using MauiWindow = Microsoft.Maui.Controls.Window;
using NativeBorder = Microsoft.UI.Xaml.Controls.Border;
using NativeButton = Microsoft.UI.Xaml.Controls.Button;
using NativeGrid = Microsoft.UI.Xaml.Controls.Grid;
using NativeText = Microsoft.UI.Xaml.Controls.TextBlock;
using NativeColor = Windows.UI.Color;
using NativeBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using NativeFontIcon = Microsoft.UI.Xaml.Controls.FontIcon;
using NativeThickness = Microsoft.UI.Xaml.Thickness;
using NativeCornerRadius = Microsoft.UI.Xaml.CornerRadius;
using NativeVisibility = Microsoft.UI.Xaml.Visibility;
using NativePanel = Microsoft.UI.Xaml.Controls.Panel;
using NativeAutomation = Microsoft.UI.Xaml.Automation.AutomationProperties;

namespace Laveyana.Maui.UI.Windows.Windowing;

/// <summary>
/// Переиспользуемый WinUI Chrome для приложений Laveyana.
/// Не владеет экранами, навигацией или состоянием приложения.
/// </summary>
public sealed class UiWindowsChrome : IDisposable
{
    private readonly NativeWindow _native;
    private readonly NativePanel _root;
    private readonly AppWindow _app;
    private readonly OverlappedPresenter _presenter;
    private readonly UiChromeOptions _options;
    private readonly InputNonClientPointerSource _pointer;
    private const int ButtonWidth = 36;
    private const int ToolbarWidth = ButtonWidth * 4 + 3 * 3 + 3 * 2 + 8;
    private readonly NativeGrid _caption;
    private readonly NativeGrid _drag;
    private readonly StackPanel _navigation;
    private readonly NativeText _brandText;
    private readonly NativeBorder _toolbar;
    private readonly NativeButton _fullscreenButton;
    private readonly NativeButton _minimizeButton;
    private readonly NativeButton _maximizeButton;
    private readonly NativeButton _closeButton;
    private readonly StackPanel _captionContent;
    private readonly Dictionary<int, NativeButton> _tabs = [];
    private readonly TranslateTransform _translation = new();
    private readonly UISettings _uiSettings = new();
    private readonly NativeKeyboardAccelerator _f11;
    private readonly NativeKeyboardAccelerator _escape;
    private XamlRoot? _xamlRoot;
    private bool _captionVisible = true, _disposed, _updatingRegions;
    private long _started;
    private double _from;
    private RectInt32 _restoreBounds;
    private bool _restoreMaximized;
    private int _activeTab;

    /// <summary>Высота зоны заголовка, необходимая для отступа контента.</summary>
    public int ContentInset => _options.Height;
    /// <summary>Признак полноэкранного режима.</summary>
    public bool IsFullscreen => _app.Presenter.Kind == AppWindowPresenterKind.FullScreen;

    /// <summary>Вызывать в OnPlatformWindowSubclassed до создания MAUI окна.</summary>
    public static void Prepare(NativeWindow native)
    {
        ArgumentNullException.ThrowIfNull(native);
        if (!AppWindowTitleBar.IsCustomizationSupported()) return;
        native.ExtendsContentIntoTitleBar = true;
        // Collapse system caption buttons; the toolkit owns all four buttons.
        native.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
    }

    /// <summary>Вызвать после появления Handler у MAUI Window. На неподдерживаемых окнах возвращает null.</summary>
    public static UiWindowsChrome? Attach(MauiWindow window, UiChromeOptions options)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(options);
        if (!AppWindowTitleBar.IsCustomizationSupported() ||
            window.Handler?.PlatformView is not NativeWindow native ||
            native.Content is not NativePanel root ||
            native.AppWindow.Presenter is not OverlappedPresenter presenter)
            return null;
        if (options.Height < 38 || options.AnimationMilliseconds < 0 ||
            !double.IsFinite(options.LogoWidth) || !double.IsFinite(options.LogoHeight) ||
            options.LogoWidth <= 0 || options.LogoHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(options));
        return new UiWindowsChrome(native, root, presenter, options);
    }

    private UiWindowsChrome(NativeWindow native, NativePanel root, OverlappedPresenter presenter, UiChromeOptions options)
    {
        _native = native;
        _root = root;
        _presenter = presenter;
        _options = options;
        _app = native.AppWindow;
        _pointer = InputNonClientPointerSource.GetForWindowId(_app.Id);
        _activeTab = Math.Clamp(options.InitialSelectedTab, 0, Math.Max(0, options.Tabs.Count - 1));
        _drag = new NativeGrid { Background = new NativeBrush(Microsoft.UI.Colors.Transparent) };
        var brand = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8,
            VerticalAlignment = NativeVerticalAlignment.Center };
        Uri? logo = options.LogoUri;
        if (logo is null && options.IconFilePath is { } path && File.Exists(path))
            logo = new Uri(Path.GetFullPath(path));
        if (logo is not null)
        {
            brand.Children.Add(new NativeImage { Source = new BitmapImage(logo),
                Width = options.LogoWidth, Height = options.LogoHeight, Stretch = NativeStretch.Uniform });
        }
        _brandText = new NativeText
        {
            Text = options.Title, FontSize = 16,
            Foreground = Brush(options.Text),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            VerticalAlignment = NativeVerticalAlignment.Center
        };
        brand.Children.Add(_brandText);
        _navigation = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4,
            VerticalAlignment = NativeVerticalAlignment.Center };
        for (int index = 0; index < options.Tabs.Count; index++)
        {
            int id = index;
            var button = CreateButton("", options.Tabs[index].Title, () => SelectTab(id), 84);
            button.Content = options.Tabs[index].Title;
            button.Padding = new NativeThickness(10, 0, 10, 0);
            _tabs.Add(id, button);
            _navigation.Children.Add(button);
        }
        _captionContent = new StackPanel
        {
            Orientation = Orientation.Horizontal, Spacing = 18,
            Margin = new NativeThickness(14, 0, 0, 0),
            VerticalAlignment = NativeVerticalAlignment.Center
        };
        _captionContent.Children.Add(brand);
        _captionContent.Children.Add(_navigation);
        _drag.Children.Add(_captionContent);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 3 };
        _minimizeButton = CreateButton("\uE921", "Свернуть", () =>
        {
            if (IsFullscreen) ExitFullscreen();
            _presenter.Minimize();
        });
        _maximizeButton = CreateButton("\uE922", "Развернуть", () =>
        {
            if (IsFullscreen)
            {
                ExitFullscreen();
                _presenter.Maximize();
            }
            else if (_presenter.State == OverlappedPresenterState.Maximized) _presenter.Restore();
            else _presenter.Maximize();
            UpdateLayout();
        });
        _fullscreenButton = CreateButton("\uE92D", "Полноэкранный режим (F11)", ToggleFullscreen);
        _closeButton = CreateButton("\uE8BB", "Закрыть", native.Close);
        buttons.Children.Add(_minimizeButton);
        buttons.Children.Add(_maximizeButton);
        buttons.Children.Add(_fullscreenButton);
        buttons.Children.Add(_closeButton);
        _closeButton.Resources["ButtonBackgroundPointerOver"] = new NativeBrush(NativeColor.FromArgb(220, 180, 36, 53));
        _closeButton.Resources["ButtonForegroundPointerOver"] = new NativeBrush(Microsoft.UI.Colors.White);
        _toolbar = new NativeBorder
        {
            Child = buttons, Padding = new NativeThickness(3),
            CornerRadius = new NativeCornerRadius(8),
            Background = Brush(options.Surface, options.ToolbarOpacity),
            HorizontalAlignment = NativeHorizontalAlignment.Right,
            VerticalAlignment = NativeVerticalAlignment.Center
        };
        _caption = new NativeGrid
        {
            Height = options.Height,
            VerticalAlignment = NativeVerticalAlignment.Top,
            Background = new NativeBrush(Microsoft.UI.Colors.Transparent),
            RenderTransform = _translation
        };
        _caption.Children.Add(_drag);
        _caption.Children.Add(_toolbar);
        NativeCanvas.SetZIndex(_caption, 1000);
        root.Children.Add(_caption);

        _drag.Loaded += OnDragLoaded;
        _drag.SizeChanged += OnLayoutChanged;
        _captionContent.SizeChanged += OnLayoutChanged;
        _toolbar.SizeChanged += OnLayoutChanged;
        _root.Background ??= new NativeBrush(Microsoft.UI.Colors.Transparent);
        _root.PointerEntered += OnPointer;
        _native.VisibilityChanged += OnVisibilityChanged;
        _root.AddHandler(UIElement.PointerMovedEvent, (PointerEventHandler)OnPointer, handledEventsToo: true);
        _root.PointerExited += OnPointer;
        _native.Activated += OnActivated;
        _native.Closed += OnClosed;
        _app.Changed += OnAppChanged;

        _f11 = new NativeKeyboardAccelerator { Key = VirtualKey.F11 };
        _f11.Invoked += OnFullscreenHotkey;
        _escape = new NativeKeyboardAccelerator { Key = VirtualKey.Escape };
        _escape.Invoked += OnEscapeHotkey;
        _root.KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
        _root.KeyboardAccelerators.Add(_f11);
        _root.KeyboardAccelerators.Add(_escape);
        Prepare(native);
        UpdateLayout();
        ApplyColors();
        SetActiveTab(_activeTab);
    }

    /// <summary>Выбирает вкладку и вызывает пользовательский обработчик.</summary>
    public void SelectTab(int index)
    {
        if (index < 0 || index >= _options.Tabs.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        SetActiveTab(index);
        _options.Tabs[index].Selected();
    }

    /// <summary>Синхронизировать выделение при навигации, инициированной из самого приложения.</summary>
    public void SetActiveTab(int index)
    {
        if (index < 0 || index >= _options.Tabs.Count)
        {
            if (_options.Tabs.Count != 0) throw new ArgumentOutOfRangeException(nameof(index));
            return;
        }
        _activeTab = index;
        foreach (var (key, button) in _tabs)
        {
            bool selected = key == index;
            button.Background = Brush(selected ? _options.Surface : _options.Background,
                selected ? (byte)230 : (byte)0);
            button.Foreground = Brush(selected ? _options.Accent : _options.Text);
        }
    }

    /// <summary>Вызывайте после ThemeChanged, чтобы нативный заголовок следовал MAUI-палитре.</summary>
    public void ApplyTheme(UiTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        _options.Background = theme[UiColorRole.Background];
        _options.Surface = theme[UiColorRole.Card];
        _options.Text = theme[UiColorRole.Text];
        _options.Accent = theme[UiColorRole.Accent];
        _options.Border = theme[UiColorRole.Border];
        ApplyColors();
        SetActiveTab(_activeTab);
        UpdateLayout();
    }

    private void ApplyColors()
    {
        _brandText.Foreground = Brush(_options.Text);
        _toolbar.Background = Brush(_options.Surface, _options.ToolbarOpacity);
        foreach (var button in new[] { _minimizeButton, _maximizeButton, _fullscreenButton, _closeButton })
            button.Foreground = Brush(_options.Text);
    }

    private void OnFullscreenHotkey(NativeKeyboardAccelerator _, NativeKeyboardAcceleratorInvokedEventArgs e)
    {
        ToggleFullscreen();
        e.Handled = true;
    }

    private void OnEscapeHotkey(NativeKeyboardAccelerator _, NativeKeyboardAcceleratorInvokedEventArgs e)
    {
        if (!IsFullscreen) return;
        ExitFullscreen();
        e.Handled = true;
    }

    /// <summary>Переключает полноэкранное состояние.</summary>
    public void ToggleFullscreen()
    {
        if (IsFullscreen) { ExitFullscreen(); return; }
        _restoreMaximized = _presenter.State == OverlappedPresenterState.Maximized;
        _restoreBounds = new RectInt32(_app.Position.X, _app.Position.Y, _app.Size.Width, _app.Size.Height);
        _native.SetTitleBar(null);
        _app.SetPresenter(AppWindowPresenterKind.FullScreen);
        SetCaptionVisible(false, animate: true);
        UpdateLayout();
    }

    /// <summary>Выходит из полноэкранного режима с восстановлением размеров.</summary>
    public void ExitFullscreen()
    {
        if (!IsFullscreen) return;
        _app.SetPresenter(_presenter);
        if (_restoreMaximized) _presenter.Maximize();
        else { _presenter.Restore(); _app.MoveAndResize(_restoreBounds); }
        Prepare(_native);
        if (_drag.IsLoaded) _native.SetTitleBar(_drag);
        SetCaptionVisible(true, animate: false);
        UpdateLayout();
    }

    private void OnDragLoaded(object _, RoutedEventArgs e)
    {
        if (_xamlRoot != _drag.XamlRoot)
        {
            if (_xamlRoot is not null) _xamlRoot.Changed -= OnRootChanged;
            _xamlRoot = _drag.XamlRoot;
            if (_xamlRoot is not null) _xamlRoot.Changed += OnRootChanged;
        }
        if (!IsFullscreen) _native.SetTitleBar(_drag);
        UpdateLayout();
    }

    private void OnPointer(object sender, PointerRoutedEventArgs args)
    {
        if (!IsFullscreen || args.Pointer.PointerDeviceType != PointerDeviceType.Mouse) return;
        var pointer = args.GetCurrentPoint(_root);
        var pos = pointer.Position;
        bool within = pos.X >= 0 && pos.X < _root.ActualWidth && pos.Y >= 0 && pos.Y < _root.ActualHeight;
        if (_captionVisible && (!within || pos.Y > _options.Height + 8)) SetCaptionVisible(false, true);
        else if (!_captionVisible && within && pos.Y <= 8 && !pointer.Properties.IsLeftButtonPressed)
            SetCaptionVisible(true, true);
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        if (IsFullscreen && args.WindowActivationState == WindowActivationState.Deactivated)
            SetCaptionVisible(false, false);
    }

    private void OnVisibilityChanged(object sender, WindowVisibilityChangedEventArgs args)
    {
        if (!args.Visible && IsFullscreen) SetCaptionVisible(false, false);
    }

    private void SetCaptionVisible(bool visible, bool animate)
    {
        if (_captionVisible == visible && animate) return;
        _captionVisible = visible;
        CompositionTarget.Rendering -= OnFrame;
        if (visible) _caption.Visibility = NativeVisibility.Visible;
        double target = visible ? 0 : -_options.Height;
        if (!animate || !_uiSettings.AnimationsEnabled || _options.AnimationMilliseconds == 0)
        {
            Finish(target);
            return;
        }
        _from = _translation.Y;
        _started = Stopwatch.GetTimestamp();
        CompositionTarget.Rendering += OnFrame;
    }

    private void OnFrame(object? sender, object args)
    {
        double target = _captionVisible ? 0 : -_options.Height;
        double progress = Stopwatch.GetElapsedTime(_started).TotalMilliseconds / _options.AnimationMilliseconds;
        if (progress >= 1) { Finish(target); return; }
        _translation.Y = target + (_from - target) * Math.Pow(1 - progress, 3);
    }

    private void Finish(double y)
    {
        CompositionTarget.Rendering -= OnFrame;
        _translation.Y = y;
        _caption.Visibility = _captionVisible ? NativeVisibility.Visible : NativeVisibility.Collapsed;
    }

    private void OnAppChanged(AppWindow sender, AppWindowChangedEventArgs e)
    {
        if (e.DidPresenterChange || e.DidSizeChange) UpdateLayout();
    }
    private void OnRootChanged(XamlRoot sender, XamlRootChangedEventArgs e) => UpdateLayout();
    private void OnLayoutChanged(object sender, SizeChangedEventArgs args) => QueueRegions();

    private void UpdateLayout()
    {
        bool full = IsFullscreen;
        bool maximized = !full && _presenter.State == OverlappedPresenterState.Maximized;
        // All four controls stay visible in windowed and fullscreen modes.
        _toolbar.Margin = new NativeThickness(0, 0, 8, 0);
        _drag.Margin = new NativeThickness(0, 0, ToolbarWidth, 0);
        _caption.Height = _options.Height;
        _caption.Background = Brush(_options.Background, _options.CaptionOpacity);
        UpdateButton(_maximizeButton, maximized ? "\uE923" : "\uE922",
            maximized ? "Восстановить размер окна" : "Развернуть");
        UpdateButton(_fullscreenButton, full ? "\uE92C" : "\uE92D",
            full ? "Выйти из полноэкранного режима (F11 / Esc)" : "На весь экран (F11)");
        QueueRegions();
    }

    private void QueueRegions()
    {
        if (_updatingRegions || _disposed) return;
        _updatingRegions = _caption.DispatcherQueue.TryEnqueue(() =>
        {
            _updatingRegions = false;
            if (_disposed || !_drag.IsLoaded || _xamlRoot is null) return;
            if (IsFullscreen)
            {
                _pointer.ClearRegionRects(NonClientRegionKind.Passthrough);
                return;
            }
            var rects = new List<RectInt32>();
            AppendRegion(_navigation, rects, _xamlRoot.RasterizationScale);
            AppendRegion(_toolbar, rects, _xamlRoot.RasterizationScale);
            _pointer.SetRegionRects(NonClientRegionKind.Passthrough, rects.ToArray());
        });
    }

    private void AppendRegion(FrameworkElement element, List<RectInt32> rects, double scale)
    {
        if (element.Visibility != NativeVisibility.Visible || element.ActualWidth <= 0 || element.ActualHeight <= 0)
            return;
        var bounds = element.TransformToVisual(_root).TransformBounds(
            new global::Windows.Foundation.Rect(0, 0, element.ActualWidth, element.ActualHeight));
        int x = (int)Math.Floor(bounds.X * scale), y = (int)Math.Floor(bounds.Y * scale);
        rects.Add(new RectInt32(x, y,
            (int)Math.Ceiling(bounds.Right * scale) - x,
            (int)Math.Ceiling(bounds.Bottom * scale) - y));
    }

    private static NativeButton CreateButton(string glyph, string tooltip, Action click, double width = 36)
    {
        var button = new NativeButton
        {
            Width = width, Height = 32, Padding = new NativeThickness(0),
            HorizontalContentAlignment = NativeHorizontalAlignment.Center,
            VerticalContentAlignment = NativeVerticalAlignment.Center,
            BorderThickness = new NativeThickness(0),
            Content = new NativeFontIcon { Glyph = glyph,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), FontSize = 13 },
            Background = new NativeBrush(Microsoft.UI.Colors.Transparent)
        };
        button.Click += (_, _) => click();
        UpdateButton(button, glyph, tooltip);
        return button;
    }

    private static void UpdateButton(NativeButton button, string glyph, string tooltip)
    {
        if (button.Content is NativeFontIcon icon) icon.Glyph = glyph;
        NativeAutomation.SetName(button, tooltip);
        ToolTipService.SetToolTip(button, tooltip);
    }

    private static NativeBrush Brush(Color color, byte alpha = 255)
    {
        color.ToRgb(out byte r, out byte g, out byte b);
        return new NativeBrush(NativeColor.FromArgb(alpha, r, g, b));
    }

    private void OnClosed(object sender, WindowEventArgs e) => Dispose();

    /// <summary>Освобождает подписки на системные события.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        CompositionTarget.Rendering -= OnFrame;
        _root.KeyboardAccelerators.Remove(_f11);
        _root.KeyboardAccelerators.Remove(_escape);
        _f11.Invoked -= OnFullscreenHotkey;
        _escape.Invoked -= OnEscapeHotkey;
        _root.RemoveHandler(UIElement.PointerMovedEvent, (PointerEventHandler)OnPointer);
        _root.PointerExited -= OnPointer;
        _root.PointerEntered -= OnPointer;
        _native.VisibilityChanged -= OnVisibilityChanged;
        _native.Activated -= OnActivated;
        _native.Closed -= OnClosed;
        _app.Changed -= OnAppChanged;
        if (_xamlRoot is not null) _xamlRoot.Changed -= OnRootChanged;
        _drag.Loaded -= OnDragLoaded;
        _drag.SizeChanged -= OnLayoutChanged;
        _captionContent.SizeChanged -= OnLayoutChanged;
        _toolbar.SizeChanged -= OnLayoutChanged;
        _root.Children.Remove(_caption);
        _pointer.ClearRegionRects(NonClientRegionKind.Passthrough);
    }
}
