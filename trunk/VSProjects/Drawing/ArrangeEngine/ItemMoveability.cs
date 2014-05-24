using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.ArrangeEngine
{
    /// <summary>
    /// Direction of move in 2D space
    /// <remarks>
    /// Integer representation of moves is important because of inverse move computation
    /// and stretchable determination
    /// </remarks>
    /// </summary>
    enum MoveDirection { Up, Left, Down, Right }

    /// <summary>
    /// Representation of Move
    /// </summary>
    class Move
    {
        /// <summary>
        /// Direction of move
        /// </summary>
        internal readonly MoveDirection Direction;

        /// <summary>
        /// Length of move in specified direction
        /// </summary>
        internal readonly double Length;

        /// <summary>
        /// Get <see cref="Move"/> that is inverse to current <see cref="Move"/>
        /// </summary>
        internal Move Inverse { get { return new Move(GetInverseDirection(Direction), Length); } }

        /// <summary>
        /// Determine that direction of current move is stretchable
        /// </summary>
        internal bool HasStretchableDirection { get { return (int)Direction > 1; } }

        /// <summary>
        /// Initialize new Move
        /// </summary>
        /// <param name="direction">Direction of move</param>
        /// <param name="length">Length of move in given direction</param>
        internal Move(MoveDirection direction, double length)
        {
            Direction = direction;
            if (length < 0)
                length = 0;

            Length = length;
        }

        /// <summary>
        /// Determine that move is included within
        /// given possibility
        /// </summary>
        /// <param name="possibility">Tested possibility</param>
        /// <returns><c>True</c> if move is satisfied, <c>false</c> otherwise</returns>
        internal bool IsSatisfiedBy(Move possibility)
        {
            return Direction == possibility.Direction && Length <= possibility.Length;
        }

        /// <summary>
        /// Apply move on given point
        /// </summary>
        /// <param name="point">Point which is moved according to current move</param>
        /// <returns>Moved point</returns>
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
                    point.X += Length;
                    break;
                default:
                    throw new NotSupportedException("Given direction is not supported");
            }

            return point;
        }

        /// <summary>
        /// Get <see cref="MoveDirection"/> that is inverse to given direction
        /// </summary>
        /// <param name="direction">Direction to inverse</param>
        /// <returns>Inversed direction</returns>
        internal static MoveDirection GetInverseDirection(MoveDirection direction)
        {
            return (MoveDirection)((int)(direction + 2) % 4);
        }

    }

    /// <summary>
    /// Representation of moving possibilities in multiple directions
    /// </summary>
    class ItemMoveability
    {
        /// <summary>
        /// Moves that are possible according ot current <see cref="ItemMoveability"/>
        /// </summary>
        internal readonly Move[] Moves;

        internal ItemMoveability(double up, double down, double left, double right)
        {
            Moves = new[]{
                new Move(MoveDirection.Up,up),
                new Move(MoveDirection.Left,left),
                new Move(MoveDirection.Down,down),   
                new Move(MoveDirection.Right,right),
            };
        }

        /// <summary>
        /// Get possible move that has given direction
        /// </summary>
        /// <param name="moveDirection">Descired direction of move</param>
        /// <returns><see cref="Move"/> with specified direction</returns>
        internal Move GetMove(MoveDirection moveDirection)
        {
            return Moves[(int)moveDirection];
        }
    }
}
