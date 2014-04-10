using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;
using Analyzing;
using Analyzing.Editing;

using AssemblyProviders.ProjectAssembly;

using AssemblyProviders.CSharp.Compiling;
using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

using AssemblyProviders.CSharp.Transformations;


namespace AssemblyProviders.CSharp
{

    /// <summary>
    /// Representation of C# source code, with ability to perform transformation.
    /// </summary>
    public class Source
    {
        /// <summary>
        /// Namespaces available for source of represented method
        /// </summary>
        private HashSet<string> _namespaces = new HashSet<string>();

        /// <summary>
        /// Contains method representing this source (e.g with generic parameters - it can be used for type translation)
        /// </summary>
        internal readonly TypeMethodInfo OriginalMethod;

        /// <summary>
        /// Method path of possible generic ancestors of current method
        /// </summary>
        public PathInfo OriginalMethodPath { get { return OriginalMethod.Path; } }

        /// <summary>
        /// Event fired whenever change on this source is commited
        /// </summary>
        public event SourceChangeCommitedEvent SourceChangeCommited;

        /// <summary>
        /// Compilation information collected by compiler.
        /// </summary>
        public readonly CompilationInfo CompilationInfo = new CompilationInfo();

        /// <summary>
        /// Original code of source
        /// </summary>
        public readonly string OriginalCode;

        /// <summary>
        /// Namespaces available for source of represented method
        /// </summary>
        public IEnumerable<string> Namespaces { get { return _namespaces; } }

        public Source(string code, TypeMethodInfo methodInfo)
        {
            OriginalCode = code;
            OriginalMethod = methodInfo;
            addImplicitNamespaces(methodInfo.DeclaringType);
        }

        #region Public API exposed by Source

        /// <summary>
        /// Get code according to given view
        /// </summary>
        /// <param name="view">View used for obtaining code</param>
        /// <returns>Code according to given view</returns>
        public string GetCode(ExecutionView view)
        {
            return EditContext(view).Code;
        }

        #endregion

        /// <summary>
        /// Add namespaces that are imported through using construct
        /// </summary>
        /// <param name="namespaces">Enumeration of imported namespaces</param>
        internal void AddExternalNamespaces(IEnumerable<string> namespaces)
        {
            _namespaces.UnionWith(namespaces);
        }

        /// <summary>
        /// Get <see cref="EditContext"/> available in given view
        /// </summary>
        /// <param name="view">View where <see cref="EditContext"/> is needed</param>
        /// <returns>Created/obtained <see cref="EditContext"/></returns>
        internal EditContext EditContext(ExecutionView view)
        {
            return view.Data(this, () => new EditContext(view, this, OriginalCode));
        }

        /// <summary>
        /// Event notifier fired by <see cref="EditContext"/>
        /// <param name="commitedContext">Source that has been commited</param>
        /// </summary>
        internal void OnCommited(EditContext commitedContext)
        {
            if (SourceChangeCommited != null)
                SourceChangeCommited(commitedContext.Code);
        }


        #region Services exposed for transformation implementations

        /// <summary>
        /// Remove node in given view
        /// </summary>
        /// <param name="view">View where transformation is processed</param>
        /// <param name="node">Node that is removed</param>        
        /// <param name="keepSideEffect">Determine that side effect of given node will be preserved</param>
        internal void RemoveNode(ExecutionView view, INodeAST node, bool keepSideEffect)
        {
            remove(view, node, keepSideEffect);
            onChildRemoved(view, node);
        }

        /// <summary>
        /// Rewrite given node with code representation of given value
        /// </summary>
        /// <param name="view">View where transformation is processed</param>
        /// <param name="node">Node that is rewritten</param>
        /// <param name="value">Value which rewrite given node</param>
        /// <param name="keepSideEffect">Determine that side effect of given node will be preserved</param>
        internal void Rewrite(ExecutionView view, INodeAST node, object value, bool keepSideEffect)
        {
            if (keepSideEffect)
                preserveSideEffect(view, node);

            int p1, p2;
            getBorderPositions(node, out p1, out p2);


            write(view, p1, p2, toCSharp(value));
        }

