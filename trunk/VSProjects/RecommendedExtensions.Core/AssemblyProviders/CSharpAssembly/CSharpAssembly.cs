using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

using EnvDTE;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;
using MEFEditor.Interoperability;

using RecommendedExtensions.Core.Languages.CSharp;
using RecommendedExtensions.Core.Languages.CSharp.Compiling;
using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly;
using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.MethodBuilding;

namespace RecommendedExtensions.Core.AssemblyProviders.CSharpAssembly
{
    /// <summary>
    /// Concrete implementation of <see cref="VsProjectAssembly"/> for loading opened C# projects
    /// within Visual Studio
    /// </summary>
    public class CSharpAssembly : VsProjectAssembly
    {
        /// <summary>
        /// Initialize new instance of assembly provider for C# projects within Visual Studio
        /// </summary>
        /// <param name="project">Project which assembly will be provided</param>
        /// <param name="visualStudioServices">Services for communicating with Visual Studio</param>
        public CSharpAssembly(Project project, VisualStudioServices visualStudioServices)
            : base(project, visualStudioServices,
            new CSharpNames(), new CSharpMethodInfoBuilder(), new CSharpMethodBuilder())
        {

        }

        /// <inheritdoc />
        public override string TranslatePath(string path)
        {
            var translated = TypeDescriptor.TranslatePath(path, (toResolve) =>
            {
                string result;
                if (CompilationContext.AliasLookup.TryGetValue(toResolve, out result))
                    return result;
                return toResolve;
            }, true);

            return translated;
        }

        #region CSharp parsing

        /// <inheritdoc />
        public override void ParsingProvider(ParsingActivation activation, EmitterBase emitter)
        {
            var w = Stopwatch.StartNew();

            var source = Compiler.GenerateInstructions(activation, emitter, TypeServices);

            var methodID = activation.Method == null ? new MethodID("$inline", false) : activation.Method.MethodID;
            VS.Log.Message("Parsing time for {0} {1}ms", methodID, w.ElapsedMilliseconds);
        }

        /// <inheritdoc />
        public override object ParseValue(string valueRepresentation, TypeDescriptor contextType, CodeElement contextElement)
        {
            object value;
            if (Compiler.TryParseValue(valueRepresentation, out value))
                return value;

            return parseType(ref valueRepresentation, contextType, contextElement);
        }
        
        /// <summary>
        /// Parses typeof representation.
        /// </summary>
        /// <param name="valueRepresentation">The value representation.</param>
        /// <param name="contextType">Type of the context.</param>
        /// <param name="contextElement">The context element.</param>
        /// <returns>Descriptor of type if available, <c>null</c> otherwise.</returns>
        private TypeDescriptor parseType(ref string valueRepresentation, TypeDescriptor contextType, CodeElement contextElement)
        {
            var typePrefix = "typeof(";
            if (valueRepresentation.StartsWith(typePrefix))
            {
                valueRepresentation = valueRepresentation.Substring(typePrefix.Length).Replace(")", "");
                valueRepresentation = TranslatePath(valueRepresentation);

                //find  type

                var implicitNamespaces = GetImplicitNamespaces(contextType);

                var namespaces = implicitNamespaces.Concat(GetNamespaces(contextElement));
                foreach (var ns in namespaces)
                {
                    var prefix = ns == "" ? "" : ns + ".";

                    var descriptor = TypeDescriptor.Create(prefix + valueRepresentation);
                    if (TypeServices.GetChain(descriptor) != null)
                        return descriptor;
                }
            }

            return null;
        }

        #endregion

    }
}
