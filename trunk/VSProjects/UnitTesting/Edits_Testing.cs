using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing.Execution.Instructions;
using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;
using UnitTesting.RuntimeTypeDefinitions;

using Analyzing;
using Analyzing.Execution;
using Analyzing.Editing;

using TypeSystem;
using MEFAnalyzers;
using AssemblyProviders.CSharp;
using AssemblyProviders.CSharp.Compiling;
using AssemblyProviders.DirectDefinitions;


using UnitTesting.AssemblyProviders_TestUtils;

namespace UnitTesting
{
    /// <summary>
    /// Testing of IAL Edits supported by C# Compiler from Recommended Extensions.
    /// </summary>
    [TestClass]
    public class Edits_Testing
    {
        [TestMethod]
        public void Edit_SimpleReject()
        {
            AssemblyUtils.Run(@"
                var arg=""input"";
                DirectMethod(arg);
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var arg = c.CurrentArguments[1];
                c.Edits.RemoveArgument(arg, 1, ".reject");
                c.Edits.SetOptional(1);
            }, Method.Void_StringParam)

            .RunEditAction("arg", ".reject")
            .AssertSourceEquivalence(@"
                var arg=""input"";
                DirectMethod();
            ");
        }

        [TestMethod]
        public void Edit_RejectWithSideEffect()
        {
            AssemblyUtils.Run(@"
                var arg=""input"";
                DirectMethod(arg=""input2"");
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var arg = c.CurrentArguments[1];
                c.Edits.RemoveArgument(arg, 1, ".reject");
                c.Edits.SetOptional(1);
            }, Method.Void_StringParam)

            .RunEditAction("arg", ".reject")
            .AssertSourceEquivalence(@"
                var arg=""input"";
                arg=""input2"";
                DirectMethod();
            ");
        }


        [TestMethod]
        public void Edit_RewriteWithSideEffect()
        {
            AssemblyUtils.Run(@"
                var arg=""input"";
                DirectMethod(arg=""input2"");
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var arg = c.CurrentArguments[1];
                c.Edits.ChangeArgument(arg, 1, "Change", (s) => "input3");
            }, Method.Void_StringParam)

            .RunEditAction("arg", "Change")
            .AssertSourceEquivalence(@"
                var arg=""input"";
                arg=""input2"";
                DirectMethod(""input3"");
            ");
        }

