using Laveyana.Maui.UI.Layout;
using NUnit.Framework;

namespace Laveyana.Maui.UI.Tests;

[TestFixture]
public sealed class GeometryTests
{
    [TestCase(0, 3, 280, 12, 1)]
    [TestCase(280, 3, 280, 12, 1)]
    [TestCase(600, 3, 280, 12, 2)]
    [TestCase(1200, 3, 280, 12, 3)]
    public void Grid_respects_minimum_width(double width, int max, double minimum, double spacing, int expected)
        => Assert.That(UiAdaptiveGrid.CalculateColumns(width, max, minimum, spacing), Is.EqualTo(expected));

    [Test]
    public void Virtual_list_extent_has_no_trailing_spacing()
    {
        Assert.Multiple(() =>
        {
            Assert.That(UiVirtualListMath.Extent(0, 54, 5), Is.EqualTo(0));
            Assert.That(UiVirtualListMath.Extent(1, 54, 5), Is.EqualTo(54));
            Assert.That(UiVirtualListMath.Extent(3, 54, 5), Is.EqualTo(172));
        });
    }

    [Test]
    public void Virtual_list_renders_only_visible_neighbours()
    {
        var range = UiVirtualListMath.Range(10000, 5900, 540, 54, 5, 2);
        Assert.Multiple(() =>
        {
            Assert.That(range.First, Is.EqualTo(98));
            Assert.That(range.End, Is.EqualTo(112));
            Assert.That(range.End - range.First, Is.LessThan(20));
        });
    }

    [Test]
    public void Grid_rejects_invalid_minimum_and_spacing()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => UiAdaptiveGrid.CalculateColumns(800, 0, 200));
            Assert.Throws<ArgumentOutOfRangeException>(() => UiAdaptiveGrid.CalculateColumns(800, 3, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => UiAdaptiveGrid.CalculateColumns(800, 3, 200, -1));
        });
    }

    [Test]
    public void List_range_is_bounded_near_end()
    {
        var (first, end) = UiVirtualListMath.Range(3, 10_000, 300, 54, 5, 2);
        Assert.That((first, end), Is.EqualTo((3, 3)));
    }

    [Test]
    public void Empty_and_invalid_ranges_are_safe()
    {
        Assert.That(UiVirtualListMath.Range(0, 10, 100, 54, 5), Is.EqualTo((0, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => UiVirtualListMath.Range(10, 0, 100, 0, 5));
    }
}
