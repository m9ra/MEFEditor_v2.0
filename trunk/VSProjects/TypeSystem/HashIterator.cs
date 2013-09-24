using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public delegate MethodItem GenericMethodProvider(PathInfo searchedPath, TypeMethodInfo genericMethod);

    public class MethodItem
    {
        public readonly GenericMethodProvider MethodProvider;
        public readonly GeneratorBase Generator;
        public readonly TypeMethodInfo Info;

        public MethodItem(GeneratorBase generator, TypeMethodInfo info)
        {
            if (info.HasGenericParameters)
                throw new NotSupportedException("Cannot create generic method item without GenericMethodProvider");

            Info = info;
            Generator = Info.IsAbstract ? null : generator;
        }

        public MethodItem(GenericMethodProvider methodProvider, TypeMethodInfo genericInfo)
        {
            MethodProvider = methodProvider;
            Info = genericInfo;
        }
    }



    public class HashIterator : SearchIterator
    {
        readonly private HashedMethodContainer _methods;

        readonly PathInfo _actualPath;

        public HashIterator(HashedMethodContainer methods, PathInfo actualPath = null)
        {
            if (actualPath == null)
            {
                actualPath = new PathInfo("");
            }

            _methods = methods;
            _actualPath = actualPath;
        }

        public override SearchIterator ExtendName(string suffix)
        {
            return new HashIterator(_methods, extendPath(suffix));
        }

        public override IEnumerable<TypeMethodInfo> FindMethods(string searchedName)
        {
            var path = extendPath(searchedName);

            return _methods.AccordingPath(path);
        }

        private PathInfo extendPath(string suffix)
        {
            return new PathInfo(_actualPath, suffix);
        }
    }
}
