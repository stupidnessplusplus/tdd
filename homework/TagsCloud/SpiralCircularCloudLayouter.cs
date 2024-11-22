using System.Diagnostics;
using System.Drawing;

namespace TagsCloud;

public class SpiralCircularCloudLayouter
    : ICircularCloudLayouter
{
    private readonly SortedRectanglesList _rectangles;
    private readonly List<Rectangle> _rectanglesSpiralStack;
    private readonly Point _center;

    public SpiralCircularCloudLayouter(
        Point center)
    {
        _rectangles = new SortedRectanglesList();
        _rectanglesSpiralStack = [];

        _center = center;
    }

    public Rectangle PutNextRectangle(
        Size rectangleSize)
    {
        var rectangle = GetNextRectangle(rectangleSize);

        _rectangles.Add(rectangle);
        _rectanglesSpiralStack.Add(rectangle);

        return rectangle;
    }

    private Rectangle GetNextRectangle(
        Size rectangleSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rectangleSize.Width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rectangleSize.Height);

        if (_rectanglesSpiralStack.Count == 0)
        {
            Debug.Assert(_rectangles.Count == 0);

            return GetFirstRectangle(rectangleSize, _center);
        }

        if (_rectanglesSpiralStack.Count == 1)
        {
            Debug.Assert(_rectangles.Count == 1
                && _rectangles.Get(Direction.Left, 0) == _rectanglesSpiralStack[0]);

            return GetSecondRectangle(rectangleSize, _rectanglesSpiralStack[0]);
        }

        while (_rectanglesSpiralStack.Count >= 2)
        {
            if (TryGetRectangleAttachedToPreviousRectangle(rectangleSize, out var result))
            {
                return result;
            }

            _rectanglesSpiralStack.RemoveAt(_rectanglesSpiralStack.Count - 1);
        }

        throw new Exception("Unable to find a suitable location for the rectangle.");
    }

    private bool TryGetRectangleAttachedToPreviousRectangle(
        Size rectangleSize,
        out Rectangle result)
    {
        var (rectangle, direction) = GetInitialRectangleAndRotationDirection(
            rectangleSize, _rectanglesSpiralStack[^1], _rectanglesSpiralStack[^2]);

        // Цикл по сторонам предыдущего прямоугольника
        // 0: первую сторону в первый раз обходит только от изначальной позиции;
        // 1-3: 3 другие стороны полностью;
        // 4: первую сторону обходит от угла прямоугольника).
        for (var i = 0; i < 5; i++)
        {
            var target = GetTarget(rectangleSize, _rectanglesSpiralStack[^1], direction);
            var hasIntersection = _rectangles.HasIntersection(rectangle, direction, 0, out var j);
            var isTargetOvershot = false;

            while (hasIntersection && !isTargetOvershot)
            {
                rectangle = AttachToSide(rectangle, _rectangles.Get(direction, j), direction);
                hasIntersection = _rectangles.HasIntersection(rectangle, direction, j + 1, out j);
                isTargetOvershot = IsTargetOvershot(rectangle.Location, target, direction);
            }

            if (!hasIntersection && !isTargetOvershot)
            {
                result = rectangle;
                return true;
            }

            rectangle.Location = target;
            direction = RotateCounterclockwise(direction);
        }

        result = default;
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

    private static bool IsTargetOvershot(
        Point position,
        Point target,
        Direction movingDirection)
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
        Rectangle rectangleToMove,
        Rectangle rectangleToAttachTo,
        Direction direction)
    {
        var x = direction switch
        {
            Direction.Left => rectangleToAttachTo.Left - rectangleToMove.Width,
            Direction.Right => rectangleToAttachTo.Right,
            _ => rectangleToMove.X,
        };

        var y = direction switch
        {
            Direction.Up => rectangleToAttachTo.Top - rectangleToMove.Height,
            Direction.Down => rectangleToAttachTo.Bottom,
            _ => rectangleToMove.Y,
        };

        var position = new Point(x, y);
        return new Rectangle(position, rectangleToMove.Size);
    }

    private static Direction RotateCounterclockwise(
        Direction direction)
    {
        return direction switch
        {
            Direction.Left => Direction.Down,
            Direction.Down => Direction.Right,
            Direction.Right => Direction.Up,
            Direction.Up => Direction.Left,
            _ => direction,
        };
    }

    /// <summary>
    /// Возвращает прямоугольник размера rectangleSize,
    /// стоящий на границе с previousRectangle с той же стороны, что и prePreviousRectangle,
    /// и изначальное направление поиска подходящей для него позиции.
    /// </summary>
    private static (Rectangle, Direction) GetInitialRectangleAndRotationDirection(
        Size rectangleSize,
        Rectangle previousRectangle,
        Rectangle prePreviousRectangle)
    {
        var position = prePreviousRectangle.Location;
        var direction = Direction.None;

        if (position.X == previousRectangle.Right)
        {
            direction = Direction.Up;
        }
        else if (position.X < previousRectangle.X)
        {
            position.X = previousRectangle.X - rectangleSize.Width;
            direction = Direction.Down;
        }

        if (position.Y == previousRectangle.Bottom)
        {
            direction = Direction.Right;
        }
        else if (position.Y < previousRectangle.Y)
        {
            position.Y = previousRectangle.Y - rectangleSize.Height;
            direction = Direction.Left;
        }

        var rectangle = new Rectangle(position, rectangleSize);
        return (rectangle, direction);
    }

    /// <summary>
    /// Возвращает прямоугольник с центром в точке center.
    /// </summary>
    private static Rectangle GetFirstRectangle(
        Size rectangleSize,
        Point center)
    {
        var position = center - rectangleSize / 2;
        return new Rectangle(position, rectangleSize);
    }

    /// <summary>
    /// Возвращает прямоугольник, центр которого наиболее близок к центру прямоугольника firstRectangle.
    /// </summary>
    private static Rectangle GetSecondRectangle(
        Size rectangleSize,
        Rectangle firstRectangle)
    {
        var center = firstRectangle.Location + firstRectangle.Size / 2;
        var distanceToCenterIfPlacedLeft = rectangleSize.Width + firstRectangle.Width / 2;
        var distanceToCenterIfPlacedUp = rectangleSize.Height + firstRectangle.Height / 2;

        var position = distanceToCenterIfPlacedLeft < distanceToCenterIfPlacedUp
            ? new Point(center.X - distanceToCenterIfPlacedLeft, center.Y - rectangleSize.Height / 2)
            : new Point(center.X - rectangleSize.Width / 2, center.Y - distanceToCenterIfPlacedUp);

        return new Rectangle(position, rectangleSize);
    }
}
