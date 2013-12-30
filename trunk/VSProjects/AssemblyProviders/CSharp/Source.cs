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
            return view.Data(this, () => new EditContext(view, this, OriginalCode));
        }

        public Source(string code, TypeMethodInfo methodInfo)
        {
            OriginalCode = code;
            OriginalMethod = methodInfo;
        }

        internal void RemoveNode(ExecutionView view, INodeAST node, bool keepSideEffect)
        {
            remove(view, node, keepSideEffect);
            OnChildRemoved(view, node);
        }

        private void remove(ExecutionView view, INodeAST node, bool keepSideEffect)
        {
            if (keepSideEffect)
                handleSideEffect(view, node);

            int p1, p2;
            getBorderPositions(node, out p1, out p2);

            write(view, p1, p2, "");
        }

        private void remove(ExecutionView view, IToken token)
        {
            var p1 = token.Position.Offset;
            var p2 = token.Position.Offset + token.Value.Length;
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
            var callRepresentation = callToCSharp(call);

            var behindLineOffset = getBehindOffset(lineNode);
            write(view, behindLineOffset, callRepresentation);
        }

        internal void PrependCall(ExecutionView view, INodeAST lineNode, CallEditInfo call)
        {
            var callRepresentation = callToCSharp(call);

            var beforeLineOffset = getBeforeOffset(lineNode);
            write(view, beforeLineOffset, callRepresentation);
        }

        private string callToCSharp(CallEditInfo call)
        {
            string thisObj;
            string callFormat;
            if (call.CallName == Naming.CtorName)
            {
                callFormat = "new {0}({2})";
                thisObj = call.ThisObj.ToString();
            }
            else
            {
                callFormat = "{0}.{1}({2})";
                thisObj = toCSharp(call.ThisObj);
            }

            var args = (from arg in call.CallArguments select toCSharp(arg)).ToArray();
            var argsList = string.Join(",", args);
            var callRepresentation = string.Format(callFormat + ";\n", thisObj, call.CallName, argsList);

            if (call.ReturnName != null)
            {
                //assign to desired variable
                callRepresentation = string.Format("var {0} = {1}", call.ReturnName, callRepresentation);
            }

            return callRepresentation;
        }

        internal void AppendArgument(ExecutionView view, INodeAST call, object value)
        {
            var stringRepresentation = toCSharp(value);

            int behindArgOffset;
            var argCn = call.Arguments.Length;
            if (argCn == 0)
            {
                //                     callName      (    )
                behindArgOffset = call.StartingToken.Next.Next.Position.Offset;
            }
            else
            {
                stringRepresentation = "," + stringRepresentation;
                var lastArg = call.Arguments[argCn - 1];
                behindArgOffset = getBehindOffset(lastArg);
            }

            write(view, behindArgOffset, stringRepresentation);
        }

        internal void ShiftBehind(ExecutionView view, INodeAST shiftedLine, INodeAST behindLine)
        {
            var shiftTargetOffset = getBehindOffset(behindLine);

            int shiftStart, shiftEnd;
            getBorderPositions(shiftedLine, out shiftStart, out shiftEnd);
            var shiftLen = shiftEnd - shiftStart;

            move(view, shiftStart, shiftTargetOffset, shiftLen);
        }

        internal void OnChildRemoved(ExecutionView view, INodeAST removedChild)
        {
            var parent = removedChild.Parent;

            if (parent == null)
            {
                //node has been removed and has no parent - handle it
                OnNodeRemoved(view, removedChild);

                //there is no action for keeping parent
                return;
            }

            switch (parent.NodeType)
            {
                case NodeTypes.binaryOperator:
                    remove(view, parent, false);
                    OnChildRemoved(view, parent);
                    return;

                case NodeTypes.call:
                    if (IsOptionalArgument(parent, removedChild))
                    {
                        //check for remaining argument delimiters
                        var argCount = parent.Arguments.Length;
                        if (argCount == 1)
                            //there is no missing delimiter
                            break;

                        //needs removing remaining argument delimiter
                        var argIndex = parent.GetArgumentIndex(removedChild);

                        //last argument has leading delimiter, non last has trailing delimiter
                        var isLastArg = argCount - 1 == argIndex;
                        var delimiterToken = isLastArg ? removedChild.StartingToken.Previous : removedChild.EndingToken.Next;
                        remove(view, delimiterToken);
                    }
                    else
                    {
                        remove(view, parent, false);
                        OnChildRemoved(view, parent);
                        return;
                    }
                    break;

                case NodeTypes.prefixOperator:
                case NodeTypes.hierarchy:
                    remove(view, parent, false);
                    OnChildRemoved(view, parent);
                    return;

                default:
                    throw new NotImplementedException();
            }

            //BE CAREFULL: Only node which parent is not handled by OnChildRemoved
            //travers top down nodes and call removed handler
            OnNodeRemoved(view, removedChild);
        }

        internal void OnNodeRemoved(ExecutionView view, INodeAST removedNode)
        {
            if (removedNode == null)
                return;

            EditContext(view).NodeRemoved(removedNode);

            foreach (var removedChild in removedNode.Arguments)
            {
                OnNodeRemoved(view, removedChild);
            }

            OnNodeRemoved(view, removedNode.Child);
        }

        internal bool IsOptionalArgument(INodeAST call, INodeAST argument)
        {
            var provider = call.Source.CompilationInfo.GetProvider(call);
            var index = call.GetArgumentIndex(argument);
            return provider.IsOptionalArgument(index + 1);
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
            if (node == null)
                return OriginalCode.Length - 1;

            var end = node.EndingToken;
            return end.Next.Position.Offset;
        }

        /// <summary>
        /// Get offset before given node
        /// </summary>
        /// <param name="node">Resolved node</param>
        /// <returns>Offset before node</returns>
        private int getBeforeOffset(INodeAST node)
        {
            if (node == null)
                return 0;

            return node.StartingToken.Position.Offset;
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

    }
}
