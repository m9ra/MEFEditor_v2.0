using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using Analyzing;
using TypeSystem;

using AssemblyProviders.TypeDefinitions;
using AssemblyProviders.ProjectAssembly;
using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;
using AssemblyProviders.CSharp.Compiling;

namespace AssemblyProviders.CSharp
{

    /// <summary>
    /// Compiler from C# to AIL implementation. Internaly uses <see cref="SyntaxParser"/>. Cover 
    /// all important .NET constructs that are needed for analyzing MEF composition.
    /// 
    /// Emit methods - are for instruction emitting
    /// Resolve methods - are for getting result of operation
    /// Get methods - are for getting value representation of operation
    /// </summary>
    public class Compiler
    {
        /// <summary>
        /// Activation that represents request for compiling
        /// </summary>
        private readonly ParsingActivation _activation;

        /// <summary>
        /// Source of parsed method
        /// </summary>
        private readonly Source _source;

        /// <summary>
        /// Parser used for creating tokens and theri clasification
        /// </summary>
        private readonly static SyntaxParser _parser = new SyntaxParser();

        /// <summary>
        /// Result of syntax parsing that is already compiled
        /// </summary>
        private readonly CodeNode _method;

        /// <summary>
        /// Context of compilation process
        /// </summary>
        private readonly CompilationContext _context;

        /// <summary>
        /// Mapping operators on .NET methods. First operand is treated as called object, second one is passed as call argument.
        /// </summary>
        private static Dictionary<string, string> _mathOperatorMethods = new Dictionary<string, string>(){
              {"+","add_operator"},
              {"-","sub_operator"},
              {"*","mul_operator"},
              {"/","div_operator"},
              {"<","lesser_operator"},
              {">","greater_operator"},
              
              {"==","Equals"}
        };

        /// <summary>
        /// Emitter where compiled instructions are emitted
        /// </summary>
        private EmitterBase E { get { return _context.Emitter; } }

        /// <summary>
        /// Info that is collected about source during compilation
        /// </summary>
        private CompilationInfo CompilationInfo { get { return _source.CompilationInfo; } }

        /// <summary>
        /// Info of method that is compiled
        /// </summary>
        private TypeMethodInfo MethodInfo { get { return _activation.Method; } }

        /// <summary>
        /// Method for obtaining instructions from given activation
        /// </summary>
        /// <param name="activation">Activation which instructions are generated</param>
        /// <param name="emitter">Emitter where instructions will be generated</param>
        /// <param name="services">Services from current type system environment</param>
        /// <returns><see cref="Source"/> object created for given activation. It contains detailed info collected during compilation</returns>
        public static Source GenerateInstructions(ParsingActivation activation, EmitterBase emitter, TypeServices services)
        {
            var compiler = new Compiler(activation, emitter, services);

            compiler.generateInstructions();

            return compiler._source;
        }

        private Compiler(ParsingActivation activation, EmitterBase emitter, TypeServices services)
        {
            _activation = activation;
            _context = new CompilationContext(emitter, services);

            _source = new Source(activation.SourceCode, activation.Method);
            _source.AddExternalNamespaces(activation.Namespaces);

            _method = _parser.Parse(_source);

            registerGenericArguments(activation);
        }

        #region Debug Info utilities

        /// <summary>
        /// Get text representing node in human readable form
        /// </summary>
        /// <param name="node">Node which textual representation is needed</param>
        /// <returns>Textual representation of given node</returns>
        private string getStatementText(INodeAST node)
        {
            return node.ToString().Trim();
        }

        /// <summary>
        /// Get textual representation of node that represents conditional block
        /// </summary>
        /// <param name="block">Conditional block which representation is needed</param>
        /// <returns>Textual representation of given node</returns>
        private string getConditionalBlockTest(INodeAST block)
        {
            return string.Format("{0}({1})", block.Value, block.Arguments[0]);
        }

        #endregion

        #region Instruction generating

