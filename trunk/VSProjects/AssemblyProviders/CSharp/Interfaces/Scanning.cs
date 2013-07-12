using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyProviders.CSharp.Interfaces
{
    /// <summary>
    /// Operations provided by lexer.
    /// </summary>
    interface ILexer
    {
        /// <summary>
        /// Determine if lexert has proceeded last token.
        /// </summary>
        bool End { get; }
        /// <summary>
        /// Current token.
        /// </summary>
        IToken Current { get; }
        /// <summary>
        /// Move to next token.
        /// </summary>
        /// <returns>Return token which was current before move.</returns>
        IToken Move();
    }

    /// <summary>
    /// Token representation.
    /// </summary>
    public interface IToken
    {
        /// <summary>
        /// Token value.
        /// </summary>
        string Value { get; }
        /// <summary>
        /// Token position in source code.
        /// </summary>
        IPosition Position { get; }
        /// <summary>
        /// Previous token.
        /// </summary>
        IToken Previous { get; }
        /// <summary>
        /// Next token.
        /// </summary>
        IToken Next { get; }
    }  
}
