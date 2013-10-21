using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace MEFAnalyzers.Drawings
{

    /// <summary>
    /// Bezier Spline methods
    /// This code is changed one from 
    /// http://ovpwp.wordpress.com/2008/12/17/how-to-draw-a-smooth-curve-through-a-set-of-2d-points-with-bezier-methods/
    /// </summary>
    class BezierSpline
    {
        public readonly Point[] Knots;

        /// <summary>
        /// Output First Control points array of knots.Length - 1 length.
        /// </summary>
        public readonly Point[] FirstControlPoints;

        /// <summary>
        /// Output Second Control points array of knots.Length - 1 length
        /// </summary>
        public readonly Point[] SecondControlPoints;

        public readonly int N;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="knots">Input Knot Bezier spline points.</param>
        internal BezierSpline(Point[] knots)
        {
            Knots = knots;

            N = Knots.Length - 1;
            if (N < 1)
            {
                FirstControlPoints = new Point[0];
                SecondControlPoints = new Point[0];
            }
            else
            {
                FirstControlPoints = new Point[N];
                SecondControlPoints = new Point[N];
                computeControlPoints();
            }
        }

        /// <summary>
        /// Get open-ended Bezier Spline Control Points.
        /// </summary>
        public void computeControlPoints()
        {
            // Calculate first Bezier control points
            // Right hand side vector
            double[] rhs = new double[N];

            // Set right hand side X values
            for (int i = 1; i < N - 1; ++i)
                rhs[i] = 4 * Knots[i].X + 2 * Knots[i + 1].X;
            rhs[0] = Knots[0].X + 2 * Knots[1].X;
            rhs[N - 1] = 3 * Knots[N - 1].X;
            // Get first control points X-values
            double[] x = GetFirstControlPoints(rhs);

            // Set right hand side Y values
            for (int i = 1; i < N - 1; ++i)
                rhs[i] = 4 * Knots[i].Y + 2 * Knots[i + 1].Y;
            rhs[0] = Knots[0].Y + 2 * Knots[1].Y;
            rhs[N - 1] = 3 * Knots[N - 1].Y;
            // Get first control points Y-values
            double[] y = GetFirstControlPoints(rhs);

            // Fill output arrays.
   
            for (int i = 0; i < N; ++i)
            {
                // First control point
                FirstControlPoints[i] = new Point(x[i], y[i]);
                // Second control point
                if (i < N - 1)
                    SecondControlPoints[i] = new Point(2 * Knots[i + 1].X - x[i + 1], 2 * Knots[i + 1].Y - y[i + 1]);
                else
                    SecondControlPoints[i] = new Point((Knots[N].X + x[N - 1]) / 2, (Knots[N].Y + y[N - 1]) / 2);
            }
        }

        /// <summary>
        /// Solves a tridiagonal system for one of coordinates (x or y) of first Bezier control points.
        /// </summary>
        /// <param name="rhs">Right hand side vector.</param>
        /// <returns>Solution vector.</returns>
        private double[] GetFirstControlPoints(double[] rhs)
        {
            int n = rhs.Length;
            double[] x = new double[n]; // Solution vector.
            double[] tmp = new double[n]; // Temp workspace.

            double b = 2.0;
            x[0] = rhs[0] / b;
            for (int i = 1; i < n; i++) // Decomposition and forward substitution.
            {
                tmp[i] = 1 / b;
                b = (i < n - 1 ? 4.0 : 2.0) - tmp[i];
                x[i] = (rhs[i] - x[i - 1]) / b;
            }
            for (int i = 1; i < n; i++)
                x[n - i - 1] -= tmp[n - i] * x[n - i]; // Backsubstitution.
            return x;
        }
    }
}
