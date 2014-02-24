using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;

using AssemblyProviders.CSharp;

namespace AssemblyProviders.ProjectAssembly.MethodBuilding
{
    /// <summary>
    /// Provider for parsing source code 
    /// </summary>
    /// <param name="activation">Activation with information for parsing</param>
    /// <param name="emitter">Emitter where parsed instructions are emitted</param>
    public delegate void ParsingProvider(ParsingActivation activation, EmitterBase emitter);

    /// <summary>
    /// Generator of IAL instructions from methods with source code
    /// </summary>
    public class SourceMethodGenerator : GeneratorBase, GenericMethodGenerator
    {

        /// <summary>
        /// Parser used for generating method instructions
        /// </summary>
        private readonly ParsingProvider _parsingProvider;

        /// <summary>
        /// <see cref="ParsingActivation"/> of generated method
        /// </summary>
        public readonly ParsingActivation Activation;

        /// <summary>
        /// Initialize <see cref="SourceMethodGenerator"/> object from non-generic or non-specialized generic method definitions.
        /// </summary>
        /// <param name="sourceCode">Source code of generated method</param>
        /// <param name="method">Method info of generated method</param>
        internal SourceMethodGenerator(string sourceCode, TypeMethodInfo method, ParsingProvider parsingProvider)
            : this(sourceCode, method, method, parsingProvider)
        {

        }

        /// <summary>
        /// Initialize <see cref="SourceMethodGenerator"/> object from non-generic or non-specialized generic method definitions.
        /// </summary>
        /// <param name="sourceCode">Source code of generated method</param>
        /// <param name="method">Method info of generated method</param>
        /// <param name="method">Method info of generic definition of generated method</param>
        private SourceMethodGenerator(string sourceCode, TypeMethodInfo method, TypeMethodInfo methodDefinition, ParsingProvider parsingProvider)
        {
            if (sourceCode == null)
                throw new ArgumentNullException("sourceCode");

            if (method == null)
                throw new ArgumentNullException("method");

            if (method == null)
                throw new ArgumentNullException("methodDefinition");

            if (parsingProvider == null)
                throw new ArgumentNullException("parsingProvider");

            var activation = new ParsingActivation(sourceCode, method, method);

            _parsingProvider = parsingProvider;
        }

        /// <inheritdoc />
        protected override void generate(EmitterBase emitter)
        {
            _parsingProvider(Activation, emitter);
        }

        /// <inheritdoc />
        public MethodItem Make(PathInfo methodPath, TypeMethodInfo methodDefinition)
        {
            var specializedMethod = Activation.Method.MakeGenericMethod(methodPath);
            var specializedGenerator = new SourceMethodGenerator(Activation.SourceCode, specializedMethod, methodDefinition, _parsingProvider);

            return new MethodItem(specializedGenerator, specializedMethod);
        }
    }
}
