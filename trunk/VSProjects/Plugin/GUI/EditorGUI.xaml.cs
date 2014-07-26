using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;

using Drawing;

using MEFEditor.Plugin.GUI.LoadingControl;

namespace Plugin.GUI
{
    /// <summary>
    /// Enumeration of content panels that can be displayed in
    /// GUI's main area.
    /// </summary>
    internal enum ContentPanel { Workspace, Settings, Loading, Element };

    /// <summary>
    /// Delagate used for reporting changes in path
    /// </summary>
    /// <param name="path">Path that has been set</param>
    public delegate void PathChange(string path);

    /// <summary>
    /// Interaction logic for EditorGUI.xaml
    /// </summary>
    public partial class EditorGUI : UserControl
    {
        /// <summary>
        /// Determine that cross interpreting is enabled in settings
        /// </summary>
        private bool _crossInterpretingEnabled = false;

        /// <summary>
        /// Main workspace where composition scheme is drawed
        /// </summary>
        public DiagramCanvas Workspace { get { return _Workspace; } }

        /// <summary>
        /// Settings of assemblies
        /// </summary>
        public SettingsSection Assemblies { get { return _Assemblies; } }

        /// <summary>
        /// Log where messages to user are displayed
        /// </summary>
        public StackPanel Log { get { return _Log; } }

        /// <summary>
        /// Event fired whenever log refresh is needed
        /// </summary>
        public event Action LogRefresh;

        /// <summary>
        /// Determine that log is visible to user
        /// </summary>
        public bool IsLogVisible { get { return logsExpander.IsExpanded; } }

        /// <summary>
        /// List of composition points provided to user
        /// </summary>
        public ComboBox CompositionPoints { get { return _CompositionPoints; } }

        /// <summary>
        /// Event for handling changes in host application path
        /// </summary>
        public event PathChange HostPathChanged;

        /// <summary>
        /// Event for handling requests for workspace refresh
        /// </summary>
        public event Action RefreshClicked;

        /// <summary>
        /// Event for handling notices when drawing settings has been changed
        /// </summary>
        public event Action DrawingSettingsChanged;

        /// <summary>
        /// Determine that item avoiding algorithm will be used
        /// </summary>
        public bool UseItemAvoidance
        {
            get
            {
                var value = _UseItemAvoidance.IsChecked;
                return value.HasValue && value.Value;
            }
        }

        /// <summary>
        /// Determine that join avoiding algorithm will be used
        /// </summary>
        public bool UseJoinAvoidance
        {
            get
            {
                var value = _UseJoinAvoidance.IsChecked;
                return value.HasValue && value.Value;
            }
        }

        /// <summary>
        /// Determine that composition scheme should be automatically refreshed
        /// when composition point change is detected
        /// </summary>
        public bool AutoRefresh
        {
            get
            {
                var value = _AutoRefresh.IsChecked;
                return value.HasValue && value.Value;
            }
        }

        /// <summary>
        /// Determine join lines should be dipslayed in composition scheme
        /// </summary>
        public bool ShowJoinLines
        {
            get
            {
                var value = _ShowJoinLines.IsChecked;
                return value.HasValue && value.Value;
            }
        }

        /// <summary>
        /// Path for host application defined within runtime settings section
        /// </summary>
        public string HostPath
        {
            set
            {
                if (value == "")
                    value = null;

                //check incomming value
                var hasValue = value != null;
                var hasSamePath = _HostPath.Text == value;
                var hasSameStatus = _crossInterpretingEnabled == hasValue;

                if (hasSamePath && hasSameStatus)
                    //nothing has changed
                    return;

                if (hasValue)
                {
                    _HostPath.Text = value;
                }

                //because of avoiding recursion
                _crossInterpretingEnabled = hasValue;
                _CrossInterpretationValue.IsChecked = hasValue;

                if (HostPathChanged != null)
                    HostPathChanged(value);
            }

            get
            {
                if (!_CrossInterpretationValue.IsChecked.Value)
                {
                    return null;
                }

                return _HostPath.Text;
            }
        }

        /// <summary>
        /// Initializes new instance of editor gui
        /// </summary>
        public EditorGUI()
        {
            InitializeComponent();
            setVisibleContent(ContentPanel.Workspace);
        }

        #region Display handlinga

        /// <summary>
        /// Show specified loading message
        /// </summary>
        /// <param name="message">Message that will be shown to user</param>
        public void ShowLoadingMessage(string message)
        {
            _Loading.Message.Text = message;
            if (!isLoadingVisible())
                setVisibleContent(ContentPanel.Loading);
        }

        /// <summary>
        /// Show workspace so it can be visible to user
        /// </summary>
        public void ShowWorkspace()
        {
            setVisibleContent(ContentPanel.Workspace);
        }

        /// <summary>
        /// Show given element so it can be visible to user
        /// </summary>
        /// <param name="element">Shown element</param>
        public void ShowElement(FrameworkElement element)
        {
            _Element.Children.Clear();
            _Element.Children.Add(element);
            element.MaxWidth = _Workspace.ActualWidth;
            setVisibleContent(ContentPanel.Element);
        }

        #endregion

        #region Basic GUI events handling

