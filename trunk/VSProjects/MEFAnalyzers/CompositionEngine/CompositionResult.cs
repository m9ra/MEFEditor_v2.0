using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFAnalyzers.CompositionEngine
{
    /// <summary>
    /// Result of composition engine.
    /// </summary>
    public class CompositionResult
    {
        /// <summary>
        /// Determines if composition has failed.
        /// </summary>
        public readonly bool Failed;

        /// <summary>
        /// Error which identifies, why result failed.
        /// </summary>
        public readonly string Error;

        /// <summary>
        /// All joins collected during composition. If Failed==true, only ErrorJoins should be displayed.
        /// </summary>
        public readonly Join[] Joins;

        /// <summary>
        /// All join points collected during composition.
        /// </summary>
        public readonly JoinPoint[] Points;

        /// <summary>
        /// Generator for instructions providing composition
        /// </summary>
        internal readonly CompositionGenerator Generator;

        /// <summary>
        /// Create CompositionResult.
        /// </summary>
        /// <param name="joins">Collected joins.</param>
        /// <param name="points">Collected points</param>
        /// <param name="error">Error which appeared during composition.</param>
        internal CompositionResult(Join[] joins, JoinPoint[] points,CompositionGenerator generator, string error)
        {
            Joins = joins;
            Points = points;
            Failed = error != null;
            Error = error;

            Generator = generator;
        }
    }
}
