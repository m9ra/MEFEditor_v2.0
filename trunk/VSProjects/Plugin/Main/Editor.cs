using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Drawing;
using Analyzing;
using TypeSystem;
using TypeSystem.Runtime;
using TypeSystem.Transactions;
using TypeSystem.DrawingServices;
using Interoperability;


using AssemblyProviders.CIL;
using AssemblyProviders.CSharp;
using AssemblyProviders.ProjectAssembly;
using AssemblyProviders.DirectDefinitions;

using MEFAnalyzers;
using MEFAnalyzers.Drawings;

using Plugin.GUI;
using MEFEditor.Plugin.Drawing;

namespace MEFEditor.Plugin.Main
{
    /// <summary>
    /// Implementation of Enahnced MEF Component Architecture Editor behaviour
    /// </summary>
    class Editor
    {
        #region Private members

        /// <summary>
        /// Services used for interconnection with visual studio
        /// </summary>
        private readonly VisualStudioServices _vs;

        /// <summary>
        /// Manager of GUI used by editor
        /// </summary>
        private GUIManager _guiManager;

        /// <summary>
        /// Loader used for loading assemblies into <see cref="AppDomain"/>
        /// </summary>
        private AssemblyLoader _loader;

        /// <summary>
        /// Machine that is used for composition point analyzing
        /// </summary>
        private Machine _machine;

        /// <summary>
        /// Content drawers that were loaded through extensions
        /// </summary>
        private ContentDrawer[] _contentDrawers;

        /// <summary>
        /// Transaction used for handling changes
        /// </summary>
        private Transaction _changesTransaction;

        /// <summary>
        /// Transaction used for handling project adding
        /// </summary>
        private Transaction _projectAddTransaction;

        /// <summary>
        /// Transaction used for handling solution opening
        /// </summary>
        private Transaction _solutionOpenTransaction;

        /// <summary>
        /// Current result of analysis
        /// </summary>
        private AnalyzingResult _currentResult;

        /// <summary>
        /// Error of last analysis run if any, <c>null</c> otherwise
        /// </summary>
        private LogEntry _analysisError;

        #endregion

        /// <summary>
        /// GUI used by editor
        /// </summary>
        internal readonly EditorGUI GUI;

        /// <summary>
        /// Runtime used by editor
        /// </summary>
        internal RuntimeAssembly Runtime { get { return _loader.Settings.Runtime; } }

        /// <summary>
        /// Transaction available in current domain
        /// </summary>
        internal TransactionManager Transactions { get { return _loader.AppDomain.Transactions; } }

        /// <summary>
        /// Initialize instance of <see cref="Editor"/> which
        /// </summary>
        /// <param name="vs"></param>
        internal Editor(VisualStudioServices vs)
        {
            _vs = vs;

            GUI = new EditorGUI();
        }

        #region Initialization routines

        /// <summary>
        /// Initialize editor - extensions and environmennt is loaded, event handlers hooked
        /// </summary>
        internal void Initialize()
        {
            var settings = new MachineSettings(true);
            _machine = new Machine(settings);

            _loader = new AssemblyLoader(settings,
                new AssemblyProviders.CILAssembly.CILAssemblyFactory(),
                new AssemblyProviders.ProjectAssembly.ProjectAssemblyFactory(_vs)
                );

            loadUserExtensions();
            settings.Runtime.BuildAssembly();

            //TODO draw according to extensions
            var factory = new DiagramFactory(_contentDrawers);
            _guiManager = new GUIManager(_loader.AppDomain, GUI, factory, _vs);

            hookHandlers();
            _vs.Log.Message("EDITOR INITIALIZED");
        }

