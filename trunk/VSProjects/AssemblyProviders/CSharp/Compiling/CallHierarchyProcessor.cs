using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

using TypeSystem;

using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.LanguageDefinitions;

namespace AssemblyProviders.CSharp.Compiling
{
    class CallHierarchyProcessor
    {
        /// <summary>
        /// Compiler that uses current processor
        /// </summary>
        private readonly Compiler _compiler;

        /// <summary>
        /// Entry node of hierarchy
        /// </summary>
        private readonly INodeAST _entryNode;

        /// <summary>
        /// Searcher that is currently used for resolving hierarchies
        /// </summary>
        private MethodSearcher _searcher;

        /// <summary>
        /// Current object where hierarchy is resolved
        /// </summary>
        private RValueProvider _currentObject;

        /// <summary>
        /// Context available from compiler that uses current processor
        /// </summary>
        protected CompilationContext Context { get { return _compiler.Context; } }


        /// <summary>
        /// Initialize <see cref="CallHierarchyProcessor"/> with given node
        /// </summary>
        /// <param name="callHierarchy">Node where call hierarchy starts</param>
        internal CallHierarchyProcessor(INodeAST callHierarchy, Compiler compiler)
        {
            _entryNode = callHierarchy;
            _compiler = compiler;
        }

        /// <summary>
        /// Try to get call hierarchy (chained calls, properties, indexes, namespaces and statit classes)
        /// </summary>
        /// <param name="call">Result representation of call hierarchy</param>
        /// <param name="calledObject">Object on which call hierarchy starts if any</param>
        /// <returns><c>true</c> if call hierarchy is recognized, <c>false</c> otherwise</returns>
        internal bool TryGetSetter(out LValueProvider call, RValueProvider calledObject)
        {
            _currentObject = calledObject;
            _searcher = createMethodSearcher();

            var currNode = _entryNode;
            while (currNode != null)
            {
                if (currNode.Child == null)
                {
                    //setter could be only the last child in hierarchy
                    call = processLNode(currNode);
                    return call != null;
                }

                if (!processRNode(currNode))
                    break;

                currNode = currNode.Child;
            }

            //setter hasnt been found
            call = null;
            return false;
        }

        /// <summary>
        /// Try to get call hierarchy (chained calls, properties, indexes, namespaces and statit classes)
        /// </summary>
        /// <param name="call">Result representation of call hierarchy</param>
        /// <param name="calledObject">Object on which call hierarchy starts if any</param>
        /// <returns><c>true</c> if call hierarchy is recognized, <c>false</c> otherwise</returns>
        internal bool TryGetCall(out RValueProvider call, RValueProvider calledObject)
        {
            _currentObject = calledObject;
            _searcher = createMethodSearcher();

            var currNode = _entryNode;
            while (currNode != null)
            {
                if (!processRNode(currNode))
                    break;

                currNode = currNode.Child;
            }

            call = _currentObject;

            //if searcher is null, it means that there are no
            //buffered nodes left within the searcher
            return _searcher == null;
        }

        private SetterLValue processLNode(INodeAST currNode)
        {

            dispatchByNode(_searcher, currNode, true);

            if (_searcher.HasResults)
            {
                //overloading on setters is not supported
                var overloads = _searcher.FoundResult.ToArray();
                //TODO indexer arguments
                if (overloads.Length > 1)
                    throw CSharpSyntax.ParsingException(currNode, "Cannot select setter overload for {0}", currNode.Value);

                var overload = overloads[0];
                if (!overload.IsStatic && _currentObject == null)
                {
                    if (!_compiler.MethodInfo.HasThis)
                    {
                        //cannot get implicit this object
                        return null;
                    }

                    _currentObject = _compiler.CreateImplicitThis(currNode);
                }

                return new SetterLValue(overloads[0], _currentObject, new RValueProvider[0], Context);
            }

            return null;
        }

        private bool processRNode(INodeAST node)
        {
            //require available searcher
            if (_searcher == null)
            {
                //new searcher is needed, based on current object
                if (_currentObject == null)
                    //object is needed for searcher creation
                    //in current state - hierarchy has to be continuous
                    return false;

                _searcher = createMethodSearcher();
            }

            //search within the searcher
            dispatchByNode(_searcher, node, false);

            if (_searcher.HasResults)
            {
                //there are possible overloads for call
                return setCalledObject(node);
            }
            else
            {
                //methods are not available - probably namespace
                //cascade is processed
                return extendName(node);
            }
        }

        private bool extendName(INodeAST currNode)
        {
            if (_currentObject != null)
                //call hierarchy on objects couldnt be
                //extended.
                return false;

            if (currNode.NodeType == NodeTypes.hierarchy)
            {
                //only hierarchy hasn't been resolved immediately(namespaces) -> shift to next node
                _searcher.ExtendName(currNode.Value);
                return true;
            }
            else
            {
                //only hierarchy node could extend name
                return false;
            }
        }

        private bool setCalledObject(INodeAST currNode)
        {
            var callActivation = _compiler.CreateCallActivation(_currentObject, currNode, _searcher.FoundResult);
            if (callActivation == null)
            {
                //overloads doesnt match to arguments
                return false;
            }

            var resolvedCall = new CallValue(callActivation, Context);

            _currentObject = resolvedCall;

            //old searcher is not needed now
            _searcher = null;
            return true;
        }



        /// <summary>
        /// Create method searcher filled with valid namespaces according to _currentObject
        /// </summary>
        /// <returns>Created <see cref="MethodSearcher"/></returns>
        private MethodSearcher createMethodSearcher()
        {
            //x without base can resolve to:            
            //[this namespace].this.get_x /this.set_x
            //[this namespace].[static class x]
            //[this namespace].[namespace x]
            //[imported namespaces].[static class x]
            //[imported namespaces].[namespace x]

            var searcher = Context.CreateSearcher();

            if (_currentObject == null)
            {
                searcher.ExtendName(_compiler.Namespaces.ToArray());
            }
            else
            {
                var calledObjectInfo = _currentObject.Type;
                searcher.SetCalledObject(calledObjectInfo);
            }
            return searcher;
        }

        /// <summary>
        /// Dispatch given searcher by node. It means that dispatch 
        /// calls on searcher will be called according to node value and structure
        /// </summary>
        /// <param name="searcher">Searcher which is dispatched</param>
        /// <param name="node">Node dispatching searcher</param>
        /// <param name="dispatchSetter">Determine that only setter will be dispatche, otherwise only getter will be dispatched</param>
        private void dispatchByNode(MethodSearcher searcher, INodeAST node, bool dispatchSetter)
        {
            var name = Context.MapGeneric(node.Value);

            switch (node.NodeType)
            {
                case NodeTypes.hierarchy:
                    if (node.Indexer == null)
                    {
                        Debug.Assert(node.Arguments.Length == 0);
                        if (dispatchSetter)
                        {
                            searcher.Dispatch(Naming.SetterPrefix + name);
                        }
                        else
                        {
                            searcher.Dispatch(Naming.GetterPrefix + name);
                        }
                    }
                    else
                    {
                        if (dispatchSetter)
                        {
                            searcher.Dispatch(Naming.ArrayItemSetter);
                        }
                        else
                        {
                            searcher.Dispatch(Naming.ArrayItemGetter);
                        }
                    }
                    break;

                case NodeTypes.call:
                    //TODO this is not correct!!
                    searcher.Dispatch(name);
                    break;

                default:
                    throw new NotSupportedException("Cannot resolve given node type inside hierarchy");
            }
        }
    }
}
