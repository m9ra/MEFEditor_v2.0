using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RecommendedExtensions.Core.Languages.CSharp.Interfaces;

namespace RecommendedExtensions.Core.Languages.CSharp.Primitives
{
    /// <summary>
    /// Used for getting max/min of token1 and token2
    /// </summary>
    /// <param name="token1">token1</param>
    /// <param name="token2">token2</param>
    /// <returns>Token1 or token2</returns>
    delegate IToken TokenComparer(IToken token1, IToken token2);
    
    /// <summary>
    /// Class CodeSeq.
    /// </summary>
    internal class CodeSeq : ISeqAST
    {
        /// <summary>
        /// The _ending token
        /// </summary>
        private readonly IToken _endingToken;

        /// <summary>
        /// Lines in sequence
        /// </summary>
        /// <value>The lines.</value>
        public INodeAST[] Lines { get; private set; }

        /// <summary>
        /// Node where current subsequence is contained.
        /// </summary>
        /// <value>The containing node.</value>
        public INodeAST ContainingNode { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSeq"/> class.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <param name="endingToken">The ending token.</param>
        public CodeSeq(IEnumerable<CodeNode> lines, IToken endingToken)
        {
            _endingToken = endingToken;
            Lines = lines.ToArray();
            foreach (var line in lines)
            {
                line.ContainingSequence = this;
            }
        }

        /// <summary>
        /// Ending token according all lines
        /// </summary>
        /// <value>The ending token.</value>
        public IToken EndingToken
        {
            get
            {
                return _endingToken;
            }
        }

        /// <summary>
        /// Starting token according all lines
        /// </summary>
        /// <value>The starting token.</value>
        /// <exception cref="System.NotSupportedException">Cannot get ending token of empty sequence</exception>
        public IToken StartingToken
        {
            get
            {
                if (Lines.Length == 0) throw new NotSupportedException("Cannot get ending token of empty sequence");
                return Lines[0].StartingToken;
            }
        }
    }

    /// <summary>
    /// Class Indexer.
    /// </summary>
    internal class Indexer : IIndexer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Indexer"/> class.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public Indexer(IEnumerable<INodeAST> args)
        {
            Arguments = args.ToArray();
        }
        /// <summary>
        /// Indexer arguments.
        /// </summary>
        /// <value>The arguments.</value>
        public INodeAST[] Arguments { get; private set; }
    }

    /// <summary>
    /// Class CodeNode.
    /// </summary>
    public class CodeNode : INodeAST
    {
        /// <summary>
        /// The _ops
        /// </summary>
        List<CodeNode> _ops = new List<CodeNode>();
        /// <summary>
        /// The _args
        /// </summary>
        List<CodeNode> _args = new List<CodeNode>();

        /// <summary>
        /// Hint for ending token.
        /// </summary>
        IToken _endHint;
        /// <summary>
        /// The _child
        /// </summary>
        CodeNode _child;

        /// <summary>
        /// Token, from which was created this node.
        /// </summary>
        /// <value>The source token.</value>
        public IToken SourceToken { get; private set; }

        /// <summary>
        /// String value of this node.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get { return SourceToken.Value; } }

        /// <summary>
        /// If this code node is ending of any tree expression.
        /// </summary>
        public bool IsTreeEnding;

        /// <summary>
        /// Type of node.
        /// </summary>
        /// <value>The type of the node.</value>
        public NodeTypes NodeType { get; set; }

        /// <summary>
        /// Subsequence if available.
        /// </summary>
        /// <value>The subsequence.</value>
        public ISeqAST Subsequence { get; private set; }

        /// <summary>
        /// Sequence where node is listed (null if there is no such subsequence).
        /// </summary>
        /// <value>The containing sequence.</value>
        public ISeqAST ContainingSequence { get; internal set; }

        /// <summary>
        /// Indexer associated with this node.
        /// </summary>
        /// <value>The indexer.</value>
        public IIndexer Indexer { get; private set; }

        /// <summary>
        /// Operands for operator, arguments for call, condition and block nodes for if, switch,...
        /// </summary>
        /// <value>The arguments.</value>
        public INodeAST[] Arguments { get { return _args.ToArray(); } }

        /// <summary>
        /// Node for that Parent.Child==this.
        /// </summary>
        /// <value>The parent.</value>
        public INodeAST Parent { get; private set; }

