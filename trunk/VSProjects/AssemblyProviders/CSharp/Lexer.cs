using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp
{
    class Lexer:ILexer
    {
        const string _tokenComments = @"/\*([^*]|\*[^/])*\*/|//.*";
        const string _tokenString = "@\"[^\"]*\"|\"(\\\\.|[^\"])*\"";
        const string _tokenNumber = @"[\d]+(\.[\d]+)?[fdl]?";
        const string _tokenChar = @"'[^\\]'|'\\.'";
        const string _tokenOperator = @"\+\+|--|&&|\|\||[-+\*/&<>!=|]=";
        const string _tokenIdentif = @"[_a-zA-Z]\w*( *<[,_a-zA-Z\w ]+>)?";
        const string _tokenSplit = "(" + _tokenComments + "|" + _tokenOperator + "|" + _tokenIdentif + "|" + _tokenString + "|" + _tokenNumber + "|" + _tokenChar + "|\\S)";

        const string _baseMatch = @": *(base.+)";

        static readonly Regex _tokenizer = new Regex(_tokenSplit, RegexOptions.Compiled);
        static readonly Regex _baseExtractor = new Regex(_baseMatch, RegexOptions.Compiled);


        int _current;
        List<Token> _tokens = new List<Token>();

        readonly string _source;

        /// <summary>
        /// Determine if there are next tokens.
        /// </summary>
        public bool End { get { return _current >= _tokens.Count; } }
        /// <summary>
        /// Get current token.
        /// </summary>
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
        /// Create lexer for given object and language definition.
        /// </summary>        
        internal Lexer(string source)
        {
            _source = source;
            createTokens(source);
        }

        private void createTokens(string code)
        {
            var match = _tokenizer.Match(code);

            while (match.Success)
            {
                addToken(match.Value, match.Index);
                match = match.NextMatch();
            }
        }

        private void addTokens(params string[] toks)
        {
            foreach (var tok in toks)
                addToken(tok);
        }

        /// <summary>
        /// Add new token into _tokens according to last token
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pos"></param>
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
