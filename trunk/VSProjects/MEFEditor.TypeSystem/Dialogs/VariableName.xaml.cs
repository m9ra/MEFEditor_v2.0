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

using System.Text.RegularExpressions;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;

using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.TypeSystem.Dialogs
{
    /// <summary>
    /// Dialog provider for variable name selection.
    /// </summary>
    public partial class VariableName : Window
    {
        /// <summary>
        /// Name of variable that has been selected by user. Name is validated according
        /// to basic naming rules, context duplicity is checked.
        /// If no name is selected null is present.
        /// </summary>
        /// <value>The name of the result.</value>
        public string ResultName { get; private set; }

        /// <summary>
        /// Context of call, where new variable name will be used. Is used for
        /// duplicity checking.
        /// </summary>
        private readonly CallContext _context;

        /// <summary>
        /// Validator for variable form.
        /// </summary>
        static readonly Regex _variableValidator = new Regex(@"^[a-zA-Z]\w*$", RegexOptions.Compiled);

        /// <summary>
        /// Keywords that cannot be used as variable names.
        /// </summary>
        static readonly HashSet<string> _keywords = new HashSet<string>(){
            //TODO extend this list
            "while", "do", "this", "self", "until", "base", "class", "interface", "public", "protected",
        };

        /// <summary>
        /// Create dialog for selecting.
        /// </summary>
        /// <param name="initialName">Initial hint that will be offered to user.</param>
        /// <param name="context">Context of call, where new variable name will be used. Is used for duplicity checking.</param>
        private VariableName(string initialName, CallContext context)
        {
            InitializeComponent();

            _context = context;
            Input.Text = initialName;
            Input.TextChanged += (e, s) => hasError();

            hasError();
        }

        /// <summary>
        /// Provide variable name dialog for creation edit of given definition.
        /// </summary>
        /// <param name="definition">Definition which creation edit is active.</param>
        /// <param name="creationContext">Context of call, where new variable name will be used. Is used for duplicity checking.</param>
        /// <returns>Name of variable that has been selected by user. Name is validated according
        /// to basic naming rules, context duplicity is checked.
        /// If no name is selected null is returned.</returns>
        public static string GetName(RuntimeTypeDefinition definition, CallContext creationContext)
        {
            return GetName(definition.TypeInfo, creationContext);
        }


        /// <summary>
        /// Provide variable name dialog for creation edit of given type.
        /// </summary>
        /// <param name="type">The created type.</param>
        /// <param name="creationContext">Context of call, where new variable name will be used. Is used for duplicity checking.</param>
        /// <returns>Name of variable that has been selected by user. Name is validated according
        /// to basic naming rules, context duplicity is checked.
        /// If no name is selected null is returned.</returns>
        public static string GetName(InstanceInfo type, CallContext creationContext)
        {
            var defaultName = getDefaultName(type, creationContext);
            var dialog = new VariableName(defaultName, creationContext);
            dialog.ShowDialog();
            return dialog.ResultName;
        }


        /// <summary>
        /// Create default name hint for given type.
        /// </summary>
        /// <param name="type">Definition which name is created.</param>
        /// <param name="context">Context used for uniqueness guaranty.</param>
        /// <returns>Created name.</returns>
        private static string getDefaultName(InstanceInfo type, CallContext context)
        {
            var basename = Naming.SplitGenericPath(type.TypeName).Last();
            basename = char.ToLowerInvariant(basename[0]) + basename.Substring(1);

            var name = basename;
            var variableNumber = 0;
            while (context.IsVariableDefined(name))
            {
                ++variableNumber;
                name = basename + variableNumber;
            }

            return name;
        }

        /// <summary>
        /// Determine that current name has validation error.
        /// </summary>
        /// <returns>True if there is validation error in variable name.</returns>
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
            else if (_context.IsVariableDefined(name))
            {
                error = "There already exists variable with same name";
            }

            var hasError = error != null;

            Error.Text = error;
            Error.Visibility = hasError ? Visibility.Visible : Visibility.Hidden;

            OK.IsEnabled = !hasError;

            return hasError;
        }

        /// <summary>
        /// Handles the Click event of the OK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!hasError())
            {
                ResultName = Input.Text;
                DialogResult = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the Storno control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Storno_Click(object sender, RoutedEventArgs e)
        {
            ResultName = null;
            DialogResult = true;
        }
    }
}
