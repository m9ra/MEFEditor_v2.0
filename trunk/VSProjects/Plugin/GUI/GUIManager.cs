using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;


using Drawing;
using Analyzing;
using MEFEditor;
using Interoperability;
using TypeSystem;
using TypeSystem.Runtime;

using AssemblyProviders.CILAssembly;

namespace Plugin.GUI
{

    /// <summary>
    /// Manager class used for handling GUI behaviour
    /// </summary>
    public class GUIManager
    {
        private readonly EditorGUI _gui;

        private readonly AbstractDiagramFactory _diagramFactory;

        private readonly AppDomainServices _appDomain;

        private readonly Dictionary<CompositionPoint, ComboBoxItem> _compositionPoints = new Dictionary<CompositionPoint, ComboBoxItem>();

        private readonly DrawingProvider _drawingProvider;

        private readonly VisualStudioServices _vs;

        private readonly Queue<LogEntry> _logQueue = new Queue<LogEntry>();

        private MethodID _desiredCompositionPointMethod;

        private CompositionPoint _selectedCompositionPoint;

        private AssemblyProvider _hostAssembly;

        /// <summary>
        /// Number of log entries displayed
        /// </summary>
        public readonly int LogHistorySize = 200;

        /// <summary>
        /// Event fired whenever composition point is selected
        /// </summary>
        public event Action CompositionPointSelected;

        /// <summary>
        /// Event fired whenever new cross assembly is loaded
        /// </summary>
        public event AssemblyEvent HostAssemblyLoaded;

        /// <summary>
        /// Event fired whenever previously loaded cross assembly is unloaded
        /// </summary>
        public event AssemblyEvent HostAssemblyUnLoaded;

        /// <summary>
        /// Composition point that is currently selected
        /// </summary>
        public CompositionPoint SelectedCompositionPoint
        {
            get
            {
                return _selectedCompositionPoint;
            }
            private set
            {
                if (value != null)
                    _desiredCompositionPointMethod = value.EntryMethod;

                _selectedCompositionPoint = value;
            }
        }



        public GUIManager(AppDomainServices appDomain, EditorGUI gui, AbstractDiagramFactory diagramFactory, VisualStudioServices vs = null)
        {
            _appDomain = appDomain;
            _gui = gui;
            _diagramFactory = diagramFactory;
            _vs = vs;

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
            if (_vs != null)
            {
                _vs.Log.OnLog += logHandler;
            }

            _appDomain.ComponentAdded += onComponentAdded;
            _appDomain.ComponentRemoved += onComponentRemoved;

            _appDomain.AssemblyAdded += onAssemblyAdded;
            _appDomain.AssemblyRemoved += onAssemblyRemoved;

            _gui.HostPathChanged += onHostPathChanged;
            _gui.RefreshClicked += forceRefresh;
            _gui.LogRefresh += logRefresh;
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
                return;

            _hostAssembly = _appDomain.Loader.LoadRoot(path);

            if (_hostAssembly != null && HostAssemblyLoaded != null)
                HostAssemblyLoaded(_hostAssembly);
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

            var isDesiredCompositionPoint=_desiredCompositionPointMethod != null && _desiredCompositionPointMethod.Equals(compositionPoint.EntryMethod);
            if (isDesiredCompositionPoint)
                _gui.CompositionPoints.SelectedIndex = _gui.CompositionPoints.Items.Count - 1;
        }

        private void removeCompositionPoint(CompositionPoint compositionPoint)
        {
            if (!_compositionPoints.ContainsKey(compositionPoint))
            {
                //nothing to remove
                return;
            }

            var item = _compositionPoints[compositionPoint];
            _compositionPoints.Remove(compositionPoint);

            _gui.CompositionPoints.Items.Remove(item);

            if (compositionPoint.Equals(SelectedCompositionPoint))
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
                item.Dispatcher.BeginInvoke(new Action(() => onCompositionPointSelected(compositionPoint)));
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

        #region Logging service handling

        /// <summary>
        /// Handler called for every logged entry
        /// </summary>
        /// <param name="entry">Logged entry</param>
        private void logHandler(LogEntry entry)
        {
            _logQueue.Enqueue(entry);

            while (_logQueue.Count > LogHistorySize)
                _logQueue.Dequeue();

            if (_gui.IsLogVisible)
                drawLogEntry(entry);
        }

        /// <summary>
        /// Draw given entry into Log
        /// </summary>
        /// <param name="entry">Entry to draw</param>
        private void drawLogEntry(LogEntry entry)
        {
            var drawing = createLogEntryDrawing(entry);

            _gui.Log.Children.Insert(0, drawing);
            while (_gui.Log.Children.Count > LogHistorySize)
                _gui.Log.Children.RemoveAt(LogHistorySize);
        }

        private void logRefresh()
        {
            foreach (var entry in _logQueue)
            {
                drawLogEntry(entry);
            }
        }

        private UIElement createLogEntryDrawing(LogEntry entry)
        {
            Brush entryColor;
            switch (entry.Level)
            {
                case LogLevels.Error:
                    entryColor = Brushes.Red;
                    break;
                case LogLevels.Warning:
                    entryColor = Brushes.Orange;
                    break;
                case LogLevels.Notification:
                    entryColor = Brushes.Black;
                    break;
                default:
                    entryColor = Brushes.Gray;
                    break;
            }

            var heading = new TextBlock();
            heading.Text = entry.Message;
            heading.Foreground = entryColor;

            //set navigation handler
            if (entry.Navigate != null)
                heading.PreviewMouseDown += (a, b) => entry.Navigate();

            if (entry.Description == null)
                //no description is available
                return heading;

            var expander = new Expander();
            expander.Header = heading;
            var description = new TextBlock();
            description.Text = entry.Description;
            description.Margin = new Thickness(10, 0, 0, 10);

            expander.Content = description;
            expander.Margin = new Thickness(20, 0, 0, 0);

            return expander;
        }
        #endregion
    }
}
