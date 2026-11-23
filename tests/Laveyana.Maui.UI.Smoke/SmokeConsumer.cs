using Laveyana.Maui.UI.Controls;
using Laveyana.Maui.UI.Fluent;
using Laveyana.Maui.UI.Layout;
using Laveyana.Maui.UI.Themes;
using Microsoft.Maui.Controls;

namespace Laveyana.Maui.UI.Smoke;

/// <summary>Гарантия возможности ссылаться на portable toolkit из платформенного MAUI-проекта.</summary>
public static class SmokeConsumer
{
    public static View Create()
    {
        var grid = Ui.Cards(3);
        grid.SetCards(new View[]
        {
            Ui.Card(Ui.Text("LavPulse", 16, UiColorRole.Accent).WithMargin(4).At(0, 0)),
            Ui.Card(Ui.Number("Порт", 1, 65535, 9000, 1, showSlider: false))
        });
        var entry = Ui.Entry("Название").WithMargin(6);
        return Ui.Stack(8, grid, entry);
    }
}
