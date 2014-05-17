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
        /// Initialize <see cref="SourceMethodGenerator"/> object from given activation.
        /// </summary>
        /// <param name="activation">Activation of parser for generated method</param>
        /// <param name="parsingProvider">Provider of parsing service for method</param>
        internal SourceMethodGenerator(ParsingActivation activation, ParsingProvider parsingProvider)
        {
            if (parsingProvider == null)
                throw new ArgumentNullException("parsingProvider");

            if (activation == null)
                throw new ArgumentNullException("activation");

            _parsingProvider = parsingProvider;
            Activation = activation;
        }


        /// <inheritdoc />
        protected override void generate(EmitterBase emitter)
        {
            if (Activation == null)
                throw new NotSupportedException("Cannot parse abstract method: " + Activation.Method.MethodID);

            _parsingProvider(Activation, emitter);
        }

        /// <inheritdoc />
        public MethodItem Make(PathInfo methodPath, TypeMethodInfo methodDefinition)
        {
            var specializedMethod = Activation.Method.MakeGenericMethod(methodPath);

            var activation = new ParsingActivation(Activation.SourceCode, specializedMethod, Activation.GenericParameters);
            var specializedGenerator = new SourceMethodGenerator(activation, _parsingProvider);

            return new MethodItem(specializedGenerator, specializedMethod);
        }
    }
}
