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
using AssemblyProviders.CSharp.LanguageDefinitions;

namespace AssemblyProviders.CSharp
{

    /// <summary>
    /// Compiler from C# to AIL implementation. Internaly uses <see cref="SyntaxParser"/>. Cover 
    /// all important .NET constructs that are needed for analyzing MEF composition.
    /// 
    /// Generate methods - are for instruction emitting
    /// Resolve methods - are for getting result of operation
    /// Get methods - are for getting value representation of operation
    /// </summary>
    public class Compiler
    {
        /// <summary>
        /// Mapping operators on .NET methods. First operand is treated as called object, second one is passed as call argument.
        /// </summary>
        private static readonly Dictionary<string, string> _mathOperatorMethods = new Dictionary<string, string>(){
              {"+","add_operator"},
              {"-","sub_operator"},
              {"*","mul_operator"},
              {"/","div_operator"},
              {"<","lesser_operator"},
              {">","greater_operator"},
              
              {"==","Equals"}
        };

        #region Compiler constants

        /// <summary>
        /// Comment used for entry block of method
        /// </summary>
        private static readonly string EntryComment = "===Compiler initialization===";

        /// <summary>
        /// End of comment emmited for C# instruction
        /// </summary>
        private static readonly string InstructionCommentEnd = "---";

        /// <summary>
        /// Start of comment emmited for C# instruction
        /// </summary>
        private static readonly string InstructionCommentStart = '\n' + InstructionCommentEnd;

        /// <summary>
        /// Caption for labels on true branches
        /// </summary>
        private static readonly string TrueLabelCaption = "_true";

        /// <summary>
        /// Caption for labels on false branches
        /// </summary>
        private static readonly string FalseLabelCaption = "_false";

        /// <summary>
        /// Caption for labels on block ends
        /// </summary>
        private static readonly string EndLabelCaption = "_end";

        #endregion

        /// <summary>
        /// Activation that represents request for compiling
        /// </summary>
        private readonly ParsingActivation _activation;

        /// <summary>
        /// Source of parsed method
        /// </summary>
        private readonly Source _source;

        /// <summary>
        /// Parser used for creating tokens and their clasification
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
        /// API method providing access to compiler instruction emitting services.
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

        #region Instruction generating

        /// <summary>
        /// Emit instructions of whole method
        /// </summary>
        private void generateInstructions()
        {
            //information attached to entry block of method preparation
            var entryBlock = E.StartNewInfoBlock();
            entryBlock.Comment = EntryComment;
            entryBlock.BlockTransformProvider = new Transformations.BlockProvider(getFirstLine(), _method.Source);

            //generate argument assigns
            generateArgumentsInitialization();

            //generate method body
            generateSubsequence(_method);
        }

        /// <summary>
        /// Emit assigns of arguments and this variable
        /// </summary>
        private void generateArgumentsInitialization()
        {
            //prepare object that is called
            if (MethodInfo.HasThis)
            {
                E.AssignArgument(CSharpSyntax.ThisVariable, MethodInfo.DeclaringType, 0);
            }
            else
            {
                //self object is not available in C#
            }

            //prepare arguments of method
            for (uint i = 0; i < MethodInfo.Parameters.Length; ++i)
            {
                var arg = MethodInfo.Parameters[i];
                E.AssignArgument(arg.Name, arg.Type, i + 1); //argument 0 is always object which method is called

                var variable = new VariableInfo(arg.Name);
                variable.HintAssignedType(arg.Type);
                declareVariable(variable);
            }
        }

        /// <summary>
        /// Generate instructions from subsequence of given node
        /// </summary>
        /// <param name="node">Node which subsequence instructions will be generated</param>
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

