using Laveyana.Maui.UI.Controls;
using Laveyana.Maui.UI.Themes;
using Laveyana.Maui.UI.Animation;
using NUnit.Framework;

namespace Laveyana.Maui.UI.Tests;

[TestFixture]
public sealed class ThemeAndNumberTests
{
    [Test]
    public void Amoled_background_is_black_and_dark()
    {
        var theme = UiTheme.Create(new UiThemeSettings { DarkBackground = UiDarkBackground.Amoled });
        Assert.Multiple(() =>
        {
            Assert.That(theme.IsAmoled, Is.True);
            Assert.That(theme.IsLight, Is.False);
            Assert.That(theme[UiColorRole.Background].Red, Is.EqualTo(0));
            Assert.That(theme[UiColorRole.Background].Green, Is.EqualTo(0));
            Assert.That(theme[UiColorRole.Background].Blue, Is.EqualTo(0));
        });
    }

    [Test]
    public void System_theme_uses_platform_request()
    {
        var settings = new UiThemeSettings { Mode = UiThemeMode.System };
        Assert.That(UiTheme.Create(settings, Microsoft.Maui.ApplicationModel.AppTheme.Light).IsLight, Is.True);
        Assert.That(UiTheme.Create(settings, Microsoft.Maui.ApplicationModel.AppTheme.Dark).IsLight, Is.False);
    }

    [Test]
    public void Text_has_readable_contrast()
    {
        foreach (UiPalettePreset preset in Enum.GetValues<UiPalettePreset>())
        {
            foreach (UiThemeMode mode in new[] { UiThemeMode.Dark, UiThemeMode.Light })
            {
                var theme = UiTheme.Create(new UiThemeSettings { Preset = preset, Mode = mode });
                Assert.That(UiTheme.Contrast(theme[UiColorRole.Text], theme[UiColorRole.Background]),
                    Is.GreaterThanOrEqualTo(7), $"{preset}: {mode}");
            }
        }
    }

    [Test]
    public void Numeric_values_are_clamped_and_rounded()
    {
        var range = new UiNumberRange(0, 100, 50, .01, 2);
        Assert.Multiple(() =>
        {
            Assert.That(range.Normalize(150), Is.EqualTo(100));
            Assert.That(range.Normalize(-1), Is.EqualTo(0));
            Assert.That(range.Normalize(12.345), Is.EqualTo(12.35));
            Assert.That(range.Normalize(double.NaN), Is.EqualTo(50));
        });
    }

    [Test]
    public void Range_rejects_invalid_configuration()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => new UiNumberRange(10, 10, 10).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new UiNumberRange(0, 1, .5, 0).Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new UiNumberRange(0, 1, .5, 1, 9).Validate());
        });
    }

    [Test]
    public void Nonfinite_numbers_fall_back_and_are_clamped()
    {
        var range = new UiNumberRange(-5, 5, 15, 1, 1);
        Assert.Multiple(() =>
        {
            Assert.That(range.Normalize(double.NaN), Is.EqualTo(5));
            Assert.That(range.Normalize(double.PositiveInfinity), Is.EqualTo(5));
            Assert.That(range.Normalize(-4.25), Is.EqualTo(-4.3));
        });
    }

    [Test]
    public void Custom_palette_requires_explicit_values()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => UiPaletteCatalog.Get(UiPalettePreset.Custom));
        var custom = UiPaletteCatalog.Get(UiPalettePreset.Ocean);
        var settings = new UiThemeSettings { Preset = UiPalettePreset.Custom, CustomPalette = custom };
        Assert.That(settings.ResolvePalette(), Is.SameAs(custom));
    }

    [Test]
    public void Contrast_respects_minimum_when_possible()
    {
        var background = Microsoft.Maui.Graphics.Colors.White;
        var initial = Microsoft.Maui.Graphics.Color.FromArgb("#BBBBBB");
        var updated = UiTheme.EnsureContrast(initial, background, 7, Microsoft.Maui.Graphics.Colors.Black);
        Assert.That(UiTheme.Contrast(updated, background), Is.GreaterThanOrEqualTo(7));
    }

    [Test]
    public void Frame_policy_is_bounded()
    {
        Assert.Multiple(() =>
        {
            Assert.That(UiFramePolicy.FrameIntervalMilliseconds(UiFrameRate.Fps60), Is.EqualTo(17u));
            Assert.That(UiFramePolicy.FrameIntervalMilliseconds(UiFrameRate.Fps120), Is.EqualTo(8u));
            Assert.That(UiFramePolicy.EaseOutCubic(1), Is.EqualTo(1).Within(1e-10));
        });
    }
}
