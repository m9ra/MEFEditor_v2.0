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

namespace Plugin.GUI
{
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
        private bool _crossInterpretingEnabled = false;

        public DiagramCanvas Workspace { get { return _Workspace; } }

        public SettingsSection Assemblies { get { return _Assemblies; } }

        public StackPanel Log { get { return _Log; } }

        public ComboBox CompositionPoints { get { return _CompositionPoints; } }


        /// <summary>
        /// Event for handling changes in host application path
        /// </summary>
        public event PathChange HostPathChanged;

        /// <summary>
        /// Event for handling requests for workspace refresh
        /// </summary>
        public event Action RefreshClicked;

        public string HostPath
        {
            set
            {
                if (value == "")
                    value = null;

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

        public EditorGUI()
        {
            InitializeComponent();
            settingsVisibility(false);
        }

        #region Basic GUI events handling

        private void switch_Click(object sender, RoutedEventArgs e)
        {
            toggleSettingsVisibility();
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            if (isSettingsVisible())
            {
                settingsVisibility(false);
            }

            if (RefreshClicked != null)
                RefreshClicked();
        }

        private void interpretingEnabled_Checked(object sender, RoutedEventArgs e)
        {
            HostPath = _HostPath.Text;
        }

        private void interpretingEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            HostPath = null;
        }

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

        #endregion

        #region Settings tab switching

        private void toggleSettingsVisibility()
        {
            var isVisible = isSettingsVisible();

            settingsVisibility(!isVisible);
        }

        private bool isSettingsVisible()
        {
            return _Settings.Visibility == Visibility.Visible;
        }

        private void settingsVisibility(bool isVisible)
        {
            string settingsButtonText;

            if (isVisible)
            {
                _Settings.Visibility = Visibility.Visible;
                _Workspace.Visibility = Visibility.Hidden;
                settingsButtonText = "Workspace";
            }
            else
            {
                _Settings.Visibility = Visibility.Hidden;
                _Workspace.Visibility = Visibility.Visible;
                settingsButtonText = "Settings";
            }

            var switchCaption = new TextBlock();
            switchCaption.Text = settingsButtonText;

            ScreenSwitchButton.Content = switchCaption;
        }

        #endregion
    }
}
