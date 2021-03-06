﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;
using Utilities;

namespace RecommendedExtensions.Core.TypeDefinitions.CompositionEngine
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
        /// <param name="context">Context of composition, contains list of components to be composed</param>
        /// <returns>CompositionResult which is created according to composition simulation.</returns>
        public static CompositionResult Compose(CompositionContext context)
        {
            if (_processingCompositions > _processingCompositionsLimit)
                return new CompositionResult(context,null, null, null, "Limit of simultaneously processing compositions was reached - possible recursion in ImportingConstructor?");

            ++_processingCompositions;
            var worker = new CompositionWorker(context);
            var result = worker.GetResult();
            --_processingCompositions;

            return result;
        }
    }
}
