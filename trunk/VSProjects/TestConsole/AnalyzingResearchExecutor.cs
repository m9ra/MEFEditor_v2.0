using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

using Drawing;
using TypeSystem;
using TypeSystem.DrawingServices;
using Analyzing;
using Analyzing.Editing;
using Analyzing.Execution;
using MEFAnalyzers.Drawings;

using Plugin.GUI;

using TestConsole.Drawings;

using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;


namespace TestConsole
{
    delegate IEnumerable<Instance> EntryArgumentsProvider(Machine machine);

    /// <summary>
    /// Executor for running tasks on testing assembly
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

        private void analyzeResult()
        {
            _entryContext = _result.Execution.EntryContext;

            createDrawings();
            printEntryContext();
            printOtherContexts();
            printAdditionalInfo();
        }

        private void refreshResult()
        {
            _watch.Reset();
            _watch.Start();

            var entryArguments = _entryArgumentsProvider(_assembly.Machine).ToArray();
            _result = _assembly.GetResult(_entryMethod, entryArguments);
            _watch.Stop();
        }

        private void refreshDrawing()
        {
            refreshResult();
            analyzeResult();
            TryShowDrawings();
        }


        /// <summary>
        /// If there are available drawings, display window is opened
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
                _guiManager.Display(_diagramDefinition);
            }
        }

        #region Printing services

        /// <summary>
        /// Print count-times new line on console output
        /// </summary>
        /// <param name="count">Number of printed new lines</param>
        internal static void PrintLines(int count = 1)
        {
            for (int i = 0; i < count; ++i)
                Print(ConsoleColor.White, "\n");
        }

        /// <summary>
        /// Print text with specified color on console output. Text is suffixed by new line character.
        /// </summary>
        /// <param name="color">Color of printed output</param>
        /// <param name="text">Text printed to output</param>
        /// <param name="formatArgs">Format arguments for printed text</param>
        internal static void Println(ConsoleColor color, string text, params object[] formatArgs)
        {
            Print(color, text + "\n", formatArgs);
        }

        /// <summary>
        /// Print text with specified color on console output
        /// </summary>
        /// <param name="color">Color of printed output</param>
        /// <param name="text">Text printed to output</param>
        /// <param name="formatArgs">Format arguments for printed text</param>
        internal static void Print(ConsoleColor color, string text, params object[] formatArgs)
        {
            Printer.Print(color, text, formatArgs);
        }

        #endregion

        #region Output building

        /// <summary>
        /// Proces showing form with discovered drawings
        /// </summary>
        private void showDrawings()
        {
            var form = new TestForm();
            
            //default drawers
            _assembly.RegisterDrawing<ComponentDrawing>(""); //define default drawer
            _assembly.RegisterDrawing<CompositionTesterDrawing>("CompositionTester");
            _assembly.RegisterDrawing<CompositionTesterDrawing>("System.ComponentModel.Composition.Hosting.CompositionContainer");
            _assembly.RegisterDrawing<DirectoryCatalogDrawing>("System.ComponentModel.Composition.Hosting.DirectoryCatalog");
            _assembly.RegisterDrawing<AggregateCatalogDrawing>("System.ComponentModel.Composition.Hosting.AggregateCatalog");
            _assembly.RegisterDrawing<TypeCatalogDrawing>("System.ComponentModel.Composition.Hosting.TypeCatalog");
            _assembly.RegisterDrawing<AssemblyCatalogDrawing>("System.ComponentModel.Composition.Hosting.AssemblyCatalog");

            var factory = new DiagramFactory(_assembly.RegisteredDrawers);                

            _guiManager = new GUIManager(_assembly.AppDomain, form.GUI, factory);
            _guiManager.CompositionPointSelected += onCompositionPointSelected;
       /*     _guiManager.HostAssemblyLoaded += (a) => _assembly.AddAssembly(a);
            _guiManager.HostAssemblyUnLoaded += (a) => { _assembly.RemoveAssembly(a); };*/
            _guiManager.Display(_diagramDefinition);

            form.Show();
            Dispatcher.Run();
        }

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

        private void setDefaultCompositionPoint()
        {
            _entryMethod = Method.EntryInfo.MethodID;
            _entryArgumentsProvider = getDefaultArguments;
        }

        private IEnumerable<Instance> getDefaultArguments(Machine m)
        {
            yield return m.CreateDirectInstance("Default object");
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

        private void generalDrawer(DrawedInstance instance)
        {
            var componentInfo = _assembly.Loader.GetComponentInfo(instance.WrappedInstance.Info);
            GeneralDefinitionProvider.Draw(instance, componentInfo);
        }

        /// <summary>
        /// Print entry context information
        /// </summary>
        private void printEntryContext()
        {
            Println(ConsoleColor.Cyan, "ENTRY CONTEXT - Variable values");
            Printer.PrintVariables(_entryContext);

            Println(ConsoleColor.Red, "\n\nENTRY CONTEXT");
            Printer.PrintIAL(_entryContext.Program.Code);
        }

        /// <summary>
        /// Print other that entry context information
        /// </summary>
        private void printOtherContexts()
        {
            Println(ConsoleColor.Cyan, "\nGENERATED METHODS");

            var contexts = generatedContexts();

            //entry context has already been printed
            contexts.Remove(_entryContext);

            foreach (var context in contexts)
            {
                Println(ConsoleColor.Red, "Method: {0}", context.Name);
                Printer.PrintIAL(context.Program.Code);
                PrintLines();
            }
        }

        /// <summary>
        /// Print additional information about execution
        /// </summary>
        private void printAdditionalInfo()
        {
            PrintLines(2);
            Println(ConsoleColor.Green, "Elapsed time: {0}ms", _watch.ElapsedMilliseconds);

            PrintLines(2);
            Println(ConsoleColor.Yellow, "Entry source result:");
            Println(ConsoleColor.Gray, "{0}", formatSource(_assembly.GetSource(_entryMethod, _result.View)));
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

        #region Source formatting

        /// <summary>
        /// Format indented source
        /// </summary>
        /// <param name="source">Source to be formatted</param>
        /// <returns>Formatted source</returns>
        private string formatSource(string source)
        {
            var result = new StringBuilder();

            var lines = source.Replace("\r", "").Split('\n');

            var lastOriginalIndent = getIndentLevel(lines[0]);
            var currIndent = 0;
            foreach (var line in lines)
            {
                var originalIndent = getIndentLevel(line);

                if (originalIndent > lastOriginalIndent)
                {
                    ++currIndent;
                }
                else if (originalIndent < lastOriginalIndent)
                {
                    --currIndent;
                }

                lastOriginalIndent = originalIndent;
                if (currIndent < 0)
                {
                    currIndent = 0;
                }

                var indentedLine = "".PadLeft(currIndent * 3, ' ') + line.Trim();
                result.AppendLine(indentedLine);

            }

            return result.ToString();
        }

        /// <summary>
        /// Get level of indentation on given line
        /// </summary>
        /// <param name="line">Line which indentation is resolved</param>
        /// <returns>Level of indentation</returns>
        private int getIndentLevel(string line)
        {
            int i;
            for (i = 0; i < line.Length; ++i)
            {
                var ch = line[i];
                if (!char.IsWhiteSpace(ch))
                    break;
            }
            return i;

        }

        #endregion
    }
}
