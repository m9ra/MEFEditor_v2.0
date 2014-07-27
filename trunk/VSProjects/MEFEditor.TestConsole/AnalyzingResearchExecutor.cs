using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

using MEFEditor.Drawing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.DrawingServices;
using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Execution;
using RecommendedExtensions.Core.Drawings;

using MEFEditor.Plugin.GUI;

using MEFEditor.UnitTesting.Analyzing_TestUtils;
using MEFEditor.UnitTesting.TypeSystem_TestUtils;

using MEFEditor.TestConsole.Drawings;

namespace MEFEditor.TestConsole
{
    /// <summary>
    /// Provider that is used for creating entry arguments
    /// </summary>
    /// <param name="machine">Machine that is used for construction of entry arguments</param>
    /// <returns>Created arguments</returns>
    delegate IEnumerable<Instance> EntryArgumentsProvider(Machine machine);

    /// <summary>
    /// Executor for running tasks on testing assembly. Results
    /// are shown in console or in editor GUI when there are 
    /// found some drawable instances.
    /// </summary>
    class AnalyzingResearchExecutor
    {
        /// <summary>
        /// Testing assembly defining test execution
        /// </summary>
        private readonly TestingAssembly _assembly;

        /// <summary>
        /// All discovered drawings (is filled in post processing)
        /// </summary>
        private DiagramDefinition _diagramDefinition;

        /// <summary>
        /// Result of analyzing execution is stored here
        /// </summary>
        private TestResult _result;

        /// <summary>
        /// Entry context of analyzing execution is stored here
        /// </summary>
        private CallContext _entryContext;

        /// <summary>
        /// ID of method that will be used as entry one (can be changed via GUI)
        /// </summary>
        private MethodID _entryMethod = Method.EntryInfo.MethodID;

        /// <summary>
        /// Arguments that are used for entry method call
        /// </summary>
        private EntryArgumentsProvider _entryArgumentsProvider;

        /// <summary>
        /// Stopwatch used for measuring execution time
        /// </summary>
        private Stopwatch _watch = new Stopwatch();

        /// <summary>
        /// Manager of GUI, that is displayed for methods containing composition
        /// </summary>
        private GUIManager _guiManager;


        /// <summary>
        /// Initializes new instance of executor of research test cases.
        /// </summary>
        /// <param name="assembly">Assembly</param>
        internal AnalyzingResearchExecutor(TestingAssembly assembly)
        {
            _assembly = assembly;
            setDefaultCompositionPoint();
        }

        /// <summary>
        /// Execute test defined by TestingAssembly
        /// </summary>
        internal void Execute()
        {
            runExecution();

            analyzeResult();
        }

        /// <summary>
        /// Analyze result of test case running
        /// </summary>
        private void analyzeResult()
        {
            _entryContext = _result.Execution.EntryContext;

            createDrawings();
            printEntryContext();
            printOtherContexts();
            printAdditionalInfo();
        }

        /// <summary>
        /// Refresh analysis result
        /// </summary>
        private void refreshResult()
        {
            _watch.Reset();
            _watch.Start();

            var entryArguments = _entryArgumentsProvider(_assembly.Machine).ToArray();
            _result = _assembly.GetResult(_entryMethod, entryArguments);
            _watch.Stop();
        }

        /// <summary>
        /// Refresh drawing in GUI
        /// </summary>
        private void refreshDrawing()
        {
            refreshResult();
            analyzeResult();
            TryShowDrawings();
        }
        
