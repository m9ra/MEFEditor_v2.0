using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem.Runtime.Building
{
    class RuntimeMethodGenerator : GeneratorBase
    {
        internal readonly TypeMethodInfo MethodInfo;

        internal readonly IEnumerable<Type> ImplementTypes;

        private readonly DirectMethod _method;

        /// <summary>
        /// Initialize method generator for methods defined in runtime type definitions
        /// </summary>
        /// <param name="method">Method represented by this generator</param>
        /// <param name="methodInfo">Info of represented method</param>
        internal RuntimeMethodGenerator(DirectMethod method, TypeMethodInfo methodInfo, IEnumerable<Type> implementTypes)
        {
            MethodInfo = methodInfo;
            ImplementTypes = implementTypes;
            _method = method;
        }

        internal IEnumerable<InstanceInfo> Implemented()
        {
            var implementedTypes = new List<InstanceInfo>();
            foreach (var implemented in ImplementTypes)
            {
                implementedTypes.Add(new InstanceInfo(implemented));
            }
            return implementedTypes;
        }

        protected override void generate(EmitterBase emitter)
        {
            emitter.DirectInvoke(_method);
        }

        internal GenericMethodProvider GetProvider()
        {
            return (searchPath, info) =>
            {
                var genericMethod = info.MakeGenericMethod(searchPath);
                var generator = new RuntimeMethodGenerator(_method, genericMethod, ImplementTypes);
                return new MethodItem(generator, genericMethod);
            };
        }


    }
}
