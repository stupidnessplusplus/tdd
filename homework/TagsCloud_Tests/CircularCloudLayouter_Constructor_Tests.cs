using System.Drawing;
using FluentAssertions;
using TagsCloud;

namespace TagsCloud_Tests;

[TestFixture]
public class CircularCloudLayouter_Constructor_Tests
{
    [Test]
    [Description("В конструкторе указывается центр первого прямоугольника")]
    public void Constructor_SetsCenterOfFirstRectangle()
    {
        var center = new Point(7, 10);
        var size = new Size(5, 5);
        var expectedLocation = center - size / 2;

        var layouter = new SpiralCircularCloudLayouter(center);
        var rectangle = layouter.PutNextRectangle(size);

        rectangle.Location.Should().Be(expectedLocation);
    }
}
