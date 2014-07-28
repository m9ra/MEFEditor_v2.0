using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace RecommendedExtensions.Core.TypeDefinitions
{
    /// <summary>
    /// Analyzing definition of <see cref="Assembly" />.
    /// </summary>
    public class AssemblyDefinition : DataTypeDefinition
    {
        /// <summary>
        /// The assembly full path.
        /// </summary>
        protected readonly Field<string> AssemblyFullPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyDefinition" /> class.
        /// </summary>
        public AssemblyDefinition()
        {
            Simulate<Assembly>();
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>System.String.</returns>
        public string _get_FullPath()
        {
            return AssemblyFullPath.Value;
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>Instance.</returns>
        [ReturnType(typeof(Assembly))]
        public Instance _static_method_GetEntryAssembly()
        {
            return _static_method_GetCallingAssembly();
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>Instance.</returns>
        [ReturnType(typeof(Assembly))]
        public Instance _static_method_GetExecutingAssembly()
        {
            return _static_method_GetCallingAssembly();
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>Instance.</returns>
        [ReturnType(typeof(Assembly))]
        public Instance _static_method_GetCallingAssembly()
        {
            return constructAssemblyRepresentation(GetCallerAssembly());
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Instance.</returns>
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
        /// Create <see cref="Instance" /> representation of given <see cref="TypeAssembly" />.
        /// </summary>
        /// <param name="assembly">assembly to be represented.</param>
        /// <returns>Created <see cref="Instance" />.</returns>
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