        [TestMethod]
        public void Edit_AppendWithValidScope()
        {
            AssemblyUtils.Run(@"
                var arg=""input"";
                DirectMethod(""input2"");
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var thisInst = c.CurrentArguments[0];
                var e = c.Edits;
                c.Edits.AppendArgument(thisInst, 2, "Append", (s) => e.GetVariableFor(AssemblyUtils.EXTERNAL_INPUT, s));
            }, Method.Void_StringParam)

            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = c.EntryContext.GetValue(new VariableName("arg"));
            })

            .RunEditAction("this", "Append")

            .AssertSourceEquivalence(@"
                var arg=""input"";
                DirectMethod(""input2"",arg);
            ");
        }

        [TestMethod]
        public void Edit_AppendScopeEndShifting()
        {
            AssemblyUtils.Run(@"
                var arg=""input"";
                Report(arg);

                arg=""scope end"";
                arg=""tight scope end"";
                var arg2=""spliting line"";
                arg=""another scope end"";

                DirectMethod(""input2"");             
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var thisInst = c.CurrentArguments[0];
                var e = c.Edits;
                e.AppendArgument(thisInst, 2, "Append", (s) => e.GetVariableFor(AssemblyUtils.EXTERNAL_INPUT, s));

            }, Method.Void_StringParam)


            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = AssemblyUtils.REPORTED_INSTANCE;
            })

            .RunEditAction("this", "Append")

            .AssertSourceEquivalence(@"
                var arg=""input"";
                Report(arg);

                var arg2=""spliting line"";
                DirectMethod(""input2"",arg);  

                arg=""scope end"";
                arg=""tight scope end"";
                arg=""another scope end"";
            ");
        }

        [TestMethod]
        public void Edit_AppendScopeStartShifting()
        {
            AssemblyUtils.Run(@"
                DirectMethod(""input2"");             
                var arg2=""spliting line"";

                var arg=""input"";
                Report(arg);
                              
                arg=""scope end"";
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var thisInst = c.CurrentArguments[0];
                var e = c.Edits;
                e.AppendArgument(thisInst, 2, "Append", (s) => e.GetVariableFor(AssemblyUtils.EXTERNAL_INPUT, s));

            }, Method.Void_StringParam)


            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = AssemblyUtils.REPORTED_INSTANCE;
            })

            .RunEditAction("this", "Append")

            .AssertSourceEquivalence(@"
                var arg2=""spliting line"";

                var arg=""input"";
                DirectMethod(""input2"",arg);                  
                
                Report(arg);
                arg=""scope end"";
            ");
        }

        [TestMethod]
        public void Edit_AcceptWithValidScope()
        {
            AssemblyUtils.Run(@"
                var accepter=""accepter"";
                AddAccept(accepter);             

                var toAccept=""accepted"";
                Report(toAccept);
            ")

            .AddMethod("Test.AddAccept", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var accepter = c.CurrentArguments[1];
                var e = c.Edits;

                e.AddCall(thisObj, "Accept", (view) =>
                {
                    return new CallEditInfo(accepter, "Accept", AssemblyUtils.EXTERNAL_INPUT);
                });

            }, Method.Void_StringParam)


            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = AssemblyUtils.REPORTED_INSTANCE;
            })

            .RunEditAction("this", "Accept")

            .AssertSourceEquivalence(@"
                var accepter=""accepter"";
                AddAccept(accepter);             

                var toAccept=""accepted"";
                accepter.Accept(toAccept);
                Report(toAccept);
            ");
        }

        [TestMethod]
        public void Edit_AcceptAtSemanticEnd_ScopeBlock()
        {
            AssemblyUtils.Run(@"
                var container=new System.ComponentModel.Composition.Hosting.CompositionContainer();

                var toAccept=new System.ComponentModel.Composition.Hosting.AggregateCatalog();
                toAccept.Catalogs.Add(new System.ComponentModel.Composition.Hosting.TypeCatalog());            
            ")

           .AddToRuntime<MEFAnalyzers.AggregateCatalogDefinition>()
           .AddToRuntime<MEFAnalyzers.TypeCatalogDefinition>()
           .AddToRuntime<MEFAnalyzers.CompositionContainerDefinition>()
           .AddToRuntime<MEFAnalyzers.ComposablePartCatalogCollectionDefinition>()

            .UserAction((c) =>
            {
                UserInteraction.DraggedInstance = c.EntryContext.GetValue(new VariableName("toAccept"));
            })

            .RunEditAction("container", UserInteraction.AcceptEditName)

            .AssertSourceEquivalence(@"
                var toAccept=new System.ComponentModel.Composition.Hosting.AggregateCatalog();
                toAccept.Catalogs.Add(new System.ComponentModel.Composition.Hosting.TypeCatalog());    

                var container=new System.ComponentModel.Composition.Hosting.CompositionContainer(toAccept);
            ");
        }


        [TestMethod]
        public void Edit_AcceptAtSemanticEnd_CommonScope()
        {
            AssemblyUtils.Run(@"
                var catalog=new System.ComponentModel.Composition.Hosting.AggregateCatalog();

                var toAccept=new System.ComponentModel.Composition.Hosting.AggregateCatalog();
                toAccept.Catalogs.Add(new System.ComponentModel.Composition.Hosting.TypeCatalog());            
            ")

            .AddToRuntime<MEFAnalyzers.AggregateCatalogDefinition>()
            .AddToRuntime<MEFAnalyzers.TypeCatalogDefinition>()
            .AddToRuntime<MEFAnalyzers.CompositionContainerDefinition>()
            .AddToRuntime<MEFAnalyzers.ComposablePartCatalogCollectionDefinition>()

            .UserAction((c) =>
            {
                UserInteraction.DraggedInstance = c.EntryContext.GetValue(new VariableName("toAccept"));
            })

            .RunEditAction("catalog", UserInteraction.AcceptEditName)

            .AssertSourceEquivalence(@"
                var catalog=new System.ComponentModel.Composition.Hosting.AggregateCatalog();

                var toAccept=new System.ComponentModel.Composition.Hosting.AggregateCatalog();
                toAccept.Catalogs.Add(new System.ComponentModel.Composition.Hosting.TypeCatalog());
                catalog.Catalogs.Add(toAccept);                
            ");
        }


        [TestMethod]
        public void Edit_AcceptWithEndScopeShifting()
        {
            AssemblyUtils.Run(@"
                var accepter=""accepter"";
                AddAccept(accepter);             

                accepter=""scope end"";
                var toAccept=""accepted"";

                Report(toAccept);
            ")

            .AddMethod("Test.AddAccept", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var accepter = c.CurrentArguments[1];
                var e = c.Edits;

                e.AddCall(thisObj, "Accept", (view) =>
                {
                    return new CallEditInfo(accepter, "Accept", AssemblyUtils.EXTERNAL_INPUT);
                });

            }, Method.Void_StringParam)


            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = AssemblyUtils.REPORTED_INSTANCE;
            })

            .RunEditAction("this", "Accept")

            .AssertSourceEquivalence(@"
                var accepter=""accepter"";
                AddAccept(accepter);             
                
                var toAccept=""accepted"";
                accepter.Accept(toAccept);
                accepter=""scope end"";
                Report(toAccept);
            ");
        }

        [TestMethod]
        public void Edit_RemoveWithRedeclaration()
        {
            AssemblyUtils.Run(@"
                var toDelete=""toDelete"";
                
                var anotherDeleted=PassThrough(toDelete);
                
                anotherDeleted=""force redeclaration"";                  
            ")

         .AddMethod("Test.PassThrough", (c) =>
         {
             var arg = c.CurrentArguments[1];
             c.Return(arg);
         }, Method.String_StringParam)

         .RunRemoveAction("toDelete")

         .AssertSourceEquivalence(@"
            var anotherDeleted=""force redeclaration"";         
         ");
        }

        [TestMethod]
        public void Edit_RemoveFromOptionalParam()
        {
            AssemblyUtils.Run(@"
                var toDelete=""toDelete"";                
                CallWithOptional(toDelete);                
            ")

         .AddMethod("Test.CallWithOptional", (c) =>
         {
             var arg = c.CurrentArguments[1];
             c.Edits.SetOptional(1);
             c.Return(arg);
         }, Method.String_StringParam)

         .RunRemoveAction("toDelete")

         .AssertSourceEquivalence(@"
            CallWithOptional();         
         ");
        }

        [TestMethod]
        public void Edit_RemoveFromCallCascade()
        {
            AssemblyUtils.Run(@"
                var toDelete=""toDelete"";                
                CallWithOptional(CallWithRequired(toDelete));                
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

            .AssertSourceEquivalence(@"
                CallWithOptional();         
            ");
        }

        [TestMethod]
        public void Edit_RemovePreserveCtor()
        {
            AssemblyUtils.Run(@"            
                var obj=new TestObj(""toDelete"");
                obj.CallWithRequired(new TestObj(""abc""));                
            ")

            .AddMethod("TestObj." + Naming.CtorName, (c) => { }, Method.Ctor_StringParam)

            .AddMethod("TestObj.CallWithRequired", (c) =>
            {
                var arg = c.CurrentArguments[1];
                c.Return(arg);
            }, Method.Void_ObjectParam)

            .RunRemoveAction("obj")

            .AssertSourceEquivalence(@"
                new TestObj(""abc"");         
            ");
        }

        [TestMethod]
        public void Edit_RemoveMultiVariable()
        {
            AssemblyUtils.Run(@"                
                var toDelete=""toDelete"";
                var a=toDelete;
                var b=a;
            ")

            .RunRemoveAction("toDelete")

            .AssertSourceEquivalence(@"
                
            ");
        }


        [TestMethod]
        public void Edit_RemoveChainedAssign()
        {
            AssemblyUtils.Run(@"
                var a=""valA"";
                var b=""toDelete"";
                var c=""valC"";
        
                var toDelete=b;
                a=b=c;              
            ")

         .RunRemoveAction("toDelete")

         .AssertSourceEquivalence(@"
                var a=""valA"";                
                var c=""valC"";
                
                System.String b;        
                a=b=c;      
         ");
        }

        [TestMethod]
        public void Edit_RemoveVariableCall()
        {
            AssemblyUtils.Run(@"
                var a=""valA"";
                var b=a.ToString();
                var c=b.ToString();
                c=""ForceRedeclare"";
            ")

         .RunRemoveAction("a")

         .AssertSourceEquivalence(@"
                var c=""ForceRedeclare"";
         ");
        }

        [TestMethod]
        public void Edit_GroupShifting()
        {
            AssemblyUtils.Run(@"
                var toAccept = new SimpleStringExport();
                Report(toAccept);
                System.ComponentModel.Composition.Hosting.CompositionContainer cont;

                if (true)
                {
                    //here is end of comp scope - if condition of upper block is true
                    toAccept = null;
                    //some complicated block structure
                    var test = ""f"";
                    if (true)
                    {
                    }
                    switch (test)
                    {
                        case ""f"":
                        case ""e"":
                        default:
                            break;
                    }
                }
                cont = new System.ComponentModel.Composition.Hosting.CompositionContainer();          
            ")

            .AddToRuntime<CompositionContainerDefinition>()
            .AddToRuntime<SimpleStringExport>()

            .UserAction((c) =>
            {
                UserInteraction.DraggedInstance = AssemblyUtils.REPORTED_INSTANCE;
            })

            .RunEditAction("cont", UserInteraction.AcceptEditName)

            .AssertSourceEquivalence(@"
                var toAccept = new SimpleStringExport();
                Report(toAccept);
                System.ComponentModel.Composition.Hosting.CompositionContainer cont;

                cont = new System.ComponentModel.Composition.Hosting.CompositionContainer();   
                cont.ComposeParts(toAccept);       
                if (true)
                {
                    //here is end of comp scope - if condition of upper block is true
                    toAccept = null;
                    //some complicated block structure
                    var test = ""f"";
                    if (true)
                    {
                    }
                    switch (test)
                    {
                        case ""f"":
                        case ""e"":
                        default:
                            break;
                    }
                }
            ");
        }
    }
}
