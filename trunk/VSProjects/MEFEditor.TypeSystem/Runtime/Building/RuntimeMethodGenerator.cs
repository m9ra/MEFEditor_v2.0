using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem.Runtime.Building
{
    /// <summary>
    /// <see cref="GeneratorBase" /> implementation for methods defined by <see cref="RuntimeTypeDefinition" />.
    /// </summary>
    class RuntimeMethodGenerator : GeneratorBase, GenericMethodGenerator
    {
        /// <summary>
        /// The method signature information.
        /// </summary>
        internal readonly TypeMethodInfo MethodInfo;

        /// <summary>
        /// The implemented types.
        /// </summary>
        internal readonly IEnumerable<Type> ImplementTypes;

        /// <summary>
        /// The generated method.
        /// </summary>
        private readonly DirectMethod _method;


        /// <summary>
        /// Gets the enumeration of implemented types.
        /// </summary>
        /// <value>The implemented types enumeration.</value>
        internal IEnumerable<InstanceInfo> Implemented
        {
            get
            {
                var implementedTypes = new List<InstanceInfo>();
                foreach (var implemented in ImplementTypes)
                {
                    implementedTypes.Add(TypeDescriptor.Create(implemented));
                }
                return implementedTypes;
            }
        }

        /// <summary>
        /// Initialize method generator for methods defined in runtime type definitions.
        /// </summary>
        /// <param name="method">Method represented by this generator.</param>
        /// <param name="methodInfo">Info of represented method.</param>
        /// <param name="implementTypes">Types that are implemented by given method.</param>
        internal RuntimeMethodGenerator(DirectMethod method, TypeMethodInfo methodInfo, IEnumerable<Type> implementTypes)
        {
            MethodInfo = methodInfo;
            ImplementTypes = implementTypes;
            _method = method;
        }

        /// <summary>
        /// Makes the specified search path.
        /// </summary>
        /// <param name="searchPath">The search path.</param>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns>MethodItem.</returns>
        public MethodItem Make(PathInfo searchPath, TypeMethodInfo methodDefinition)
        {
            var genericMethod = methodDefinition.MakeGenericMethod(searchPath);
            var generator = new RuntimeMethodGenerator(_method, genericMethod, ImplementTypes);
            return new MethodItem(generator, genericMethod);
        }

        /// <summary>
        /// Generate instructions through given emitter.
        /// <remarks>Throwing any exception will immediately stops analyzing.</remarks>.
        /// </summary>
        /// <param name="emitter">The emitter which will be used for instruction generation.</param>
        protected override void generate(EmitterBase emitter)
        {
            emitter.DirectInvoke(_method);
        }
    }
}
