using Laveyana.Maui.UI.Display;
using Laveyana.Maui.UI.Themes;
using NUnit.Framework;

namespace Laveyana.Maui.UI.Tests;

[TestFixture]
public sealed class VisibilityAndPaletteTests
{
    [Test]
    public void Render_availability_changes_only_when_effective_state_changes()
    {
        var state = new UiVisibilityState();
        var changes = new List<bool>();
        state.RenderAvailabilityChanged += changes.Add;
        state.Update(active: true);
        state.Update(visible: true);
        state.Update(screenInteractive: false);
        state.Update(active: false);
        state.Update(screenInteractive: true);
        state.Update(active: true);
        state.Update(active: true);
        Assert.That(changes, Is.EqualTo(new[] { true, false, true }));
    }

    [Test]
    public void Default_palette_preserves_numeric_preferences_and_rgb()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new UiThemeSettings().Preset, Is.EqualTo(UiPalettePreset.Laveyana));
            Assert.That((int)UiPalettePreset.Laveyana, Is.Zero);
            Assert.That((int)UiPalettePreset.Mint, Is.EqualTo(3));
            Assert.That((int)UiPalettePreset.Custom, Is.EqualTo(4));
            Assert.That(UiPaletteCatalog.Get(UiPalettePreset.Laveyana).Accent,
                Is.EqualTo(Microsoft.Maui.Graphics.Color.FromArgb("#58E5FC")));
        });
    }
}
