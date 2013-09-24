using System;
using System.Linq;
using System.Collections.Generic;
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

        [TestMethod]
        public void Compose_StringManyArrayImport_StringExport()
        {
            AssemblyUtils.Run(@"        
                var partImport=new ManyStringImport();       
                var partExport=new StringExport(""ExportedValue"");

                var test=new CompositionTester(partImport,partExport);   
                var importValues=partImport.Import;                   
                var result=importValues[0];
            ")

            .AddToRuntime<CompositionTesterDefinition>()
            .AddToRuntime<ManyStringImport>()
            .AddToRuntime<StringExport>()
            .AddWrappedGenericToRuntime(typeof(ICollection<>)) //because composition engine needs it
            .AssertVariable("result").HasValue("ExportedValue");
            ;
        }

        [TestMethod]
        public void Compose_StringManyICollectionImport_StringExport()
        {
            AssemblyUtils.Run(@"        
                var partImport=new ICollectionStringImport();       
                var partExport=new StringExport(""ExportedValue"");

                var test=new CompositionTester(partImport,partExport);   
                var importValues=partImport.Import;                   
                var result=importValues[0];
            ")

           .AddToRuntime<CompositionTesterDefinition>()
           .AddToRuntime<ICollectionStringImport>()
           .AddToRuntime<StringExport>()
           .AddDirectToRuntime<List<string>>()
           .AddDirectToRuntime<ICollection<string>>()
           .AssertVariable("result").HasValue("ExportedValue")

           ;
        }
    }
}
