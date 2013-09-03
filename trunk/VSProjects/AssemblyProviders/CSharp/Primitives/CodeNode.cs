using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Primitives
{
    /// <summary>
    /// Used for geting max/min of token1 and token2
    /// </summary>
    /// <param name="token1"></param>
    /// <param name="token2"></param>
    /// <returns></returns>
    delegate IToken TokenComparer(IToken token1, IToken token2);


    class CodeSeq : ISeqAST
    {
        public INodeAST[] Lines { get; private set; }
        public CodeSeq(IEnumerable<CodeNode> lines)
        {
            Lines = lines.ToArray();
        }

        public IToken EndingToken
        {
            get {
                if (Lines.Length == 0) throw new NotSupportedException("Cannot get ending token of empty sequence");
                return Lines[Lines.Length-1].EndingToken;
            }
        }

        public IToken StartingToken
        {
            get
            {
                if (Lines.Length == 0) throw new NotSupportedException("Cannot get ending token of empty sequence");
                return Lines[0].StartingToken;
            }
        }
    }

    class Indexer:IIndexer
    {
        public Indexer(IEnumerable<INodeAST> args)
        {
            Arguments = args.ToArray();
        }
        public INodeAST[] Arguments { get; private set; }
    }

    public class CodeNode : INodeAST
    {
        List<CodeNode> _ops = new List<CodeNode>();
        List<CodeNode> _args = new List<CodeNode>();
        
        /// <summary>
        /// Hint for ending token 
        /// </summary>
        IToken _endHint;
        CodeNode _child;

        /// <summary>
        /// Token, from which was created this node
        /// </summary>
        public IToken SourceToken { get; private set; }
        /// <summary>
        /// String value of this node
        /// </summary>
        public string Value { get { return SourceToken.Value; } }
        /// <summary>
        /// If this code node is ending of any tree expression
        /// </summary>
        public bool IsTreeEnding;
        /// <summary>
        /// Type of node
        /// </summary>
        public NodeTypes NodeType { get; set; }  
        /// <summary>
        /// Subsequence if available
        /// </summary>
        public ISeqAST Subsequence { get; private set; }
        /// <summary>
        /// Indexer associated with this node
        /// </summary>
        public IIndexer Indexer { get; private set; }
        /// <summary>
        /// Operands for operator, arguments for call, condition and block nodes for if, switch,...
        /// </summary>        
        public INodeAST[] Arguments { get { return _args.ToArray(); } }
        /// <summary>
        /// Node for that Parent.Child==this
        /// </summary>
        public INodeAST Parent { get; private set; }

        /// <summary>
        /// Source from where this Code node comes
        /// </summary>
        public Source Source { get {return SourceToken.Position.Source; } }



        /// <summary>
        /// Create CodeNode object from sourceToken.
        /// </summary>
        /// <param name="sourceToken">Expect token, which caused creating this node.</param>
        /// <param name="type">Type of created node.</param>
        public CodeNode(IToken sourceToken, NodeTypes type)
        {
            NodeType = type;
            SourceToken = sourceToken;
        }
        /// <summary>
        /// Create CodeNode from two arguments
        /// </summary>
        /// <param name="type">Type of created node</param>
        /// <param name="node">First node argument.</param>
        /// <param name="node2">Second node argument.</param>
        public CodeNode(NodeTypes type, CodeNode node, CodeNode node2)
        {
            NodeType = type;
            SourceToken = node.StartingToken;
            AddArgument(node);
            AddArgument(node2);
            IsTreeEnding = node2.IsTreeEnding;
        }

        /// <summary>
        /// get child available for this node 
        /// set next child at end of child queue
        /// </summary>
        public INodeAST Child
        {
            get
            {
                return _child;
            }
            set
            {   
                var delegateChild = this;
                while (delegateChild.Child != null)
                    delegateChild = delegateChild.Child as CodeNode;

                if (delegateChild != this)
                    //delegate to child
                    delegateChild.Child = value;
                else
                {
                    //set child to this node
                    _child = value as CodeNode;
                    if (_child == null) throw new NullReferenceException("Cannot set null as child");
                    if (_child.Parent != null) throw new NotSupportedException("Cannot add child, which already has children");
                    _child.Parent = this;
                }
            }
        }

        /// <summary>
        /// Add argument to current collection
        /// </summary>
        /// <param name="node"></param>
        public void AddArgument(CodeNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            node.Parent = this;
            _args.Add(node);
        }

        /// <summary>
        /// Set subsequence from node to current node
        /// </summary>
        /// <param name="subseq"></param>
        public void SetSubsequence(CodeNode subseq)
        {
            if (Subsequence != null) throw new NotSupportedException("Cannot reset subsequence");
            if (subseq.Subsequence == null) throw new NotSupportedException("Expected node with subsequence");
            Subsequence = subseq.Subsequence;
        }
        /// <summary>
        /// Get given lines to subsequence
        /// </summary>
        /// <param name="lines"></param>
        public void SetSubsequence(IEnumerable<CodeNode> lines)
        {
            Subsequence = new CodeSeq(lines);
        }

        public void SetIndexer(IEnumerable<INodeAST> args)
        {
            if (Indexer != null) throw new NotSupportedException("Indexer can be set only once");
            Indexer = new Indexer(args);
        }

        /// <summary>
        /// Find first token, used in all children, arguments, subsequence,...
        /// </summary>
        public IToken StartingToken
        {
            get
            {
                var result = SourceToken;
                if(Child!=null)
                    result = getMin(result, Child.StartingToken);

                if (_args.Count > 0)
                {
                    var last = _args[0].StartingToken;
                    result = getMin(result, last);
                }

                if (Subsequence != null && Subsequence.Lines.Length>0)
                    result = getMin(result, Subsequence.StartingToken);
                return result;
            }
        }

        /// <summary>
        /// Find ending token of its farest operand, or return specified token
        /// </summary>
        public IToken EndingToken
        {
            get
            {
                var result =getMax(SourceToken,_endHint);

                if(Child!=null)
                    result = getMax(result, Child.EndingToken);

                if (_args.Count > 0)
                {
                    var last = _args[_args.Count - 1].EndingToken;
                    result = getMax(result, last);
                }

                if (Subsequence != null && Subsequence.Lines.Length>0)
                    result = getMax(result, Subsequence.EndingToken);
                return result;
            }
            set
            {
                _endHint = value;
            }
        }

        private static IToken getMax(IToken tok1, IToken tok2)
        {
            return compareToks(tok1, tok2, 1);
        }

        private static IToken getMin(IToken tok1, IToken tok2)
        {
            return compareToks(tok1, tok2, -1);
        }

        private static IToken compareToks(IToken tok1, IToken tok2, int compareCrit)
        {
            if (tok1 == null) return tok2;
            if (tok2 == null) return tok1;

            return tok1.Position.Offset.CompareTo(tok2.Position.Offset) == compareCrit ? tok1 : tok2;
        }

        public override string ToString()
        {
            var start=StartingToken.Position.Offset;
            var end=EndingToken.Next.Position.Offset;
            return Source.OriginalCode.Substring(start, end - start);            
        }
    }
}