        /// <summary>
        /// Append call at new line after lineNode
        /// </summary>
        /// <param name="view">View where transformation is processed</param>
        /// <param name="lineNode">Node with line where call will be appended</param>
        /// <param name="call">Appended call</param>
        internal void AppendCall(ExecutionView view, INodeAST lineNode, CallEditInfo call)
        {
            var callRepresentation = callToCSharp(call);

            var behindLineOffset = getBehindOffset(lineNode);
            write(view, behindLineOffset, callRepresentation);
        }

        /// <summary>
        /// Append call at new line after lineNode
        /// </summary>
        /// <param name="view">View where transformation is processed</param>
        /// <param name="lineNode">Node with line where call will be appended</param>
        /// <param name="call">Prepended call</param>
        internal void PrependCall(ExecutionView view, INodeAST lineNode, CallEditInfo call)
        {
            var callRepresentation = callToCSharp(call);

            var beforeLineOffset = getBeforeOffset(lineNode);
            write(view, beforeLineOffset, callRepresentation);
        }

        /// <summary>
        /// Add argument to end of argument list of given call
        /// </summary>
        /// <param name="view">View where transformation is processed</param>
        /// <param name="call">Call where argument will be added</param>
        /// <param name="value">Value that will be passed as argument</param>
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

        /// <summary>
        /// Shift shiftedLine behind behindLine in given view
        /// </summary>
        /// <param name="view">View where transformation is processed</param>
        /// <param name="shiftedLine">Line that will be shifted behind behindLine</param>
        /// <param name="behindLine">Line that will be before shiftedLine</param>
        internal void ShiftBehind(ExecutionView view, INodeAST shiftedLine, INodeAST behindLine)
        {
            var shiftTargetOffset = getBehindOffset(behindLine);

            int shiftStart, shiftEnd;
            getBorderPositions(shiftedLine, out shiftStart, out shiftEnd);
            var shiftLen = shiftEnd - shiftStart;

            move(view, shiftStart, shiftTargetOffset, shiftLen);
        }

        #endregion

        #region Remove events handling

        /// <summary>
        /// Handler called for every node that is removed (recursively from removedNode to descendants)
        /// </summary>
        /// <param name="view">View where node has been removed</param>
        /// <param name="removedNode">Node that has been removed</param>
        private void onNodeRemoved(ExecutionView view, INodeAST removedNode)
        {
            if (removedNode == null)
                return;

            EditContext(view).NodeRemoved(removedNode);

            foreach (var removedChild in removedNode.Arguments)
            {
                onNodeRemoved(view, removedChild);
            }

            onNodeRemoved(view, removedNode.Child);
        }

        /// <summary>
        /// Handler that is called for every removed node (recursively from removedChild to ancestors). When there is no parent for removed node
        /// or node parent of node wont be removed, onNodeRemoved cascade is processed. 
        /// </summary>
        /// <param name="view">View where node has been removed</param>
        /// <param name="removedChild">Child that has been removed</param>
        private void onChildRemoved(ExecutionView view, INodeAST removedChild)
        {
            var parent = removedChild.Parent;

            if (parent == null)
            {
                //node has been removed and has no parent - handle it
                onNodeRemoved(view, removedChild);

                //there is no action for keeping parent
                return;
            }

            switch (parent.NodeType)
            {
                case NodeTypes.binaryOperator:
                    remove(view, parent, false);
                    onChildRemoved(view, parent);
                    //let parent report removing
                    return;

                case NodeTypes.call:
                    if (isOptionalArgument(parent, removedChild))
                    {
                        //check for remaining argument delimiters
                        var argCount = parent.Arguments.Length;
                        if (argCount == 1)
                            //there is no delimiter that should be also removed
                            //stop propagation
                            break;

                        //needs removing remaining argument delimiter
                        var argIndex = parent.GetArgumentIndex(removedChild);

                        //last argument has leading delimiter, non last has trailing delimiter
                        var isLastArg = argCount - 1 == argIndex;
                        var delimiterToken = isLastArg ? removedChild.StartingToken.Previous : removedChild.EndingToken.Next;
                        remove(view, delimiterToken);

                        //stop propagation
                        break;
                    }
                    else
                    {
                        remove(view, parent, false);
                        onChildRemoved(view, parent);
                        //let parent report removing
                        return;
                    }


                case NodeTypes.prefixOperator:
                case NodeTypes.hierarchy:
                    remove(view, parent, false);
                    onChildRemoved(view, parent);
                    //let parent report removing
                    return;

                default:
                    throw new NotImplementedException();
            }

            //BE CAREFULL: Only node which parent is not handled by OnChildRemoved
            //travers top down nodes and call removed handler
            onNodeRemoved(view, removedChild);
        }