        /// <summary>
        /// Emit instructions of whole method
        /// </summary>
        private void generateInstructions()
        {
            var entryBlock = E.StartNewInfoBlock();
            entryBlock.Comment = "===Compiler initialization===";
            entryBlock.BlockTransformProvider = new Transformations.BlockProvider(getFirstLine(), _method.Source);

            if (MethodInfo.HasThis)
            {
                E.AssignArgument("this", MethodInfo.DeclaringType, 0);
            }

            //generate argument assigns
            for (uint i = 0; i < MethodInfo.Parameters.Length; ++i)
            {
                var arg = MethodInfo.Parameters[i];
                E.AssignArgument(arg.Name, arg.Type, i + 1); //argument 0 is always this object

                var variable = new VariableInfo(arg.Name);
                variable.HintAssignedType(arg.Type);
                declareVariable(variable);
            }

            //generate method body
            generateSubsequence(_method);
        }

        private INodeAST getFirstLine()
        {
            if (_method.Subsequence.Lines.Length > 0)
            {
                return _method.Subsequence.Lines[0];
            }
            else
            {
                return null;
            }
        }

        private void generateSubsequence(INodeAST node)
        {
            foreach (var line in node.Subsequence.Lines)
            {
                if (line.NodeType == NodeTypes.block)
                {
                    generateBlock(line);
                }
                else
                {
                    generateLine(line);
                }
            }
        }

