using System.Drawing;
using System.Drawing.Imaging;
using TagsCloud;

namespace TagsCloudVisualization;

public static class Program
{
    private const string StandardInputPath = "in.txt";
    private const string StandardOutputPath = "out.png";

    public static void Main()
    {
        try
        {
            Run(StandardInputPath, StandardOutputPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static void Run(string inputPath, string outputPath)
    {
        var layouter = new SpiralCircularCloudLayouter(Point.Empty);
        var visualizer = new RectanglesVisualizer();
        var rectangles = PutRectanglesFromFile(layouter, inputPath);

        visualizer.AddRectangles(rectangles);
        visualizer.GetImage().Save(outputPath, ImageFormat.Png);
    }

    private static IEnumerable<Rectangle> PutRectanglesFromFile(ICircularCloudLayouter layouter, string path)
    {
        var sizeParser = new SizeParser();
        var lines = ReadLinesFromFile(path);

        foreach (var line in lines)
        {
            if (sizeParser.TryParse(line, out var size))
            {
                yield return layouter.PutNextRectangle(size);
            }
            else
            {
                Console.WriteLine($"Unable to parse line '{line}'");
            }
        }
    }

    private static IEnumerable<string> ReadLinesFromFile(string path)
    {
        using var reader = new StreamReader(path);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();

            if (!string.IsNullOrWhiteSpace(line))
            {
                yield return line;
            }
        }
    }
}
