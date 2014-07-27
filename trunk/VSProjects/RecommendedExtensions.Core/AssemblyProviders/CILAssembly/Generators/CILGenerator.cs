using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;

using RecommendedExtensions.Core.Languages.CIL;

namespace RecommendedExtensions.Core.AssemblyProviders.CILAssembly.Generators
{
    class CILGenerator : GeneratorBase, GenericMethodGenerator
    {
        private readonly MethodDefinition _method;

        /// <summary>
        /// Definition of represented method info
        /// </summary>
        private readonly TypeMethodInfo _info;

        private readonly TypeServices _services;

        internal CILGenerator(MethodDefinition method, TypeMethodInfo methodInfo, TypeServices services)
        {
            if (services == null)
                throw new ArgumentNullException("services");

            //method can be null - is used for default implementations
            /*if(method==null)
                throw new ArgumentNullException("method");*/

            _method = method;
            _info = methodInfo;
            _services = services;
        }

        protected override void generate(EmitterBase emitter)
        {
            if (_method == null)
                //nothing to emit
                return;

            var method = new CILMethod(_method, _info);
            Compiler.GenerateInstructions(method, _info, emitter, _services);
        }

        protected CILGenerator makeGeneric(TypeMethodInfo genericMethod)
        {
            return new CILGenerator(_method, genericMethod, _services);
        }

        public MethodItem Make(PathInfo methodPath, TypeMethodInfo methodDefinition)
        {
            var genericMethod = methodDefinition.MakeGenericMethod(methodPath);
            var generator = this.makeGeneric(genericMethod);

            return new MethodItem(generator, genericMethod);
        }
    }
}
