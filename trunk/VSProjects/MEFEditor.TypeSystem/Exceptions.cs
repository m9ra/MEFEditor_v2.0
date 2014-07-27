using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Exception that should be thrown from parsers when 
    /// error within source code of emitted method is detected
    /// </summary>
    public class ParsingException : Exception
    {
        public readonly Action Navigate;

        public ParsingException(string errorDescription, Action navigate)
            : base("Error during parsing detected. " + errorDescription)
        {
            Navigate = navigate;
        }
    }
}