        /// <summary>
        /// Load user extensions from extension assemblies
        /// </summary>
        private void loadUserExtensions()
        {
            //TODO load extensions

            _contentDrawers = new[]{
                new ContentDrawer(null, (item) => new ComponentDrawing(item)),
                new ContentDrawer("CompositionTester", (item) => new CompositionTesterDrawing(item)),
                new ContentDrawer("System.ComponentModel.Composition.Hosting.CompositionContainer", (item) => new CompositionTesterDrawing(item)),
                new ContentDrawer("System.ComponentModel.Composition.Hosting.DirectoryCatalog", (item) => new DirectoryCatalogDrawing(item)),
                new ContentDrawer("System.ComponentModel.Composition.Hosting.AggregateCatalog", (item) => new AggregateCatalogDrawing(item)),
                new ContentDrawer("System.ComponentModel.Composition.Hosting.TypeCatalog", (item) => new TypeCatalogDrawing(item)),
                new ContentDrawer("System.ComponentModel.Composition.Hosting.AssemblyCatalog", (item) => new AssemblyCatalogDrawing(item))
            };

            InitializeRuntime(new[]{
                typeof(string),
                typeof(bool),   
                typeof(VMStack),
                typeof(LiteralType)
            }, new[]{
                typeof(int),
                typeof(double)
            });

            Runtime.AddDefinition(new CompositionContainerDefinition());
            Runtime.AddDefinition(new AggregateCatalogDefinition());
            Runtime.AddDefinition(new TypeCatalogDefinition());
            Runtime.AddDefinition(new DirectoryCatalogDefinition());
            Runtime.AddDefinition(new ComposablePartCatalogCollectionDefinition());
        }

        #region TODO: Methods that are used only for RESEARCH purposes

        internal void InitializeRuntime(Type[] directTypes, Type[] mathTypes)
        {
            foreach (var directType in directTypes)
            {
                AddDirectType(directType);
            }

            foreach (var mathType in mathTypes)
            {
                AddDirectMathType(mathType);
            }
        }

        public void AddDirectMathType(Type mathType)
        {
            var type = typeof(MathDirectType<>).MakeGenericType(mathType);

            var typeDefinition = Activator.CreateInstance(type) as DirectTypeDefinition;
            Runtime.AddDirectDefinition(typeDefinition);
        }

        public void AddDirectType(Type directType)
        {
            var typeDefinition = new DirectTypeDefinition(directType);
            Runtime.AddDirectDefinition(typeDefinition);
        }
        #endregion

        /// <summary>
        /// Hook all event handlers that are used for editor interaction
        /// </summary>
        private void hookHandlers()
        {
            _guiManager.CompositionPointSelected += requireRedraw;

            Transactions.TransactionOpened += (t) => _vs.Log.Message(">> {0}", t.Description);
            Transactions.TransactionCommit += (t) => _vs.Log.Message("<< {0}", t.Description);

            _vs.BeforeFlushingChanges += () => _changesTransaction = _loader.AppDomain.Transactions.StartNew("Handling user changes");
            _vs.AfterFlushingChanges += () => _changesTransaction.Commit();

            _vs.SolutionOpened += _vs_SolutionOpened;
            _vs.SolutionOpeningStarted += _vs_SolutionOpeningStarted;
            _vs.SolutionClosed += _vs_SolutionClosed;

            _vs.ProjectAdded += _vs_ProjectAdded;
            _vs.ProjectAddingStarted += _vs_ProjectAddingStarted;
            _vs.ProjectRemoved += _vs_ProjectRemoved;

            if (_vs.IsSolutionOpen)
            {
                //force loading solution
                _vs_SolutionOpeningStarted();
                _vs_SolutionOpened();
            }

            _loader.AppDomain.MethodInvalidated += methodInvalidated;
        }
        #endregion

        #region Drawing providing routines

