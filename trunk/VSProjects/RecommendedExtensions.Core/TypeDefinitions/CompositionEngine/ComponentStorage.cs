using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utilities;
using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;

namespace RecommendedExtensions.Core.TypeDefinitions.CompositionEngine
{
    /// <summary>
    /// Storage of composed components. Provides searching routines on components and its exports.
    /// </summary>
    public class ComponentStorage
    {
        MultiDictionary<string, ComponentRef> _contrToComponents = new MultiDictionary<string, ComponentRef>();
        Dictionary<KeyValuePair<object, ComponentRef>, JoinPoint> _pairToJoinPoint = new Dictionary<KeyValuePair<object, ComponentRef>, JoinPoint>();
        HashSet<ComponentRef> _components = new HashSet<ComponentRef>();

        /// <summary>
        /// Errors collected during contract indexing.
        /// </summary>
        public string Error { get; private set; }
        /// <summary>
        /// Determine if indexing components failed.
        /// </summary>
        public bool Failed { get; private set; }

        /// <summary>
        /// Create component storage from given components.
        /// </summary>
        /// <param name="context">Context where storage is defined.</param>
        public ComponentStorage(CompositionContext context)
        {
            foreach (var component in context.Components)
            {
                _components.Add(component);

                //register component contracts
                foreach (var point in component.Points)
                {
                    _contrToComponents.Add(point.Contract, component);
                }
            }
        }
        
        /// <summary>
        /// Get components, which imports/exports has given contract.
        /// </summary>
        /// <param name="contract">Contract of searched imports/exports</param>
        /// <returns>Components available for contract.</returns>
        internal IEnumerable<ComponentRef> GetComponents(string contract)
        {
            return _contrToComponents.Get(contract);
        }

        /// <summary>
        /// Get all stored components without duplicities.
        /// </summary>
        /// <returns>all components enumeration.</returns>
        internal IEnumerable<ComponentRef> GetComponents()
        {
            return _components.ToArray();
        }

        /// <summary>
        /// Get all collected join points.
        /// </summary>
        /// <returns>All Collected join points.</returns>
        internal JoinPoint[] GetPoints()
        {
            //collect join points from all components
            var points = new List<JoinPoint>();

            foreach (var component in _components)
            {
                points.AddRange(component.Points);
            }
            return points.ToArray();
        }
    }
}
