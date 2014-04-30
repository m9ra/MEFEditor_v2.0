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
        /// Current result of analysis
        /// </summary>
        private AnalyzingResult _currentResult;

        /// <summary>
        /// GUI used by editor
        /// </summary>
        internal readonly EditorGUI GUI;

        /// <summary>
        /// Runtime used by editor
        /// </summary>
        internal RuntimeAssembly Runtime { get { return _loader.Settings.Runtime; } }

        internal Editor(VisualStudioServices vs)
        {
            _vs = vs;

            GUI = new EditorGUI();
        }

        internal void Initialize()
        {
            var settings = new MachineSettings();
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
        }

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

        private void hookHandlers()
        {
            _guiManager.CompositionPointSelected += _guiManager_CompositionPointSelected;

            _vs.BeforeFlushingChanges += () => _changesTransaction = _loader.AppDomain.Transactions.StartNew("Handling user changes");
            _vs.AfterFlushingChanges += () => _loader.AppDomain.Transactions.EndTransaction(_changesTransaction);

            _vs.SolutionOpened += _vs_SolutionOpened;
            _vs.SolutionClosed += _vs_SolutionClosed;

            if (_vs.IsSolutionOpen)
            {
                //force loading solution
                _vs_SolutionOpened();
            }

            _loader.AppDomain.MethodInvalidated += _methodInvalidated;
        }

        #region Drawing providing routines

        private void showComposition(MethodID entryMethod, Instance[] entryArguments)
        {
            if (entryMethod == null)
            {
                _guiManager.Display(null);
                _currentResult = null;
            }
            else
            {
                var watch = Stopwatch.StartNew();
                _currentResult = _machine.Run(_loader, entryMethod, entryArguments);
                _vs.Log.Message("Executing composition point {0}ms", watch.ElapsedMilliseconds);
                watch.Restart();

                var drawing = createDrawings(_currentResult);                
                _guiManager.Display(drawing);
                _vs.Log.Message("Drawing composition point {0}ms", watch.ElapsedMilliseconds);
            }
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

            return pipeline.GetOutput();
        }

        private void generalDrawer(DrawedInstance instance)
        {
            var componentInfo = _loader.GetComponentInfo(instance.WrappedInstance.Info);
            GeneralDefinitionProvider.Draw(instance, componentInfo);
        }

        #endregion

        #region Event handlers


        void _methodInvalidated(MethodID invalidatedMethod)
        {
            _vs.Log.Message("Method invalidation {0}", invalidatedMethod);

            if (_currentResult == null)
                return;

            if (!_currentResult.Uses(invalidatedMethod))
                return;

            _guiManager_CompositionPointSelected();
        }

        private void _guiManager_CompositionPointSelected()
        {
            var compositionPoint = _guiManager.SelectedCompositionPoint;

            if (compositionPoint == null)
            {
                showComposition(null, null);
            }
            else
            {
                var entryMethod = compositionPoint.EntryMethod;

                //TODO proper arguments resolving
                var entryArguments = new[]{
                    _machine.CreateInstance(compositionPoint.DeclaringComponent)
                };

                showComposition(entryMethod, entryArguments);
            }
        }

        private void _vs_SolutionOpened()
        {
            foreach (var project in _vs.SolutionProjects)
            {
                var assembly = new VsProjectAssembly(project, _vs);
                _loader.LoadRoot(project);
            }
        }

        void _vs_SolutionClosed()
        {
            //there is nothing to do, projects are unloaded by ProjectRemoved
        }

        #endregion
    }
}
