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
    public class Lexer:ILexer
    {
        static readonly string _tokenComments = @"/\*([^*]|\*[^/])*\*/|//.*";
        static readonly string _tokenString = "@\"[^\"]*\"|\"(\\\\.|[^\"])*\"";
        static readonly string _tokenNumber = @"[\d]+(\.[\d]+)?[fdl]?";
        static readonly string _tokenChar = @"'[^\\]'|'\\.'";
        static readonly string _tokenOperator = @"\+\+|--|&&|\|\||[-+\*/&<>!=|]=";
        static readonly string _tokenIdentif =
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
        static readonly string _tokenSplit = "(" + _tokenComments + "|" + _tokenOperator + "|" + _tokenIdentif + "|" + _tokenString + "|" + _tokenNumber + "|" + _tokenChar + "|\\S)";

        static readonly string _baseMatch = @": *(base.+)";

        static readonly Regex _tokenizer = new Regex(_tokenSplit, RegexOptions.Compiled| RegexOptions.IgnorePatternWhitespace);
        static readonly Regex _baseExtractor = new Regex(_baseMatch, RegexOptions.Compiled);


        int _current;
        List<Token> _tokens = new List<Token>();

        readonly Source _source;

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
        public Lexer(Source source)
        {
            _source = source;
            createTokens(source.OriginalCode);
        }

        /// <summary>
        /// Get tokens obtained from source
        /// </summary>
        /// <returns>Tokens</returns>
        public IToken[] GetTokens()
        {
            return _tokens.ToArray();
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
