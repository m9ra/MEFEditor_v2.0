using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Exception that should be thrown from parsers when
    /// error within source code of emitted method is detected.
    /// </summary>
    public class ParsingException : Exception
    {
        /// <summary>
        /// Navigation action which can navigate to place
        /// where parsing error occurred. If navigation is not available
        /// it is <c>null</c>.
        /// </summary>
        public readonly Action Navigate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingException"/> class.
        /// </summary>
        /// <param name="errorDescription">The error description.</param>
        /// <param name="navigate">The error navigation.</param>
        public ParsingException(string errorDescription, Action navigate)
            : base("Error during parsing detected. " + errorDescription)
        {
            Navigate = navigate;
        }
    }
}
