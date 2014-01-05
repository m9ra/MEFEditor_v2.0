﻿using System;
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

using System.Text.RegularExpressions;

using Analyzing;
using TypeSystem;
using TypeSystem.Runtime;

using MEFAnalyzers.Drawings;

namespace MEFAnalyzers.Dialogs
{
    /// <summary>
    /// Interaction logic for ComponentTypeDialog.xaml
    /// </summary>
    public partial class VariableName : Window
    {
        public string ResultName { get; private set; }

        static readonly Regex _variableValidator = new Regex(@"^[a-zA-Z]\w*$", RegexOptions.Compiled);

        static readonly HashSet<string> _keywords = new HashSet<string>(){
            //TODO extend this list
            "while", "do", "this", "self", "until", "base", "class", "interface", "public", "protected",
        };

        public VariableName(string initialName)
        {
            InitializeComponent();

            Input.Text = initialName;

            hasError();
        }

        public static string GetName(RuntimeTypeDefinition namedDefinition)
        {
            var dialog = new VariableName("test");

            dialog.ShowDialog();
            return dialog.ResultName;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!hasError())
            {
                ResultName = Input.Text;
                DialogResult = true;
            }
        }

        private void Storno_Click(object sender, RoutedEventArgs e)
        {
            ResultName = null;
            DialogResult = true;
        }

        private bool hasError()
        {
            //error that will be displayed
            string error = null;

            var name = Input.Text;
            if (name == null || name == "")
            {
                error = "Name cannot be blank";
            }
            else if (_keywords.Contains(name))
            {
                error = "Name is same as keyword";
            }
            else if (!_variableValidator.IsMatch(name))
            {
                error = "Name has incorrect format";
            }

            var hasError = error != null;

            Error.Text = error;
            Error.Visibility = hasError ? Visibility.Visible : Visibility.Hidden;

            return hasError;
        }


    }
}
