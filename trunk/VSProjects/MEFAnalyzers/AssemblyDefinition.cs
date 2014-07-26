using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

using Analyzing;
using Analyzing.Editing;
using TypeSystem;
using TypeSystem.Runtime;

namespace MEFAnalyzers
{
    public class AssemblyDefinition : DataTypeDefinition
    {
        protected readonly Field<string> AssemblyFullPath;

        public AssemblyDefinition()
        {
            Simulate<Assembly>();
        }

        public string _get_FullPath()
        {
            return AssemblyFullPath.Value;
        }

        [ReturnType(typeof(Assembly))]
        public Instance _static_method_GetEntryAssembly()
        {
            return _static_method_GetCallingAssembly();
        }

        [ReturnType(typeof(Assembly))]
        public Instance _static_method_GetExecutingAssembly()
        {
            return _static_method_GetCallingAssembly();
        }

        [ReturnType(typeof(Assembly))]
        public Instance _static_method_GetCallingAssembly()
        {
            return constructAssemblyRepresentation(GetCallerAssembly());
        }

        [ParameterTypes(typeof(Type))]
        [ReturnType(typeof(Assembly))]
        public Instance _static_method_GetAssembly(Instance type)
        {
            AsyncCall<string>(type, "get_FullName", (fullname) =>
            {
                var assembly = Services.GetDefiningAssembly(TypeDescriptor.Create(fullname));
                var resultAssembly = constructAssemblyRepresentation(assembly);

                Context.Return(resultAssembly);
            });
            
            //result will be overriden
            return Context.Machine.Null;
        }

        /// <summary>
        /// Create <see cref="Instance"/> representation of given <see cref="TypeAssembly"/>
        /// </summary>
        /// <param name="assembly">assembly to be represented</param>
        /// <returns>Created <see cref="Instance"/></returns>
        private Instance constructAssemblyRepresentation(TypeAssembly assembly)
        {
            var resultAssembly = Context.Machine.CreateInstance(TypeInfo);

            RunInContextOf(resultAssembly, () =>
            {
                AssemblyFullPath.Value = assembly.FullPathMapping;
            });
            return resultAssembly;
        }
    }
}
