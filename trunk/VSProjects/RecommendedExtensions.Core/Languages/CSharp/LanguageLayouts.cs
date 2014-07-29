using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RecommendedExtensions.Core.Languages.CSharp.Interfaces;
using RecommendedExtensions.Core.Languages.CSharp.Primitives;

namespace RecommendedExtensions.Core.Languages.CSharp
{
    /// <summary>
    /// Encapsulates method returning bool.
    /// </summary>
    /// <returns>Bool value.</returns>
    delegate bool BoolAction();
    /// <summary>
    /// Encapsulates method returning CodeNode.
    /// </summary>
    /// <returns>CodeNode object.</returns>
    delegate CodeNode GetNextTree();

    /// <summary>
    /// Layouts for context parsing of C# methods.        
    /// </summary>
    /// <remarks>Layout methods expect source token in _lexer.Current</remarks>
    class LanguageLayouts
    {
        readonly GetNextTree _nextTree;
        readonly ILexer _lexer;

        /// <summary>
        /// Create layouts object.
        /// </summary>
        /// <param name="nextTree">Method which will be used for getting tree nodes.</param>
        /// <param name="lexer">Source of parsed tokens.</param>
        public LanguageLayouts(GetNextTree nextTree, ILexer lexer)
        {
            _nextTree = nextTree;
            _lexer = lexer;
        }

        /// <summary>
        /// Create <see cref="CodeNode"/> representing implicitly typed array with initializer
        /// </summary>
        /// <returns></returns>
        public CodeNode ImplicitArray()
        {
            var node = new CodeNode(_lexer.Move(), NodeTypes.hierarchy);
            _shiftToken("]", "Expected '{0}' for implicitly typed array");
            //_shiftToken("{", "Expected '{0}' initializer expected for implicitly type array");

            var initializer = InitializerLayout();
            node.SetSubsequence(initializer);
            node.EndingToken = _lexer.Current;
            return node;
        }

        /// <summary>
        /// Create CodeNode representing for block.
        /// </summary>
        /// <returns>CodeNode created according to layout.</returns>
        public CodeNode ForLayout()
        {
            var node = new CodeNode(_lexer.Move(), NodeTypes.block);

            _shiftToken("(", "Error in For layout, expected '('");

            for (int i = 0; i < 3; i++)
            {
                node.AddArgument(_nextTree());
                if (i < 2) _shiftToken(";", "Error in For layout, expected ';'");
            }

            _shiftToken(")", "Error in For layout, expected ')'");

            node.Child = _nextTree();
            node.EndingToken = _lexer.Current;

            return node;
        }

        /// <summary>
        /// Create CodeNode representing for block.
        /// </summary>
        /// <returns>CodeNode created according to layout.</returns>
        public CodeNode ForeachLayout()
        {
            var node = new CodeNode(_lexer.Move(), NodeTypes.block);

            _shiftToken("(", "Error in Foreach layout, expected '('");
            var declaration = _nextTree();
            node.AddArgument(declaration);
            _shiftToken("in", "Expected 'in'");
            var expression = _nextTree();
            node.AddArgument(expression);
            _shiftToken(")", "Error in For layout, expected ')'");

            node.Child = _nextTree();
            node.EndingToken = _lexer.Current;
            return node;
        }


        /// <summary>
        /// Create CodeNode representing conditional block.
        /// </summary>
        /// <returns>CodeNode created according to layout.</returns>
        public CodeNode CondBlockLayout()
        {
            var condNode = new CodeNode(_lexer.Move(), NodeTypes.block);
            condition(condNode);

            //on { nextTree gives sequence, else return command

            condNode.AddArgument(_nextTree());
            if (_shiftToken("else")) condNode.AddArgument(_nextTree());

            return condNode;
        }


        /// <summary>
        /// Create <see cref="CodeNode"/> representing do{}while() block
        /// </summary>
        /// <returns><see cref="CodeNode"/> created according to layout</returns>
        public CodeNode DoLayout()
        {
            var commandNode = new CodeNode(_lexer.Move(), NodeTypes.block);

            //on { nextTree gives sequence, else return command
            commandNode.AddArgument(_nextTree());

            _shiftToken("while", "Missing {0} after do");
            condition(commandNode);
            commandNode.EndingToken = _lexer.Current.Next;

            return commandNode;
        }


