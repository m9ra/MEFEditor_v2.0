﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;


using EnvDTE;

using Drawing;
using Analyzing;
using MEFEditor;
using Interoperability;
using TypeSystem;
using TypeSystem.Runtime;
using TypeSystem.Transactions;

using AssemblyProviders.CILAssembly;

namespace Plugin.GUI
{

    /// <summary>
    /// Manager class used for handling GUI behaviour
    /// </summary>
    public class GUIManager
    {
        #region Private members

        /// <summary>
        /// Gui managed by current manager
        /// </summary>
        private readonly EditorGUI _gui;

        /// <summary>
        /// Factory used for diagram drawings
        /// </summary>
        private readonly AbstractDiagramFactory _diagramFactory;

        /// <summary>
        /// Appdomain where analysis is processed
        /// </summary>
        private readonly AppDomainServices _appDomain;

        /// <summary>
        /// WPF items according to defining composition points
        /// </summary>
        private readonly Dictionary<CompositionPoint, ComboBoxItem> _compositionPoints = new Dictionary<CompositionPoint, ComboBoxItem>();

        /// <summary>
        /// List of composition point updates
        /// </summary>
        private readonly Dictionary<MethodID, CompositionPoint> _compositionPointRemoves = new Dictionary<MethodID, CompositionPoint>();

        /// <summary>
        /// List of composition point updates
        /// </summary>
        private readonly Dictionary<MethodID, CompositionPoint> _compositionPointAdds = new Dictionary<MethodID, CompositionPoint>();

        /// <summary>
        /// Composition that is desired by user to be displayed. It is used 
        /// for remembering the composition point when is it not currently available
        /// </summary>
        private MethodID _desiredCompositionPointMethod;

        /// <summary>
        /// Composition point that is currently selected
        /// </summary>
        private CompositionPoint _selectedCompositionPoint;

        /// <summary>
        /// Assembly that is currently hosted
        /// </summary>
        private AssemblyProvider _hostAssembly;

        /// <summary>
        /// Access point to drawing services and drawing engine
        /// </summary>
        private readonly DrawingProvider _drawingProvider;

        /// <summary>
        /// Available services exposed by visual studio
        /// </summary>
        private readonly VisualStudioServices _vs;

        /// <summary>
        /// Queue of logged entries
        /// </summary>
        private readonly Queue<LogEntry> _logQueue = new Queue<LogEntry>();

        #endregion

        /// <summary>
        /// Transaction available in current domain
        /// </summary>
        internal TransactionManager Transactions { get { return _appDomain.Transactions; } }

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
                if (value != null && !value.EntryMethod.Equals(_desiredCompositionPointMethod))
                {
                    _desiredCompositionPointMethod = value.EntryMethod;
                    //reset positions when changing desired composition point
                    _drawingProvider.ResetPositions();
                }

