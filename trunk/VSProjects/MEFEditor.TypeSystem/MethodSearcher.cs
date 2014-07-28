using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Provides <see cref="MEFEditor.TypeSystem"/> services for method signatures searching.
    /// It is able to handle multiple variants, that are useful for searching method in unknown
    /// namespace.
    /// </summary>
    public class MethodSearcher
    {
        /// <summary>
        /// Currently active iterators.
        /// </summary>
        LinkedList<SearchIterator> _activeIterators = new LinkedList<SearchIterator>();

        /// <summary>
        /// Currently found methods.
        /// </summary>
        List<TypeMethodInfo> _foundMethods = new List<TypeMethodInfo>();

        /// <summary>
        /// The assemblies where methods are searched.
        /// </summary>
        AssemblyProvider[] _assemblies;

        /// <summary>
        /// Gets a value indicating whether this instance has a results.
        /// </summary>
        /// <value><c>true</c> if this instance has results; otherwise, <c>false</c>.</value>
        public bool HasResults { get { return _foundMethods.Count > 0; } }

        /// <summary>
        /// Gets methods that has been found.
        /// </summary>
        /// <value>The search result.</value>
        public IEnumerable<TypeMethodInfo> FoundResult { get { return _foundMethods; } }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodSearcher" /> class.
        /// </summary>
        /// <param name="assemblies">The assemblies where methods will be searched.</param>
        internal MethodSearcher(IEnumerable<AssemblyProvider> assemblies)
        {
            _assemblies = assemblies.ToArray();
            foreach (var assembly in _assemblies)
            {
                _activeIterators.AddLast(assembly.CreateRootIterator());
            }
        }

        /// <summary>
        /// Extend name according to possible suffixes. Suffixes are searched in parallel.
        /// </summary>
        /// <param name="possibleSuffixes">Possible suffixes that are added behind current position in paralel.</param>
        public void ExtendName(params string[] possibleSuffixes)
        {
            extendName(_activeIterators, possibleSuffixes);
        }

        /// <summary>
        /// Extend name according to possible suffixes. Suffixes are searched in parallel.
        /// </summary>
        /// <param name="activeIterators">List of active iterators, where extending is processed.</param>
        /// <param name="possibleSuffixes">Possible suffixes that are added behind current position in paralel.</param>
        private void extendName(LinkedList<SearchIterator> activeIterators, IEnumerable<string> possibleSuffixes)
        {
            var currentIterator = activeIterators.First;

            var partedSuffixes = new List<string[]>();
            foreach (var possibleSuffix in possibleSuffixes)
            {
                var partedSuffix = Naming.SplitGenericPath(possibleSuffix);
                partedSuffixes.Add(partedSuffix);
            }

            while (currentIterator != null)
            {
                foreach (var partedSuffix in partedSuffixes)
                {
                    var newIt = currentIterator.Value;

                    //walk all parts
                    foreach (var suffix in partedSuffix)
                    {
                        newIt = newIt.ExtendName(suffix);
                        if (newIt == null)
                            break;
                    }

                    if (newIt != null)
                        activeIterators.AddFirst(newIt);
                }

                var lastIt = currentIterator;
                currentIterator = currentIterator.Next;
                activeIterators.Remove(lastIt);
            }
        }

        /// <summary>
        /// Dispatch currently reached locations according to given name.
        /// </summary>
        /// <param name="searchedMethod">Method that is searched at reached locations.</param>
        public void Dispatch(string searchedMethod)
        {
            expandIterators();

            foreach (var iterator in _activeIterators)
            {
                var methods = iterator.FindMethods(searchedMethod);
                if (methods != null)
                    _foundMethods.AddRange(methods);
            }
        }

        /// <summary>
        /// Expands the iterators.
        /// </summary>
        private void expandIterators()
        {
            var expandPaths = new List<string>();
            var toExpand = new List<SearchIterator>(_activeIterators);

            while (toExpand.Count > 0)
            {
                foreach (var iterator in toExpand)
                {
                    expandPaths.AddRange(iterator.GetExpansions());
                }

                if (expandPaths.Count == 0)
                    //no iterator needs to be expanded
                    break;

                var toExtend = new LinkedList<SearchIterator>();
                foreach (var assembly in _assemblies)
                {
                    toExtend.AddLast(assembly.CreateRootIterator());
                }

                extendName(toExtend, expandPaths);
                if (toExtend.Count == 0)
                    //no iterator needs to be added
                    break;

                expandPaths.Clear();
                toExpand.Clear();
                toExpand.AddRange(toExtend);

                //TODO it could be implement in O(1) with own LinkedList implementation
                foreach (var extended in toExtend)
                {
                    _activeIterators.AddLast(extended);
                }
            }
        }

        /// <summary>
        /// Clears the current result.
        /// </summary>
        public void ClearResult()
        {
            _foundMethods.Clear();
        }

        /// <summary>
        /// Sets the type of called object as base for searching.
        /// </summary>
        /// <param name="instanceInfo">Called object's type.</param>
        public void SetCalledObject(InstanceInfo instanceInfo)
        {
            ExtendName(instanceInfo.TypeName);
        }

    }

    /// <summary>
    /// Immutable iterator used for searching method signatures in 
    /// form of <see cref="TypeMethodInfo"/>.
    /// </summary>
    public abstract class SearchIterator
    {
        /// <summary>
        /// Create iterator that is extended by given suffix
        /// </summary>
        /// <param name="suffix">Extending suffix</param>
        /// <returns>Extended search iterator</returns>
        public abstract SearchIterator ExtendName(string suffix);

        /// <summary>
        /// Find methods in "locations" that has been previously reached by extending
        /// method name by <see cref="ExtendName"/>.
        /// </summary>
        /// <param name="searchedName">Method that is searched at reached locations, 
        /// <c>null</c> if there is no constraint on searched method. In that case all 
        /// methods of current type should be listed</param>
        /// <returns>Methods which match given search name and previously extended name.</returns>
        public abstract IEnumerable<TypeMethodInfo> FindMethods(string searchedName);

        /// <summary>
        /// If overridden can cause expanding into multiple iterators
        /// It is called before <see cref="FindMethods"/>
        /// </summary>
        /// <returns>Fullpaths that will be expanded into new iterators (recursively)</returns>
        public virtual IEnumerable<string> GetExpansions()
        {
            yield break;
        }
    }
}
