using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{

    public class MethodItem
    {
        public readonly GeneratorBase Generator;
        public readonly TypeMethodInfo Info;

        public MethodItem(GeneratorBase generator, TypeMethodInfo info)
        {
            Generator = generator;
            Info = info;
        }
    }

    public class HashIterator : SearchIterator
    {
        readonly private Dictionary<string, MethodItem> _methods = new Dictionary<string, MethodItem>();

        readonly string _actualPath;

        public HashIterator(Dictionary<string, MethodItem> methods, string actualPath = "")
        {
            _methods = methods;
            _actualPath = actualPath;
        }

        public override SearchIterator ExtendName(string suffix)
        {
            return new HashIterator(_methods, extendPath(suffix));
        }

        public override IEnumerable<TypeMethodInfo> FindMethods(string searchedName)
        {
            var name = extendPath(searchedName);

            MethodItem method;
            if (_methods.TryGetValue(name, out method))
            {
                yield return method.Info;
            }
        }


        private string extendPath(string name)
        {
            if (_actualPath == "")
            {
                return name;
            }
            else
            {
                return string.Format("{0}.{1}", _actualPath, name);
            }
        }
    }
}