                _selectedCompositionPoint = value;
            }
        }

        /// <summary>
        /// Initialize instance of <see cref="GUIManager"/>
        /// </summary>
        /// <param name="appDomain">Appdomain where analysis is processed</param>
        /// <param name="gui">Gui managed by current manager</param>
        /// <param name="diagramFactory">Factory used for diagram drawings</param>
        /// <param name="vs">Available services exposed by visual studio</param>
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

        /// <summary>
        /// Reset workspace
        /// </summary>
        public void ResetWorkspace()
        {
            _gui.Workspace.Reset();
            _drawingProvider.ResetPositions();
            _drawingProvider.Redraw();
        }

        /// <summary>
        /// Display specified entry within workspace
        /// </summary>
        /// <param name="entry">Entry to be displayed</param>
        internal void DisplayEntry(LogEntry entry)
        {
            _gui.Workspace.Clear();
            _gui.Workspace.Reset();
            var entryDrawing = createLogEntryDrawing(entry, true) as Expander;

            var heading = entryDrawing.Header as TextBlock;
            heading.FontSize = 20;

            entryDrawing.Margin = new Thickness(20);
            _gui.Workspace.Children.Add(entryDrawing);
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

                _appDomain.Loader.UnloadRoot(_hostAssembly.FullPath);

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

        internal bool FlushCompositionPointUpdates()
        {
            //keep currently selected index
            var selectedIndex = _gui.CompositionPoints.SelectedIndex;

            //apply composition point removings
            foreach (var compositionPoint in _compositionPointRemoves.Values)
            {
                var item = _compositionPoints[compositionPoint];
                _compositionPoints.Remove(compositionPoint);

                _gui.CompositionPoints.Items.Remove(item);

                //exact composition has been removed
                if (compositionPoint.Equals(SelectedCompositionPoint))
                {
                    selectedIndex = 0;
                }

            }
            _compositionPointRemoves.Clear();

            //apply composition point addings
            foreach (var compositionPoint in _compositionPointAdds.Values)
            {
                var item = createCompositionPointItem(compositionPoint);

                _compositionPoints.Add(compositionPoint, item);
                _gui.CompositionPoints.Items.Add(item);

                var isDesiredCompositionPoint = _desiredCompositionPointMethod != null && _desiredCompositionPointMethod.Equals(compositionPoint.EntryMethod);
                if (isDesiredCompositionPoint)
                    selectedIndex = _gui.CompositionPoints.Items.Count - 1;
            }
            _compositionPointAdds.Clear();

            //composition point could be changed (even no update is processed!)
            refreshSelectedCompositionPoint();

            //refresh displayed names of composition points
            refreshComositionPointNames();

            //refresh selected index
            if (_gui.CompositionPoints.SelectedIndex != selectedIndex)
            {
                _gui.CompositionPoints.SelectedIndex = selectedIndex;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Refresh names of composition points, so they are shortest as possible
        /// </summary>
        private void refreshComositionPointNames()
        {
            var names = new HashSet<string>();
            foreach (var compositionPointPair in _compositionPoints)
            {
                var compositionPoint = compositionPointPair.Key;
                var item = compositionPointPair.Value;

                var name = distinguishName(names, compositionPoint);

                if (item.Tag as string == name)
                    //no need to change
                    continue;

                item.Tag = name;

                var itemContent = new TextBlock();
                itemContent.Text = name;
                item.Content = itemContent;
            }

        }
        /// <summary>
        /// Create shortest distinguishing name for given composition point against names.
        /// </summary>
        /// <param name="names">Collection of unavailable names.</param>
        /// <param name="compPointName">Name of composition point to display.</param>
        /// <returns>Distinguish name.</returns>
        private string distinguishName(HashSet<string> names, CompositionPoint compositionPoint)
        {
            var compPointName = Naming.GetMethodPath(compositionPoint.EntryMethod).Name;
            var subNames = compPointName.Split('.');
            subNames = subNames.Reverse().ToArray();

            var distName = subNames[0];
            for (var i = 1; names.Contains(distName) && subNames.Length > i || i <= 1; i++)
            {
                distName = subNames[i] + "." + distName;
            }

            names.Add(distName);
            return distName;
        }

        /// <summary>
        /// refresh composition point - because old one and selected have same hash, 
        /// but they can differ in ArgumentsProvider 
        /// </summary>
        private void refreshSelectedCompositionPoint()
        {
            if (_selectedCompositionPoint == null)
                //nothing selected - nothing to do
                return;

            var component = _appDomain.Loader.GetComponentInfo(_selectedCompositionPoint.DeclaringComponent);
            if (component == null)
                //component is no more available
                return;

            var compositionPoints = component.CompositionPoints;
            foreach (var compPoint in compositionPoints)
            {
                if (_selectedCompositionPoint.Equals(compPoint))
                {
                    //refresh composition point
                    _selectedCompositionPoint = compPoint;
                    break;
                }
            }
        }

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
            requireUpdate();
        }

        private void onComponentRemoved(ComponentInfo component)
        {
            foreach (var compositionPoint in component.CompositionPoints)
            {
                removeCompositionPoint(compositionPoint);
            }
            requireUpdate();
        }

        private void addCompositionPoint(CompositionPoint compositionPoint)
        {
            if (!_compositionPoints.ContainsKey(compositionPoint))
                _compositionPointAdds[compositionPoint.EntryMethod] = compositionPoint;
            _compositionPointRemoves.Remove(compositionPoint.EntryMethod);
        }

        private void removeCompositionPoint(CompositionPoint compositionPoint)
        {
            if (_compositionPoints.ContainsKey(compositionPoint))
                _compositionPointRemoves[compositionPoint.EntryMethod] = compositionPoint;
            _compositionPointAdds.Remove(compositionPoint.EntryMethod);
        }

        private void requireUpdate()
        {
            var action = new TransactionAction(() => FlushCompositionPointUpdates(), "UpdateCompositionPoints", (t) => t.Name == "UpdateCompositionPoints", this);
            Transactions.AttachAfterAction(null, action);
        }

        private ComboBoxItem createCompositionPointItem(CompositionPoint compositionPoint)
        {
            var itemContent = new TextBlock();
            itemContent.Text = compositionPoint.EntryMethod.MethodString;

            var item = new ComboBoxItem();
            item.Content = itemContent;
            item.Selected += (e, s) =>
            {
                dispatchedAction(new Action(() => onCompositionPointSelected(compositionPoint)));
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

        private void dispatchedAction(Action action)
        {
            _gui._Workspace.Dispatcher.BeginInvoke(action);
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

        private UIElement createLogEntryDrawing(LogEntry entry, bool expanded = false)
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
            expander.IsExpanded = expanded;

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
