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
using Analyzing;
using Analyzing.Execution;
using MEFAnalyzers.Drawings;

using Research.Drawings;

using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;

namespace Research
{
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
        private DiagramDefinition _drawings;

        /// <summary>
        /// Result of analyzing execution is stored here
        /// </summary>
        private AnalyzingResult _result;

        /// <summary>
        /// Entry context of analyzing execution is stored here
        /// </summary>
        private CallContext _entryContext;

        /// <summary>
        /// Stopwatch used for measuring execution time
        /// </summary>
        private Stopwatch _watch = new Stopwatch();

        internal AnalyzingResearchExecutor(TestingAssembly assembly)
        {
            _assembly = assembly;
        }

        /// <summary>
        /// Execute test defined by TestingAssembly
        /// </summary>
        internal void Execute()
        {
            runExecution();

            findDrawings();
            printEntryContext();
            printOtherContexts();
            printAdditionalInfo();
        }

        /// <summary>
        /// If there are available drawings, display window is opened
        /// </summary>
        internal void TryShowDrawings()
        {
            if (_drawings.Count == 0)
                return;

            var thread = new Thread(showDrawings);

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
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
            var factory = new DiagramFactory(
                new ContentDrawer(null, (item) => new ComponentDrawing(item)),
                new ContentDrawer("CompositionTester", (item) => new CompositionTesterDrawing(item))
                );
            var provider = new DrawingProvider(form.Output, factory);

            form.Show();

            provider.Display(_drawings);
            Dispatcher.Run();
        }

        /// <summary>
        /// Run execution defined by assembly
        /// </summary>
        private void runExecution()
        {
            _assembly.Runtime.BuildAssembly();
            _watch.Start();
            _result = _assembly.GetResult();
            _watch.Stop();

            _entryContext = _result.EntryContext;
        }

        /// <summary>
        /// Find drawings between instances created during execution
        /// </summary>
        private void findDrawings()
        {
            _drawings = new DiagramDefinition();

            foreach (var instance in _result.CreatedInstances)
            {
                //TODO display components or types with defined drawers
                var info = _assembly.Loader.GetComponentInfo(instance);

                if (info != null || instance.Info.TypeName == "CompositionTester")
                {
                    _assembly.Runtime.Draw(instance, _drawings);
                }
            }
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
            Println(ConsoleColor.Gray, "{0}", formatSource(_assembly.GetEntrySource()));
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
