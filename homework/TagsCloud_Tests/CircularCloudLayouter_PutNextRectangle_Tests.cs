using FluentAssertions;
using System.Drawing;
using TagsCloud;

namespace TagsCloud_Tests;

[TestFixture]
public class CircularCloudLayouter_PutNextRectangle_Tests
{
    private ICircularCloudLayouter _layouter;

    [SetUp]
    public void Setup()
    {
        _layouter = new SpiralCircularCloudLayouter(Point.Empty);
    }

    [TestCase(0, 1)]
    [TestCase(-1, 1)]
    [TestCase(1, 0)]
    [TestCase(1, -1)]
    [Description("Бросает исключение, если длина или ширина меньше или равны 0")]
    public void PutNextRectangle_ThrowsException_WhenSizeIsNotPositive(
        int width,
        int height)
    {
        var size = new Size(width, height);

        var putRectangle = () => _layouter.PutNextRectangle(size);

        putRectangle.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestCase(1, 1, 0, 0)]
    [TestCase(4, 6, -2, -3)]
    [TestCase(5, 7, -2, -3)]
    [Description("Центр первого прямоугольника должен совпадать с центром облака")]
    public void PutNextRectangle_SetsFirstRectangleInCenter(
        int width,
        int height,
        int expectedX,
        int expectedY)
    {
        var size = new Size(width, height);
        var expectedLocation = new Point(expectedX, expectedY);

        var rectangle = _layouter.PutNextRectangle(size);

        rectangle.Location.Should().Be(expectedLocation);
        rectangle.Size.Should().Be(size);
    }

    [Test]
    [Description("Прямоугольники не имеют пересечений")]
    public void PutNextRectangle_ReturnsNonIntersectingWithEachOtherRectangles()
    {
        var count = 1_000;
        var random = new Random();
        var rectangles = new Rectangle[count];

        for (var i = 0; i < count; i++)
        {
            var size = new Size(random.Next(2, 100), random.Next(2, 100));
            var putRectangle = _layouter.PutNextRectangle(size);
            rectangles[i] = putRectangle;
        }

        for (var i = 0; i < count; i++)
        {
            for (var j = i + 1; j < count; j++)
            {
                var haveIntersection = rectangles[i].IntersectsWith(rectangles[j]);

                haveIntersection.Should().BeFalse();
            }
        }
    }
}
