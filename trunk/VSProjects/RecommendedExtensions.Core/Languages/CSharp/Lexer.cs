using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using RecommendedExtensions.Core.Languages.CSharp.Interfaces;
using RecommendedExtensions.Core.Languages.CSharp.Primitives;

namespace RecommendedExtensions.Core.Languages.CSharp
{
    /// <summary>
    /// Lexer for C# language. It produces tokens from given source.
    /// </summary>
    public class Lexer:ILexer
    {
        /// <summary>
        /// Regex for lexing comments.
        /// </summary>
        static readonly string _tokenComments = @"/\*([^*]|\*[^/])*\*/|//.*";

        /// <summary>
        /// Regex for lexing string.
        /// </summary>
        static readonly string _tokenString = "@\"[^\"]*\"|\"(\\\\.|[^\"])*\"";

        /// <summary>
        /// Regex for lexing number.
        /// </summary>
        static readonly string _tokenNumber = @"[\d]+(\.[\d]+)?[fdl]?";

        /// <summary>
        /// Regex for lexing character.
        /// </summary>
        static readonly string _tokenChar = @"'[^\\]'|'\\.'";

        /// <summary>
        /// Regex for lexing operator.
        /// </summary>
        static readonly string _tokenOperator = @"\+\+|--|&&|\|\||[-+\*/&<>!=|]=";

        /// <summary>
        /// Regex for lexing identifiers
        /// </summary>
        static readonly string _tokenIdentifier =
@"
[_a-zA-Z]\w*
(
 [ ]*<[ ]* 
            (
                (?<GEN> <)      | 
                (?<-GEN> >)     | 
                [_a-zA-Z]\w*    | 
                [ ]*[\.,][ ]*
            )* 
 [ ]*>
)?";
        /// <summary>
        /// Regex for source splitting.
        /// </summary>
        static readonly string _tokenSplit = "(" + _tokenComments + "|" + _tokenOperator + "|" + _tokenIdentifier + "|" + _tokenString + "|" + _tokenNumber + "|" + _tokenChar + "|\\S)";

        /// <summary>
        /// Regex for lexing base calls.
        /// </summary>
        static readonly string _baseMatch = @": *(base.+)";

        /// <summary>
        /// Regex for complete tokenizer.
        /// </summary>
        static readonly Regex _tokenizer = new Regex(_tokenSplit, RegexOptions.Compiled| RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Regex for extracting base calls.
        /// </summary>
        static readonly Regex _baseExtractor = new Regex(_baseMatch, RegexOptions.Compiled);


        /// <summary>
        /// Index of currently processed token.
        /// </summary>
        int _current;

        /// <summary>
        /// Created tokens.
        /// </summary>
        List<Token> _tokens = new List<Token>();

        /// <summary>
        /// Processed C# source.
        /// </summary>
        readonly Source _source;

        /// <summary>
        /// Determine if there are next tokens.
        /// </summary>
        /// <value><c>true</c> if token stream ends; otherwise, <c>false</c>.</value>
        public bool End { get { return _current >= _tokens.Count; } }

        /// <summary>
        /// Get current token.
        /// </summary>
        /// <value>The current.</value>
        public IToken Current { get; private set; }

        /// <summary>
        /// Move to next token.
        /// </summary>
        /// <returns>Return token which was current before move.</returns>
        public IToken Move()
        {
            ++_current;
            var last = Current;
            if (End) return last;
            Current = _tokens[_current];
            return last;
        }

        /// <summary>
        /// Create lexer that produce tokens from given source.
        /// </summary>
        /// <param name="source">The source.</param>
        public Lexer(Source source)
        {
            _source = source;
            createTokens(source.OriginalCode);
        }

        /// <summary>
        /// Get tokens obtained from source.
        /// </summary>
        /// <returns>Tokens.</returns>
        public IToken[] GetTokens()
        {
            return _tokens.ToArray();
        }

        /// <summary>
        /// Creates tokens from given code.
        /// </summary>
        /// <param name="code">The code.</param>
        private void createTokens(string code)
        {
            var match = _tokenizer.Match(code);

            while (match.Success)
            {
                addToken(match.Value, match.Index);
                match = match.NextMatch();
            }
        }

        /// <summary>
        /// Add new token into _tokens according to last token.
        /// </summary>
        /// <param name="str">The token source.</param>
        /// <param name="pos">The token initial position.</param>
        private void addToken(string str, int pos = 0)
        {
            if (
                str.Length >= 2 &&
                (str.Substring(0, 2) == "//" || str.Substring(0, 2) == "/*")
                )
                //skip comment
                return;

            Token lastToken = null;
            if (_tokens.Count > 0)
            {
                lastToken = _tokens[_tokens.Count - 1];
                if (pos == 0) pos = lastToken.Position.Offset + lastToken.Value.Length;
            }

            _tokens.Add(new Token(str, pos, lastToken,_source));

            if (lastToken == null) Current = _tokens[_current]; //first token
        }
    }
}
