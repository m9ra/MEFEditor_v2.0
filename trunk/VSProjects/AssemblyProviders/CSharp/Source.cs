using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp
{
    class WriteItem
    {
        public readonly string Data;

        public int ReplacedLength;

        public int StartOffset;

        public int Elongation { get { return Data.Length - ReplacedLength; } }

        public WriteItem(int startOffset, string data)
        {
            Data = data;
            StartOffset = startOffset;
        }

        public WriteItem(int startOffset, int replacedLength, string data)
        {
            StartOffset = startOffset;
            ReplacedLength = replacedLength;
            Data = data;
        }
    }

    public class Source
    {
        LinkedList<WriteItem> _items = new LinkedList<WriteItem>();

        public string Code { get; private set; }
        
        public Source(string code)
        {
            Code = code;
        }

        internal void Remove(INodeAST node, bool keepSideEffect)
        {
            if (keepSideEffect)
                handleSideEffect(node);

            int p1, p2;
            getBorderPositions(node, out p1, out p2);

            write(p1, p2, "");
        }

        internal void Rewrite(INodeAST node, object value, bool keepSideEffect)
        {
            if (keepSideEffect)
                handleSideEffect(node);

            int p1, p2;
            getBorderPositions(node, out p1, out p2);
                  

            write(p1, p2, toCSharp(value));
        }

        internal void AppendArgument(INodeAST node, object value)
        {
            var lastArg = node.Arguments.Last();

            var behindArg = getBehindOffset(lastArg);
            var stringRepresentation = toCSharp(value);

            write(behindArg, ","+stringRepresentation);
        }

        internal void handleSideEffect(INodeAST node)
        {
            int p1, p2;
            getBorderPositions(node,out p1,out p2);

            var keepExpression = Code.Substring(p1, p2 - p1) + ";\n";
            var insertPos = beforeStatementPosition(node);

            write(insertPos.Offset, keepExpression);            
        }

        private void getBorderPositions(INodeAST node, out int p1, out int p2)
        {
            p1 = node.StartingToken.Position.Offset;
            p2 = getBehindOffset(node);
        }

        /// <summary>
        /// Get offset behind given node
        /// </summary>
        /// <param name="node">Resolved node</param>
        /// <returns>Offset behind node</returns>
        private int getBehindOffset(INodeAST node)
        {
            var end = node.EndingToken;
            return end.Next.Position.Offset;
        }

        /// <summary>
        /// Converts given value into C# representation
        /// </summary>
        /// <param name="value">Converted value</param>
        /// <returns>Value representation in C# syntax</returns>
        private string toCSharp(object value)
        {
            var variable = value as Analyzing.VariableName;
            if (variable != null)
            {
                return variable.Name;
            }

            if (value is string)
            {
                value = string.Format("\"{0}\"", value);
            }

            return value.ToString();
        }

        #region AST node utilies
        /// <summary>
        /// Find position which can be used for inserting statement before nodes statement
        /// </summary>
        /// <param name="node">Node for that is searched position for inserting previous statement</param>
        /// <returns>Position before nodes statement</returns>
        private Position beforeStatementPosition(INodeAST node)
        {
            var current = node;
            while (current.Parent != null)
            {
                current = current.Parent;
            }

            return current.StartingToken.Position;
        }
        #endregion

        #region Writing utilities

        /// <summary>
        /// Write data to region between start, end
        /// </summary>
        /// <param name="start">Start offset of replaced region</param>
        /// <param name="end">End offset of replaced region</param>
        /// <param name="data">Written data</param>
        private void write(int start, int end, string data)
        {
            var convertedStart = convertOffset(start);
            var convertedEnd = convertOffset(end);

            var item = new WriteItem(convertedStart, convertedEnd - convertedStart, data);
            writeRaw(item);
        }

        /// <summary>
        /// Write data at given start offset
        /// </summary>
        /// <param name="start">Start offset for written data</param>
        /// <param name="data">Written data</param>
        private void write(int start, string data)
        {
            var converted = convertOffset(start);
            var item = new WriteItem(converted, data);
            writeRaw(item);
        }

        /// <summary>
        /// Write given item into code
        /// </summary>
        /// <param name="item">Item to be written</param>
        private void writeRaw(WriteItem item)
        {
            Code = Code.Remove(item.StartOffset, item.ReplacedLength);
            Code = Code.Insert(item.StartOffset, item.Data);

            insertItem(item);
        }
              
        /// <summary>
        /// Insert item into items list, according to offset
        /// </summary>
        /// <param name="item">Inserted item</param>
        private void insertItem(WriteItem item)
        {
            var node = _items.Last;
            while (node != null && node.Value.StartOffset >= item.StartOffset)
            {
                node.Value.StartOffset += item.Elongation;
                node = node.Previous;
            }

            if (node == null)
            {
                _items.AddFirst(item);
            }
            else
            {
                _items.AddAfter(node, item);
            }
        }

        /// <summary>
        /// Convert given offset according to applied changes
        /// </summary>
        /// <param name="offset">Offset to be converted</param>
        /// <returns>Converted offset</returns>
        private int convertOffset(int offset)
        {
            var node = _items.First;
            while (node != null && node.Value.StartOffset < offset)
            {
                offset += node.Value.Elongation;
                node = node.Next;
            }

            return offset;
        }
        #endregion
    }
}
