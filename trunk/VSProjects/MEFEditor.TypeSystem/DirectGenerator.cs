using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// <see cref="GeneratorBase"/> implementation of generator that
    /// provides <see cref="DirectMethod"/> generation.
    /// </summary>
    public class DirectGenerator : GeneratorBase, GenericMethodGenerator
    {
        /// <summary>
        /// The method that will be generated.
        /// </summary>
        private readonly DirectMethod _method;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectGenerator"/> class.
        /// </summary>
        /// <param name="directMethod">The method that will be generated.</param>
        public DirectGenerator(DirectMethod directMethod)
        {
            _method = directMethod;
        }

        /// <summary>
        /// Generate instructions through given emitter.
        /// <remarks>Throwing any exception will immediately stops analyzing.</remarks>
        /// </summary>
        /// <param name="emitter">The emitter which will be used for instruction generation.</param>
        protected override void generate(EmitterBase emitter)
        {
            emitter.DirectInvoke(_method);
        }

        /// <summary>
        /// Make generic specialization of method.
        /// </summary>
        /// <param name="methodPath">The method path with generic arguments.</param>
        /// <param name="methodDefinition">The method signature with generic parameters.</param>
        /// <returns>Generic specialization of method.</returns>
        public MethodItem Make(PathInfo methodPath, TypeMethodInfo methodDefinition)
        {
            return new MethodItem(this, methodDefinition.MakeGenericMethod(methodPath));
        }
    }
}
