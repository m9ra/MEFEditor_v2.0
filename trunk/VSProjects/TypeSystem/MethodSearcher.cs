using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public class MethodSearcher
    {
        LinkedList<SearchIterator> _activeIteartors = new LinkedList<SearchIterator>();
        List<TypeMethodInfo> _foundMethods = new List<TypeMethodInfo>();

        public bool HasResults { get { return _foundMethods.Count > 0; } }
        public IEnumerable<TypeMethodInfo> FoundResult { get { return _foundMethods; } }


        internal MethodSearcher(IEnumerable<AssemblyProvider> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                _activeIteartors.AddLast(assembly.CreateRootIterator());
            }
        }

        public void ExtendName(params string[] suffixes)
        {
            var currentIterator = _activeIteartors.First;

            while (currentIterator != null)
            {
                foreach (var suffix in suffixes)
                {
                    var newIt = currentIterator.Value.ExtendName(suffix);
                    if (newIt != null)
                        _activeIteartors.AddFirst(newIt);
                }

                var lastIt = currentIterator;
                currentIterator = currentIterator.Next;
                _activeIteartors.Remove(lastIt);
            }
        }

        public void Dispatch(string searchedMethod)
        {
            foreach (var iterator in _activeIteartors)
            {
                var methods = iterator.FindMethods(searchedMethod);
                if (methods != null)
                    _foundMethods.AddRange(methods);
            }
        }

        public void ClearResult()
        {
            _foundMethods.Clear();
        }

        public void SetCalledObject(InstanceInfo instanceInfo)
        {
            var namespaces = instanceInfo.TypeName.Split('.');
            foreach (var ns in namespaces)
            {
                ExtendName(ns);
            }
        }
    }

    /// <summary>
    /// Immutable iterator
    /// </summary>
    public abstract class SearchIterator
    {
        /// <summary>
        /// Create iterator that is extended by given suffix
        /// </summary>
        /// <param name="suffix">Extending suffix</param>
        /// <returns>Extended search iterator</returns>
        public abstract SearchIterator ExtendName(string suffix);

        public abstract IEnumerable<TypeMethodInfo> FindMethods(string searchedName);
    }
}
