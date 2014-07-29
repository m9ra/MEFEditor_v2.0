using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

using RecommendedExtensions.Core.Languages.CSharp.Interfaces;

namespace RecommendedExtensions.Core.Languages.CSharp.Primitives
{
    /// <summary>
    /// Position used for determining offset in given source.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// The offset of current position from <see cref="Source"/> beginning. 
        /// </summary>
        public readonly int Offset;

        /// <summary>
        /// The position source.
        /// </summary>
        public readonly Source Source;

        /// <summary>
        /// Initializes a new instance of the <see cref="Position" /> class.
        /// </summary>
        /// <param name="source">The position source.</param>
        /// <param name="offset">The offset of position.</param>
        internal Position(Source source,int offset)
        {
            Offset = offset;
            Source = source;
        }

        /// <summary>
        /// Creates new position that is shifted according to specified offset
        /// from current position.
        /// </summary>
        /// <param name="offset">The shift offset.</param>
        /// <returns>Shifted position.</returns>
        public Position Shift(int offset)
        {
            return new Position(Source,Offset + offset);
        }

        /// <summary>
        /// Gets source strip from given position.
        /// </summary>
        /// <param name="position">The strip position.</param>
        /// <returns>Text of requested strip.</returns>
        public string GetStrip(Position position)
        {
            return Source.OriginalCode.Substring(Offset, position.Offset - Offset);
        }

        /// <summary>
        /// Navigates to current position in source code.
        /// </summary>
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
