using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Director emitting method instructions. It can
    /// be used with <see cref="DirectedGenerator"/>.
    /// </summary>
    /// <param name="emitter">The emitter.</param>
    public delegate void EmitDirector(EmitterBase emitter);

    /// <summary>
    /// Instruction generator that is directed by given <see cref="EmitDirector" />.
    /// </summary>
    public class DirectedGenerator : GeneratorBase, GenericMethodGenerator
    {
        /// <summary>
        /// The _director.
        /// </summary>
        private readonly EmitDirector _director;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGenerator" /> class.
        /// </summary>
        /// <param name="director">The director.</param>
        public DirectedGenerator(EmitDirector director)
        {
            _director = director;
        }

        /// <summary>
        /// Generate instructions through given emitter.
        /// <remarks>Throwing any exception will immediately stops analyzing.</remarks>.
        /// </summary>
        /// <param name="emitter">The emitter which will be used for instruction generation.</param>
        protected override void generate(EmitterBase emitter)
        {
            _director(emitter);
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
