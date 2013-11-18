using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;
using Analyzing.Editing;

using AssemblyProviders.CSharp.Compiling;
using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

using AssemblyProviders.CSharp.Transformations;

namespace AssemblyProviders.CSharp
{
    public class Source
    {
        /// <summary>
        /// Contains method representing this source (e.g with generic parameters - it can be used for type translation)
        /// </summary>
        internal readonly TypeMethodInfo OriginalMethod;

        public readonly CompilationInfo CompilationInfo = new CompilationInfo();

        public readonly string OriginalCode;

        public string Code(ExecutionView view)
        {
            return EditContext(view).Code;
        }

        internal EditContext EditContext(ExecutionView view)
        {
            return view.Data(() => new EditContext(this, OriginalCode));
        }

        public Source(string code, TypeMethodInfo methodInfo)
        {
            OriginalCode = code;
            OriginalMethod = methodInfo;
        }

        internal void Remove(ExecutionView view, INodeAST node, bool keepSideEffect)
        {
            if (keepSideEffect)
                handleSideEffect(view, node);

            int p1, p2;
            getBorderPositions(node, out p1, out p2);

            write(view, p1, p2, "");
        }

        internal void Rewrite(ExecutionView view, INodeAST node, object value, bool keepSideEffect)
        {
            if (keepSideEffect)
                handleSideEffect(view, node);

            int p1, p2;
            getBorderPositions(node, out p1, out p2);


            write(view, p1, p2, toCSharp(value));
        }


        internal void AppendCall(ExecutionView view, INodeAST lineNode, CallEditInfo call)
        {
            var thisObj = toCSharp(call.ThisObj);
            var args = (from arg in call.CallArguments select toCSharp(arg)).ToArray();


            var callRepresentation = string.Format("{0}.{1}({2});\n", thisObj, call.CallName, string.Join(",", args));
            var behindLineOffset = getBehindOffset(lineNode);

            write(view, behindLineOffset, callRepresentation);
        }

        internal void AppendArgument(ExecutionView view, INodeAST call, object value)
        {
            var lastArg = call.Arguments.Last();

            var behindArg = getBehindOffset(lastArg);
            var stringRepresentation = toCSharp(value);

            write(view, behindArg, "," + stringRepresentation);
        }

        internal void ShiftBehind(ExecutionView view, INodeAST shiftedLine, INodeAST behindLine)
        {
            var shiftTargetOffset = getBehindOffset(behindLine);

            int shiftStart, shiftEnd;
            getBorderPositions(shiftedLine, out shiftStart, out shiftEnd);
            var shiftLen = shiftEnd - shiftStart;

            move(view, shiftStart, shiftTargetOffset, shiftLen);
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
        internal int BeforeStatementOffset(INodeAST node)
        {
            var current = node;
            while (current.Parent != null)
            {
                current = current.Parent;
            }

            return current.StartingToken.Position.Offset;
        }

        private void handleSideEffect(ExecutionView view, INodeAST node)
        {
            var keepExpression = getCode(node) + ";\n";
            var insertPos = BeforeStatementOffset(node);

            write(view, insertPos, keepExpression);
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
        private void write(ExecutionView view, int start, int end, string data)
        {
            EditContext(view).Strips.Remove(start, end - start);
            if (data.Length > 0)
            {
                EditContext(view).Strips.Write(start, data);
            }
        }

        /// <summary>
        /// Write data at given start offset
        /// </summary>
        /// <param name="start">Start offset for written data</param>
        /// <param name="data">Written data</param>
        private void write(ExecutionView view, int start, string data)
        {
            EditContext(view).Strips.Write(start, data);
        }

        private void move(ExecutionView view, int p1, int np1, int length)
        {
            EditContext(view).Strips.Move(p1, length, np1);
        }


        #endregion

        internal bool Commit(ExecutionView view)
        {
            var context = view.Data<EditContext>();
            context.Commit();
            return true;
        }
    }
}
