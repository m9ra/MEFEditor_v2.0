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
        internal Position(string source,int offset)
        {
            Offset = offset;
            Source = source;
        }

        public int Offset { get; private set; }
        public string Source { get; private set; }

        public IPosition Shift(int offset)
        {
            return new Position(Source,Offset + offset);
        }

        public string GetStrip(IPosition position)
        {
            return Source.Substring(Offset, position.Offset - Offset);
        }
    }

    /// <summary>
    /// Implementation of IToken.
    /// </summary>
    class Token : IToken
    {
        public Token(string value, int position, Token previousToken,string source)
        {
            this.Value = value;
            this.Previous = previousToken;
            this.Position = new Position(source,position);
            if (previousToken != null) previousToken.Next = this;
        }

        public IToken Previous { get; private set; }
        public IToken Next { get; private set; }
        public string Value { get; private set; }


        public IPosition Position { get; private set; }

    }
}
