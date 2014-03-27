using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnitTesting;
using UnitTesting.RuntimeTypeDefinitions;
using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;

using TypeSystem;
using TypeSystem.Runtime;

using Analyzing;
using Analyzing.Execution;
using Analyzing.Editing;

using MEFEditor;
using MEFAnalyzers;

using AssemblyProviders.CILAssembly;
using AssemblyProviders.CIL.Providing;

namespace Research
{
    static class ResearchSources
    {

        static internal TestingAssembly CECIL_Components()
        {
            var cilAssembly = new CILAssembly("Research.exe");

            return AssemblyUtils.Run(@"        
                var assemCat=new System.ComponentModel.Composition.Hosting.AssemblyCatalog(""" + cilAssembly.FullPath + @""");                  
            ")

           .AddMethod("System.Object." + Naming.CtorName, (c) => { }, Method.Ctor_NoParam)

           .AddToRuntime<CompositionContainerDefinition>()
           .AddToRuntime<DirectoryCatalogDefinition>()
           .AddToRuntime<AggregateCatalogDefinition>()
           .AddToRuntime<TypeCatalogDefinition>()
           .AddToRuntime<AssemblyCatalogDefinition>()
           .AddToRuntime<ComposablePartCatalogCollectionDefinition>()
           .AddToRuntime<SimpleStringExport>()
           .AddToRuntime<StringImport>()
           .AddWrappedGenericToRuntime(typeof(List<>))

           .AddAssembly(cilAssembly);
        }

        static internal TestingAssembly CECIL_GeneriInterfaceResolving()
        {
            var cilAssembly = new CILAssembly("Research.exe");
            //return AssemblyUtils.RunCECIL("Research.exe", "CecilTestSources.RunSimpleGenericTest")
            return AssemblyUtils.RunCECIL("Research.exe", "CecilTestSources.RunGenericIfaceTest")
                .AddMethod("System.Object." + Naming.CtorName, (c) => { }, Method.Ctor_NoParam)
                .AddWrappedGenericToRuntime(typeof(List<>))
                .AddAssembly(cilAssembly);
        }

        static internal TestingAssembly CECIL_InterfaceResolving()
        {
            var cilAssembly = new CILAssembly("Research.exe");
            return AssemblyUtils.RunCECIL("Research.exe", "CecilTestSources.RunIfaceTest")
                .AddMethod("System.Object." + Naming.CtorName, (c) => { }, Method.Ctor_NoParam)
                .AddAssembly(cilAssembly);
        }

        static internal TestingAssembly MEF_Demo()
        {
            var testAssembly = new RuntimeAssembly("test.exe");
            testAssembly.AddDefinition(new StringImport());

            return AssemblyUtils.Run(@"        
                var dirCat=new System.ComponentModel.Composition.Hosting.DirectoryCatalog(""."");       
                var assemCat=new System.ComponentModel.Composition.Hosting.AssemblyCatalog(""test.exe"");
                var compCont=new System.ComponentModel.Composition.Hosting.CompositionContainer(dirCat);
            
                var export=new SimpleStringExport();
                
                compCont.ComposeParts();
   
            ")

            .AddMethod("System.Object." + Naming.CtorName, (c) => { }, Method.Ctor_NoParam)

            .AddToRuntime<CompositionContainerDefinition>()
            .AddToRuntime<DirectoryCatalogDefinition>()
            .AddToRuntime<AggregateCatalogDefinition>()
            .AddToRuntime<TypeCatalogDefinition>()
            .AddToRuntime<AssemblyCatalogDefinition>()
            .AddToRuntime<ComposablePartCatalogCollectionDefinition>()
            .AddToRuntime<SimpleStringExport>()
            .AddToRuntime<StringImport>()
            .AddWrappedGenericToRuntime(typeof(List<>))

            .AddAssembly(testAssembly)
            ;
        }

        static internal TestingAssembly CrossInterpreting_Simple()
        {
            return AssemblyUtils.Run(@"
                var dirCat=new System.ComponentModel.Composition.Hosting.DirectoryCatalog(""test.exe"");   
            ")

             .AddMethod("System.Object." + Naming.CtorName, (c) => { }, Method.Ctor_NoParam)
             .AddToRuntime<DirectoryCatalogDefinition>();
        }

        static internal TestingAssembly CECIL_CompositionPoint()
        {
            var assembly = new CILAssembly("Research.exe");


            return AssemblyUtils.Run(@"
                var c=new CecilComponent();
            ")

             .AddMethod("System.Object." + Naming.CtorName, (c) => { }, Method.Ctor_NoParam)
             .AddToRuntime<DirectoryCatalogDefinition>()
             .AddToRuntime<AssemblyCatalogDefinition>()
             .AddAssembly(assembly)

             ;
        }

        static internal TestingAssembly MEF_AssemblyCatalog()
        {
            var testAssembly = new RuntimeAssembly("test.exe");
            testAssembly.AddDefinition(new SimpleStringExport());
            testAssembly.AddDefinition(new ICollectionStringImport());

            return AssemblyUtils.Run(@"        
                var assemCat=new System.ComponentModel.Composition.Hosting.AssemblyCatalog(""test.exe"");       
                var compCont=new System.ComponentModel.Composition.Hosting.CompositionContainer(assemCat);
            
                var export=new SimpleStringExport();
                
                compCont.ComposeParts(export);
   
            ")
            .AddToRuntime<AssemblyCatalogDefinition>()
            .AddToRuntime<CompositionContainerDefinition>()
            .AddToRuntime<SimpleStringExport>() //because of drawing provider
            .AddToRuntime<ICollectionStringImport>()
            .AddDirectToRuntime<ICollection<string>>()
            .AddDirectToRuntime<List<string>>()

            .AddAssembly(testAssembly)
            ;
        }

        static internal TestingAssembly MEF_TypeCatalog()
        {
            return AssemblyUtils.Run(@"                        
                var exportType=typeof(SimpleStringExport);
                var typeCat=new System.ComponentModel.Composition.Hosting.TypeCatalog(exportType);       

                var compCont=new System.ComponentModel.Composition.Hosting.CompositionContainer(typeCat);            
                compCont.ComposeParts();
            ")
            .AddToRuntime<CompositionContainerDefinition>()
            .AddToRuntime<TypeCatalogDefinition>()
            .AddToRuntime<SimpleStringExport>()
            .AddToRuntime<StringImport>()
            ;
        }

        static internal TestingAssembly MEF_CompositionErrors()
        {
            var testAssembly = new RuntimeAssembly("test.exe");
            testAssembly.AddDefinition(new StringImport());

            return AssemblyUtils.Run(@"        
                var dirCat=new System.ComponentModel.Composition.Hosting.DirectoryCatalog(""test.exe"");       
                var compCont=new System.ComponentModel.Composition.Hosting.CompositionContainer(dirCat);
            
                var export=new SimpleStringExport();
                
                compCont.ComposeParts();
   
            ")

            .AddToRuntime<CompositionContainerDefinition>()
            .AddToRuntime<DirectoryCatalogDefinition>()
            .AddToRuntime<SimpleStringExport>()
            .AddToRuntime<StringImport>()

            .AddAssembly(testAssembly)
            ;
        }

        static internal TestingAssembly ParamTesting()
        {
            return AssemblyUtils.Run(@"
                var formated=System.String.Format(""{0}{1}{2}"",""a"",""b"",""c"");               
            ");
        }

        static internal TestingAssembly MEF_AggregateCatalog()
        {
            var testAssembly = new RuntimeAssembly("test.exe");
            testAssembly.AddDefinition(new StringImport());

            var testAssembly2 = new RuntimeAssembly("test2.exe");
            testAssembly2.AddDefinition(new SimpleStringExport());

            return AssemblyUtils.Run(@"                        
                var aggrCat=new System.ComponentModel.Composition.Hosting.AggregateCatalog();       
                var aggrCat2=new System.ComponentModel.Composition.Hosting.AggregateCatalog(); 
                var dirCat=new System.ComponentModel.Composition.Hosting.DirectoryCatalog(""test.exe"");      
                var dirCat2=new System.ComponentModel.Composition.Hosting.DirectoryCatalog(""test2.exe"");    

                aggrCat.Catalogs.Add(aggrCat2);
                aggrCat.Catalogs.Add(dirCat);
                
                aggrCat2.Catalogs.Add(dirCat2);
                var compCont=new System.ComponentModel.Composition.Hosting.CompositionContainer(aggrCat);
                compCont.ComposeParts();
            ")
            .AddToRuntime<AggregateCatalogDefinition>()
            .AddToRuntime<DirectoryCatalogDefinition>()
            .AddToRuntime<CompositionContainerDefinition>()
            .AddToRuntime<ComposablePartCatalogCollectionDefinition>()
            .AddToRuntime<StringImport>()
            .AddToRuntime<SimpleStringExport>()

            .AddAssembly(testAssembly)
            .AddAssembly(testAssembly2)
            ;
        }

        static internal TestingAssembly MEF_DirectoryCatalog()
        {
            var testAssembly = new RuntimeAssembly("test.exe");
            testAssembly.AddDefinition(new SimpleStringExport());
            testAssembly.AddDefinition(new ICollectionStringImport());

            return AssemblyUtils.Run(@"        
                var dirCat=new System.ComponentModel.Composition.Hosting.DirectoryCatalog(""test.exe"");       
                var compCont=new System.ComponentModel.Composition.Hosting.CompositionContainer(dirCat);
            
                var export=new SimpleStringExport();
                var export2=new SimpleStringExport();
                
                compCont.ComposeParts(export);
   
            ")
            .AddToRuntime<AggregateCatalogDefinition>()
            .AddToRuntime<ComposablePartCatalogCollectionDefinition>()
            .AddToRuntime<DirectoryCatalogDefinition>()
            .AddToRuntime<CompositionContainerDefinition>()
            .AddToRuntime<SimpleStringExport>() //because of drawing provider
            .AddToRuntime<ICollectionStringImport>()
            .AddDirectToRuntime<ICollection<string>>()
            .AddDirectToRuntime<List<string>>()

            .AddAssembly(testAssembly)
            ;


        }

        static internal TestingAssembly CECIL_AssemblyProviding()
        {
            var cilAssembly = new CILAssembly("Research.exe");
            return AssemblyUtils.RunCECIL("Research.exe", "CecilTestSources.CrossStart")
                .AddAssembly(cilAssembly);
        }

        [CompositionPoint]
        static internal TestingAssembly CECIL_ForLoop()
        {
            return AssemblyUtils.RunCECIL("Research.exe", "CecilTestSources.ForLoop");
        }

        static internal TestingAssembly CIL_ForLoop()
        {
            return AssemblyUtils.RunCIL(() =>
            {
                string str = "";
                for (int i = 0; i < 10; ++i)
                {
                    str += "a";
                }

                return str;
            });
        }

        static internal TestingAssembly CIL_ObjectCall()
        {
            return AssemblyUtils.RunCIL(() =>
            {
                var x1 = "ABCD";

                return x1.Substring(2);
            });
        }

        static internal TestingAssembly CIL_StaticCall()
        {
            return AssemblyUtils.RunCIL(() =>
            {
                var x1 = "A";
                var x2 = "B";
                string.Concat(x1, x2);
            });
        }

        static internal TestingAssembly CIL_HelloWorld()
        {
            return AssemblyUtils.RunCIL(() =>
            {
                var x = "hello CIL world";
                return x;
            });
        }

        static internal TestingAssembly DrawingTester_TwoContainers()
        {
            return AssemblyUtils.Run(@"        
                var partImport=new StringImport();       
                                             
                var test=new CompositionTester();   
                test.Add(partImport);
                
                var test2=new CompositionTester();
                
                test.Compose();
                test2.Compose();
            ")

           .AddToRuntime<CompositionTesterDefinition>()
           .AddToRuntime<StringImport>()
           .AddToRuntime<StringExport>()
           .AddToRuntime<SimpleType>()
           .AddDirectToRuntime<List<string>>()
           .AddDirectToRuntime<ICollection<string>>()
           ;
        }

        static internal TestingAssembly DrawingTester_BoundsCheck()
        {
            return AssemblyUtils.Run(@"        
                var partImport=new StringImport();       
                                             
                var test=new CompositionTester();   
                test.Add(partImport);
                test.Compose();
            ")

           .AddToRuntime<CompositionTesterDefinition>()
           .AddToRuntime<StringImport>()
           .AddToRuntime<StringExport>()
           .AddToRuntime<SimpleType>()
           .AddDirectToRuntime<List<string>>()
           .AddDirectToRuntime<ICollection<string>>()
           ;
        }

        static internal TestingAssembly DrawingTester_SingleComponent()
        {
            return AssemblyUtils.Run(@"        
                var partImport=new StringImport();                    
            ")

            .AddToRuntime<StringImport>()
         ;
        }

        static internal TestingAssembly DrawingTester_SinlgeJoin()
        {
            return AssemblyUtils.Run(@"        
                var partImport=new StringImport();       
                var partExport=new StringExport(""Export1"");
                              
                var test=new CompositionTester();   
                test.Add(partImport);
                test.Add(partExport);
                var empty=new SimpleType(""nothing"");
                test.Add(empty);
                var empty2=new SimpleType(""nothing"");
                test.Add(empty2);
                var empty3=new SimpleType(""nothing"");
                test.Add(empty3);
                test.Compose();
                
            ")

           .AddToRuntime<CompositionTesterDefinition>()
           .AddToRuntime<StringImport>()
           .AddToRuntime<StringExport>()
           .AddToRuntime<SimpleType>()
           .AddDirectToRuntime<List<string>>()
           .AddDirectToRuntime<ICollection<string>>()
           ;
        }

        static internal TestingAssembly CompositionTester_LoadAssembly()
        {
            var testAssembly = new RuntimeAssembly("test.exe");
            testAssembly.AddDefinition(new StringExport());

            return AssemblyUtils.Run(@"        
                var partExport=new StringExport(""PastedExport"");       
                var partImport=new ICollectionStringImport();
                
                var test=new CompositionTester(""test.exe"");   
                test.Add(partExport);
                test.Add(partImport);
                test.Compose();
                
                var import=partImport.Import;
                var result1=import[0];
                var result2=import[1];          
            ")

            .AddToRuntime<CompositionTesterDefinition>()
            .AddToRuntime<StringExport>()
            .AddToRuntime<ICollectionStringImport>()
            .AddDirectToRuntime<List<string>>()
            .AddDirectToRuntime<ICollection<string>>()
            .AddAssembly(testAssembly)
            ;
        }

        static internal TestingAssembly GenericInterfaceCall()
        {
            return AssemblyUtils.Run(@"
                var list=new System.Collections.Generic.List<string>();               
                var interface=Convert(list);
                interface.Add(""AddedValue"");
                var result=list[0];
            ")

            .AddMethod("Test.Convert", (c) =>
            {
                c.Return(c.CurrentArguments[1]);
            }, Method.StringICollection_StringICollectionParam)

            .AddWrappedGenericToRuntime(typeof(List<>))
            .AddWrappedGenericToRuntime(typeof(ICollection<>))
            ;
        }


        static internal TestingAssembly InterfaceCall()
        {
            return AssemblyUtils.Run(@"
                var test=new Test();
                
                var interface=Convert(test);
                interface.Add(""AddedValue"");
                var result=test.Get();
            ")

            .AddMethod("Test.Convert", (c) =>
            {
                c.Return(c.CurrentArguments[1]);
            }, Method.StringICollection_StringICollectionParam)

            .AddMethod("Test.#ctor", (c) =>
            {
            }, Method.Ctor_NoParam)

            .AddMethod("Test.Add", (c) =>
            {
                var thisInstance = c.CurrentArguments[0];
                var arg = c.CurrentArguments[1];
                c.SetField(thisInstance, "data", arg);
            }, Method.Void_StringParam.Implements(typeof(ICollection<string>)))

            .AddMethod("Test.Get", (c) =>
            {
                var thisInstance = c.CurrentArguments[0];
                var inst = c.GetField(thisInstance, "data") as Instance;
                c.Return(inst);
            }, Method.String_NoParam)

            .AddWrappedGenericToRuntime(typeof(ICollection<>))
            ;
        }

        static internal TestingAssembly CompositionTester_ManyImport()
        {
            return AssemblyUtils.Run(@"        
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
            ;
        }

        static internal TestingAssembly CompositionTester_ManyStringImport()
        {
            return AssemblyUtils.Run(@"        
                var partImport=new ManyStringImport();       
                var partExport=new StringExport(""ExportedValue"");

                var test=new System.ComponentModel.Composition.Hosting.CompositionContainer();
                test.ComposeParts(partImport,partExport);   
                var importValues=partImport.Import;                   
                var result=importValues[0];
            ")

            .AddToRuntime<CompositionContainerDefinition>()
            .AddToRuntime<ManyStringImport>()
            .AddToRuntime<StringExport>()
            .AddWrappedGenericToRuntime(typeof(ICollection<>)) //because composition engine needs it
            ;
        }

        static internal TestingAssembly ArrayTesting()
        {
            return AssemblyUtils.Run(@"                
                   var list=new System.Collections.Generic.List<System.String>();
                   list.Add(""Item0"");                   
                   var arr=list.ToArray();
                   
                   var result=arr[0];
               ")

               .AddWrappedGenericToRuntime(typeof(List<>))
            ;
        }

        static internal TestingAssembly GenericTesting()
        {
            return AssemblyUtils.Run(@"                
                   var list=new System.Collections.Generic.Dictionary<System.String,System.Int32>();     
                   list.Add(""key"", 1234);

                   var result=list[""key""];
               ")

            .AddWrappedGenericToRuntime(typeof(Dictionary<,>))

            ;
        }

        static internal TestingAssembly GenericMethodTesting()
        {
            return AssemblyUtils.Run(@"                
                   var cls=new GenericClass<System.Int32>();     
                   var result = cls.GenericMethod(""aa"");
               ")

            .AddWrappedGenericToRuntime(typeof(GenericClass<>))

            ;
        }

        static internal TestingAssembly GenericMethodDefinition()
        {
            return AssemblyUtils.Run(@"                
                var test=new Test();     
                var result=test.Generic<Test2>(""GenericCallArg"");
            ")

            .AddMethod("Test." + Naming.CtorName, @"
                
                    ", Method.Ctor_NoParam)

            .AddMethod("Test.Generic<T1>", @"
                        var x=new T1(p);
                        return x.GetValue();
                    ", Method.Void_StringParam)

            .AddMethod("Test2." + Naming.CtorName, (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var arg = c.CurrentArguments[1];
                c.SetField(thisObj, "value", arg.DirectValue);
            }, Method.Ctor_StringParam)

            .AddMethod("Test2.GetValue", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var value = c.GetField(thisObj, "value") as string;
                var result = c.Machine.CreateDirectInstance("Test2_" + value, TypeDescriptor.Create<string>());
                c.Return(result);
            }, Method.String_NoParam);
        }


        static internal TestingAssembly ExplicitGenericTesting()
        {
            return AssemblyUtils.Run(@"                
                   var list=new System.Collections.Generic.List<System.String>();     
                   list.Add(""test"");

                   var result=list[0];
               ")

            .AddDirectToRuntime<List<string>>()

            ;
        }


        static internal TestingAssembly ComplexDirectRuntime()
        {
            return AssemblyUtils.Run(@"                
                var test=new System.Text.StringBuilder();
                test.Append(""Data"");
                test.Append(""2"");
                var result=test.ToString();      
            ")

            .AddDirectToRuntime<StringBuilder>()

            ;
        }

        static internal TestingAssembly CompositionTester()
        {
            return AssemblyUtils.Run(@"        
                var partImport=new StringImport();       
                var partExport=new StringExport(""ExportedValue"");

                var test=new CompositionTester(partImport,partExport);   
                var importValue=partImport.Import;                   
            ")

            .AddToRuntime<CompositionTesterDefinition>()
            .AddToRuntime<StringImport>()
            .AddToRuntime<StringExport>()
            ;
        }

        static internal TestingAssembly RuntimeCall_Default()
        {
            return AssemblyUtils.Run(@"                
                var test=new SimpleType(""CtorValue"");      
                var result=test.Concat();      
            ")

           .AddToRuntime<SimpleType>()

           ;
        }

        static internal TestingAssembly InstanceRemoving()
        {
            return AssemblyUtils.Run(@"
                var toDelete=""toDelete"";                
                CallWithOptional(CallWithRequired(toDelete));           
                var x=1;
                var y=2;
                if(x<y){
                    toDelete=""smaller"";
                }else{
                    toDelete=""greater"";
                }
            ")

         .AddMethod("Test.CallWithOptional", (c) =>
         {
             var arg = c.CurrentArguments[1];
             c.Edits.SetOptional(1);
             c.Return(arg);
         }, Method.String_StringParam)

         .AddMethod("Test.CallWithRequired", (c) =>
         {
             var arg = c.CurrentArguments[1];
             c.Return(arg);
         }, Method.String_StringParam)

         .RunRemoveAction("toDelete")

         ;
        }

        static internal TestingAssembly StaticCall()
        {
            return AssemblyUtils.Run(@"
                var test=StaticClass.StaticMethod(""CallArg"");
            ")

            .AddMethod("StaticClass.StaticMethod", (c) =>
            {
                var self = c.CurrentArguments[0];
                var arg = c.CurrentArguments[1].DirectValue as string;
                var field = c.GetField(self, "StaticField");

                var result = c.Machine.CreateDirectInstance(field + "_" + arg);
                c.Return(result);
            }, Method.StaticString_StringParam)

            .AddMethod("StaticClass.#initializer", (c) =>
            {
                var self = c.CurrentArguments[0];
                c.SetField(self, "StaticField", "InitValue");
            }, Method.StaticInitializer)

            ;
        }

        static internal TestingAssembly FieldUsage()
        {
            return AssemblyUtils.Run(@"
                var obj=new TestObj(""input"");
                
                var result = obj.GetInput();          
            ")

            .AddMethod("TestObj.#ctor", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var arg = c.CurrentArguments[1];
                c.SetField(thisObj, "inputData", arg);

            }, Method.Ctor_StringParam)

            .AddMethod("TestObj.GetInput", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var data = c.GetField(thisObj, "inputData") as Instance;
                c.Return(data);
            }, Method.String_NoParam)


            ;
        }

        static object acceptInstance(EditsProvider edits, ExecutionView services)
        {
            var variable = edits.GetVariableFor(AssemblyUtils.EXTERNAL_INPUT, services);
            if (variable == null)
            {
                return services.Abort("Cannot get variable for instance");
            }

            return variable;
        }

        static internal TestingAssembly Operators()
        {
            return AssemblyUtils.Run(@"
                var inc=1;
                ++inc;
                inc++;
                var post=inc++;
                var pref=++inc;
            ");
        }

        static internal TestingAssembly Fibonacci(int n)
        {
            return AssemblyUtils.Run(@"
                var result=fib(" + n + @");
            ")

            .AddMethod("Test.fib", @"    
                if(n<3){
                    return 1;
                }else{
                    return fib(n-1)+fib(n-2);
                }
            ", Method.Int_IntParam);
        }
    }
}
