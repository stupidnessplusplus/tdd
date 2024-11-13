using System.Diagnostics;
using System.Drawing;

namespace TagsCloud;

/// <summary>
/// Вспомогательная структура для хранения прямоугольников, отсортированных по координатам сторон,
/// с возможностью получения прямоугольника по индексу в отсортированном списке.
/// </summary>
public class SortedRectanglesList
{
    private static readonly Dictionary<Direction, Comparer<Rectangle>> _comparers;

    private readonly Dictionary<Direction, List<Rectangle>> _sortedRectangles;

    static SortedRectanglesList()
    {
        var leftComparer = Comparer<Rectangle>.Create((x1, x2) => x1.Left.CompareTo(x2.Left));
        var rightComparer = Comparer<Rectangle>.Create((x1, x2) => -x1.Right.CompareTo(x2.Right));
        var topComparer = Comparer<Rectangle>.Create((x1, x2) => x1.Top.CompareTo(x2.Top));
        var bottomComparer = Comparer<Rectangle>.Create((x1, x2) => -x1.Bottom.CompareTo(x2.Bottom));

        _comparers = new Dictionary<Direction, Comparer<Rectangle>>(4)
        {
            { Direction.Left, rightComparer },
            { Direction.Right, leftComparer },
            { Direction.Up, bottomComparer },
            { Direction.Down, topComparer },
        };
    }

    public SortedRectanglesList()
    {
        _sortedRectangles = new Dictionary<Direction, List<Rectangle>>(4)
        {
            { Direction.Left, [] },
            { Direction.Right, [] },
            { Direction.Up, [] },
            { Direction.Down, [] },
        };
    }

    public int Count { get; private set; }

    public void Add(
        Rectangle rectangle)
    {
        Insert(_sortedRectangles[Direction.Left], rectangle, _comparers[Direction.Left]);
        Insert(_sortedRectangles[Direction.Right], rectangle, _comparers[Direction.Right]);
        Insert(_sortedRectangles[Direction.Up], rectangle, _comparers[Direction.Up]);
        Insert(_sortedRectangles[Direction.Down], rectangle, _comparers[Direction.Down]);

        Count++;
    }

    public Rectangle Get(
        Direction sortingDirection,
        int index)
    {
        if (!_sortedRectangles.TryGetValue(sortingDirection, out var rectangles))
        {
            throw new ArgumentException($"Unsupported sorting direction: {sortingDirection}.");
        }

        if (index < 0 || index >= Count)
        {
            throw new IndexOutOfRangeException($"Index was out of range: {index}.");
        }

        return rectangles[index];
    }

    public bool HasIntersection(
        Rectangle rectangle,
        Direction sortingDirection,
        int startIndex,
        out int intersectedRectangleIndex)
    {
        if (!_sortedRectangles.TryGetValue(sortingDirection, out var rectangles))
        {
            throw new ArgumentException($"Unsupported sorting direction: {sortingDirection}.");
        }

        if (startIndex < 0)
        {
            throw new IndexOutOfRangeException($"Index was out of range: {startIndex}.");
        }

        for (var i = startIndex; i < rectangles.Count; i++)
        {
            if (rectangle.IntersectsWith(rectangles[i]))
            {
                intersectedRectangleIndex = i;
                return true;
            }
        }

        intersectedRectangleIndex = -1;
        return false;
    }

    private static void Insert(
        List<Rectangle> rectangles,
        Rectangle rectangle,
        Comparer<Rectangle>? comparer)
    {
        Debug.Assert(rectangles != null);

        var index = rectangles.BinarySearch(rectangle, comparer);

        if (index < 0)
        {
            index = -index - 1;
        }

        rectangles.Insert(index, rectangle);
    }
}