        /// <summary>
        /// GUI event handler for drawing settings changes
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event object</param>
        private void drawingSettings_Changed(object sender, RoutedEventArgs e)
        {
            if (DrawingSettingsChanged != null)
                DrawingSettingsChanged();
        }

        /// <summary>
        /// GUI event handler for switch button
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event object</param>
        private void switch_Click(object sender, RoutedEventArgs e)
        {
            toggleSettingsVisibility();
        }

        /// <summary>
        /// GUI event handler refresh button
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event object</param>
        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            if (!isWorkspaceVisible())
            {
                setVisibleContent(ContentPanel.Workspace);
            }

            if (RefreshClicked != null)
                RefreshClicked();
        }

        /// <summary>
        /// GUI event handler enabling cross interpretation
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event object</param>
        private void interpretingEnabled_Checked(object sender, RoutedEventArgs e)
        {
            HostPath = _HostPath.Text;
        }

        /// <summary>
        /// GUI event handler for disabling cross interpretation 
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event object</param>
        private void interpretingEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            HostPath = null;
        }

        /// <summary>
        /// GUI event handler for selecting hosted application path
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event object</param>
        private void hostPath_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();

            dialog.DefaultExt = ".exe";
            dialog.Filter = "Assembly files (*.dll;*.exe)|*.exe;*.dll|All files (*.*)|*.*";

            var initialDirectory = _HostPath.Text;
            if (initialDirectory != null && initialDirectory != "")
            {
                initialDirectory = System.IO.Path.GetDirectoryName(initialDirectory);
            }

            dialog.InitialDirectory = initialDirectory;

            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                HostPath = dialog.FileName;
            }
        }

        /// <summary>
        /// GUI event handler for expanding log
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event object</param>
        private void logExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource != logsExpander)
                //misplaced event
                return;

            e.Handled = true;
            logsExpander.Header = "Hide logs";

            if (LogRefresh != null)
                LogRefresh();
        }

        /// <summary>
        /// GUI event handler for collapsing log
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event object</param>
        private void logExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource != logsExpander)
                //misplaced event
                return;

            Log.Children.Clear();
            e.Handled = true;
            logsExpander.Header = "Show logs";
        }

        #endregion

        #region Content panels switching

        /// <summary>
        /// Toggle between workspace and settings visiblity
        /// </summary>
        private void toggleSettingsVisibility()
        {
            var isVisible = isWorkspaceVisible();

            var toToggle = isVisible ? ContentPanel.Settings : ContentPanel.Workspace;
            setVisibleContent(toToggle);
        }

        /// <summary>
        /// Determine that workspace is visible
        /// </summary>
        /// <returns><c>true</c> if workspace is visible, <c>false</c> otherwise</returns>
        private bool isWorkspaceVisible()
        {
            return _Workspace.Visibility == Visibility.Visible;
        }

        /// <summary>
        /// Determine that loading screen is visible
        /// </summary>
        /// <returns><c>true</c> if loading screen is visible, <c>false</c> otherwise</returns>
        private bool isLoadingVisible()
        {
            return _Loading.Visibility == Visibility.Visible;
        }

        /// <summary>
        /// Set content screen that will be visible
        /// </summary>
        /// <param name="panel">Panel of content screen that will be visible</param>
        private void setVisibleContent(ContentPanel panel)
        {
            string settingsButtonText;

            switch (panel)
            {
                case ContentPanel.Loading:
                    _Loading.Visibility = Visibility.Visible;
                    _Workspace.Visibility = Visibility.Hidden;
                    _Settings.Visibility = Visibility.Hidden;
                    _Element.Visibility = Visibility.Hidden;
                    settingsButtonText = null;
                    break;

                case ContentPanel.Settings:
                    _Loading.Visibility = Visibility.Hidden;
                    _Settings.Visibility = Visibility.Visible;
                    _Workspace.Visibility = Visibility.Hidden;
                    _Element.Visibility = Visibility.Hidden;
                    settingsButtonText = "Workspace";
                    break;

                case ContentPanel.Workspace:
                    _Loading.Visibility = Visibility.Hidden;
                    _Settings.Visibility = Visibility.Hidden;
                    _Workspace.Visibility = Visibility.Visible;
                    _Element.Visibility = Visibility.Hidden;
                    settingsButtonText = "Settings";
                    break;

                case ContentPanel.Element:
                    _Loading.Visibility = Visibility.Hidden;
                    _Settings.Visibility = Visibility.Hidden;
                    _Workspace.Visibility = Visibility.Hidden;
                    _Element.Visibility = Visibility.Visible;
                    settingsButtonText = "Settings";
                    break;
                    
                default:
                    throw new NotImplementedException("Unsupported content panel" + panel);
            }

            if (settingsButtonText == null)
            {
                ScreenSwitchButton.IsEnabled = false;
                RefreshButton.IsEnabled = false;
            }
            else
            {
                var switchCaption = new TextBlock();
                switchCaption.Text = settingsButtonText;

                ScreenSwitchButton.Content = switchCaption;
                ScreenSwitchButton.IsEnabled = true;
                RefreshButton.IsEnabled = true;
            }
        }

        #endregion
    }
}
