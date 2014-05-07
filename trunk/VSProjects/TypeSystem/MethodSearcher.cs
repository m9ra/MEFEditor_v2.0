using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

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

        /// <summary>
        /// Extend name according to possible suffixes. Suffixes are searched in paralel        /// 
        /// </summary>
        /// <param name="possibleSuffixes">Possible suffixes that are added behind current position in paralel</param>
        public void ExtendName(params string[] possibleSuffixes)
        {
            var currentIterator = _activeIteartors.First;

            var partedSuffixes = new List<string[]>();
            foreach (var possibleSuffix in possibleSuffixes)
            {
                var partedSuffix = splitSuffix(possibleSuffix);
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
                        _activeIteartors.AddFirst(newIt);
                }

                var lastIt = currentIterator;
                currentIterator = currentIterator.Next;
                _activeIteartors.Remove(lastIt);
            }
        }

        /// <summary>
        /// Dispatch currently reached locations according to given name
        /// </summary>
        /// <param name="searchedMethod">Method that is searched at reached locations</param>
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
            ExtendName(instanceInfo.TypeName);
        }

        private string[] splitSuffix(string suffix)
        {
            var parts = new List<string>();
            var genericDepth = 0;
            var lastPartEndIndex = 0;
            for (int i = 0; i < suffix.Length; ++i)
            {
                var ch = suffix[i];
                switch (ch)
                {
                    case '<':
                        ++genericDepth;
                        break;
                    case '>':
                        --genericDepth;
                        break;
                    case '.':
                        if (genericDepth > 0)
                            //inside namespace of generic argument
                            break;

                        var part = suffix.Substring(lastPartEndIndex, i - lastPartEndIndex);
                        parts.Add(part);
                        lastPartEndIndex = i + 1;
                        break;
                }
            }

            if (lastPartEndIndex + 1 < suffix.Length)
                parts.Add(suffix.Substring(lastPartEndIndex, suffix.Length - lastPartEndIndex));

            return parts.ToArray();
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

        /// <summary>
        /// Dispatch currently reached locations according to given name
        /// </summary>
        /// <param name="searchedMethod">Method that is searched at reached locations</param>
        public abstract IEnumerable<TypeMethodInfo> FindMethods(string searchedName);
    }
}
