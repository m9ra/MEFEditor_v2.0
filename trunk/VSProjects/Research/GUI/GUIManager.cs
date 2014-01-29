using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

using Drawing;
using Analyzing;
using MEFEditor;
using TypeSystem;
using TypeSystem.Runtime;

using AssemblyProviders.CILAssembly;

namespace Research.GUI
{

    /// <summary>
    /// Manager class used for handling GUI behaviour
    /// </summary>
    class GUIManager
    {
        private readonly EditorGUI _gui;

        private readonly AbstractDiagramFactory _diagramFactory;

        private readonly AppDomainServices _appDomain;

        private readonly Dictionary<CompositionPoint, ComboBoxItem> _compositionPoints = new Dictionary<CompositionPoint, ComboBoxItem>();

        private readonly DrawingProvider _drawingProvider;

        private AssemblyProvider _hostAssembly;


        /// <summary>
        /// Event fired whenever composition point is selected
        /// </summary>
        public event Action CompositionPointSelected;

        /// <summary>
        /// Event fired whenever new cross assembly is loaded
        /// </summary>
        public event AssemblyAction HostAssemblyLoaded;

        /// <summary>
        /// Event fired whenever previously loaded cross assembly is unloaded
        /// </summary>
        public event AssemblyAction HostAssemblyUnLoaded;

        /// <summary>
        /// Composition point that is currently selected
        /// </summary>
        public CompositionPoint SelectedCompositionPoint { get; private set; }



        public GUIManager(AppDomainServices appDomain, EditorGUI gui, AbstractDiagramFactory diagramFactory)
        {
            _appDomain = appDomain;
            _gui = gui;
            _diagramFactory = diagramFactory;

            _drawingProvider = new DrawingProvider(_gui.Workspace, _diagramFactory);

            hookEvents();
            initialize();
        }

        /// <summary>
        /// Display given diagram definition within editors workspace
        /// </summary>
        /// <param name="diagram">Displayed diagram</param>
        public void Display(DiagramDefinition diagram)
        {
            _drawingProvider.Display(diagram);
        }

        #region Initialization routines

        /// <summary>
        /// Hook events needed for GUI interaction
        /// </summary>
        private void hookEvents()
        {
            _appDomain.ComponentAdded += onComponentAdded;
            _appDomain.ComponentRemoved += onComponentRemoved;

            _appDomain.Assemblies.OnAdd += onAssemblyAdded;
            _appDomain.Assemblies.OnRemove += onAssemblyRemoved;

            _gui.HostPathChanged += onHostPathChanged;
            _gui.RefreshClicked += forceRefresh;
        }


        /// <summary>
        /// Initialize GUI according to current environment state
        /// </summary>
        private void initialize()
        {
            foreach (var assembly in _appDomain.Assemblies)
            {
                onAssemblyAdded(assembly);
            }

            var emptyItem = createNoCompositionPointItem();
            _gui.CompositionPoints.Items.Clear();
            _gui.CompositionPoints.Items.Add(emptyItem);
            _gui.CompositionPoints.SelectedItem = emptyItem;

            foreach (var component in _appDomain.Components)
            {
                onComponentAdded(component);
            }
        }

        #endregion


        #region Assembly settings handling

        private void onAssemblyRemoved(AssemblyProvider provider)
        {
            //TODO remove mapping changed handler
            _gui.Assemblies.RemoveItem(provider);
        }

        private void onAssemblyAdded(AssemblyProvider provider)
        {
            var assemblyItem = createAssemblyItem(provider);
            assemblyItem.MappingChanged += onAssemblyMappingChanged;
            _gui.Assemblies.AddItem(provider, assemblyItem);
        }

        private AssemblyItem createAssemblyItem(AssemblyProvider assembly)
        {
            return new AssemblyItem(assembly);
        }

        private void onAssemblyMappingChanged(AssemblyProvider assembly)
        {
            forceRefresh();
        }

        #endregion

        #region Cross interpreting handling

        void onHostPathChanged(string path)
        {
            if (_hostAssembly != null)
            {
                if (HostAssemblyUnLoaded != null)
                    HostAssemblyUnLoaded(_hostAssembly);

                _hostAssembly = null;
            }

            if (path == null)
            {
                //there is nothing to do
                return;
            }
            else
            {
                //TODO refactor - assembly loading algorithm - this is responsibily of managers user
                _hostAssembly = new CILAssembly(path);
                if (HostAssemblyLoaded != null)
                    HostAssemblyLoaded(_hostAssembly);
            }
        }

        #endregion

        #region Composition point list handling

        private void forceRefresh()
        {
            onCompositionPointSelected(SelectedCompositionPoint);
        }

        private void onComponentAdded(ComponentInfo component)
        {
            foreach (var compositionPoint in component.CompositionPoints)
            {
                addCompositionPoint(compositionPoint);
            }
        }

        private void onComponentRemoved(ComponentInfo component)
        {
            foreach (var compositionPoint in component.CompositionPoints)
            {
                removeCompositionPoint(compositionPoint);
            }
        }

        private void addCompositionPoint(CompositionPoint compositionPoint)
        {
            var item = createCompositionPointItem(compositionPoint);

            _compositionPoints.Add(compositionPoint, item);
            _gui.CompositionPoints.Items.Add(item);
        }

        private void removeCompositionPoint(CompositionPoint compositionPoint)
        {
            var item = _compositionPoints[compositionPoint];
            _compositionPoints.Remove(compositionPoint);

            _gui.CompositionPoints.Items.Remove(item);

            if (compositionPoint == SelectedCompositionPoint)
            {
                _gui.CompositionPoints.SelectedIndex = 0;
            }
        }

        private ComboBoxItem createCompositionPointItem(CompositionPoint compositionPoint)
        {
            var itemContent = new TextBlock();
            itemContent.Text = compositionPoint.EntryMethod.MethodString;

            var item = new ComboBoxItem();
            item.Content = itemContent;
            item.Selected += (e, s) =>
            {
                onCompositionPointSelected(compositionPoint);
            };

            return item;
        }

        private ComboBoxItem createNoCompositionPointItem()
        {
            var itemContent = new TextBlock();
            itemContent.Text = "None";

            var item = new ComboBoxItem();
            item.Content = itemContent;

            item.Selected += (e, s) =>
            {
                onCompositionPointSelected(null);
            };

            return item;
        }

        /// <summary>
        /// Event handler for composition point items
        /// </summary>
        /// <param name="selectedCompositionPoint"></param>
        private void onCompositionPointSelected(CompositionPoint selectedCompositionPoint)
        {
            SelectedCompositionPoint = selectedCompositionPoint;

            if (CompositionPointSelected != null)
                CompositionPointSelected();
        }

        #endregion
    }
}
