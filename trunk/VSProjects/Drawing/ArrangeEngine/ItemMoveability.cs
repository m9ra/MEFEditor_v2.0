using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.ArrangeEngine
{
    enum MoveDirection { Up, Down, Left, Right }

    struct Move
    {
        internal readonly MoveDirection Direction;
        internal readonly double Length;

        internal Move(MoveDirection direction, double length)
        {
            Direction = direction;
            if (length < 0)
                length = 0;

            Length = length;
        }

        internal bool IsSatisfiedBy(Move possibility)
        {
            return Direction == possibility.Direction && Length <= possibility.Length;
        }

        internal Point Apply(Point point)
        {
            switch (Direction)
            {
                case MoveDirection.Up:
                    point.Y -= Length;
                    break;
                case MoveDirection.Down:
                    point.Y += Length;
                    break;
                case MoveDirection.Left:
                    point.X -= Length;
                    break;
                case MoveDirection.Right:
                    point.X+=Length;
                    break;
                default:
                    throw new NotSupportedException("Given direction is not supported");
            }

            return point;
        }
    }

    class ItemMoveability
    {
        internal Move[] Moves;

        internal ItemMoveability(double up, double down, double left, double right)
        {
            Moves = new[]{
                new Move(MoveDirection.Up,up),
                new Move(MoveDirection.Down,down),   
                new Move(MoveDirection.Left,left),
                new Move(MoveDirection.Right,right),
            };
        }

        internal Move GetMove(MoveDirection moveDirection)
        {
            return Moves[(int)moveDirection];
        }
    }
}
