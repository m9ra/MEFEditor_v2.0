using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;

namespace RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly
{
    /// <summary>
    /// Handler for events raised for source changes
    /// </summary>
    /// <param name="commitedSource">Source that has been commited</param>
    /// <param name="requiredNamespaces">Namespaces that are required for code change</param>
    public delegate void SourceChangeCommitedEvent(string commitedSource, IEnumerable<string> requiredNamespaces);

    /// <summary>
    /// Handler for events raised when user wants to navigate at given offset
    /// </summary>
    /// <param name="offset">Offset of navigation target</param>
    public delegate void NavigationRequestEvent(int offset);

    /// <summary>
    /// Activation describing method that could be parsed.
    /// </summary>
    public class ParsingActivation
    {
        /// <summary>
        /// Source code of parsed method.
        /// </summary>
        internal readonly string SourceCode;

        /// <summary>
        /// <see cref="TypeMethodInfo" /> describing generated method.
        /// </summary>
        internal readonly TypeMethodInfo Method;

        /// <summary>
        /// Generic parameters of method path. They are used for translating source codes according to
        /// generic parameters.
        /// </summary>
        internal readonly IEnumerable<string> GenericParameters;

        /// <summary>
        /// Namespaces that are available for current method.
        /// </summary>
        internal readonly IEnumerable<string> Namespaces;

        /// <summary>
        /// Determine that method has to be compiled inline.
        /// </summary>
        /// <value><c>true</c> if this instance is inline; otherwise, <c>false</c>.</value>
        internal bool IsInline { get { return Method.MethodName == InlineMethodName; } }

        /// <summary>
        /// Event fired whenever change on this source is committed.
        /// </summary>
        public event SourceChangeCommitedEvent SourceChangeCommited;

        /// <summary>
        /// Event fired whenever navigation request is detected.
        /// </summary>
        public event NavigationRequestEvent NavigationRequested;

        /// <summary>
        /// Name for methods that will be treated as inline ones by compiler.
        /// </summary>
        public static readonly string InlineMethodName = "#inline";

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingActivation" /> class.
        /// </summary>
        /// <param name="sourceCode">The source code.</param>
        /// <param name="method">The method.</param>
        /// <param name="genericParameters">The generic parameters.</param>
        /// <param name="namespaces">The namespaces.</param>
        /// <exception cref="System.ArgumentNullException">genericParameters
        /// or
        /// method</exception>
        public ParsingActivation(string sourceCode, TypeMethodInfo method, IEnumerable<string> genericParameters, IEnumerable<string> namespaces = null)
        {
            /*  if (sourceCode == null)
                  throw new ArgumentNullException("sourceCode");*/

            if (genericParameters == null)
                throw new ArgumentNullException("genericParameters");

            if (method == null)
                throw new ArgumentNullException("method");

            if (namespaces == null)
                namespaces = new string[0];

            SourceCode = sourceCode;
            Method = method;

            //create defensive copy
            GenericParameters = genericParameters.ToArray();

            //create defensive copy
            Namespaces = namespaces.ToArray();
        }

        /// <summary>
        /// Method for reporting commits on represented source code.
        /// </summary>
        /// <param name="committedSource">Code that has been committed.</param>
        /// <param name="requiredNamespaces">Namespaces which presence is required.</param>
        internal void OnCommited(string committedSource, IEnumerable<string> requiredNamespaces)
        {
            if (SourceChangeCommited != null)
                SourceChangeCommited(committedSource, requiredNamespaces);
        }

        /// <summary>
        /// Method for reporting navigation requests.
        /// </summary>
        /// <param name="offset">Offset of navigation target.</param>
        internal void OnNavigated(int offset)
        {
            if (NavigationRequested != null)
                NavigationRequested(offset);
        }
    }
}
