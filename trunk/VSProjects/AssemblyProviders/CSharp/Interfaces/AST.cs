using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp.Interfaces
{
    /// <summary>
    /// Describes abstract syntax node type.
    /// </summary>
    public enum NodeTypes
    {
        /// <summary>
        /// Bracket node.
        /// </summary>
        bracket,
        /// <summary>
        /// Hierachy node.
        /// </summary>
        hierarchy,
        /// <summary>
        /// Call node.
        /// </summary>
        call,
        /// <summary>
        /// Variable declaration node.
        /// </summary>
        declaration,
        /// <summary>
        /// Block node.
        /// </summary>
        block,
        /// <summary>
        /// Binary operator node.
        /// </summary>
        binaryOperator,
        /// <summary>
        /// Prefix operator node.
        /// </summary>
        prefixOperator,
        /// <summary>
        /// Post operator node.
        /// </summary>
        postOperator,
        /// <summary>
        /// Empty node.
        /// </summary>
        empty,
        /// <summary>
        /// Keyword node.
        /// </summary>
        keyword,
        /// <summary>
        /// Explicit conversion node.
        /// </summary>
        conversion
    }

    /// <summary>
    /// Result instructions of syntax parser.
    /// </summary>
    interface ISynaxTree
    {    
        /// <summary>
        /// Parsed syntax tree root.
        /// </summary>
        INodeAST Node { get; }
    }

    /// <summary>
    /// Indexer representation.
    /// </summary>
    public interface IIndexer
    {
        /// <summary>
        /// Indexer arguments.
        /// </summary>
        INodeAST[] Arguments { get; }
    }

    /// <summary>
    /// Abstract syntax node representation.
    /// </summary>
    public interface INodeAST
    {
        /// <summary>
        /// Type of node
        /// </summary>
        NodeTypes NodeType { get; }
        /// <summary>
        /// Parent node of this node
        /// </summary>
        INodeAST Parent { get; }
        /// <summary>
        /// Arguments for calls, operands for operators
        /// </summary>
        INodeAST[] Arguments { get; }
        /// <summary>
        /// Child node of this node in hierarchy
        /// </summary>
        INodeAST Child { get; }
        /// <summary>
        /// All children (arguments, hierarchy child... of current node)
        /// </summary>
        IEnumerable<INodeAST> AllChildren { get; }
        /// <summary>
        /// Subsequence of this node
        /// </summary>
        ISeqAST Subsequence { get; }
        /// <summary>
        /// indexer aplied to node. If null - no indexer is available
        /// </summary>
        IIndexer Indexer { get; }

        /// <summary>
        /// Original source of node
        /// </summary>
        Source Source { get; }

        /// <summary>
        /// Value contained in node
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Most left token of al sub nodes
        /// </summary>
        IToken StartingToken { get; }
        /// <summary>
        /// Most top token of all sub nodes
        /// </summary>
        IToken EndingToken { get; }
    }

    /// <summary>
    /// Representation of sequence.
    /// </summary>
    public interface ISeqAST
    {
        /// <summary>
        /// Lines in sequence
        /// </summary>
        INodeAST[] Lines { get; }
        /// <summary>
        /// Ending token according all lines
        /// </summary>
        IToken EndingToken { get; }

        /// <summary>
        /// Starting token according all lines
        /// </summary>
        IToken StartingToken { get; }
    }
}
