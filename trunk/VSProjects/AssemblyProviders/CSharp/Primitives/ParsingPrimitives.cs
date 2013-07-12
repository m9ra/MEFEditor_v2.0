using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Primitives
{
    /// <summary>
    /// Implementation for IPosition.
    /// </summary>
    class Position : IPosition
    {
        public Position(Match match)
        {
            this.Offset = match.Index;
        }

        internal Position(int offset)
        {
            Offset = offset;
        }

        public int Offset { get; private set; }
        public object Source { get; private set; }

        public IPosition Shift(int offset)
        {
            return new Position(Offset + offset);
        }
    }

    /// <summary>
    /// Implementation of IToken.
    /// </summary>
    class Token : IToken
    {
        public Token(string value, int position, Token previousToken)
        {
            this.Value = value;
            this.Previous = previousToken;
            this.Position = new Position(position);
            if (previousToken != null) previousToken.Next = this;
        }

        public IToken Previous { get; private set; }
        public IToken Next { get; private set; }
        public string Value { get; private set; }


        public IPosition Position { get; private set; }

    }
}
