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

            _genericGenerator = generator as GenericMethodGenerator;
            if (info.HasGenericParameters)
            {                
                if (_genericGenerator == null)
                    throw new NotSupportedException("Cannot create method item for generic method without generic generator");
            }
            else
            {
                Generator =  generator;
            }
        }

        public MethodItem Make(PathInfo methodPath)
        {
            return _genericGenerator.Make(methodPath, Info);
        }
    }


    public class HashedIterator : SearchIterator
    {
        readonly private HashedMethodContainer _methods;

        readonly PathInfo _actualPath;

        public HashedIterator(HashedMethodContainer methods, PathInfo actualPath = null)
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
            return new HashedIterator(_methods, extendPath(suffix));
        }

        public override IEnumerable<TypeMethodInfo> FindMethods(string searchedName)
        {
            if (searchedName == null)
            {
                return _methods.AccordingType(_actualPath);
            }
            else
            {
                var path = extendPath(searchedName);

                return _methods.AccordingPath(path);
            }
        }

        private PathInfo extendPath(string suffix)
        {
            return new PathInfo(_actualPath, suffix);
        }
    }
}
