using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;

using Drawing;
using Analyzing;
using MEFEditor;
using TypeSystem;
using TypeSystem.Runtime;

namespace Research
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

        private DrawingProvider _drawingProvider;

        /// <summary>
        /// Event fired whenever composition point is selected
        /// </summary>
        public event Action CompositionPointSelected;

        /// <summary>
        /// Composition point that is currently selected
        /// </summary>
        public CompositionPoint SelectedCompositionPoint { get; private set; }

        public GUIManager(AppDomainServices appDomain, EditorGUI gui, AbstractDiagramFactory diagramFactory)
        {
            _appDomain = appDomain;
            _gui = gui;
            _diagramFactory = diagramFactory;

            hookEvents();
            initialize();
        }

        /// <summary>
        /// Display given diagram definition within editors workspace
        /// </summary>
        /// <param name="diagram">Displayed diagram</param>
        public void Display(DiagramDefinition diagram)
        {
            _drawingProvider = new DrawingProvider(_gui.Workspace, _diagramFactory);
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
        }

        /// <summary>
        /// Initialize GUI according to current environment state
        /// </summary>
        private void initialize()
        {
            var emptyItem=createNoCompositionPointItem();
            _gui.CompositionPoints.Items.Clear();
            _gui.CompositionPoints.Items.Add(emptyItem);
            _gui.CompositionPoints.SelectedItem = emptyItem;

            foreach (var component in _appDomain.Components)
            {
                onComponentAdded(component);
            }
        }

        #endregion

        #region Composition point list handling

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
