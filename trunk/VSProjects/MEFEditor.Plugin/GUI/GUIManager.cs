using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;


using EnvDTE;

using MEFEditor.Drawing;
using MEFEditor.Analyzing;
using MEFEditor;
using MEFEditor.Interoperability;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;
using MEFEditor.TypeSystem.Transactions;

using RecommendedExtensions.Core.AssemblyProviders.CILAssembly;

namespace MEFEditor.Plugin.GUI
{

    /// <summary>
    /// Manager class used for handling GUI behaviour.
    /// </summary>
    public class GUIManager
    {
        #region Private members

        /// <summary>
        /// Gui managed by current manager.
        /// </summary>
        private readonly EditorGUI _gui;

        /// <summary>
        /// Factory used for diagram drawings.
        /// </summary>
        private AbstractDiagramFactory _diagramFactory;

        /// <summary>
        /// Appdomain where analysis is processed.
        /// </summary>
        private AppDomainServices _appDomain;

        /// <summary>
        /// WPF items according to defining composition points.
        /// </summary>
        private readonly Dictionary<CompositionPoint, ComboBoxItem> _compositionPoints = new Dictionary<CompositionPoint, ComboBoxItem>();

        /// <summary>
        /// List of composition point updates.
        /// </summary>
        private readonly Dictionary<MethodID, CompositionPoint> _compositionPointRemoves = new Dictionary<MethodID, CompositionPoint>();

        /// <summary>
        /// List of composition point updates.
        /// </summary>
        private readonly Dictionary<MethodID, CompositionPoint> _compositionPointAdds = new Dictionary<MethodID, CompositionPoint>();

        /// <summary>
        /// Composition that is desired by user to be displayed. It is used
        /// for remembering the composition point when is it not currently available.
        /// </summary>
        private MethodID _desiredCompositionPointMethod;

        /// <summary>
        /// Composition point that is currently selected.
        /// </summary>
        private CompositionPoint _selectedCompositionPoint;

        /// <summary>
        /// Assembly that is currently hosted.
        /// </summary>
        private AssemblyProvider _hostAssembly;

        /// <summary>
        /// Access point to drawing services and drawing engine.
        /// </summary>
        private DrawingProvider _drawingProvider;

        /// <summary>
        /// Available services exposed by visual studio.
        /// </summary>
        private readonly VisualStudioServices _vs;

        /// <summary>
        /// Queue of logged entries.
        /// </summary>
        private readonly Queue<LogEntry> _logQueue = new Queue<LogEntry>();

        #endregion

        /// <summary>
        /// Transaction available in current domain.
        /// </summary>
        /// <value>The transactions.</value>
        internal TransactionManager Transactions { get { return _appDomain.Transactions; } }

        /// <summary>
        /// Number of log entries displayed.
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
        /// Composition point that is currently selected.
        /// </summary>
        /// <value>The selected composition point.</value>
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
        /// Determine that item avoiding algorithm will be used.
        /// </summary>
        /// <value><c>true</c> if item avoidance should be used otherwise, <c>false</c>.</value>
        public bool UseItemAvoidance { get { return _gui.UseItemAvoidance; } }

        /// <summary>
        /// Determine that join avoiding algorithm will be used.
        /// </summary>
        /// <value><c>true</c> if join avoidance should be used otherwise, <c>false</c>.</value>
        public bool UseJoinAvoidance { get { return _gui.UseJoinAvoidance; } }

        /// <summary>
        /// Determine join lines should be displayed in composition scheme.
        /// </summary>
        /// <value><c>true</c> if joins should be shown otherwise, <c>false</c>.</value>
        public bool ShowJoinLines { get { return _gui.ShowJoinLines; } }

        /// <summary>
        /// Determine that composition scheme should be automatically refreshed
        /// when composition point change is detected.
        /// </summary>
        /// <value><c>true</c> if auto refresh should be used; otherwise, <c>false</c>.</value>
        public bool AutoRefresh { get { return _gui.AutoRefresh; } }

        /// <summary>
        /// Initialize instance of <see cref="GUIManager" />.
        /// </summary>
        /// <param name="gui">Gui managed by current manager.</param>
        /// <param name="vs">Available services exposed by visual studio.</param>
        public GUIManager(EditorGUI gui, VisualStudioServices vs = null)
        {
            _gui = gui;
            _vs = vs;

            //initialize logging as soon as possible
            if (_vs != null)
                _vs.Log.OnLog += logHandler;
        }

        /// <summary>
        /// Initialization routine that makes <see cref="GUIManager" /> active.
        /// </summary>
        /// <param name="appDomain">AppDomain where analysis is processed.</param>
        /// <param name="diagramFactory">Factory used for diagram drawings.</param>
        public void Initialize(AppDomainServices appDomain, AbstractDiagramFactory diagramFactory)
        {
            _appDomain = appDomain;
            _diagramFactory = diagramFactory;

            _drawingProvider = new DrawingProvider(_gui.Workspace, _diagramFactory);

            hookEvents();
            initialize();
        }

