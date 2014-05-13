using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;
using EnvDTE80;

namespace AssemblyProviders.ProjectAssembly
{
    /// <summary>
    /// Wrap <see cref="CodeAttribute2"/> and provide additional services for resolving arguments.
    /// </summary>
    class AttributeInfo
    {
        /// <summary>
        /// Storage for values of named arguments
        /// </summary>
        private readonly Dictionary<string, string> _namedArguments = new Dictionary<string, string>();

        /// <summary>
        /// Storage for values of positioned arguments
        /// </summary>
        private readonly List<string> _positionalArguments = new List<string>();

        /// <summary>
        /// Wrapped <see cref="CodeAttribute2"/>
        /// </summary>
        internal readonly CodeAttribute2 Element;

        /// <summary>
        /// Number of positional arguments
        /// </summary>
        internal int PositionalArgumentsCount { get { return _positionalArguments.Count; } }

        /// <summary>
        /// Initialize instance of <see cref="AttributeInfo"/>
        /// </summary>
        /// <param name="attribute">Wrapped attribute</param>
        internal AttributeInfo(CodeAttribute2 attribute)
        {
            Element = attribute;

            foreach (CodeAttributeArgument arg in attribute.Arguments)
            {
                var name = arg.Name;
                var value = arg.Value;

                if (name == null || name == "")
                {
                    _positionalArguments.Add(value);
                }
                else
                {
                    _namedArguments[name] = value;
                }
            }
        }

        /// <summary>
        /// Get value of named argument with given name
        /// </summary>
        /// <param name="name">Name of named argument</param>
        /// <returns>Value of named argument if any, <c>null</c> otherwise</returns>
        internal string GetArgument(string name)
        {
            string result;
            _namedArguments.TryGetValue(name, out result);

            return result;
        }

        /// <summary>
        /// Get value of positioned argument at given zero-based position
        /// </summary>
        /// <param name="position">Zero based position of argument</param>
        /// <returns>Value of positioned argument if any, <c>null</c> otherwise</returns>
        internal string GetArgument(int position)
        {
            if (_positionalArguments.Count <= position)
            {
                return null;
            }

            return _positionalArguments[position];
        }

        /// <summary>
        /// Determine that value of named argumet with given name is set to true
        /// </summary>
        /// <param name="name">Name of named argument</param>
        /// <returns><c>true</c> if argument is set to true, <c>false</c> otherwise</returns>
        internal bool IsTrue(string name)
        {
            var value = GetArgument(name);
            if (value == null)
                return false;

            //TODO is this correct form ?
            return value == "true";
        }
    }
}
