using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing.Execution.Instructions;
using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;

using Analyzing;
using TypeSystem;
using AssemblyProviders.CSharp;

namespace UnitTesting
{
    [TestClass]
    public class Parsing_Testing
    {
        [TestMethod]
        public void BasicParsing()
        {
            var parser = new SyntaxParser();
            var result = parser.Parse(@"{
var test=System.String.test;
var test2=System.String.test();
}
");
        }


        [TestMethod]
        public void AssemblyLoading()
        {
            var parsedAssembly = new ParsedAssembly();
            parsedAssembly.AddMethod("StartMethod", @"{
var test=""hello"";
var test2=test;
}
");

            var testAssemblies = new TestAssemblyCollection(parsedAssembly);
            
                        

            var loader = new AssemblyLoader(testAssemblies);
            var entryLoader = new EntryPointLoader(
                new VersionedName("StartMethod",0)
                ,loader);
            

            var machine = new Machine<MethodID, InstanceInfo>(new MachineSettings());

            machine.Run(entryLoader);
        }

    }
}