        #region Display handling

        /// <summary>
        /// Display given <see cref="DiagramDefinition" /> within editors workspace.
        /// </summary>
        /// <param name="diagram">Displayed diagram.</param>
        public void Display(DiagramDefinition diagram)
        {
            DispatchedAction(() =>
            {
                if (diagram != null)
                    //prevent showing workspace when
                    //there is no diagram
                    _gui.ShowWorkspace();

                _drawingProvider.Display(diagram);
            });
        }

        /// <summary>
        /// Show workspace to the user.
        /// </summary>
        public void ShowWorkspace()
        {
            DispatchedAction(() =>
            {
                _gui.ShowWorkspace();
            });
        }

        /// <summary>
        /// Display specified entry to the user.
        /// </summary>
        /// <param name="entry">Entry to be displayed.</param>
        internal void DisplayEntry(LogEntry entry)
        {
            DispatchedAction(() =>
            {
                var entryDrawing = createLogEntryDrawing(entry) as Expander;

                var heading = entryDrawing.Header as TextBox;
                heading.FontSize = 20;

                entryDrawing.Margin = new Thickness(20);
                _gui.ShowElement(entryDrawing);
            });
        }

        /// <summary>
        /// Display message with loading bar.
        /// </summary>
        /// <param name="message">Displayed loading message.</param>
        public void DisplayLoadingMessage(string message)
        {
            DispatchedAction(() =>
            {
                _gui.ShowLoadingMessage(message);
            });
        }

        /// <summary>
        /// Reset workspace.
        /// </summary>
        public void ResetWorkspace()
        {
            DispatchedAction(() =>
           {
               _gui.Workspace.Reset();
               _drawingProvider.ResetPositions();
               _drawingProvider.Redraw();
           });
        }

        #endregion

        #region Dispatcher handling

        /// <summary>
        /// Run given action in context of gui thread.
        /// </summary>
        /// <param name="action">Action to be run.</param>
        public void DispatchedAction(Action action)
        {
            _gui.Dispatcher.BeginInvoke(action);
        }