        /// <summary>
        /// Show composition based on analysis of given method
        /// </summary>
        /// <param name="compositionPoint">Composition point to be analyzed</param>
        private void showComposition(CompositionPoint compositionPoint)
        {
            if (compositionPoint == null)
            {
                _guiManager.Display(null);
                _currentResult = null;
            }
            else
            {
                var watch = Stopwatch.StartNew();

                runAnalysis(compositionPoint);
                _vs.Log.Message("Executing composition point {0}ms", watch.ElapsedMilliseconds);

                if (_analysisError == null)
                {
                    watch.Restart();

                    //analysis has been successful
                    var drawing = createDrawings(_currentResult);
                    _guiManager.Display(drawing);
                    _vs.Log.Message("Drawing composition point {0}ms", watch.ElapsedMilliseconds);
                }
                else
                {
                    _guiManager.DisplayEntry(_analysisError);
                }
            }
        }

        /// <summary>
        /// Run analysis on given composition point
        /// </summary>
        /// <param name="compositionPoint">Composition point to be analyzed</param>
        private void runAnalysis(CompositionPoint compositionPoint)
        {
            _analysisError = null;

            try
            {
                var entryMethod = compositionPoint.EntryMethod;
                var entryArguments = getCompositionPointArguments(compositionPoint);

                //run analysis on selected compsition with obtained arguments
                _currentResult = _machine.Run(_loader, entryMethod, entryArguments);
                _currentResult.OnViewCommit += (v) =>
                {
                    _vs.ForceFlushChanges();
                };

                handleRuntimeException(_currentResult.RuntimeException);
            }
            catch (Exception ex)
            {
                _analysisError = _vs.LogErrorEntry(ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// Handle exception that could be thrown at analysis runtime
        /// </summary>
        /// <param name="exception">Runtime exception</param>
        private void handleRuntimeException(Exception exception)
        {
            if (exception == null)
                return;

            var parsingException = exception as ParsingException;
            if (parsingException != null)
            {
                _analysisError = _vs.LogErrorEntry(parsingException.Message, parsingException.ToString(), parsingException.Navigate);
            }
            else
            {
                _analysisError = _vs.LogErrorEntry(exception.Message, exception.ToString());
            }
        }

        /// <summary>
        /// Get arguments for given composition point
        /// </summary>
        /// <param name="compositionPoint">Composition point which arguments are requested</param>
        /// <returns>Composition point's arguments</returns>
        private Instance[] getCompositionPointArguments(CompositionPoint compositionPoint)
        {
            var entryMethod = compositionPoint.EntryMethod;
            var entryArguments = new List<Instance>();

            //prepare composition point arguments
            entryArguments.Add(_machine.CreateInstance(compositionPoint.DeclaringComponent));
            if (compositionPoint.ArgumentProvider != null)
            {
                var result = _machine.Run(_loader, compositionPoint.ArgumentProvider);
                var context = result.EntryContext;

                for (var i = 0; i < Naming.GetMethodParamCount(entryMethod); ++i)
                {
                    var argVariable = "arg" + i;
                    var entryArgument = context.GetValue(new VariableName(argVariable));
                    entryArguments.Add(entryArgument);
                }

                if (result.RuntimeException != null)
                    throw new InvalidOperationException("Preparing composition point arguments failed", result.RuntimeException);
            }

            return entryArguments.ToArray();
        }

        /// <summary>
        /// Refresh drawing according to current composition point
        /// <remarks>Is called only</remarks>
        /// </summary>
        private void refreshDrawing()
        {
            if (_guiManager.FlushCompositionPointUpdates())
                //flushing will cause event that refresh drawing again - so 
                //we dont need to refresh drawing immediately
                return;

            var compositionPoint = _guiManager.SelectedCompositionPoint;
            showComposition(compositionPoint);
        }

        /// <summary>
        /// Create drawings from given result
        /// </summary>
        private DiagramDefinition createDrawings(AnalyzingResult result)
        {
            var pipeline = _loader.Settings.Runtime.CreateDrawingPipeline(generalDrawer, result);

            foreach (var instance in result.CreatedInstances)
            {
                var hasDrawer = Runtime.GetDrawer(instance) != null;
                var hasComponentInfo = _loader.GetComponentInfo(instance.Info) != null;

                var addToQueue = hasDrawer || hasComponentInfo;

                if (addToQueue)
                {
                    pipeline.AddToDrawQueue(instance);
                    if (hasComponentInfo)
                        pipeline.ForceDisplay(instance);
                }
            }

            var definition = pipeline.GetOutput();

            definition.AddCommand(new CommandDefinition("Reset workspace", _guiManager.ResetWorkspace));

            return definition;
        }

        /// <summary>
        /// General drawing provider that is commonly used for all instances
        /// Drawings of required instances are specialized by concrete drawers
        /// </summary>
        /// <param name="instance">Instance to be drawn</param>
        private void generalDrawer(DrawedInstance instance)
        {
            if (instance.WrappedInstance.CreationNavigation != null)
                instance.Drawing.AddCommand(new CommandDefinition("Navigate to", () => instance.WrappedInstance.CreationNavigation()));

            instance.Drawing.AddEdit(new EditDefinition("Remove", (v) =>
            {
                var view = (v as EditView).CopyView();
                view.Remove(instance.WrappedInstance);
                return EditView.Wrap(view);
            }, (v) =>
            {
                var view = (v as EditView).CopyView();
                return view.CanRemove(instance.WrappedInstance);
            }));


            var componentInfo = _loader.GetComponentInfo(instance.WrappedInstance.Info);
            GeneralDefinitionProvider.Draw(instance, componentInfo);
        }

        #endregion

        #region Transaction handling

        /// <summary>
        /// Attach refreshRedraw routine into root transaction
        /// </summary>
        private void requireRedraw()
        {
            var action = new TransactionAction(refreshDrawing, "RefreshDrawing", (t) => t.Name == "RefreshDrawing", this);
            Transactions.AttachAfterAction(null, action);
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Handler called for methods that has been invalidated
        /// </summary>
        /// <param name="invalidatedMethod">Identifier of invalidated method</param>
        private void methodInvalidated(MethodID invalidatedMethod)
        {
            _vs.Log.Message("Method invalidation {0}", invalidatedMethod);

            if (_currentResult == null)
                return;

            if (!_currentResult.Uses(invalidatedMethod))
                return;

            requireRedraw();
        }

        /// <summary>
        /// Handler called before project is added. Transaction for project adding is started.
        /// </summary>
        /// <param name="project">Project that will be added</param>
        void _vs_ProjectAddingStarted(EnvDTE.Project project)
        {
            _projectAddTransaction = Transactions.StartNew("Loading project " + project.Name);
        }

        /// <summary>
        /// Handler called when project is added.
        /// </summary>
        /// <param name="project">Added project</param>
        private void _vs_ProjectAdded(EnvDTE.Project project)
        {
            var assembly = new VsProjectAssembly(project, _vs);
            _loader.LoadRoot(project);

            _projectAddTransaction.Commit();
            _projectAddTransaction = null;
        }

        /// <summary>
        /// Handler called when project is removed
        /// </summary>
        /// <param name="project">Removed project</param>
        void _vs_ProjectRemoved(EnvDTE.Project project)
        {
            var tr = Transactions.StartNew("Removing project");
            _loader.UnloadRoot(project);
            tr.Commit();
        }

        /// <summary>
        /// Handler called before solution is opened. Here opening solution trancasction
        /// is started.
        /// </summary>
        void _vs_SolutionOpeningStarted()
        {
            _solutionOpenTransaction = Transactions.StartNew("Openining solution");
        }

        /// <summary>
        /// Handler called when solution is opened.
        /// </summary>
        private void _vs_SolutionOpened()
        {
            _solutionOpenTransaction.Commit();
            _solutionOpenTransaction = null;
        }

        /// <summary>
        /// Handler called when solution is closed.
        /// </summary>
        private void _vs_SolutionClosed()
        {
            //there is nothing to do, projects are unloaded by ProjectRemoved
        }

        #endregion

    }
}
