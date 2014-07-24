using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;

using TypeSystem;
using TypeSystem.Runtime;
using MEFAnalyzers;
using AssemblyProviders;
using AssemblyProviders.DirectDefinitions;
using AssemblyProviders.CIL;
using AssemblyProviders.CSharp;

namespace RecommendedExtensions.TypeDefinitions
{
    [Export(typeof(ExtensionExport))]
    public class TypeDefinitionsExport : ExtensionExport
    {
        protected override void Register()
        {
            Message("Exporting Recommended TypeDefinitions");

            //.NET Essentials
            exportTypes(new[]{
                typeof(char),
                typeof(string),
                typeof(bool), 
                typeof(byte),
                typeof(VMStack),
                typeof(NullLiteral),
                typeof(LiteralType),
                typeof(Lazy<>),
                typeof(Lazy<,>),
            }, new[]{
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
            ExportDefinition<TypeCatalogDefinition>();
            ExportDefinition<AttributedModelServicesDefinition>();
            ExportDefinition<ComposablePartCatalogCollectionDefinition>();

            //Support for Assembly providers
            ExportAsDirectDefinition<ICollection<InstanceWrap>>();
            ExportDefinition<ObjectDefinition>();
            ExportDefinition<TypeDefinition>();

            //TODO this is workaround
            ExportDefinition<ConsoleDefinition>();
        }

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

        private void exportDirectType(Type directType)
        {
            var typeDefinition = new DirectTypeDefinition(directType);
            ExportDefinition(typeDefinition);
        }

        private void exportDirectMathType(Type mathType)
        {
            var type = typeof(MathDirectType<>).MakeGenericType(mathType);

            var typeDefinition = Activator.CreateInstance(type) as DirectTypeDefinition;
            ExportDefinition(typeDefinition);
        }
    }
}
