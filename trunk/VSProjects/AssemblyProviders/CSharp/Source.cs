using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp.Compiling;
using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

using AssemblyProviders.CSharp.Transformations;

namespace AssemblyProviders.CSharp
{


    public class Source
    {

        internal readonly EditContext EditContext;
        public readonly CompilationInfo CompilationInfo = new CompilationInfo();

        public readonly string OriginalCode;

        public string Code
        {
            get
            {
                return EditContext.Code;
            }
        }

        public Source(string code)
        {
            OriginalCode = code;
            EditContext = new EditContext(this,code);
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

        internal void AppendArgument(INodeAST call, object value)
        {
            var lastArg = call.Arguments.Last();

            var behindArg = getBehindOffset(lastArg);
            var stringRepresentation = toCSharp(value);

            write(behindArg, "," + stringRepresentation);
        }

        internal void ShiftBehind(INodeAST shiftedLine, INodeAST behindLine)
        {
            var shiftTargetOffset = getBehindOffset(behindLine);

            int shiftStart, shiftEnd;
            getBorderPositions(shiftedLine, out shiftStart, out shiftEnd);
            var shiftLen = shiftEnd - shiftStart;

            move(shiftStart, shiftTargetOffset, shiftLen);
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

        private void handleSideEffect(INodeAST node)
        {
            var keepExpression = getCode(node) + ";\n";
            var insertPos = beforeStatementPosition(node);

            write(insertPos.Offset, keepExpression);
        }

        private string getCode(INodeAST node)
        {
            int p1, p2;
            getBorderPositions(node, out p1, out p2);

            return OriginalCode.Substring(p1, p2 - p1);
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
            EditContext.Strips.Remove(start, end - start);
            if (data.Length > 0)
            {
                EditContext.Strips.Write(start, data);
            }
        }

        /// <summary>
        /// Write data at given start offset
        /// </summary>
        /// <param name="start">Start offset for written data</param>
        /// <param name="data">Written data</param>
        private void write(int start, string data)
        {
            EditContext.Strips.Write(start, data);
        }

        private void move(int p1, int np1, int length)
        {
            EditContext.Strips.Move(p1, length, np1);
        }


        #endregion
        
        internal bool Commit()
        {
            EditContext.Commit();
            return true;
        }
    }
}