        /// <summary>
        /// Create CodeNode representing switch block.
        /// </summary>
        /// <returns>CodeNode created according to layout.</returns>
        public CodeNode SwitchLayout()
        {
            var switchNode = new CodeNode(_lexer.Move(), NodeTypes.block);
            condition(switchNode);
            _shiftToken("{", "expected '{' in switch layout");

            var inSwitch = true;
            while (inSwitch)
            {
                var label = _current();
                switch (label)
                {
                    case "case":
                    case "default":
                        var labelBlock = new CodeNode(_lexer.Move(), NodeTypes.block);
                        if (labelBlock.Value == "case") labelBlock.AddArgument(_nextTree());
                        _shiftToken(":", "expected '{0}' after '{1}', in switch statement", label);

                        var lines = new List<CodeNode>();

                        while (_current() != "case" && _current() != "default" && _current() != "}")
                        {
                            lines.Add(_nextTree());
                            _shiftToken(";");
                        }

                        labelBlock.SetSubsequence(lines, _lexer.Current);
                        switchNode.AddArgument(labelBlock);
                        break;
                    case "}":
                        inSwitch = false;
                        break;
                    default:
                        throw CSharpSyntax.ParsingException(_lexer.Current, "unrecognized label '{0}' in switch statement", label);
                }
            }

            _shiftToken("}", "expected '{0}' in switch layout");
            return switchNode;
        }


        /// <summary>
        /// Create CodeNode representing hierarchy sequence.
        /// </summary>
        /// <returns>CodeNode created according to layout.</returns>
        public CodeNode HierarchyLayout()
        {
            var node = new CodeNode(_lexer.Move(), NodeTypes.hierarchy);

            if (_checkToken("("))//function call
            {
                node.NodeType = NodeTypes.call;
                resolveBracket(() => { node.AddArgument(_nextTree()); return false; }, "(", ",", ")", "Error in call, expected '{0}'"); //on fail throw exception
                node.EndingToken = _lexer.Current.Previous;//aby ukazoval na zavorku
            }

            if (_shiftToken("."))
            {
                var child = HierarchyLayout();
                if (child == null)
                    throw CSharpSyntax.ParsingException(_lexer.Current, "Expected identifier after '.'");
                node.Child = child;
            }

            if (_checkToken("["))
            {
                var indexer = IndexerLayout();
                node.SetIndexer(indexer);
            }

            if (_checkToken("{")) //initializer
            {
                var seq = InitializerLayout();
                node.SetSubsequence(seq);
            }

            return node;
        }


        /// <summary>
        /// Create CodeNode representing keyword.
        /// </summary>
        /// <returns>CodeNode created according to layout.</returns>
        internal CodeNode KeywordLayout()
        {
            var keyword = _lexer.Move();
            var result = new CodeNode(keyword, NodeTypes.keyword);
            result.IsTreeEnding = true;
            return result;
        }


        /// <summary>
        /// Create CodeNode representing indexer.
        /// </summary>
        /// <returns>CodeNode created according to layout.</returns>
        public IEnumerable<INodeAST> IndexerLayout()
        {
            var args = new List<INodeAST>();
            resolveBracket(() => addIndexArg(args), "[", ",", "]", "Error in indexer, expected {0}");
            return args;
        }

        bool addIndexArg(List<INodeAST> args)
        {
            CodeNode node;
            if (_checkToken(",") || _checkToken("]"))
                node = new CodeNode(_lexer.Current, NodeTypes.empty);
            else
                node = _nextTree();

            node.EndingToken = _lexer.Current;
            args.Add(node);
            return false;
        }


        /// <summary>
        /// Create CodeNode representing initializer block.
        /// </summary>
        /// <returns>CodeNode created according to layout.</returns>
        public CodeNode InitializerLayout()
        {
            return treeItemsLayout(",", false);
        }


        /// <summary>
        /// Create CodeNode representing commands sequence.
        /// </summary>
        /// <returns>CodeNode created according to layout.</returns>
        public CodeNode SequenceLayout()
        {
            return treeItemsLayout(";", true);
        }

        private CodeNode treeItemsLayout(string delimiter, bool canSkipDelim)
        {
            var node = new CodeNode(_lexer.Current, NodeTypes.block);

            var lines = new List<CodeNode>();
            resolveBracket(() => addNode(lines, canSkipDelim), "{", delimiter, "}", "Error in sequence, expected '" + delimiter + "'");
            node.SetSubsequence(lines, _lexer.Current.Previous);
            if (_lexer.Current.Value == delimiter)
                node.EndingToken = _lexer.Current;

            return node;
        }


