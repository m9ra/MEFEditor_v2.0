using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

using AssemblyProviders.CSharp.Primitives;
using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.CodeInstructions;
using AssemblyProviders.CSharp.LanguageDefinitions;

namespace AssemblyProviders.CSharp
{
    /// <summary>
    /// Provide services for parsing C# syntax.
    /// </summary>
    class CSharpSyntax
    {        
        readonly Layouts layouts;
        readonly ILexer _lexer;
        
        static readonly HashSet<string> KnownTokens = new HashSet<string>();
        static readonly HashSet<string> EndingTokens = new HashSet<string>() { ";",":", ",", ")", "}", "]" };
        static readonly HashSet<string> PrefOperators = new HashSet<string>() {"!", "throw", "out", "ref", "const", "return", "new", "-","+", "--", "++", "~" };
        static readonly HashSet<string> PostOperators = new HashSet<string>() { "++", "--" };        
        static readonly Dictionary<string, int> BinOperators = new Dictionary<string, int>(){
            {":",50},
            {"=",100},        
            {"/=",100},      
            {"*=",100},      
            {"+=",100},      
            {"-=",100},      
            {"&=",100},      
            {"|=",100},      
            {"~=",100},      
            {"==",150},      
            {"&",200},      
            {"|",200},      
            {"&&",200},      
            {"||",200},      
            {"+",200},
            {"-",200},
            {"*",300},
            {"/",300},
            {"<",400},
            {">",400},
            {"<=",400},
            {">=",400},
        };

        /// <summary>
        /// Determine if there are not next tokens from lexer.
        /// </summary>
        public bool End { get { return _lexer.End; } }

        /// <summary>
        /// Create CSharpSyntax object
        /// </summary>
        /// <param name="lexer">Lexer which will be used for getting tokens.</param>
        /// <param name="nextTree">Encapsulate method which return next node tree from parser.</param>
        public CSharpSyntax(ILexer lexer, GetNextTree nextTree)
        {
            _lexer = lexer;
            layouts = new Layouts(nextTree,_lexer);
            KnownTokens.UnionWith(BinOperators.Keys);
            KnownTokens.UnionWith(EndingTokens);
            KnownTokens.UnionWith(PrefOperators);
            KnownTokens.UnionWith(PostOperators);            
        }
        
        public bool HasLesserPriority(INodeAST node, INodeAST node2)
        {
            if (!BinOperators.ContainsKey(node.Value)) return false;
            if (!BinOperators.ContainsKey(node2.Value)) return true;
            return BinOperators[node.Value] < BinOperators[node2.Value];
        }
        /// <summary>
        /// Return number of expected operands for given node.
        /// </summary>
        /// <param name="node">Node which arity is returned.</param>
        /// <returns>Arity of node.</returns>
        public int Arity(INodeAST node)
        {            
            //unary nodes are resolved in context.
            if (node.NodeType == NodeTypes.binaryOperator) return 2;
            return 0;
        }

        /// <summary>
        /// Apply layout according to current lexer token.
        /// </summary>
        /// <returns>CodeNode created from layout.</returns>
        private CodeNode applyLayout()
        {
            switch (_lexer.Current.Value)
            {
                case "for": 
                    return layouts.ForLayout();
                case "switch": 
                    return layouts.SwitchLayout();
                case "if": 
                case "while": 
                    return layouts.CondBlockLayout();                    
                case "{": 
                    return layouts.SequenceLayout();
                case "(":
                    return layouts.BracketLayout();
                case "continue":
                case "break":
                    return layouts.KeywordLayout();
                default: 
                    return layouts.HierarchyLayout();
            }
        }

        /// <summary>
        /// Return next node created from tokens in _lexer
        /// </summary>
        /// <param name="withContext">Determine if context should be used for created node.</param>
        /// <returns>Next CodeNode.</returns>
        public CodeNode Next(bool withContext)
        {
            var value = _lexer.Current.Value;
            CodeNode result = null;

            if (EndingTokens.Contains(value)) result = new CodeNode(_lexer.Current, NodeTypes.empty);
            else if (!KnownTokens.Contains(value)) result = applyLayout();
            else if (BinOperators.ContainsKey(value)) result = new CodeNode(_lexer.Move(), NodeTypes.binaryOperator);
            else if (PostOperators.Contains(value)) result = new CodeNode(_lexer.Move(), NodeTypes.postOperator);
            else if (PrefOperators.Contains(value)) result = new CodeNode(_lexer.Move(), NodeTypes.prefixOperator);
            
            if (result != null)
            {
                result.IsTreeEnding = _lexer.End || EndingTokens.Contains(_lexer.Current.Value);
                if (withContext) result = checkContext(result);             
                return result;
            }
            throw new ParsingException("Unknown token :" + value);
        }

        /// <summary>
        /// Check context for given node.
        /// </summary>
        /// <param name="node">Node which context is checked.</param>
        /// <returns>Node repaired according to context.</returns>
        private CodeNode checkContext(CodeNode node)
        {            
            if (node == null || _lexer.End)
                //ending token
                return node;
            if (node.IsTreeEnding)  
                //no context for tree ending node
                return node;

            if (node.NodeType == NodeTypes.hierarchy && isHierarchy()) 
                //variable declaration
                return new CodeNode(NodeTypes.declaration, node, Next(false));
          
            if (node.NodeType == NodeTypes.bracket && node.Child==null && isHierarchy())
            {
                //conversion expression - doesnt have child and hierarchy node is next
                return new CodeNode(NodeTypes.conversion, node.Arguments[0] as CodeNode, Next(true));
            }
                                 
            return node;
        }

        /// <summary>
        /// Determine if current token in lexer is hierarchy node.
        /// </summary>
        /// <returns>True if current lexer value is hierarchy token.</returns>
        private bool isHierarchy()
        {
            return !KnownTokens.Contains(_lexer.Current.Value);
        }

        /// <summary>
        /// Test if token is postfix operator.
        /// </summary>
        /// <param name="token">Tested token.</param>
        /// <returns>True if token is postfix operator.</returns>
        internal bool IsPostfixOperator(string token)
        {
            return PostOperators.Contains(token);
        }

        /// <summary>
        /// Test if token is prefix operator.
        /// </summary>
        /// <param name="token">Tested token.</param>
        /// <returns>True if token is prefix operator.</returns>
        internal bool IsPrefixOperator(string token)
        {
            return PrefOperators.Contains(token);         
        }
    }
}
