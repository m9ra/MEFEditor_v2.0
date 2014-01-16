using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public interface GenericMethodGenerator
    {
        MethodItem Make(PathInfo methodPath, TypeMethodInfo methodDefinition);
    }


    public class MethodItem
    {
        private readonly GenericMethodGenerator _genericGenerator;
        public readonly GeneratorBase Generator;
        public readonly TypeMethodInfo Info;

        public MethodItem(GeneratorBase generator, TypeMethodInfo info)
        {
            Info = info;
            if (info.HasGenericParameters)
            {
                _genericGenerator = generator as GenericMethodGenerator;
                if (_genericGenerator == null)
                    throw new NotSupportedException("Cannot create method item for generic method without generic generator");
            }
            else
            {
                Generator = Info.IsAbstract ? null : generator;
            }
        }

        public MethodItem Make(PathInfo methodPath, TypeMethodInfo methodDefinition)
        {
            return _genericGenerator.Make(methodPath, methodDefinition);
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
