namespace TagsCloud;

public static class DirectionOperations
{
    public static Direction RotateCounterclockwise(Direction direction)
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

    public static Direction Revert(Direction direction)
    {
        return direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => Direction.None,
        };
    }
}