        /// <summary>
        /// Generate instructions for given block.
        /// </summary>
        /// <param name="block">Block which instructions are generated</param>
        private void generateBlock(INodeAST block)
        {
            switch (block.Value)
            {
                case CSharpSyntax.IfOperator:
                    generateIf(block);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Generate instructions for if block
        /// </summary>
        /// <param name="ifBlock">If block which instructions will be generated</param>
        private void generateIf(INodeAST ifBlock)
        {
            var info = E.StartNewInfoBlock();
            info.Comment = InstructionCommentStart + getConditionalBlockTest(ifBlock) + InstructionCommentEnd;

            //block
            var condition = getRValue(ifBlock.Arguments[0]);
            var ifBranch = ifBlock.Arguments[1];
            var elseBranch = ifBlock.Arguments.Length > 2 ? ifBlock.Arguments[2] : null;

            //branch labels
            var trueLbl = E.GetTemporaryLabel(TrueLabelCaption);
            var falseLbl = E.GetTemporaryLabel(FalseLabelCaption);
            var endLbl = E.GetTemporaryLabel(EndLabelCaption);

            if (elseBranch == null)
            {
                //there is no false branch, jump directly to end
                falseLbl = endLbl;
            }

            //generate condition block with jump table
            E.ConditionalJump(condition.GenerateStorage(), trueLbl);
            E.Jump(falseLbl);

            //generate if branch
            E.SetLabel(trueLbl);
            generateSubsequence(ifBranch);

            if (elseBranch != null)
            {
                //if there is else branch generate it

                //firstly protect falling into else branch from ifbranch
                E.Jump(endLbl);
                E.SetLabel(falseLbl);
                generateSubsequence(elseBranch);
            }

            E.SetLabel(endLbl);
            //because of editing can proceed smoothly (detecting borders)
            E.Nop();
        }

        /// <summary>
        /// Generate instructions from given line
        /// </summary>
        /// <param name="line">Line which instructions will be generated</param>
        private void generateLine(INodeAST line)
        {
            var info = E.StartNewInfoBlock();
            info.Comment = InstructionCommentStart + getStatementText(line) + InstructionCommentEnd;
            info.BlockTransformProvider = new Transformations.BlockProvider(line);
            generateStatement(line);
        }

        /// <summary>
        /// Generate instructions from given statement
        /// </summary>
        /// <param name="statement">Statement which instrucitons will be generated</param>
        private void generateStatement(INodeAST statement)
        {
            switch (statement.NodeType)
            {
                case NodeTypes.binaryOperator:
                    generateBinaryOperator(statement);
                    break;
                case NodeTypes.prefixOperator:
                    generatePrefixOperator(statement);
                    break;
                case NodeTypes.postOperator:
                    generatePostfixOperator(statement);
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

        /// <summary>
        /// Generate instructions from given hierarchy node
        /// </summary>
        /// <param name="hierarchy">Node representing hierarchy of value operations</param>
        private void generateHierarchy(INodeAST hierarchy)
        {
            var resolvedHierarchy = resolveRHierarchy(hierarchy);
            resolvedHierarchy.Generate();
        }

        /// <summary>
        /// Generate instructions for given prefix operator node
        /// </summary>
        /// <param name="prefixOperator">Node representing prefix operator</param>
        private void generatePrefixOperator(INodeAST prefixOperator)
        {
            var argumentNode = prefixOperator.Arguments[0];
            switch (prefixOperator.Value)
            {
                case CSharpSyntax.ReturnOperator:
                    //return has to be resolved in special way - it cannot be 
                    //resolved as value
                    var returnValue = getRValue(argumentNode);
                    returnValue.GenerateReturn();
                    break;

                default:
                    var resolved = resolvePrefixOperator(prefixOperator);
                    resolved.Generate();
                    break;
            }
        }

        /// <summary>
        /// Generate instructions for given postfix operator node
        /// </summary>
        /// <param name="postfixOperator">Node representing postfix operator</param>
        private void generatePostfixOperator(INodeAST postfixOperator)
        {
            //there are no postfix operators that couldnt been resolved
            var resolved = resolvePostfixOperator(postfixOperator);
            resolved.Generate();
        }

        /// <summary>
        /// Generate instructions for given binary operator node
        /// </summary>
        /// <param name="binary">Node representing  binary operation</param>
        private void generateBinaryOperator(INodeAST binary)
        {
            var resolvedBinary = resolveBinary(binary);
            resolvedBinary.Generate();
        }

        /// <summary>
        /// Generate instructions for given call node
        /// </summary>
        /// <param name="call">Node representing method call</param>
        private void generateCall(INodeAST call)
        {
            var resolvedCall = resolveRHierarchy(call);
            resolvedCall.Generate();
        }

        #endregion

        #region Node value resolving

        #region General value resolving

        /// <summary>
        /// Get provider representing lvalue
        /// </summary>
        /// <param name="lValue">Node which lvalue provider is needed</param>
        /// <returns><see cref="LValueProvider"/> represented by given node</returns>
        private LValueProvider getLValue(INodeAST lValue)
        {
            switch (lValue.NodeType)
            {
                case NodeTypes.declaration:
                    //declaration of new variable
                    var variable = new VariableInfo(lValue);
                    declareVariable(variable);
                    return new VariableValue(variable, lValue, _context);

                case NodeTypes.hierarchy:
                    //TODO resolve hierarchy - property setter,...
                    var varInfo = getVariableInfo(lValue.Value);
                    return new VariableValue(varInfo, lValue, _context);

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Get provider representing rvalue
        /// </summary>
        /// <param name="rValue">Node which rvalue provider is needed</param>
        /// <returns><see cref="RValueProvider"/> represented by given node</returns>
        private RValueProvider getRValue(INodeAST rValue)
        {
            //TODO check semantic

            RValueProvider result;

            switch (rValue.NodeType)
            {
                case NodeTypes.call:
                case NodeTypes.hierarchy:
                    result = resolveRHierarchy(rValue);
                    break;
                case NodeTypes.binaryOperator:
                    result = resolveBinary(rValue);
                    break;
                case NodeTypes.prefixOperator:
                    result = resolvePrefixOperator(rValue);
                    break;
                case NodeTypes.postOperator:
                    result = resolvePostfixOperator(rValue);
                    break;
                default:
                    throw new NotImplementedException();
            }

            CompilationInfo.RegisterNodeType(rValue, result.Type);
            return result;
        }

        /// <summary>
        /// Resolve hierarchy node providing begining of rvalue expression
        /// </summary>
        /// <param name="hierarchy">Node where hierarchy of rvalue expression starts</param>
        /// <returns><see cref="RValueProvider"/> representing rvalue provided by hierarchy</returns>
        private RValueProvider resolveRHierarchy(INodeAST hierarchy)
        {
            //first token can be literal/variable/call
            //other hierarchy tokens can only be calls (fields are resolved as calls to property getters)
            RValueProvider result;

            var hasBaseObject = tryGetLiteral(hierarchy, out result) || tryGetVariable(hierarchy, out result);
            var isIndexerCall = hierarchy.Indexer != null && hierarchy.NodeType == NodeTypes.hierarchy;
            var hasCallExtending = hierarchy.Child != null || hierarchy.Indexer != null;

            if (hasBaseObject && hasCallExtending)
            {
                //object based call
                var baseObject = result;
                var callNode = isIndexerCall ? hierarchy : hierarchy.Child;

                if (!tryGetCall(callNode, out result, baseObject))
                {
                    throw new NotSupportedException("Unknown object call hierarchy construction on " + callNode);
                }
            }
            else if (!hasBaseObject)
            {
                //there can only be unbased call hierarchy (note: static method with namespaces, etc. is whole call hierarchy) 
                if (!tryGetCall(hierarchy, out result))
                {
                    throw new NotSupportedException("Unknown hierarchy construction on " + hierarchy);
                }
            }

            return result;
        }

        /// <summary>
        /// Try to get variable for given node
        /// </summary>
        /// <param name="variableNode">Node representing needed variable</param>
        /// <param name="variable">Value provider of variable if available, <c>null</c> otherwise</param>
        /// <returns><c>true</c> if variable is available, <c>false</c> otherwise</returns>
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
        #endregion

        #region Unary operators resolving

        /// <summary>
        /// Resolve prefix operation represented by given node.
        /// <remarks>Only expression prefix operations can be resolved. It means that return operator cannot be resolved here</remarks>
        /// </summary>
        /// <param name="prefixOperator">Node representing prefix operation</param>
        /// <returns>Representation of prefix operation result</returns>
        private RValueProvider resolvePrefixOperator(INodeAST prefixOperator)
        {
            var operandNode = prefixOperator.Arguments[0];
            var operatorNotation = prefixOperator.Value;
            switch (operatorNotation)
            {
                case CSharpSyntax.NewOperator:
                    return resolveNew(operandNode);

                case CSharpSyntax.IncrementOperator:
                    var incrementedLValue = getLValue(operandNode);
                    return resolveLValueAdd(incrementedLValue, 1, true);

                case CSharpSyntax.DecrementOperator:
                    var decrementedLValue = getLValue(operandNode);
                    return resolveLValueAdd(decrementedLValue, -1, true);

                default:
                    throw parsingException(prefixOperator, "Prefix operation '{0}' is not supported", operatorNotation);
            }
        }

        /// <summary>
        /// Resolve postfix operation represented by given node.        
        /// </summary>
        /// <param name="postfixOperator">Node representing postfix operation</param>
        /// <returns>Representation of postfix operation result</returns>
        private RValueProvider resolvePostfixOperator(INodeAST postfixOperator)
        {
            var operatorNotation = postfixOperator.Value;
            var operandNode = postfixOperator.Arguments[0];
            var lValue = getLValue(operandNode);

            switch (operatorNotation)
            {
                case CSharpSyntax.IncrementOperator:
                    return resolveLValueAdd(lValue, 1, false);

                case CSharpSyntax.DecrementOperator:
                    return resolveLValueAdd(lValue, -1, false);

                default:
                    throw parsingException(postfixOperator, "Postfix operation '{0}' is not supported", operatorNotation);
            }
        }

        /// <summary>
        /// Resolve adding specified number to given target
        /// </summary>
        /// <param name="target">Lvalue where value will be added</param>
        /// <param name="addedValue">Value that is added to lvalue</param>
        /// <param name="prefixReturn">Determine that result of operation is value after or before adding</param>
        /// <returns>Representation fo value adding</returns>
        private RValueProvider resolveLValueAdd(LValueProvider target, int addedValue, bool prefixReturn)
        {
            var addRepresentation = new ComputedValue(target.Type, (e, storage) =>
            {
                var lTypeInfo = E.VariableInfo(target.Storage);
                var rTypeInfo = TypeDescriptor.Create<int>();

                var addedValueStorage = E.GetTemporaryVariable();
                E.AssignLiteral(addedValueStorage, addedValue);

                var opMethodId = findOperator(lTypeInfo, "+", rTypeInfo);

                E.Call(opMethodId, target.Storage, Arguments.Values(addedValueStorage));

                if (storage != null)
                {
                    //save copy to storage
                    if (prefixReturn)
                    {
                        E.AssignReturnValue(storage, lTypeInfo);
                    }
                    else
                    {
                        E.Assign(storage, target.Storage);
                    }
                }

                //assign added value 
                E.AssignReturnValue(target.Storage, lTypeInfo);

            }, _context);

            return addRepresentation;
        }

        #endregion

        #region Binary operators resolving

        /// <summary>
        /// Resolve binary operation represented by given node
        /// </summary>
        /// <param name="binary">Node representing binary operation</param>
        /// <returns>Representation of binary operation result</returns>
        private RValueProvider resolveBinary(INodeAST binary)
        {
            var lNode = binary.Arguments[0];
            var rNode = binary.Arguments[1];

            //TODO compound operators !=
            if (_mathOperatorMethods.ContainsKey(binary.Value))
            {
                return resolveMathOperator(lNode, binary.Value, rNode);
            }
            else
            {
                return resolveAssignOperator(lNode, binary.Value, rNode);
            }
        }

        /// <summary>
        /// Resolve assign operator on given operands
        /// </summary>
        /// <param name="lNode">Left operand of assign</param>
        /// <param name="op">Assign operator notation</param>
        /// <param name="rNode">Right operand of assign</param>
        /// <returns>Representation of assign operation result</returns>
        private RValueProvider resolveAssignOperator(INodeAST lNode, string op, INodeAST rNode)
        {
            switch (op)
            {
                case CSharpSyntax.AssignOperator:
                    var lValue = getLValue(lNode);
                    var rValue = getRValue(rNode);

                    var assignComputation = new ComputedValue(lValue.Type, (e, storage) =>
                    {
                        rValue.GenerateAssignInto(lValue);

                        var lVar = getVariableInfo(lValue.Storage);

                        //report using of variable
                        lVar.AddVariableUsing(lNode);

                        if (storage != null)
                            //assign chaining
                            E.Assign(storage, lValue.Storage);
                    }, _context);

                    return assignComputation;
                default:
                    throw new NotImplementedException("TODO add mathassign operators");
            }
        }

        /// <summary>
        /// Resolve math operator on given operands
        /// </summary>
        /// <param name="lNode">Left operand of math operation</param>
        /// <param name="op">Math operator notation</param>
        /// <param name="rNode">Right operand of math operation</param>
        /// <returns>Representation of math operatorion result</returns>
        private RValueProvider resolveMathOperator(INodeAST lNode, string op, INodeAST rNode)
        {
            var lOperandProvider = getRValue(lNode);
            var rOperandProvider = getRValue(rNode);

            var lTypeInfo = lOperandProvider.Type;
            var rTypeInfo = rOperandProvider.Type;

            var lOperand = lOperandProvider.GenerateStorage();
            var rOperand = rOperandProvider.GenerateStorage();

            var opMethodId = findOperator(lTypeInfo, op, rTypeInfo);

            var computation = new ComputedValue(lTypeInfo, (e, storage) =>
            {
                e.Call(opMethodId, lOperand, Arguments.Values(rOperand));

                //TODO determine type according to operator return value
                if (storage != null)
                    e.AssignReturnValue(storage, lTypeInfo);
            }, _context);

            return computation;
        }

        /// <summary>
        /// Find method representation of operator for given nodes
        /// </summary>
        /// <param name="leftOperandType">Type of left operand</param>
        /// <param name="op">Operator notation</param>
        /// <param name="rightOperandType">Type of right operand</param>
        /// <returns>Found operator</returns>
        private MethodID findOperator(InstanceInfo leftOperandType, string op, InstanceInfo rightOperandType)
        {
            //translate method according to operators table
            var method = _mathOperatorMethods[op];

            var searcher = _context.CreateSearcher();
            searcher.SetCalledObject(leftOperandType);
            searcher.Dispatch(method);

            //TODO properly determine which call is needed (number hiearchy - overloading)
            return searcher.FoundResult.First().MethodID;
        }

        #endregion

        #region Literal value resolving

        /// <summary>
        /// Try to get litral from given literalNode
        /// </summary>
        /// <param name="literalNode">Node tested for literal presence</param>
        /// <param name="literal">Literal represented by literalNode</param>
        /// <returns><c>true</c> if literal is represented by literalNode, <c>false</c> otherwise</returns>
        private bool tryGetLiteral(INodeAST literalNode, out RValueProvider literal)
        {
            var literalToken = literalNode.Value;
            if (literalToken.Contains('"'))
            {
                //string literal

                //TODO correct string escaping
                literalToken = literalToken.Replace("\"", "");
                literal = new LiteralValue(literalToken, literalNode, _context);
                return true;
            }

            int num;
            if (int.TryParse(literalToken, out num))
            {
                //int literal

                literal = new LiteralValue(num, literalNode, _context);
                return true;
            }

            bool bl;
            if (bool.TryParse(literalToken, out bl))
            {
                //bool literal

                literal = new LiteralValue(bl, literalNode, _context);
                return true;
            }

            if (literalNode.Value == CSharpSyntax.TypeOfOperator)
            {
                //typeof(TypeLiteral) expression
                if (literalNode.Arguments.Length == 0)
                {
                    throw new NotSupportedException("typeof doesn't have type specified");
                }

                var type = resolveTypeofArgument(literalNode.Arguments[0]);

                literal = new LiteralValue(type, literalNode, _context);
                return true;
            }

            //literalToken doesnt describe any literal
            literal = null;
            return false;
        }

        /// <summary>
        /// Resolve type literal represented by argument of typeof operator
        /// </summary>
        /// <param name="typeofArgument">Argument of typeof operator</param>
        /// <returns>Literal representation of typeof</returns>
        private LiteralType resolveTypeofArgument(INodeAST typeofArgument)
        {
            var resultType = new StringBuilder();
            var currentNode = typeofArgument;

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

            //TODO consider namespaces - same case as with object construction resolving
            var info = TypeDescriptor.Create(resultType.ToString());

            return new LiteralType(info);
        }
        #endregion

        #region Object construction resolving

        /// <summary>
        /// Resolve suffix for constructor name represented by ctorCall
        /// </summary>
        /// <param name="ctorCall">Node where constructor name suffix starts</param>
        /// <param name="callNode">Node where constructor call is found</param>
        /// <returns>Suffix for name of constructor</returns>
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

        /// <summary>
        /// Resolve operand of new operator
        /// </summary>
        /// <param name="newOperand">Operand of new operator</param>
        /// <returns>Representation of newOperand result</returns>
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

            var activation = createActivation(nObject, callNode, searcher.FoundResult);
            if (activation == null)
            {
                throw new NotSupportedException("Constructor call doesn't match to any available definition");
            }

            var ctorCall = new CallRValue(activation, _context);

            nObject.SetCtor(ctorCall);
            return nObject;
        }

        #endregion

        #region Call resolving

        /// <summary>
        /// Try to get call hierarchy (chained calls, properties, indexes, namespaces and statit classes)
        /// </summary>
        /// <param name="callHierarchy">Node where call hierarchy starts</param>
        /// <param name="call">Result representation of call hierarchy</param>
        /// <param name="calledObject">Object on which call hierarchy starts if any</param>
        /// <returns><c>true</c> if call hierarchy is recognized, <c>false</c> otherwise</returns>
        private bool tryGetCall(INodeAST callHierarchy, out RValueProvider call, RValueProvider calledObject = null)
        {
            //initialize output variable
            call = null;

            var searcher = createMethodSearcher(calledObject);

            var currNode = callHierarchy;
            while (currNode != null)
            {
                var nextNode = currNode.Child;

                dispatchByNode(searcher, currNode);

                if (searcher.HasResults)
                {
                    //there are possible overloads for call
                    var callActivation = createActivation(calledObject, currNode, searcher.FoundResult);
                    if (callActivation == null)
                    {
                        //overloads doesnt match to arguments
                        return false;
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
                    //there should not left any other node types
                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// Create method searcher filled with valid namespaces according to presence 
        /// of calledObject
        /// </summary>
        /// <param name="calledObject">Object representation that is called if available, <c>null</c> otherwise</param>
        /// <returns>Created <see cref="MethodSearcher"/></returns>
        private MethodSearcher createMethodSearcher(RValueProvider calledObject = null)
        {
            //x without base can resolve to:            
            //[this namespace].this.get_x /this.set_x
            //[this namespace].[static class x]
            //[this namespace].[namespace x]
            //[imported namespaces].[static class x]
            //[imported namespaces].[namespace x]

            var searcher = _context.CreateSearcher();

            if (calledObject == null)
            {
                searcher.ExtendName(getNamespaces());
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
        private void dispatchByNode(MethodSearcher searcher, INodeAST node)
        {
            switch (node.NodeType)
            {
                case NodeTypes.hierarchy:
                    if (node.Indexer == null)
                    {
                        Debug.Assert(node.Arguments.Length == 0);
                        searcher.Dispatch(Naming.GetterPrefix + node.Value);
                        searcher.Dispatch(Naming.SetterPrefix + node.Value);
                    }
                    else
                    {
                        searcher.Dispatch(Naming.ArrayItemGetter);
                        searcher.Dispatch(Naming.ArrayItemSetter);
                    }
                    break;

                case NodeTypes.call:
                    //TODO this is not correct!!
                    searcher.Dispatch(_context.Map(node.Value));
                    break;

                default:
                    throw new NotSupportedException("Cannot resolve given node type inside hierarchy");
            }
        }

        /// <summary>
        /// Select <see cref="TypeMethodInfo"/> according to callNode arguments and creates
        /// <see cref="CallActivation"/>.
        /// </summary>
        /// <param name="calledObject">Object which method is called. Is passed only if it is available</param>
        /// <param name="callNode">Node determining call</param>
        /// <param name="methods">Methods used for right overloading selection</param>
        /// <returns>Created call activation</returns>
        private CallActivation createActivation(RValueProvider calledObject, INodeAST callNode, IEnumerable<TypeMethodInfo> methods)
        {
            var selector = new MethodSelector(methods, _context);

            var arguments = getArguments(callNode);
            var callActivation = selector.CreateCallActivation(arguments);

            if (callActivation != null)
            {
                callActivation.CallNode = callNode;

                if (calledObject == null && !callActivation.MethodInfo.IsStatic)
                {
                    //if there is no explicit calledObject, and method call is not static
                    //implicit this object has to be used
                    calledObject = new TemporaryRVariableValue(_context, CSharpSyntax.ThisVariable);
                }

                callActivation.CalledObject = calledObject;
            }

            return callActivation;
        }

        /// <summary>
        /// Get arguments representation that is available on given node
        /// </summary>
        /// <param name="node">Node which arguments are needed</param>
        /// <returns>Arguments available for given node</returns>
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

        #endregion

        #endregion

        #region Context operations

        /// <summary>
        /// Declare given variable
        /// </summary>
        /// <param name="variable">Declared variable</param>
        private void declareVariable(VariableInfo variable)
        {
            CompilationInfo.DeclareVariable(variable);
        }

        /// <summary>
        /// Get info about current scoped variable with given name
        /// </summary>
        /// <param name="variableName">Name of needed variable</param>
        /// <returns><see cref="VariableInfo"/> that is currently scoped under variableName</returns>
        private VariableInfo getVariableInfo(string variableName)
        {
            return CompilationInfo.TryGetVariable(variableName);
        }

        #endregion

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

        #region Private helpers


        /// <summary>
        /// Create exception for parsing error detected in context of given node
        /// </summary>
        /// <param name="node">Node where error has been found</param>
        /// <param name="descriptionFormat">Format of error description</param>
        /// <param name="formatArgs">Arguments for format descritpion</param>
        /// <returns>Created exception</returns>
        private ParsingException parsingException(INodeAST node, string descriptionFormat, params object[] formatArguments)
        {
            return CSharpSyntax.ParsingException(node, descriptionFormat, formatArguments);
        }

        /// <summary>
        /// Get all namespaces that are valid within compiled method
        /// </summary>
        /// <returns>namespaces</returns>
        private string[] getNamespaces()
        {
            return _source.Namespaces.ToArray();
        }

        /// <summary>
        /// Get node corresponding to first line of method
        /// </summary>
        /// <returns>Node corresponding to first line of method, <c>null</c> if method does not contain any lines</returns>
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
