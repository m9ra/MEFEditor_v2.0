using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

using MEFEditor.TypeSystem;

using RecommendedExtensions.Core.Languages.CSharp.Interfaces;

namespace RecommendedExtensions.Core.Languages.CSharp.Compiling
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
            var currNode = initializeCallSearch(calledObject, _entryNode.Child == null);

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

                //there is indexer on last child - get it as rvalue
                if (currNode.Child == null && currNode.Indexer != null && !processRNode(currNode))
                    //indexer setters are handled in 
                    //different way than getters
                    break;
            }

            //setter hasnt been found
            call = null;
            return false;
        }

        private INodeAST initializeCallSearch(RValueProvider calledObject, bool dispatchSetter)
        {
            _searcher = createMethodSearcher(calledObject);
            setCurrentObject(calledObject, _entryNode, dispatchSetter && _entryNode.Child == null);

            var isCallNode = _entryNode.NodeType == NodeTypes.call;
            var needEntryNode = calledObject == null || dispatchSetter || isCallNode;
            //if there is a called object, entry node has been already used for it
            var currNode = needEntryNode ? _entryNode : _entryNode.Child;
            return currNode;
        }

        /// <summary>
        /// Try to get call hierarchy (chained calls, properties, indexes, namespaces and statit classes)
        /// </summary>
        /// <param name="call">Result representation of call hierarchy</param>
        /// <param name="calledObject">Object on which call hierarchy starts if any</param>
        /// <returns><c>true</c> if call hierarchy is recognized, <c>false</c> otherwise</returns>
        internal bool TryGetCall(out RValueProvider call, RValueProvider calledObject)
        {
            var currNode = initializeCallSearch(calledObject, false);
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
                return createSetterValue(currNode, _searcher.FoundResult);
            }

            return null;
        }

        private SetterLValue createSetterValue(INodeAST currNode, IEnumerable<TypeMethodInfo> overloadMethods)
        {
            var overloads = overloadMethods.ToArray();
            //overloading on setters is not supported
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

                var implicitThis = _compiler.CreateImplicitThis(currNode);
                setCurrentObject(implicitThis, currNode, true);
            }

            var indexArguments = _compiler.GetArguments(currNode, currNode.Indexer != null);
            return new SetterLValue(overloads[0], _currentObject, indexArguments, Context);
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

                _searcher = createMethodSearcher(_currentObject);
            }

            //search within the searcher
            dispatchByNode(_searcher, node, false);

            if (_searcher.HasResults)
            {
                //there are possible overloads for call
                return setCalledObject(node, false);
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

        private bool setCalledObject(INodeAST currNode, bool dispatchSetter)
        {
            var resolvedCall = resolveCall(_currentObject, currNode, _searcher.FoundResult);

            if (resolvedCall == null || !setCurrentObject(resolvedCall, currNode, dispatchSetter))
                return false;

            //old searcher is not needed now
            _searcher = null;
            return true;
        }

        private CallValue resolveCall(RValueProvider calledObject, INodeAST currNode, IEnumerable<TypeMethodInfo> overloads)
        {
            var callActivation = _compiler.CreateCallActivation(calledObject, currNode, overloads);
            if (callActivation == null)
            {
                //overloads doesnt match to arguments
                return null;
            }

            return new CallValue(callActivation, Context);
        }

        private bool setCurrentObject(RValueProvider calledObject, INodeAST currNode, bool dispatchSetter)
        {
            if (calledObject != null && currNode != null && currNode.Indexer != null)
            {
                if (dispatchSetter)
                {
                    //nothing to do here - setters are created in LNode value processing
                }
                else
                {
                    var searcher = createMethodSearcher(calledObject);
                    searcher.Dispatch(Naming.IndexerGetter);
                    calledObject = resolveCall(calledObject, currNode, searcher.FoundResult);
                    if (calledObject == null)
                        //indexer hasn't been found
                        return false;

                    //reset searcher, because object has been found
                    _searcher = null;
                }
            }

            _currentObject = calledObject;
            return true;
        }

        /// <summary>
        /// Create method searcher filled with valid namespaces according to calledObject
        /// </summary>
        /// <returns>Created <see cref="MethodSearcher"/></returns>
        private MethodSearcher createMethodSearcher(RValueProvider calledObject)
        {
            //x without base can resolve to:            
            //[this namespace].this.get_x /this.set_x
            //[this namespace].[static class x]
            //[this namespace].[namespace x]
            //[imported namespaces].[static class x]
            //[imported namespaces].[namespace x]

            var searcher = Context.CreateSearcher();

            if (calledObject == null)
            {
                searcher.ExtendName(_compiler.Namespaces.ToArray());
            }
            else
            {
                var calledObjectInfo = calledObject.Type;
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
                    if (dispatchSetter)
                    {
                        if (node.Indexer != null)
                        {
                            //indexer setters are handled in different way than getters
                            searcher.Dispatch(Naming.IndexerSetter);
                        }
                        else
                        {
                            searcher.Dispatch(Naming.SetterPrefix + name);
                        }
                    }
                    else
                    {
                        searcher.Dispatch(Naming.GetterPrefix + name);
                    }
                    break;

                case NodeTypes.call:

                    //handle special name conventions
                    switch (name)
                    {
                        case CSharpSyntax.ThisVariable:
                        case CSharpSyntax.BaseVariable:
                            name = Naming.CtorName;
                            break;
                    }

                    //dispatch by name
                    searcher.Dispatch(name);
                    break;

                default:
                    throw new NotSupportedException("Cannot resolve given node type inside hierarchy");
            }
        }
    }
}