        private void generateBlock(INodeAST block)
        {
            switch (block.Value)
            {
                case "if":
                    generateIf(block);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void generateIf(INodeAST ifBlock)
        {
            var info = E.StartNewInfoBlock();
            info.Comment = "\n---" + getConditionalBlockTest(ifBlock) + "---";

            var condition = getRValue(ifBlock.Arguments[0]);
            var ifBranch = ifBlock.Arguments[1];
            var elseBranch = ifBlock.Arguments.Length > 2 ? ifBlock.Arguments[2] : null;

            var trueLbl = E.GetTemporaryLabel("_true");
            var falseLbl = E.GetTemporaryLabel("_false");
            var endLbl = E.GetTemporaryLabel("_end");

            if (elseBranch == null)
            {
                //there is no false branch, jump directly to end
                falseLbl = endLbl;
            }

            //generate condition block with jump table
            E.ConditionalJump(condition.GetStorage(), trueLbl);
            E.Jump(falseLbl);

            //generate if branch
            E.SetLabel(trueLbl);
            generateSubsequence(ifBranch);

            if (elseBranch != null)
            {
                //if there is else branch generate it
                E.Jump(endLbl);
                E.SetLabel(falseLbl);
                generateSubsequence(elseBranch);
            }

            E.SetLabel(endLbl);
            //because of editing can proceed smoothly (detecting borders)
            E.Nop();
        }

        private void generateLine(INodeAST line)
        {
            var info = E.StartNewInfoBlock();
            info.Comment = "\n---" + getStatementText(line) + "---";
            info.BlockTransformProvider = new Transformations.BlockProvider(line);
            generateStatement(line);
        }

        private void generateStatement(INodeAST statement)
        {
            switch (statement.NodeType)
            {
                case NodeTypes.binaryOperator:
                    generateBinary(statement);
                    break;
                case NodeTypes.prefixOperator:
                    generatePrefix(statement);
                    break;
                case NodeTypes.call:
                    generateCall(statement);
                    break;
                case NodeTypes.hierarchy:
                    generateHierarchy(statement);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void generateHierarchy(INodeAST statement)
        {
            var hierarchy = resolveRHierarchy(statement);
            hierarchy.Generate();
        }

        private void generatePrefix(INodeAST statement)
        {
            switch (statement.Value)
            {
                case "return":
                    var rValue = getRValue(statement.Arguments[0]);

                    rValue.Return();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void generateCall(INodeAST statement)
        {
            var call = resolveRHierarchy(statement);
            call.Generate();
        }

        private void generateBinary(INodeAST statement)
        {
            var binary = resolveBinary(statement);
            binary.Generate();
        }

        #endregion

        #region Value providing
        private LValueProvider getLValue(INodeAST lValue)
        {
            switch (lValue.NodeType)
            {
                case NodeTypes.declaration:
                    var typeNode = lValue.Arguments[0];
                    var nameNode = lValue.Arguments[1];

                    //TODO type resolving
                    var variable = new VariableInfo(lValue);
                    declareVariable(variable);
                    return new VariableValue(variable, lValue, _context);

                case NodeTypes.hierarchy:
                    //TODO resolve hierarchy
                    var varInfo = getVariableInfo(lValue.Value);
                    return new VariableValue(varInfo, lValue, _context);

                default:
                    throw new NotImplementedException();
            }
        }

        private RValueProvider getRValue(INodeAST valueNode)
        {
            var value = valueNode.Value;
            RValueProvider result;

            switch (valueNode.NodeType)
            {
                case NodeTypes.call:
                case NodeTypes.hierarchy:
                    result = resolveRHierarchy(valueNode);
                    break;
                case NodeTypes.binaryOperator:
                    result = resolveBinary(valueNode);
                    break;
                case NodeTypes.prefixOperator:
                    result = resolveUnary(valueNode);
                    break;
                default:
                    throw new NotImplementedException();
            }

            CompilationInfo.RegisterNodeType(valueNode, result.GetResultInfo());
            return result;
        }


        private RValueProvider resolveUnary(INodeAST unary)
        {
            var operand = unary.Arguments[0];
            switch (unary.Value)
            {
                case "new":
                    return resolveNew(operand);
                default:
                    throw new NotImplementedException();
            }
        }

        private RValueProvider resolveNew(INodeAST newOperand)
        {
            INodeAST callNode;
            var typeSuffix = resolveCtorSuffix(newOperand, out callNode);

            var searcher = _context.CreateSearcher();
            searcher.ExtendName(getNamespaces());
            searcher.ExtendName(typeSuffix);
            searcher.Dispatch(Naming.CtorName);

            if (!searcher.FoundResult.Any())
            {
                throw new NotSupportedException("Constructor wasn't found");
            }

            //TODO selection can be done more accurate
            var objectType = searcher.FoundResult.First().DeclaringType;
            var nObject = new NewObjectValue(objectType, _context);

            var activation = findMatchingActivation(nObject, callNode, searcher.FoundResult);
            if (activation == null)
            {
                throw new NotSupportedException("Constructor call doesn't match to any available definition");
            }

            var ctorCall = new CallRValue(activation, _context);

            nObject.SetCtor(ctorCall);
            return nObject;
        }

        private RValueProvider resolveBinary(INodeAST binary)
        {
            var lNode = binary.Arguments[0];
            var rNode = binary.Arguments[1];

            if (_mathOperatorMethods.ContainsKey(binary.Value))
            {
                return resolveMathOperator(lNode, binary.Value, rNode);
            }
            else
            {
                return resolveAssignOperator(lNode, binary.Value, rNode);
            }
        }

        private RValueProvider resolveAssignOperator(INodeAST lNode, string op, INodeAST rNode)
        {
            switch (op)
            {
                case "=":
                    var lValue = getLValue(lNode);
                    var rValue = getRValue(rNode);

                    rValue.AssignInto(lValue);

                    var lVar = getVariableInfo(lValue.Storage);
                    return new VariableRValue(lVar, lNode, _context);
                default:
                    throw new NotImplementedException();
            }
        }

        private RValueProvider resolveMathOperator(INodeAST lNode, string op, INodeAST rNode)
        {
            var lOperandProvider = getRValue(lNode);
            var rOperandProvider = getRValue(rNode);

            var lTypeInfo = lOperandProvider.GetResultInfo();
            var rTypeInfo = rOperandProvider.GetResultInfo();

            var lOperand = lOperandProvider.GetStorage();
            var rOperand = rOperandProvider.GetStorage();

            var opMethodId = findOperator(lTypeInfo, rTypeInfo, op);
            E.Call(opMethodId, lOperand, Arguments.Values(rOperand));

            var result = E.GetTemporaryVariable();
            E.AssignReturnValue(result, lTypeInfo);
            return new TemporaryRVariableValue(_context, result);
        }

        private MethodID findOperator(InstanceInfo lOp, InstanceInfo rOp, string op)
        {
            //translate method according to operators table
            var method = _mathOperatorMethods[op];

            var searcher = _context.CreateSearcher();
            searcher.SetCalledObject(lOp);
            searcher.Dispatch(method);

            //TODO properly determine which call is needed (number hiearchy - overloading)
            return searcher.FoundResult.First().MethodID;
        }

        private Argument[] getArguments(INodeAST node)
        {
            var argNodes = node.Arguments;
            if (node.NodeType == NodeTypes.hierarchy && node.Indexer != null)
            {
                argNodes = node.Indexer.Arguments;
            }

            var args = new List<Argument>();
            foreach (var argNode in argNodes)
            {
                //TODO resolve named arguments
                var arg = new Argument(getRValue(argNode));
                args.Add(arg);
            }

            return args.ToArray();
        }

        private RValueProvider resolveRHierarchy(INodeAST node)
        {
            //first token can be literal/variable/call
            //other hierarchy tokens can only be calls (fields are resolved as calls to property getters)
            RValueProvider result;

            var hasBaseObject = tryGetLiteral(node, out result) || tryGetVariable(node, out result);
            var isIndexerCall = node.Indexer != null && node.NodeType == NodeTypes.hierarchy;
            var hasCallExtending = node.Child != null || node.Indexer != null;

            if (hasBaseObject && hasCallExtending)
            {
                //object based call
                var baseObject = result;
                var callNode = isIndexerCall ? node : node.Child;

                if (!tryGetCall(callNode, out result, baseObject))
                {
                    throw new NotSupportedException("Unknown object call hierarchy construction on " + callNode);
                }
            }
            else if (!hasBaseObject)
            {
                //there can only be unbased call hierarchy (note: static method with namespaces, etc. is whole call hierarchy) 
                if (!tryGetCall(node, out result))
                {
                    throw new NotSupportedException("Unknown hierarchy construction on " + node);
                }
            }

            return result;
        }

        private bool tryGetLiteral(INodeAST literalNode, out RValueProvider literal)
        {
            var literalToken = literalNode.Value;
            if (literalToken.Contains('"'))
            {
                literalToken = literalToken.Replace("\"", "");
                literal = new LiteralValue(literalToken, literalNode, _context);
                return true;
            }

            int num;
            if (int.TryParse(literalToken, out num))
            {
                literal = new LiteralValue(num, literalNode, _context);
                return true;
            }

            bool bl;
            if (bool.TryParse(literalToken, out bl))
            {
                literal = new LiteralValue(bl, literalNode, _context);
                return true;
            }

            if (literalNode.Value == "typeof")
            {
                if (literalNode.Arguments.Length == 0)
                {
                    throw new NotSupportedException("typeof doesn't have type specified");
                }

                var type = resolveTypeofArg(literalNode.Arguments[0]);

                literal = new LiteralValue(type, literalNode, _context);
                return true;
            }

            literal = null;
            return false;
        }

        private LiteralType resolveTypeofArg(INodeAST typeofArg)
        {
            var resultType = new StringBuilder();
            var currentNode = typeofArg;

            while (currentNode != null)
            {
                if (resultType.Length > 0)
                {
                    //add delimiter between namespaces
                    resultType.Append('.');
                }
                resultType.Append(currentNode.Value);

                //shift to next namespace
                currentNode = currentNode.Child;
            }

            var info = TypeDescriptor.Create(resultType.ToString());

            return new LiteralType(info);
        }

        private bool tryGetVariable(INodeAST variableNode, out RValueProvider variable)
        {
            var variableName = variableNode.Value;
            var varInfo = getVariableInfo(variableName);
            if (varInfo != null)
            {
                variable = new VariableRValue(varInfo, variableNode, _context);
                return true;
            }

            variable = null;
            return false;
        }

        private string resolveCtorSuffix(INodeAST ctorCall, out INodeAST callNode)
        {
            var name = new StringBuilder();

            callNode = null;
            while (ctorCall != null)
            {
                if (name.Length > 0)
                {
                    name.Append('.');
                }

                name.Append(ctorCall.Value);
                callNode = ctorCall;
                ctorCall = ctorCall.Child;
            }

            var typeName = _context.Map(name.ToString());
            if (callNode.NodeType != NodeTypes.call && callNode.Indexer != null)
            {
                //array definition
                typeName = string.Format("Array<{0},{1}>", typeName, callNode.Indexer.Arguments.Length);
            }

            return typeName;
        }

        private bool tryGetCall(INodeAST callHierarchy, out RValueProvider call, RValueProvider calledObject = null)
        {
            //x without base can resolve to:            
            //[this namespace].this.get_x /this.set_x
            //[this namespace].[static class x]
            //[this namespace].[namespace x]
            //[imported namespaces].[static class x]
            //[imported namespaces].[namespace x]

            var currNode = callHierarchy;
            var searcher = _context.CreateSearcher();

            if (calledObject == null)
            {
                searcher.ExtendName(getNamespaces());
            }
            else
            {
                var calledObjectInfo = calledObject.GetResultInfo();
                searcher.SetCalledObject(calledObjectInfo);
            }

            while (currNode != null)
            {
                var nextNode = currNode.Child;

                dispatchByNode(currNode, searcher);

                if (searcher.HasResults)
                {
                    var callActivation = findMatchingActivation(calledObject, currNode, searcher.FoundResult);
                    if (callActivation == null)
                    {
                        break;
                    }

                    var resolvedCall = new CallRValue(callActivation, _context);
                    if (nextNode == null)
                    {
                        //end of call chain
                        call = resolvedCall;
                        return true;
                    }
                    else
                    {
                        //call chaining
                        return tryGetCall(nextNode, out call, resolvedCall);
                    }
                }

                if (currNode.NodeType == NodeTypes.hierarchy)
                {
                    //only hierarchy hasn't been resolved immediately(namespaces) -> shift to next node
                    searcher.ExtendName(currNode.Value);
                    currNode = nextNode;
                }
                else
                {
                    //call has to be found
                    break;
                }
            }

            call = null;
            return false;
        }

        private void dispatchByNode(INodeAST currNode, MethodSearcher searcher)
        {
            switch (currNode.NodeType)
            {
                case NodeTypes.hierarchy:
                    if (currNode.Indexer == null)
                    {
                        Debug.Assert(currNode.Arguments.Length == 0);
                        searcher.Dispatch("get_" + currNode.Value);
                        searcher.Dispatch("set_" + currNode.Value);
                    }
                    else
                    {
                        searcher.Dispatch("get_Item");
                        searcher.Dispatch("set_Item");
                    }
                    break;
                case NodeTypes.call:
                    //TODO this is not correct!!
                    searcher.Dispatch(_context.Map(currNode.Value));
                    break;
                default:
                    throw new NotSupportedException("Cannot resolve given node type inside hierarchy");
            }
        }

        private CallActivation findMatchingActivation(RValueProvider calledObject, INodeAST callNode, IEnumerable<TypeMethodInfo> methods)
        {
            var selector = new MethodSelector(methods, _context);

            var argNode = callNode;
            var arguments = getArguments(callNode);
            var callActivation = selector.CreateCallActivation(arguments);

            if (callActivation != null)
            {
                callActivation.CallNode = callNode;
                //TODO better this object resolution
                if (calledObject == null && !callActivation.MethodInfo.IsStatic)
                {
                    calledObject = new TemporaryRVariableValue(_context, "this");
                }

                callActivation.CalledObject = calledObject;
            }

            return callActivation;
        }

        private void declareVariable(VariableInfo variable)
        {
            CompilationInfo.DeclareVariable(variable);
        }

        private VariableInfo getVariableInfo(string variableName)
        {
            return CompilationInfo.GetVariable(variableName);
        }

        /// <summary>
        /// Get all namespaces that are valid within compiled method
        /// </summary>
        /// <returns>namespaces</returns>
        private string[] getNamespaces()
        {
            return _source.Namespaces.ToArray();
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Register generic arguments that are available from given activation
        /// </summary>
        /// <param name="activation">Parsing activation where generic arguments and parameters are defined</param>
        private void registerGenericArguments(ParsingActivation activation)
        {
            var genericArgs = activation.Method.Path.GenericArgs;
            var genericParams = activation.GenericParameters.ToArray();

            for (int i = 0; i < genericParams.Length; ++i)
            {
                var genericArg = genericArgs[i];
                var genericParam = genericParams[i];

                _context.RegisterGenericArgument(genericParam, genericArg);
            }
        }

        #endregion
    }
}
