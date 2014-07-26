using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace MEFEditor.Drawing.ArrangeEngine
{
    /// <summary>
    /// Representation of view angle of points
    /// </summary>
    abstract class ViewAngle
    {
        /// <summary>
        /// Determine that p1 has p2 in its angle
        /// </summary>
        /// <param name="p1">Point from which angle is measured</param>
        /// <param name="p2">Point to which angle is measured</param>
        /// <returns><c>true</c> if p2 is in p1's angle, <c>false</c> otherwise</returns>
        public abstract bool IsInAngle(Point p1, Point p2);
    }

    /// <summary>
    /// Accept points in specified quadrants
    /// 2|1
    /// ---
    /// 3|4
    /// </summary>
    class QuadrantAngle : ViewAngle
    {
        /// <summary>
        /// Indicator for quadrant 1
        /// </summary>
        private readonly bool _q1;

        /// <summary>
        /// Indicator for quadrant 2
        /// </summary>
        private readonly bool _q2;

        /// <summary>
        /// Indicator for quadrant 3
        /// </summary>
        private readonly bool _q3;

        /// <summary>
        /// Indicator for quadrant 4
        /// </summary>
        private readonly bool _q4;

        internal QuadrantAngle(bool q1, bool q2, bool q3, bool q4)
        {
            _q1 = q1;
            _q2 = q2;
            _q3 = q3;
            _q4 = q4;
        }

        /// <inheritdoc />
        public override bool IsInAngle(Point p1, Point p2)
        {
            var isAtRight = p1.X <= p2.X;
            var isAtTop = p1.Y >= p2.Y;
            var isAtBottom = p1.Y <= p2.Y;

            if (isAtRight)
            {
                return (isAtTop && _q1) || (isAtBottom && _q4);

            }
            else
            {
                return (isAtTop && _q2) || (isAtBottom && _q3);
            }
        }
    }

    /// <summary>
    /// Accept points in conus with 45degrees
    /// </summary>
    class ConusAngle : ViewAngle
    {
        /// <summary>
        /// Determine that conus is oriented verticaly
        /// </summary>
        private readonly bool _needVertical;

        /// <summary>
        /// Determine that positive part of conus is required
        /// </summary>
        private readonly bool _needPositive;

        public ConusAngle(bool needVertical, bool needPositive)
        {
            _needVertical = needVertical;
            _needPositive = needPositive;
        }

        /// <inheritdoc />
        public override bool IsInAngle(Point p1, Point p2)
        {
            double main1, main2;
            double minor1, minor2;
            if (_needVertical)
            {
                main1 = p1.Y;
                main2 = p2.Y;

                minor1 = p1.X;
                minor2 = p2.X;
            }
            else
            {
                main1 = p1.X;
                main2 = p2.X;

                minor1 = p1.Y;
                minor2 = p2.Y;
            }

            var mainDiff = main2 - main1;
            var minorDiff = Math.Abs(minor2 - minor1);

            if (Math.Abs(mainDiff) < minorDiff)
                //outside of conus
                return false;

            var isPositive = mainDiff >= 0;
            var isNegative = mainDiff <= 0;

            //check correct orientation
            return (_needPositive && isPositive) || (!_needPositive && isNegative);
        }
    }
}
