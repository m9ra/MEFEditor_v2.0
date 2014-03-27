using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeSystem
{
    /// <summary>
    /// Exception that should be thrown from parsers when 
    /// error within source code of emitted method is detected
    /// </summary>
    public class ParsingException : Exception
    {
        public ParsingException(string errorDescription, Action navigate)
            : base("Error during parsing detected. " + errorDescription)
        {
        }
    }
}
