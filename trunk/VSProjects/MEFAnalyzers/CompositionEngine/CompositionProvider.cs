using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;
using Utilities;

namespace MEFAnalyzers.CompositionEngine
{
    /// <summary>
    /// Class providing MEF composition simulation.
    /// </summary>
    public static class CompositionProvider
    {
        /// <summary>
        /// Counter for simultaneously processing compositions - stop on cyclic compositions
        /// </summary>
        private static int _processingCompositions = 0;

        /// <summary>
        /// Limit for simultaneously processing compositions.
        /// </summary>
        const int _processingCompositionsLimit = 10;

        /// <summary>
        /// Simulate composition according to given parts.
        /// </summary>
        /// <param name="context">Services available for interpreting.</param>
        /// <param name="parts">Parts which will be composed.</param>
        /// <returns>CompositionResult which is created according to composition simulation.</returns>
        public static CompositionResult Compose(CompositionContext context, IEnumerable<Instance> parts)
        {
            if (parts == null) 
                throw new ArgumentNullException("instances");

            if (_processingCompositions > _processingCompositionsLimit) 
                return new CompositionResult(null, null, "Limit of simultaneously processing compositions was reached - possible recursion in ImportingConstructor?");

            ++_processingCompositions;
            var worker = new CompositionWorker(context, parts);
            var result = worker.GetResult();
            --_processingCompositions;

            return result;
        }
    }
}