        #endregion

        #region Primitives for source editing

        /// <summary>
        /// Remove node from source in given view.
        /// </summary>
        /// <param name="view">View where source node will be removed</param>
        /// <param name="node">Node that will be removed</param>
        /// <param name="preserveSideEffect">Determine that side effect on node should be preserved</param>
        private void remove(ExecutionView view, INodeAST node, bool keepSideEffect)
        {
            if (keepSideEffect)
                preserveSideEffect(view, node);

            int p1, p2;
            getBorderPositions(node, out p1, out p2);

            write(view, p1, p2, "");
        }

        /// <summary>
        /// Remove token from source in given view
        /// </summary>
        /// <param name="view">View where source node will be removed</param>
        /// <param name="token">Token that will be removed</param>
        private void remove(ExecutionView view, IToken token)
        {
            var p1 = token.Position.Offset;
            var p2 = token.Position.Offset + token.Value.Length;
            write(view, p1, p2, "");
        }

        /// <summary>
        /// Preserve side effect of node in given view
        /// </summary>
        /// <param name="view">View where side effect is preserved</param>
        /// <param name="node">Node which side effect is preserved</param>
        private void preserveSideEffect(ExecutionView view, INodeAST node)
        {
            var keepExpression = getCode(node) + ";\n";
            var insertPos = BeforeStatementOffset(node);

            write(view, insertPos, keepExpression);
        }

        /// <summary>
        /// Determine that argument is optional in given call
        /// </summary>
        /// <param name="call">Call which argument is tested</param>
        /// <param name="argument">Argument which is tested</param>
        /// <returns><c>true</c> if argument is optional, <c>false</c> otherwise</returns>
        private bool isOptionalArgument(INodeAST call, INodeAST argument)
        {
            var provider = call.Source.CompilationInfo.GetProvider(call);
            var index = call.GetArgumentIndex(argument);
            return provider.IsOptionalArgument(index + 1);
        }

        #endregion

        #region AST node utilies

        /// <summary>
        /// Generate code for C# call
        /// </summary>
        /// <param name="call">Call which code will be generated</param>
        /// <returns>Generated code</returns>
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
                value = string.Format("\"{0}\"", (value as string).Replace("\\", "\\\\"));
            }

            if (value is InstanceInfo)
            {
                var instanceInfo = value as InstanceInfo;
                //TODO namespace shortening
                value = string.Format("typeof({0})", instanceInfo.TypeName);
            }

            if (value == null)
            {
                value = "null";
            }

            return value.ToString();
        }

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

        /// <summary>
        /// Get code represented by given node
        /// </summary>
        /// <param name="node">Node which code is needed</param>
        /// <returns>Code represented by given node</returns>
        private string getCode(INodeAST node)
        {
            int p1, p2;
            getBorderPositions(node, out p1, out p2);

            return OriginalCode.Substring(p1, p2 - p1);
        }

        /// <summary>
        /// Get positions that are borders for given node
        /// </summary>
        /// <param name="node">Node which borders are needed</param>
        /// <param name="p1">Position before the node</param>
        /// <param name="p2">Position after the node</param>
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

        #region Private utilities

        /// <summary>
        /// Add implicit namespaces that are valid for methods declared witihn given type
        /// </summary>
        /// <param name="type">Type that defines implicit namespaces</param>
        private void addImplicitNamespaces(TypeDescriptor type)
        {
            //add empty namespace
            _namespaces.Add("");

            //each part creates implicit namespace
            var parts = type.TypeName.Split(Naming.PathDelimiter);

            var buffer = new StringBuilder();
            foreach (var part in parts)
            {
                if (buffer.Length > 0)
                {
                    //add trailing char
                    buffer.Append(Naming.PathDelimiter);
                }

                buffer.Append(part);

                _namespaces.Add(buffer.ToString());
            }
        }
        #endregion

    }
}
