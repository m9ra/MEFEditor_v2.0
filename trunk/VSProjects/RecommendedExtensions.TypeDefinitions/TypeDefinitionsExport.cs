using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;

using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;
using RecommendedExtensions.Core;
using RecommendedExtensions.Core.Services;
using RecommendedExtensions.Core.TypeDefinitions;
using RecommendedExtensions.Core.Languages.CIL;

namespace RecommendedExtensions.TypeDefinitions
{
    /// <summary>
    /// Exporting class of type definitions that are exposed by <see cref="RecommendedExtensions"/> to
    /// provide MEF analyzing support.
    /// </summary>
    [Export(typeof(ExtensionExport))]
    public class TypeDefinitionsExport : ExtensionExport
    {
        /// <summary>
        /// Register type definitions of <see cref="RecommendedExtensions"/>.
        /// </summary>
        protected override void Register()
        {
            Message("Exporting Recommended TypeDefinitions");

            //.NET Essentials
            exportTypes(new[]{
                typeof(object),
                typeof(CILInstruction),
                typeof(VMStack),
                typeof(LiteralType),
                typeof(Lazy<>),
                typeof(Lazy<,>),
                typeof(List<>),
                typeof(Dictionary<,>),
                typeof(ICollection<>),
                typeof(IEnumerable<>),
                typeof(IEnumerator<>),
                typeof(System.Collections.IEnumerator)
            }, new[]{
                typeof(char),
                typeof(string),
                typeof(bool), 
                typeof(byte),
                typeof(short),        
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(double),
                typeof(float)
            });

            //MEF Essentials
            ExportDefinition<DirectoryCatalogDefinition>();
            ExportDefinition<CompositionContainerDefinition>();
            ExportDefinition<CompositionBatchDefinition>();
            ExportDefinition<AggregateCatalogDefinition>();
            ExportDefinition<AssemblyCatalogDefinition>();
            ExportDefinition<AssemblyDefinition>();
            ExportDefinition<TypeCatalogDefinition>();
            ExportDefinition<AttributedModelServicesDefinition>();
            ExportDefinition<ComposablePartCatalogCollectionDefinition>();

            //Support for Assembly providers
            ExportDefinition<ObjectDefinition>();
            ExportDefinition<TypeDefinition>();

            //TODO this is workaround
            ExportDefinition<ConsoleDefinition>();
        }

        /// <summary>
        /// Exports the types.
        /// </summary>
        /// <param name="directTypes">The direct types.</param>
        /// <param name="mathTypes">The math types.</param>
        private void exportTypes(Type[] directTypes, Type[] mathTypes)
        {
            foreach (var directType in directTypes)
            {
                exportDirectType(directType);
            }

            foreach (var mathType in mathTypes)
            {
                exportDirectMathType(mathType);
            }
        }

        /// <summary>
        /// Exports the type as <see cref="DirectTypeDefinition"/>.
        /// </summary>
        /// <param name="directType">Direct type.</param>
        private void exportDirectType(Type directType)
        {
            var typeDefinition = new DirectTypeDefinition(directType);
            ExportDefinition(typeDefinition);
        }

        /// <summary>
        /// Exports the type as <see cref="DirectTypeDefinition"/> with
        /// native support of math operators.
        /// </summary>
        /// <param name="mathType">Type of the math.</param>
        private void exportDirectMathType(Type mathType)
        {
            var type = typeof(MathDirectType<>).MakeGenericType(mathType);

            var typeDefinition = Activator.CreateInstance(type) as DirectTypeDefinition;
            ExportDefinition(typeDefinition);
        }
    }
}
