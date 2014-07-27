using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;
using EnvDTE80;

using MEFEditor.TypeSystem;

using RecommendedExtensions.Core.Languages.CSharp;
using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly;
using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.MethodBuilding;

namespace RecommendedExtensions.Core.AssemblyProviders.CSharpAssembly
{
    /// <summary>
    /// Method info builder enhanced for C# - it added support for class member initializers 
    /// and indexers.
    /// </summary>
    public class CSharpMethodInfoBuilder : MethodInfoBuilder
    {
        /// <summary>
        /// Creates <see cref="TypeMethodInfo"/> for given element
        /// </summary>
        /// <param name="element">Element which <see cref="TypeMethodInfo"/> is created</param>
        /// <returns>Created <see cref="TypeMethodInfo"/></returns>
        protected TypeMethodInfo BuildFrom(CodeClass2 element)
        {
            var declaringType = CreateDescriptor(element);

            var methodInfo = new TypeMethodInfo(
                declaringType, RequiredName, TypeDescriptor.Void, ParameterTypeInfo.NoParams,
                false, TypeDescriptor.NoDescriptors, false
                );

            return methodInfo;
        }

        /// <inheritdoc />
        public override void VisitClass(CodeClass2 e)
        {
            Result(BuildFrom(e));
        }
    }
}
