using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing;
using TypeSystem;
using MEFAnalyzers;

using UnitTesting.TypeSystem_TestUtils;
using UnitTesting.AssemblyProviders_TestUtils;

using UnitTesting.RuntimeTypeDefinitions;

namespace UnitTesting
{
    [TestClass]
    public class MEFAnalyzers_Testing
    {
        [TestMethod]
        public void Compose_StringImport_StringExport()
        {
            AssemblyUtils.Run(@"        
                var partImport=new StringImport();       
                var partExport=new StringExport(""ExportedValue"");

                var test=new CompositionTester(partImport,partExport);   
                var importValue=partImport.Import;                   
            ")

            .AddToRuntime<CompositionTesterDefinition>()
            .AddToRuntime<StringImport>()
            .AddToRuntime<StringExport>()
            .AssertVariable("importValue").HasValue("ExportedValue");
        }
    }
}
