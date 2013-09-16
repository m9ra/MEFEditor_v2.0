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
    /// Storage of composed components. Provides searching routines on components and its exports.
    /// </summary>
    public class ComponentStorage
    {
        MultiDictionary<string, Instance> _contrToComponents = new MultiDictionary<string, Instance>();
        Dictionary<KeyValuePair<object, Instance>, JoinPoint> _pairToJoinPoint = new Dictionary<KeyValuePair<object, Instance>, JoinPoint>();
        HashSet<Instance> _components = new HashSet<Instance>();

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
        /// <param name="components">Components which will be stored in storage.</param>
        public ComponentStorage(CompositionContext context, IEnumerable<Instance> components)
        {
            foreach (var comp in components)
            {
                if (_components.Contains(comp)) continue; //this component has already been registered

                var compInfo = context.GetComponentInfo(comp);
                if (compInfo == null) continue; //this isnt component

                _components.Add(comp);
                foreach (var exp in compInfo.Exports)
                    addPoint(exp, comp);
                foreach (var exp in compInfo.SelfExports)
                    addPoint(exp, comp);
                foreach (var imp in compInfo.Imports)
                    addPoint(imp, comp);
            }
        }

        private void addPoint(Export point, Instance inst)
        {
            _contrToComponents.Add(point.Contract, inst);
            _pairToJoinPoint.Add(makePair(point, inst), new JoinPoint(inst, point));
        }

        private void addPoint(Import point, Instance inst)
        {
            var imp = new JoinPoint(inst, point);
            if (imp.AllowMany && imp.ImportManyItemType == null)
            {
                imp.Error = "Import doesn't have compatible ContractType with System.Array<T,1>, which is used for ImportMany imports";
                Failed = true;
                Error = "Cannot complete composition, because of wrong importing ContractType";
            }

            _contrToComponents.Add(point.Contract, inst);
            _pairToJoinPoint.Add(makePair(point, inst), imp);
        }

        private KeyValuePair<object, Instance> makePair(object point, Instance component)
        {
            var pair = new KeyValuePair<object, Instance>(point, component);
            return pair;
        }

        /// <summary>
        /// Get components, which imports/exports has given contract.
        /// </summary>
        /// <param name="contract">Contract of searched imports/exports</param>
        /// <returns>Components available for contract.</returns>
        internal IEnumerable<Instance> GetComponents(string contract)
        {
            var res = new HashSet<Instance>(_contrToComponents.GetValues(contract)); //because instances can satisfy contracts more times
            return res;
        }

        /// <summary>
        /// Get all stored components without duplicities.
        /// </summary>
        /// <returns>all components enumeration.</returns>
        internal IEnumerable<Instance> GetComponents()
        {
            return _components.ToArray();
        }

        /// <summary>
        /// Translate point and component to JoinPoint.
        /// </summary>
        /// <param name="point">Import/Export point.</param>
        /// <param name="component">Component on which is point defined.</param>
        /// <returns>Translated join point.</returns>
        internal JoinPoint Translate(object point, Instance component)
        {
            var pair = makePair(point, component);
            return _pairToJoinPoint[pair];
        }

        /// <summary>
        /// Get all collected join points.
        /// </summary>
        /// <returns>All Collected join points.</returns>
        internal JoinPoint[] GetPoints()
        {
            return _pairToJoinPoint.Values.ToArray();
        }
    }
}
