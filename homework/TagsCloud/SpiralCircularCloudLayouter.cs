using System.Diagnostics;
using System.Drawing;

namespace TagsCloud;

public class SpiralCircularCloudLayouter
    : ICircularCloudLayouter
{
    private readonly SortedRectanglesList _rectangles = new SortedRectanglesList();
    private readonly List<(Rectangle Rectangle, Direction DirectionToPrevious)> _rectanglesSpiralStack = [];
    private readonly Point _center;

    public SpiralCircularCloudLayouter(Point center)
    {
        _center = center;
    }

    public Rectangle PutNextRectangle(Size rectangleSize)
    {
        var rectangle = GetNextRectangle(rectangleSize, out var direction);

        _rectangles.Add(rectangle);
        _rectanglesSpiralStack.Add((rectangle, direction));

        return rectangle;
    }

    private Rectangle GetNextRectangle(Size rectangleSize, out Direction directionToPrevious)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rectangleSize.Width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rectangleSize.Height);

        if (_rectanglesSpiralStack.Count == 0)
        {
            directionToPrevious = Direction.None;
            return GetFirstRectangle(rectangleSize, _center);
        }

        if (_rectanglesSpiralStack.Count == 1)
        {
            return GetSecondRectangle(rectangleSize, _rectanglesSpiralStack[0].Rectangle, out directionToPrevious);
        }

        while (_rectanglesSpiralStack.Count >= 2)
        {
            if (TryGetRectangleAttachedToPreviousRectangle(rectangleSize, out var result, out directionToPrevious))
            {
                return result;
            }

            _rectanglesSpiralStack.RemoveAt(_rectanglesSpiralStack.Count - 1);
        }

        throw new Exception("Unable to find a suitable location for the rectangle.");
    }

    private bool TryGetRectangleAttachedToPreviousRectangle(
        Size rectangleSize,
        out Rectangle result,
        out Direction directionToPrevious)
    {
        var direction = _rectanglesSpiralStack[^1].DirectionToPrevious;
        var rectangle = AttachToSide(
            rectangleSize,
            _rectanglesSpiralStack[^2].Rectangle.Location,
            _rectanglesSpiralStack[^1].Rectangle,
            direction);

        // Цикл по сторонам предыдущего прямоугольника
        // 0: первую сторону в первый раз обходит только от изначальной позиции;
        // 1-3: 3 другие стороны полностью;
        // 4: первую сторону обходит от угла прямоугольника).
        for (var i = 0; i < 5; i++)
        {
            var target = GetTarget(rectangleSize, _rectanglesSpiralStack[^1].Rectangle, direction);
            var hasIntersection = _rectangles.HasIntersection(rectangle, direction, 0, out var j);
            var isTargetOvershot = false;

            while (hasIntersection && !isTargetOvershot)
            {
                rectangle = AttachToSide(rectangleSize, rectangle.Location, _rectangles.Get(direction, j), direction);
                hasIntersection = _rectangles.HasIntersection(rectangle, direction, j + 1, out j);
                isTargetOvershot = IsTargetOvershot(rectangle.Location, target, direction);
            }

            if (!hasIntersection && !isTargetOvershot)
            {
                result = rectangle;
                directionToPrevious = DirectionOperations.Revert(direction);
                return true;
            }

            rectangle.Location = target;
            direction = DirectionOperations.RotateCounterclockwise(direction);
        }

        result = default;
        directionToPrevious = Direction.None;
        return false;
    }

    /// <summary>
    /// Возвращает крайнюю точку, до которой идет движение вдоль стороны rectangleToMoveAround.
    /// </summary>
    private static Point GetTarget(
        Size movingRectangleSize,
        Rectangle rectangleToMoveAround,
        Direction movingDirection)
    {
        return movingDirection switch
        {
            Direction.Left => rectangleToMoveAround.Location - movingRectangleSize,
            Direction.Right => rectangleToMoveAround.Location + rectangleToMoveAround.Size,
            Direction.Up => new Point(rectangleToMoveAround.Right, rectangleToMoveAround.Y - movingRectangleSize.Height),
            Direction.Down => new Point(rectangleToMoveAround.X - movingRectangleSize.Width, rectangleToMoveAround.Bottom),
            _ => throw new ArgumentException($"Unsupported moving direction: {movingDirection}."),
        };
    }

    private static bool IsTargetOvershot(Point position, Point target, Direction movingDirection)
    {
        return movingDirection switch
        {
            Direction.Left => position.X < target.X,
            Direction.Right => position.X > target.X,
            Direction.Up => position.Y < target.Y,
            Direction.Down => position.Y > target.Y,
            _ => throw new ArgumentException($"Unsupported moving direction: {movingDirection}."),
        };
    }

    /// <summary>
    /// Возвращает прямоугольник, стоящий на границе с rectangleToAttachTo со стороны direction,
    /// равный прямоугольнику rectangleToMove, сдвинутому по одной из координат.
    /// </summary>
    private static Rectangle AttachToSide(
        Size rectangleSize,
        Point defaultPosition,
        Rectangle rectangleToAttachTo,
        Direction direction)
    {
        var x = direction switch
        {
            Direction.Left => rectangleToAttachTo.Left - rectangleSize.Width,
            Direction.Right => rectangleToAttachTo.Right,
            _ => defaultPosition.X,
        };

        var y = direction switch
        {
            Direction.Up => rectangleToAttachTo.Top - rectangleSize.Height,
            Direction.Down => rectangleToAttachTo.Bottom,
            _ => defaultPosition.Y,
        };

        var position = new Point(x, y);
        return new Rectangle(position, rectangleSize);
    }

    /// <summary>
    /// Возвращает прямоугольник с центром в точке center.
    /// </summary>
    private static Rectangle GetFirstRectangle(Size rectangleSize, Point center)
    {
        var position = center - rectangleSize / 2;
        return new Rectangle(position, rectangleSize);
    }

    /// <summary>
    /// Возвращает прямоугольник, центр которого наиболее близок к центру прямоугольника firstRectangle.
    /// </summary>
    private static Rectangle GetSecondRectangle(
        Size rectangleSize,
        Rectangle firstRectangle,
        out Direction directionToFirst)
    {
        var center = firstRectangle.Location + firstRectangle.Size / 2;
        var distanceToCenterIfPlacedLeft = rectangleSize.Width + firstRectangle.Width / 2;
        var distanceToCenterIfPlacedUp = rectangleSize.Height + firstRectangle.Height / 2;

        if (distanceToCenterIfPlacedLeft < distanceToCenterIfPlacedUp)
        {
            var xLeft = center.X - distanceToCenterIfPlacedLeft;
            var yLeft = center.Y - rectangleSize.Height / 2;
            directionToFirst = Direction.Right;
            return new Rectangle(new Point(xLeft, yLeft), rectangleSize);
        }

        var xUp = center.X - rectangleSize.Width / 2;
        var yUp = center.Y - distanceToCenterIfPlacedUp;
        directionToFirst = Direction.Down;
        return new Rectangle(new Point(xUp, yUp), rectangleSize);
    }
}