        /// <summary>
        /// Source from where this Code node comes.
        /// </summary>
        /// <value>The source.</value>
        public Source Source { get { return SourceToken.Position.Source; } }

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
        /// Create CodeNode from two arguments.
        /// </summary>
        /// <param name="type">Type of created node.</param>
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
        /// set next child at end of child queue.
        /// </summary>
        /// <value>The child.</value>
        /// <exception cref="System.NullReferenceException">Cannot set null as child</exception>
        /// <exception cref="System.NotSupportedException">Cannot add child, which already has children</exception>
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
        /// All children (arguments, hierarchy child... of current node)
        /// </summary>
        /// <value>All children.</value>
        public IEnumerable<INodeAST> AllChildren
        {
            get
            {
                foreach (var arg in Arguments)
                {
                    yield return arg;
                }

                if (Child != null)
                    yield return Child;
            }
        }

        /// <summary>
        /// Add argument to current collection.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <exception cref="System.ArgumentNullException">node</exception>
        public void AddArgument(CodeNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            node.Parent = this;
            _args.Add(node);
        }

        /// <summary>
        /// Set subsequence from node to current node.
        /// </summary>
        /// <param name="subseq">The subseq.</param>
        /// <exception cref="System.NotSupportedException">
        /// Cannot reset subsequence
        /// or
        /// Expected node with subsequence
        /// </exception>
        public void SetSubsequence(CodeNode subseq)
        {
            if (Subsequence != null) throw new NotSupportedException("Cannot reset subsequence");
            if (subseq.Subsequence == null) throw new NotSupportedException("Expected node with subsequence");
            var sub = subseq.Subsequence as CodeSeq;
            sub.ContainingNode = this;
            Subsequence = sub;
        }
        /// <summary>
        /// Get given lines to subsequence.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <param name="endingToken">The ending token.</param>
        public void SetSubsequence(IEnumerable<CodeNode> lines, IToken endingToken)
        {
            var sub = new CodeSeq(lines, endingToken);
            sub.ContainingNode = this;
            Subsequence = sub;
        }

        /// <summary>
        /// Sets the indexer.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="System.NotSupportedException">Indexer can be set only once</exception>
        public void SetIndexer(IEnumerable<INodeAST> args)
        {
            if (Indexer != null) throw new NotSupportedException("Indexer can be set only once");
            Indexer = new Indexer(args);
        }

        /// <summary>
        /// Find first token, used in all children, arguments, subsequence,...
        /// </summary>
        /// <value>The starting token.</value>
        public IToken StartingToken
        {
            get
            {
                var result = SourceToken;
                if (Child != null)
                    result = getMin(result, Child.StartingToken);

                if (_args.Count > 0)
                {
                    var last = _args[0].StartingToken;
                    result = getMin(result, last);
                }

                if (Subsequence != null && Subsequence.Lines.Length > 0)
                    result = getMin(result, Subsequence.StartingToken);
                return result;
            }
        }

        /// <summary>
        /// Find ending token of its farest operand, or return specified token.
        /// </summary>
        /// <value>The ending token.</value>
        public IToken EndingToken
        {
            get
            {
                var result = getMax(SourceToken, _endHint);

                if (Child != null)
                    result = getMax(result, Child.EndingToken);

                if (_args.Count > 0)
                {
                    var last = _args[_args.Count - 1].EndingToken;
                    result = getMax(result, last);
                }

                if (Subsequence != null && Subsequence.Lines.Length > 0)
                    result = getMax(result, Subsequence.EndingToken);
                return result;
            }
            set
            {
                _endHint = value;
            }
        }

        /// <summary>
        /// Gets the maximum.
        /// </summary>
        /// <param name="tok1">The tok1.</param>
        /// <param name="tok2">The tok2.</param>
        /// <returns>IToken.</returns>
        private static IToken getMax(IToken tok1, IToken tok2)
        {
            return compareToks(tok1, tok2, 1);
        }

        /// <summary>
        /// Gets the minimum.
        /// </summary>
        /// <param name="tok1">The tok1.</param>
        /// <param name="tok2">The tok2.</param>
        /// <returns>IToken.</returns>
        private static IToken getMin(IToken tok1, IToken tok2)
        {
            return compareToks(tok1, tok2, -1);
        }

        /// <summary>
        /// Compares the toks.
        /// </summary>
        /// <param name="tok1">The tok1.</param>
        /// <param name="tok2">The tok2.</param>
        /// <param name="compareCrit">The compare crit.</param>
        /// <returns>IToken.</returns>
        private static IToken compareToks(IToken tok1, IToken tok2, int compareCrit)
        {
            if (tok1 == null) return tok2;
            if (tok2 == null) return tok1;

            return tok1.Position.Offset.CompareTo(tok2.Position.Offset) == compareCrit ? tok1 : tok2;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var start = StartingToken.Position.Offset;

            var next = EndingToken.Next;

            var end = next == null ? Source.OriginalCode.Length - 1 : next.Position.Offset;
            return Source.OriginalCode.Substring(start, end - start);
        }

    }
}