        /// <summary>
        /// If there are available drawings, display window will be opened
        /// </summary>
        internal void TryShowDrawings()
        {
            if (_diagramDefinition.Count == 0 && _guiManager == null)
                //there are no drawings and gui manager is not displayed
                return;

            _result.Execution.OnViewCommit += (view) =>
            {
                var source = _assembly.GetSource(_entryMethod, view);
                _assembly.SetSource(_entryMethod, source);

                refreshDrawing();
            };

            if (_guiManager == null)
            {
                var thread = new Thread(showDrawings);

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            else
            {
                displayDiagram(_diagramDefinition);
            }
        }

        #region Output building

        /// <summary>
        /// Proces showing form with discovered drawings
        /// </summary>
        private void showDrawings()
        {
            var form = new TestForm();

            //default drawers
            _assembly.RegisterDrawing<ComponentDrawing>(""); //define default drawer
            _assembly.RegisterDrawing<CompositionContainerDrawing>("CompositionTester");
            _assembly.RegisterDrawing<CompositionContainerDrawing>("System.ComponentModel.Composition.Hosting.CompositionContainer");
            _assembly.RegisterDrawing<CompositionBatchDrawing>("System.ComponentModel.Composition.Hosting.CompositionBatch");
            _assembly.RegisterDrawing<DirectoryCatalogDrawing>("System.ComponentModel.Composition.Hosting.DirectoryCatalog");
            _assembly.RegisterDrawing<AggregateCatalogDrawing>("System.ComponentModel.Composition.Hosting.AggregateCatalog");
            _assembly.RegisterDrawing<TypeCatalogDrawing>("System.ComponentModel.Composition.Hosting.TypeCatalog");
            _assembly.RegisterDrawing<AssemblyCatalogDrawing>("System.ComponentModel.Composition.Hosting.AssemblyCatalog");

            var factory = new DiagramFactory(_assembly.RegisteredDrawers);

            _guiManager = new GUIManager(form.GUI);
            _guiManager.Initialize(_assembly.AppDomain, factory);
            _guiManager.CompositionPointSelected += onCompositionPointSelected;
            displayDiagram(_diagramDefinition);

            form.Show();
            Dispatcher.Run();
        }

        /// <summary>
        /// Display diagram according to given definition
        /// </summary>
        /// <param name="diagramDefinition">Definition of diagram to be displayed</param>
        private void displayDiagram(DiagramDefinition diagramDefinition)
        {
            diagramDefinition.ShowJoinLines = _guiManager.ShowJoinLines;
            diagramDefinition.UseItemAvoidance = _guiManager.UseItemAvoidance;
            diagramDefinition.UseJoinAvoidance = _guiManager.UseJoinAvoidance;

            _guiManager.Display(diagramDefinition);
        }

        /// <summary>
        /// Handler called whenever composition point is selected
        /// </summary>
        private void onCompositionPointSelected()
        {
            var compositionPoint = _guiManager.SelectedCompositionPoint;

            if (compositionPoint == null)
            {
                setDefaultCompositionPoint();
            }
            else
            {
                _entryMethod = _guiManager.SelectedCompositionPoint.EntryMethod;
                _entryArgumentsProvider = (m) =>
                {
                    var thisObj = m.CreateInstance(compositionPoint.DeclaringComponent);
                    return new[]{
                        thisObj
                    };
                };
            }

            refreshDrawing();
        }

        /// <summary>
        /// Set environment to run default composition point
        /// </summary>
        private void setDefaultCompositionPoint()
        {
            _entryMethod = Method.EntryInfo.MethodID;
            _entryArgumentsProvider = getDefaultArguments;
        }

        /// <summary>
        /// Create default arguments
        /// </summary>
        /// <param name="machine">Machine used for arguments creation</param>
        /// <returns>Created arguments</returns>
        private IEnumerable<Instance> getDefaultArguments(Machine machine)
        {
            yield return machine.CreateInstance(TypeDescriptor.Create(Method.EntryClass));
        }

        /// <summary>
        /// Run execution defined by assembly
        /// </summary>
        private void runExecution()
        {
            refreshResult();
        }

        /// <summary>
        /// Find drawings between instances created during execution
        /// </summary>
        private void createDrawings()
        {
            var pipeline = _assembly.Runtime.CreateDrawingPipeline(generalDrawer, _result.Execution);

            foreach (var instance in _result.Execution.CreatedInstances)
            {
                var hasDrawer = _assembly.Runtime.GetDrawer(instance) != null;
                var hasComponentInfo = _assembly.Loader.GetComponentInfo(instance.Info) != null;

                var addToQueue = hasDrawer || hasComponentInfo;

                if (addToQueue)
                {
                    pipeline.AddToDrawQueue(instance);
                    if (hasComponentInfo)
                        pipeline.ForceDisplay(instance);
                }
            }

            _diagramDefinition = pipeline.GetOutput();
        }

        /// <summary>
        /// General drawer used for every drawed instance
        /// </summary>
        /// <param name="instance">Instance to be draw</param>
        private void generalDrawer(DrawedInstance instance)
        {
            var instanceInfo = instance.WrappedInstance.Info;
            var componentInfo = _assembly.Loader.GetComponentInfo(instanceInfo);
            var definingAssembly = _assembly.Loader.AppDomain.GetDefiningAssembly(instanceInfo);
            instance.SetProperty("DefiningAssembly", definingAssembly.Name);
            GeneralDefinitionProvider.Draw(instance, componentInfo);
        }

        /// <summary>
        /// Print entry context information
        /// </summary>
        private void printEntryContext()
        {
            Printer.Println(ConsoleColor.Cyan, "ENTRY CONTEXT - Variable values");
            Printer.PrintVariables(_entryContext);

            Printer.Println(ConsoleColor.Red, "\n\nENTRY CONTEXT");
            Printer.PrintIAL(_entryContext.Program.Code);
        }

        /// <summary>
        /// Print other that entry context information
        /// </summary>
        private void printOtherContexts()
        {
            Printer.Println(ConsoleColor.Cyan, "\nGENERATED METHODS");

            var contexts = generatedContexts();

            //entry context has already been printed
            contexts.Remove(_entryContext);

            foreach (var context in contexts)
            {
                Printer.Println(ConsoleColor.Red, "Method: {0}", context.Name);
                Printer.PrintIAL(context.Program.Code);
                Printer.PrintLines();
            }
        }

        /// <summary>
        /// Print additional information about execution
        /// </summary>
        private void printAdditionalInfo()
        {
            Printer.PrintLines(2);
            Printer.Println(ConsoleColor.Green, "Elapsed time: {0}ms", _watch.ElapsedMilliseconds);

            Printer.PrintLines(2);
            Printer.Println(ConsoleColor.Yellow, "Entry source result:");
            Printer.PrintCode(_assembly.GetSource(_entryMethod, _result.View));
        }



        /// <summary>
        /// Find generated contexts without duplicities (check for instruction batch match)
        /// </summary>
        /// <returns>Found contexts</returns>
        private HashSet<CallContext> generatedContexts()
        {
            var result = new HashSet<CallContext>();

            var knownInstructions = new HashSet<InstructionBatch>();
            var contextQueue = new Queue<CallContext>();
            contextQueue.Enqueue(_entryContext);

            //traverse all contexts
            while (contextQueue.Count > 0)
            {
                var context = contextQueue.Dequeue();

                if (!knownInstructions.Contains(context.Program))
                {
                    //if we dont know insntructions we have new call context
                    knownInstructions.Add(context.Program);
                    result.Add(context);
                }

                //enqueue all possible children
                foreach (var child in context.ChildContexts())
                {
                    contextQueue.Enqueue(child);
                }
            }

            return result;
        }

        #endregion
    }
}
