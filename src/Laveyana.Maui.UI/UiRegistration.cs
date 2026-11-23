using Laveyana.Maui.UI.Themes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Laveyana.Maui.UI;

/// <summary>Настройка необязательного UI-сервиса через привычный MAUI builder.</summary>
public static class UiRegistration
{
    /// <summary>Регистрирует менеджер темы и необязательную начальную конфигурацию.</summary>
    public static MauiAppBuilder UseLaveyanaMauiUi(this MauiAppBuilder builder, UiThemeSettings? initialTheme = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddSingleton(initialTheme ?? new UiThemeSettings());
        builder.Services.AddSingleton<UiThemeManager>();
        return builder;
    }
}