        /// <summary>
        /// Do events which is compatible with WPF and WinForms.
        /// </summary>
        public void DoEvents()
        {
            System.Windows.Forms.Application.DoEvents();
            var idle = System.Windows.Threading.DispatcherPriority.SystemIdle;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => { }), idle);
        }

        #endregion

        #region Initialization routines

        /// <summary>
        /// Hook events needed for GUI interaction.
        /// </summary>
        private void hookEvents()
        {
            _appDomain.ComponentAdded += onComponentAdded;
            _appDomain.ComponentRemoved += onComponentRemoved;

            _appDomain.AssemblyAdded += onAssemblyAdded;
            _appDomain.AssemblyRemoved += onAssemblyRemoved;

            _gui.HostPathChanged += onHostPathChanged;
            _gui.RefreshClicked += forceRefresh;
            _gui.DrawingSettingsChanged += forceRefresh;
            _gui.LogRefresh += logRefresh;
        }

        /// <summary>
        /// Initialize GUI according to current environment state.
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


        /// <summary>
        /// Disables hosted application interpreting.
        /// </summary>
        internal void DisableHostedApplication()
        {
            _gui._CrossInterpretationValue.IsChecked = false;
        }

        /// <summary>
        /// Handler called on assembly removed.
        /// </summary>
        /// <param name="provider">The assembly provider.</param>
        private void onAssemblyRemoved(AssemblyProvider provider)
        {
            DispatchedAction(() =>
            {
                //TODO remove mapping changed handler
                _gui.Assemblies.RemoveItem(provider);
            });
        }

        /// <summary>
        /// Handler called on assembly added.
        /// </summary>
        /// <param name="provider">The assembly provider.</param>
        private void onAssemblyAdded(AssemblyProvider provider)
        {
            DispatchedAction(() =>
            {
                var assemblyItem = createAssemblyItem(provider);
                assemblyItem.MappingChanged += onAssemblyMappingChanged;
                _gui.Assemblies.AddItem(provider, assemblyItem);
            });
        }

        /// <summary>
        /// Creates the assembly item displayed in settings.
        /// </summary>
        /// <param name="assembly">The assembly provider.</param>
        /// <returns>Created item.</returns>
        private AssemblyItem createAssemblyItem(AssemblyProvider assembly)
        {
            return new AssemblyItem(assembly);
        }

        /// <summary>
        /// Handler called on assembly mapping changed.
        /// </summary>
        /// <param name="assembly">The assembly provider.</param>
        private void onAssemblyMappingChanged(AssemblyProvider assembly)
        {
            forceRefresh();
        }

        #endregion

        #region Cross interpreting handling

        /// <summary>
        /// Handler called on the host path changed.
        /// </summary>
        /// <param name="path">The path.</param>
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

        /// <summary>
        /// Flushes the composition point list updates.
        /// </summary>
        /// <returns><c>true</c> if list has been changed, <c>false</c> otherwise.</returns>
        internal bool FlushCompositionPointUpdates()
        {
            //apply composition point removings
            foreach (var compositionPoint in _compositionPointRemoves.Values)
            {
                var item = _compositionPoints[compositionPoint];
                _compositionPoints.Remove(compositionPoint);

                _gui.CompositionPoints.Items.Remove(item);
            }
            _compositionPointRemoves.Clear();

            //apply composition point addings
            foreach (var compositionPoint in _compositionPointAdds.Values)
            {
                var item = createCompositionPointItem(compositionPoint);

                if (compositionPoint.IsExplicit)
                {
                    //send explicit composition points at list begining
                    _gui.CompositionPoints.Items.Insert(1, item);
                }
                else
                {
                    //implicit composition points will remain at bottom
                    _gui.CompositionPoints.Items.Add(item);
                }
                _compositionPoints.Add(compositionPoint, item);
            }
            _compositionPointAdds.Clear();


            //find desired composition point
            var selectedIndex = 0;
            foreach (var compositionPointPair in _compositionPoints)
            {
                var compositionPoint = compositionPointPair.Key;
                var isDesiredCompositionPoint = _desiredCompositionPointMethod != null && _desiredCompositionPointMethod.Equals(compositionPoint.EntryMethod);

                if (isDesiredCompositionPoint)
                {
                    var item = compositionPointPair.Value;
                    selectedIndex = _gui.CompositionPoints.Items.IndexOf(item);

                    break;
                }
            }

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
        /// Refresh names of composition points, so they are as short as possible.
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
        /// <param name="compositionPoint">Name of composition point to display.</param>
        /// <returns>Distinguish name.</returns>
        private string distinguishName(HashSet<string> names, CompositionPoint compositionPoint)
        {
            var rawName = Naming.GetMethodPath(compositionPoint.EntryMethod).Name;

            var compPointName = rawName.Replace("." + Naming.CtorName, "").Replace("." + Naming.ClassCtorName, "");
            var subNames = compPointName.Split('.');
            subNames = subNames.Reverse().ToArray();

            //allow single part names for class names
            var minLength = rawName == compPointName ? 2 : 1;

            var distName = subNames[0];
            for (var i = 1; (names.Contains(distName) || i < minLength) && subNames.Length > i; i++)
            {
                distName = subNames[i] + "." + distName;
            }

            names.Add(distName);
            return distName;
        }

        /// <summary>
        /// refresh composition point - because old one and selected have same hash,
        /// but they can differ in ArgumentsProvider.
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

        /// <summary>
        /// Forces the composition scheme refresh.
        /// </summary>
        private void forceRefresh()
        {
            onCompositionPointSelected(SelectedCompositionPoint);
        }

        /// <summary>
        /// Handler called on component added.
        /// </summary>
        /// <param name="component">The added component.</param>
        private void onComponentAdded(ComponentInfo component)
        {
            DispatchedAction(() =>
            {
                foreach (var compositionPoint in component.CompositionPoints)
                {
                    addCompositionPoint(compositionPoint);
                }
                requireUpdate();
            });
        }

        /// <summary>
        /// Handler called on component removed.
        /// </summary>
        /// <param name="component">The removed component.</param>
        private void onComponentRemoved(ComponentInfo component)
        {
            DispatchedAction(() =>
            {
                foreach (var compositionPoint in component.CompositionPoints)
                {
                    removeCompositionPoint(compositionPoint);
                }
                requireUpdate();
            });
        }

        /// <summary>
        /// Adds the composition point.
        /// </summary>
        /// <param name="compositionPoint">The composition point.</param>
        private void addCompositionPoint(CompositionPoint compositionPoint)
        {
            DispatchedAction(() =>
            {
                if (!_compositionPoints.ContainsKey(compositionPoint))
                    _compositionPointAdds[compositionPoint.EntryMethod] = compositionPoint;
                _compositionPointRemoves.Remove(compositionPoint.EntryMethod);

            });
        }

        /// <summary>
        /// Removes the composition point.
        /// </summary>
        /// <param name="compositionPoint">The composition point.</param>
        private void removeCompositionPoint(CompositionPoint compositionPoint)
        {
            DispatchedAction(() =>
            {
                if (_compositionPoints.ContainsKey(compositionPoint))
                    _compositionPointRemoves[compositionPoint.EntryMethod] = compositionPoint;
                _compositionPointAdds.Remove(compositionPoint.EntryMethod);
            });
        }

        /// <summary>
        /// Requires updating of composition point list.
        /// </summary>
        private void requireUpdate()
        {
            DispatchedAction(() =>
            {
                var action = new TransactionAction(() => FlushCompositionPointUpdates()
                 , "UpdateCompositionPoints", (t) => t.Name == "UpdateCompositionPoints", this);
                Transactions.AttachAfterAction(null, action);
            });
        }

        /// <summary>
        /// Creates the composition point item.
        /// </summary>
        /// <param name="compositionPoint">The composition point.</param>
        /// <returns>Created item.</returns>
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

        /// <summary>
        /// Creates the none composition point item.
        /// </summary>
        /// <returns>Created item.</returns>
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

            item.PreviewMouseDown += (e, s) =>
            {
                _desiredCompositionPointMethod = null;
            };

            return item;
        }

        /// <summary>
        /// Event handler for composition point items.
        /// </summary>
        /// <param name="selectedCompositionPoint">The selected composition point.</param>
        private void onCompositionPointSelected(CompositionPoint selectedCompositionPoint)
        {
            DispatchedAction(() =>
            {
                SelectedCompositionPoint = selectedCompositionPoint;

                if (CompositionPointSelected != null)
                    CompositionPointSelected();
            });
        }

        #endregion

        #region Logging service handling

        /// <summary>
        /// Handler called for every logged entry.
        /// </summary>
        /// <param name="entry">Logged entry.</param>
        private void logHandler(LogEntry entry)
        {
            DispatchedAction(() =>
            {
                _logQueue.Enqueue(entry);

                while (_logQueue.Count > LogHistorySize)
                    _logQueue.Dequeue();

                if (_gui.IsLogVisible)
                    drawLogEntry(entry);
            });
        }

        /// <summary>
        /// Draw given entry into Log.
        /// </summary>
        /// <param name="entry">Entry to draw.</param>
        private void drawLogEntry(LogEntry entry)
        {
            var drawing = createLogEntryDrawing(entry);

            _gui.Log.Children.Insert(0, drawing);
            while (_gui.Log.Children.Count > LogHistorySize)
                _gui.Log.Children.RemoveAt(LogHistorySize);
        }

        /// <summary>
        /// Refresh displayed log entries.
        /// </summary>
        private void logRefresh()
        {
            foreach (var entry in _logQueue)
            {
                drawLogEntry(entry);
            }
        }

        /// <summary>
        /// Creates drawing for given entry.
        /// </summary>
        /// <param name="entry">Entry which drawing will be created.</param>
        /// <param name="expanded">Determine that drawing should be expanded.</param>
        /// <returns>Created drawing.</returns>
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

            var heading = createText(entry.Message);
            heading.Foreground = entryColor;

            //set navigation handler
            setContextMenu(heading, entry);

            if (entry.Description == null)
                //no description is available
                return heading;

            var expander = new Expander();
            expander.Header = heading;
            expander.IsExpanded = expanded;

            var description = createText(entry.Description);
            description.Margin = new Thickness(10, 0, 0, 10);

            //set navigation handler
            setContextMenu(description, entry);

            expander.Content = description;
            expander.Margin = new Thickness(20, 0, 0, 0);

            return expander;
        }

        /// <summary>
        /// Creates text representation within created <see cref="TextBlock" />.
        /// </summary>
        /// <param name="text">Text to be represented.</param>
        /// <returns>Created text.</returns>
        private TextBox createText(string text)
        {
            var heading = new TextBox();
            heading.IsReadOnly = true;
            heading.Text = text;
            heading.TextWrapping = TextWrapping.Wrap;
            heading.BorderBrush = Brushes.Transparent;
            heading.BorderThickness = new Thickness(0);

            return heading;
        }

        /// <summary>
        /// Set context menu routines to given element.
        /// </summary>
        /// <param name="element">Element which context menu will be set.</param>
        /// <param name="entry">Entry defining context menu.</param>
        private void setContextMenu(FrameworkElement element, LogEntry entry)
        {
            var toolText = new TextBlock();
            toolText.Text = "Right click to open menu";

            element.ToolTip = toolText;

            var contextMenu = new ContextMenu();
            element.ContextMenu = contextMenu;

            if (entry.Navigate != null)
            {
                var navigate = new MenuItem();
                navigate.Header = "Navigate to";
                navigate.Click += (s, e) => _vs.SafeRunAction(entry.Navigate, "Navigation fialed");
                contextMenu.Items.Add(navigate);
            }

            var copy = new MenuItem();
            copy.Header = "Copy to clipboard";
            copy.Click += (s, e) =>
                _vs.SafeRunAction(() => Clipboard.SetText(entry.ToString()), "Clipboard text setting failed");
            contextMenu.Items.Add(copy);
        }

        #endregion
    }
}