        /// <summary>
        /// Create CodeNode representing expression prefixed with opening bracket.
        /// </summary>
        /// <returns>CodeNode created according to layout.</returns>
        public CodeNode BracketLayout()
        {
            var node = new CodeNode(_lexer.Move(), NodeTypes.bracket);

            node.AddArgument(_nextTree());
            _shiftToken(")", "Expected closing {0}");

            if (_shiftToken("."))
                //call on expression
                node.Child = HierarchyLayout();

            return node;
        }

        /// <summary>
        /// Action returns true, if delimiter can be skipped, else return false - because of missing ; after blocks
        /// </summary>
        /// <param name="action"></param>
        /// <param name="openingBr"></param>
        /// <param name="delimiter"></param>
        /// <param name="closingBr"></param>
        /// <param name="ErrorMessage"></param>
        private void resolveBracket(BoolAction action, string openingBr, string delimiter, string closingBr, string ErrorMessage)
        {
            _shiftToken(openingBr, "expected opening '{0}'");
            while (!_shiftToken(closingBr))
            {
                var canSkip = action();

                if (canSkip | _shiftToken(delimiter)) continue;//delimiter
                else if (_shiftToken(closingBr, ErrorMessage)) break;//closing bracket
                //else exception is thrown
            }
        }

        /// <summary>
        /// Add condition argument to given node.
        /// </summary>
        /// <param name="condNode">Node to add condition argument.</param>
        private void condition(CodeNode condNode)
        {
            _shiftToken("(", "expected '{0}' in {1} clause", condNode.Value);
            condNode.AddArgument(_nextTree());
            _shiftToken(")", "expected '{0}' in {1} clause", condNode.Value);
        }

        /// <summary>
        /// Add tree node into given list.
        /// </summary>
        /// <param name="nodes">List where next node will be added.</param>
        /// <param name="canSkipDelim">Determine if delimiter can be skipped.</param>
        /// <returns></returns>
        private bool addNode(List<CodeNode> nodes, bool canSkipDelim)
        {
            if (_checkToken(";"))
                return false;

            var node = _nextTree();
            bool skip = canSkipDelim && node.NodeType == NodeTypes.block;

            if (!skip)
                //blocks doesnt have ending token here
                node.EndingToken = _lexer.Current;
            nodes.Add(node);
            return skip;
        }

        /// <summary>
        /// Test if current lexer token value is equal to expectedString. On success move lexer.
        /// </summary>
        /// <param name="expectedString">Value which is used to test lexer current value.</param>
        /// <param name="errorMessage">If is not null and test doesnt suceed, is used for 
        /// throwing parsing exception. If null, no exception is thrown</param>
        /// <param name="msgArgs">Format arguments for errorMessage.</param>
        /// <returns>True if expectedString is equal to lexers current value.</returns>
        private bool _shiftToken(string expectedString, string errorMessage = null, params object[] msgArgs)
        {
            var result = _checkToken(expectedString, errorMessage, msgArgs);
            if (result)
                _lexer.Move();
            return result;
        }


        /// <summary>
        /// Test if current lexer token value is equal to expectedString.
        /// </summary>
        /// <param name="expectedString">Value which is used to test lexer current value.</param>
        /// <param name="errorMessage">If is not null and test doesn't succeed, is used for 
        /// throwing parsing exception. If null, no exception is thrown</param>
        /// <param name="msgArgs">Format arguments for errorMessage.</param>
        /// <returns>True if expectedString is equal to lexers current value.</returns>
        private bool _checkToken(string expectedString, string errorMessage = null, params object[] msgArgs)
        {
            if (_lexer.Current.Value != expectedString)
            {
                if (errorMessage != null)
                {
                    var args = new List<object>();
                    args.Add(expectedString);
                    args.AddRange(msgArgs);

                    throw CSharpSyntax.ParsingException(_lexer.Current, errorMessage, args.ToArray());
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get value of lexers current token.
        /// </summary>
        /// <returns>Value of lexers current token.</returns>
        private string _current()
        {
            return _lexer.Current.Value;
        }
    }
}
