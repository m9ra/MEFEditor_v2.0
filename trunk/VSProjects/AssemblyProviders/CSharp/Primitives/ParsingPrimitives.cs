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
    public class Position
    {
        public int Offset { get; private set; }
        public Source Source { get; private set; }

        internal Position(Source source,int offset)
        {
            Offset = offset;
            Source = source;
        }

        public Position Shift(int offset)
        {
            return new Position(Source,Offset + offset);
        }

        public string GetStrip(Position position)
        {
            return Source.OriginalCode.Substring(Offset, position.Offset - Offset);
        }

        public void Navigate()
        {
            Source.Navigate(Offset);
        }
    }

    /// <summary>
    /// Implementation of IToken.
    /// </summary>
    class Token : IToken
    {
        public IToken Previous { get; private set; }
        public IToken Next { get; private set; }
        public string Value { get; private set; }


        public Position Position { get; private set; }

        public Token(string value, int position, Token previousToken,Source source)
        {
            this.Value = value;
            this.Previous = previousToken;
            this.Position = new Position(source,position);
            if (previousToken != null) previousToken.Next = this;
        }



        public override string ToString()
        {
            return string.Format("[Token]{0}", Value);
        }

    }
}
