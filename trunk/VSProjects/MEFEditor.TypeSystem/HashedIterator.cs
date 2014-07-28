using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Interface that has to be implemented by <see cref="GeneratorBase"/> generators
    /// that supports generic method specialization.
    /// </summary>
    public interface GenericMethodGenerator
    {
        /// <summary>
        /// Make generic specialization of method.
        /// </summary>
        /// <param name="methodPath">The method path with generic arguments.</param>
        /// <param name="methodDefinition">The method signature with generic parameters.</param>
        /// <returns>Generic specialization of method.</returns>
        MethodItem Make(PathInfo methodPath, TypeMethodInfo methodDefinition);
    }

    /// <summary>
    /// Utility class that can be used for handling method definitions
    /// by <see cref="AssemblyProvider"/> implementations.
    /// </summary>
    public class MethodItem
    {
        /// <summary>
        /// The instruction generator that is able to make generic specializations.
        /// </summary>
        private readonly GenericMethodGenerator _genericGenerator;

        /// <summary>
        /// The instruction generator defining instruction of represented method.
        /// </summary>
        public readonly GeneratorBase Generator;

        /// <summary>
        /// Information about signature of represented method.
        /// </summary>
        public readonly TypeMethodInfo Info;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodItem"/> class.
        /// </summary>
        /// <param name="generator">The instruction generator defining instruction of represented method.</param>
        /// <param name="info">Information about signature of represented method.</param>
        /// <exception cref="System.NotSupportedException">Cannot create method item for generic method without generic generator</exception>
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
                Generator = generator;
            }
        }

        /// <summary>
        /// Make generic specialization of method.
        /// </summary>
        /// <param name="methodPath">The method path with generic arguments.</param>
        /// <returns>Generic specialization of method.</returns>
        public MethodItem Make(PathInfo methodPath)
        {
            return _genericGenerator.Make(methodPath, Info);
        }
    }

    /// <summary>
    /// Utility implementatoin of <see cref="SearchIterator" /> that can be used
    /// with <see cref="HashedMethodContainer" />.
    /// </summary>
    public class HashedIterator : SearchIterator
    {
        /// <summary>
        /// The contained methods.
        /// </summary>
        readonly private HashedMethodContainer _methods;

        /// <summary>
        /// Actualy iterated path.
        /// </summary>
        readonly PathInfo _actualPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashedIterator" /> class.
        /// </summary>
        /// <param name="methods">The methods w.</param>
        /// <param name="actualPath">The actual path.</param>
        public HashedIterator(HashedMethodContainer methods, PathInfo actualPath = null)
        {
            if (actualPath == null)
            {
                actualPath = new PathInfo("");
            }

            _methods = methods;
            _actualPath = actualPath;
        }

        /// <summary>
        /// Create iterator that is extended by given suffix.
        /// </summary>
        /// <param name="suffix">Extending suffix.</param>
        /// <returns>Extended search iterator.</returns>
        public override SearchIterator ExtendName(string suffix)
        {
            return new HashedIterator(_methods, extendPath(suffix));
        }



        /// <summary>
        /// Find methods in "locations" that has been previously reached by extending
        /// method name by <see cref="ExtendName" />.
        /// </summary>
        /// <param name="searchedName">Name of the searched.</param>
        /// <returns>Methods which match given search name and previously extended name.</returns>
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

        /// <summary>
        /// Extends the path with given suffix.
        /// </summary>
        /// <param name="suffix">The extending suffix.</param>
        /// <returns>Extended path.</returns>
        private PathInfo extendPath(string suffix)
        {
            return new PathInfo(_actualPath, suffix);
        }
    }
}
