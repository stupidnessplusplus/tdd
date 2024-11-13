using System.Drawing;

namespace TagsCloudVisualization;

public class RectanglesVisualizer
{
    private readonly Random _random;
    private readonly Point _imageCenter;

    public RectanglesVisualizer(
        int width,
        int height)
    {
        _random = new Random();
        Image = new Bitmap(width, height);
        _imageCenter = new Point(width / 2, height / 2);
    }

    public Bitmap Image { get; }

    public void AddRectangles(
        IEnumerable<Rectangle> rectangles)
    {
        foreach (var rectangle in rectangles)
        {
            AddRectangle(rectangle);
        }
    }

    public void AddRectangle(
        Rectangle rectangle)
    {
        var left = _imageCenter.X + rectangle.Left;
        var right = _imageCenter.X + rectangle.Right;
        var top = _imageCenter.Y + rectangle.Top;
        var bottom = _imageCenter.Y + rectangle.Bottom;

        var borderColor = Color.FromArgb(_random.Next(64, 256), _random.Next(64, 256), _random.Next(64, 256));
        var innerColor = Color.FromArgb(63, borderColor.R, borderColor.G, borderColor.B);

        for (var x = Math.Max(0, left); x < right && x < Image.Width; x++)
        {
            for (var y = Math.Max(0, top); y < bottom && y < Image.Height; y++)
            {
                var color = x == left || x == right - 1 || y == top || y == bottom - 1
                    ? borderColor
                    : innerColor;

                Image.SetPixel(x, y, color);
            }
        }
    }
}
