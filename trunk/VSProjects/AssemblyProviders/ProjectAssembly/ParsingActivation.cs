using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;

namespace AssemblyProviders.ProjectAssembly
{
    /// <summary>
    /// Handler for events raised for source changes
    /// </summary>
    /// <param name="commitedSource">Source that has been commited</param>
    public delegate void SourceChangeCommitedEvent(string commitedSource);

    /// <summary>
    /// Handler for events raised when user wants to navigate at given offset
    /// </summary>
    /// <param name="offset">Offset of navigation target</param>
    public delegate void NavigationRequestEvent(int offset);

    /// <summary>
    /// Activation describing method that could be parsed
    /// </summary>
    public class ParsingActivation
    {
        /// <summary>
        /// Source code of parsed method
        /// </summary>
        internal readonly string SourceCode;

        /// <summary>
        /// <see cref="TypeMethodInfo"/> describing generated method
        /// </summary>
        internal readonly TypeMethodInfo Method;

        /// <summary>
        /// Generic parameters of method path. They are used for translating source codes according to
        /// generic parameters.
        /// </summary>
        internal readonly IEnumerable<string> GenericParameters;

        /// <summary>
        /// Namespaces that are available for current method
        /// </summary>
        internal readonly IEnumerable<string> Namespaces;

        /// <summary>
        /// Determine that method has to be compiled inline 
        /// </summary>
        internal bool IsInline { get { return Method.MethodName == InlineMethodName; } }

        /// <summary>
        /// Event fired whenever change on this source is commited
        /// </summary>
        public event SourceChangeCommitedEvent SourceChangeCommited;

        /// <summary>
        /// Event fired whenever navigation request is detected
        /// </summary>
        public event NavigationRequestEvent NavigationRequested;

        /// <summary>
        /// Name for methods that will be treated as inline ones by compiler
        /// </summary>
        public static readonly string InlineMethodName = "#inline";

        public ParsingActivation(string sourceCode, TypeMethodInfo method, IEnumerable<string> genericParameters, IEnumerable<string> namespaces = null)
        {
            if (sourceCode == null)
                throw new ArgumentNullException("sourceCode");

            if (genericParameters == null)
                throw new ArgumentNullException("genericParameterrs");

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
        /// Method for reporting commits on represented source code
        /// </summary>
        /// <param name="commitedSource">Code that has been commited</param>
        internal void OnCommited(string commitedSource)
        {
            if (SourceChangeCommited != null)
                SourceChangeCommited(commitedSource);
        }

        /// <summary>
        /// Method for reporting navigation requests
        /// </summary>
        /// <param name="offset">Offset of navigation target</param>
        internal void OnNavigated(int offset)
        {
            if (NavigationRequested != null)
                NavigationRequested(offset);
        }
    }
}
