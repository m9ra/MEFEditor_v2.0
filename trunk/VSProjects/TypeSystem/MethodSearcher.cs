using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystem
{
    public class MethodSearcher
    {
        List<SearchIterator> _activeIteartors = new List<SearchIterator>();
        List<TypeMethodInfo> _foundMethods = new List<TypeMethodInfo>();

        public bool HasResults { get { return _foundMethods.Count > 0; } }
        public IEnumerable<TypeMethodInfo> FoundResult { get { return _foundMethods; } }


        internal MethodSearcher(IEnumerable<AssemblyProvider> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                _activeIteartors.Add(assembly.CreateRootIterator());
            }
        }

        public void ExtendName(string suffix)
        {
            var oldCount=_activeIteartors.Count;
            for (int i = 0; i < oldCount; ++i)
            {
                var iterator = _activeIteartors[i];
                var newIterator = iterator.ExtendName(suffix);
                if (newIterator == null)
                {
                    //TODO performance improvement
                    _activeIteartors.RemoveAt(i);
                    --oldCount;
                }
                else
                {
                    _activeIteartors[i] = newIterator;
                }
            }
        }

        public void Dispatch(string searchedMethod)
        {
            foreach (var iterator in _activeIteartors)
            {
                var methods=iterator.FindMethods(searchedMethod);
                _foundMethods.AddRange(methods);
            }
        }

        public void ClearResult()
        {
            _foundMethods.Clear();
        }
    }

    public abstract class SearchIterator
    {
        public abstract SearchIterator ExtendName(string suffix);

        public abstract IEnumerable<TypeMethodInfo> FindMethods(string searchedName);
    }
}
