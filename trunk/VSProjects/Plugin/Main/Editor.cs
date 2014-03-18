using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Drawing;
using Analyzing;
using TypeSystem;
using TypeSystem.Runtime;
using TypeSystem.DrawingServices;
using Interoperability;

using AssemblyProviders.ProjectAssembly;

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

            throw new NotImplementedException("Add assembly factories");
            _loader = new AssemblyLoader(settings);

            loadUserExtensions();
            settings.Runtime.BuildAssembly();

            //TODO draw according to extensions
            var factory = new DiagramFactory(new ContentDrawer(null, (item) => new ComponentDrawing(item)));
            _guiManager = new GUIManager(_loader.AppDomain, GUI, factory);

            hookHandlers();
        }

        private void loadUserExtensions()
        {
            //TODO load extensions
        }

        private void hookHandlers()
        {
            _guiManager.CompositionPointSelected += _guiManager_CompositionPointSelected;

            _vs.SolutionOpened += _vs_SolutionOpened;
            _vs.SolutionClosed += _vs_SolutionClosed;

            if (_vs.IsSolutionOpen)
            {
                //force loading solution
                _vs_SolutionOpened();
            }
        }

        #region Drawing providing routines

        private void showComposition(MethodID entryMethod, Instance[] entryArguments)
        {
            var result = _machine.Run(_loader, entryMethod, entryArguments);
            var drawing = createDrawings(result);

            _guiManager.Display(drawing);
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

        private void _guiManager_CompositionPointSelected()
        {
            var compositionPoint = _guiManager.SelectedCompositionPoint;

            if (compositionPoint != null)
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
                var assembly = new VsProjectAssembly(project);
                _loader.Load(project);
            }
        }

        void _vs_SolutionClosed()
        {
            //there is nothing to do, projects are unloaded by ProjectRemoved
        }

        #endregion
    }
}
