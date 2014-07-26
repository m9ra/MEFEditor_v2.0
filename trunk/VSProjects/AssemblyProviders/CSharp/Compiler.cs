using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using Analyzing;
using TypeSystem;
using TypeSystem.Runtime;

using AssemblyProviders.ProjectAssembly;
using AssemblyProviders.DirectDefinitions;
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
              {"+","op_Addition"},
              {"-","op_Subtraction"},
              {"*","op_Multiply"},
              {"/","op_Division"},
              {"%","op_Modulus"},

              {"^","op_ExclusiveOr"},
              {"&","op_BitwiseAnd"},
              {"|","op_BitwiseOr"},

              //we dont support partial evaluation - use bitwise instead
              //{"&&","op_LogicalAnd"},
              //{"||","op_LogicalOr"},
              {"&&","op_BitwiseAnd"},
              {"||","op_BitwiseOr"},              
              
              {">","op_GreaterThan"},
              {"<","op_LessThan"},
              {">=","op_GreaterThanOrEqual"},
              {"<=","op_LessThanOrEqual"},

              //this is workaround, because of missing CIL method in specification
              {"!","op_Not"},

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

        /// <summary>
        /// Caption for labels on explicit continue jumps
        /// </summary>
        private static readonly string ContinueLabelCaption = "_continue";

        /// <summary>
        /// Caption for labels on block conditions
        /// </summary>
        private static readonly string ConditionLabelCaption = "_test";

        /// <summary>
        /// Caption for labels on block loops
        /// </summary>
        private static readonly string LoopLabelCaption = "_loop";

        /// <summary>
        /// Caption for labels on case branches
        /// </summary>
        private static readonly string CaseLabelCaption = "_case";

        /// <summary>
        /// Caption for labels on default switch branch
        /// </summary>
        private static readonly string DefaultLabelCaption = "_default";

        /// <summary>
        /// Descriptor for string type
        /// </summary>
        private static readonly TypeDescriptor StringDescriptor = TypeDescriptor.Create<string>();

        /// <summary>
        /// Descriptor for bool type
        /// </summary>
        private static readonly TypeDescriptor BoolDescriptor = TypeDescriptor.Create<bool>();

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
        /// Stack of pushed blocks
        /// </summary>
        private readonly Stack<INodeAST> _blocks = new Stack<INodeAST>();

        /// <summary>
        /// Context of compilation process
        /// </summary>
        internal readonly CompilationContext Context;

        /// <summary>
        /// Emitter where compiled instructions are emitted
        /// </summary>
        private EmitterBase E { get { return Context.Emitter; } }

        /// <summary>
        /// Info that is collected about source during compilation
        /// </summary>
        private CompilationInfo CompilationInfo { get { return _source.CompilationInfo; } }

        /// <summary>
        /// Info of method that is compiled
        /// </summary>
        internal TypeMethodInfo MethodInfo { get { return _activation.Method; } }

        /// <summary>
        /// Determine that code will be inlined
        /// </summary>
        internal bool IsInlined { get { return _activation.IsInline; } }

        /// <summary>
        /// Namespaces that are available for compiled method
        /// </summary>
        internal IEnumerable<string> Namespaces { get { return _source.Namespaces; } }

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

            _source = new Source(activation.SourceCode, activation.Method);
            Context = new CompilationContext(emitter, _source, services);

            _source.SourceChangeCommited += activation.OnCommited;
            _source.NavigationRequested += activation.OnNavigated;

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
            startGroup(_method);
            if (!IsInlined)
            {
                //information attached to entry block of method preparation
                var entryBlock = E.StartNewInfoBlock();
                entryBlock.Comment = EntryComment;
                entryBlock.BlockTransformProvider = new Transformations.BlockProvider(getFirstLine(), _method.Source);

                //generate argument assigns
                generateArgumentsInitialization();

                //can generate initialization routines before method body
                generatePreBodyRoutines();
            }

            //generate method body
            generateSubsequence(_method);

            Debug.Assert(_blocks.Count == 1, "Block context handling is incorrect");
        }

        /// <summary>
        /// Emit assigns of arguments and this variable
        /// </summary>
        private void generateArgumentsInitialization()
        {
            //prepare object that is called
            var thisType = MethodInfo.DeclaringType;
            E.AssignArgument(CSharpSyntax.ThisVariable, thisType, 0);
            var thisVariable = new VariableInfo(CSharpSyntax.ThisVariable, _source.CompilationInfo);
            thisVariable.HintAssignedType(thisType);

            if (MethodInfo.HasThis)
            {
                declareVariable(thisVariable);
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

                var variable = new VariableInfo(arg.Name, _source.CompilationInfo);
                variable.HintAssignedType(arg.Type);
                declareVariable(variable);
            }
        }

        /// <summary>
        /// Generate instructions required for initialization purposes
        /// of constructors
        /// </summary>
        private void generatePreBodyRoutines()
        {
            var needsInitialization = MethodInfo.MethodName == Naming.ClassCtorName || MethodInfo.MethodName == Naming.CtorName;
            if (!needsInitialization)
                //there is nothing to do
                return;

            var initializer = Naming.Method(MethodInfo.DeclaringType, CSharpSyntax.MemberInitializer, false);
            E.Call(initializer, CSharpSyntax.ThisVariable, Arguments.Values());
        }

        /// <summary>
        /// Generate instructions from subsequence of given node
        /// </summary>
        /// <param name="node">Node which subsequence instructions will be generated</param>
        private void generateSubsequence(INodeAST node)
        {
            var hasSubsequence = node.Subsequence != null;
            var lines = hasSubsequence ? node.Subsequence.Lines : new[] { node };

            var times = new List<long>();
            var w = System.Diagnostics.Stopwatch.StartNew();
            foreach (var line in lines)
            {
                if (line.NodeType == NodeTypes.block)
                {
                    generateBlock(line);
                }
                else
                {
                    generateLine(line);
                }

                times.Add(w.ElapsedMilliseconds);
                w.Restart();
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

                case CSharpSyntax.DoOperator:
                    generateDo(block);
                    break;

                case CSharpSyntax.WhileOperator:
                    generateWhile(block);
                    break;

                case CSharpSyntax.ForOperator:
                    generateFor(block);
                    break;

                case CSharpSyntax.ForeachOperator:
                    generateForeach(block);
                    break;

                case CSharpSyntax.SwitchOperator:
                    generateSwitch(block);
                    break;

                default:
                    throw new NotSupportedException("Block command '" + block.Value + "' is not supported by C# compiler.");
            }
        }

        /// <summary>
        /// Generate instructions for switch block
        /// </summary>
        /// <param name="switchBlock">Switch block which instructions will be generated</param>
        private void generateSwitch(INodeAST switchBlock)
        {
            startInfoBlock(switchBlock);
            startGroup(switchBlock);

            //transfer continue from upper context
            var continueLbl = Context.CurrentBlock == null ? null : Context.CurrentBlock.ContinueLabel;
            var defaultLbl = E.GetTemporaryLabel(DefaultLabelCaption);
            var endLbl = E.GetTemporaryLabel(EndLabelCaption);

            //push block to register continue and end labels
            Context.PushBlock(switchBlock,
                    continueLbl,
                    endLbl
              );

            //collect case branches            
            var caseBranches = new List<INodeAST>();
            INodeAST defaultBranch = null;
            for (var i = 1; i < switchBlock.Arguments.Length; ++i)
            {
                var caseBranch = switchBlock.Arguments[i];
                var isDefaultBranch = caseBranch.Arguments.Length == 0;
                if (isDefaultBranch)
                {
                    defaultBranch = caseBranch;
                }
                else
                {
                    caseBranches.Add(caseBranch);
                }
            }

            //resolve condition
            var conditionNode = switchBlock.Arguments[0];

            var condition = getRValue(conditionNode);
            var conditionStorage = condition.GenerateStorage();
            var conditionRStorage = new TemporaryRVariableValue(Context, conditionStorage);

            //generate jump table
            var caseValueStorage = new TemporaryVariableValue(condition.Type, Context);
            var caseValueRStorage = new TemporaryRVariableValue(Context, caseValueStorage.Storage);
            var caseLabels = new List<Label>();

            var testValueStorage = new TemporaryVariableValue(BoolDescriptor, Context);

            foreach (var caseBranch in caseBranches)
            {
                var caseLabel = E.GetTemporaryLabel(CaseLabelCaption);
                caseLabels.Add(caseLabel);

                var caseConditionNode = caseBranch.Arguments[0];
                var caseCondition = getRValue(caseConditionNode);
                caseCondition.GenerateAssignInto(caseValueStorage);

                //note that comparing slightly differs from C# switch semantic. However
                //switch is allowed only on primitive and constant values
                //so it doesn't matter
                var comparisonActivation = createBinaryOperatorActivation(conditionRStorage, CSharpSyntax.IsEqualOperator, caseValueRStorage, caseConditionNode);
                var comparisonCall = new CallValue(comparisonActivation, Context);
                comparisonCall.GenerateAssignInto(testValueStorage);

                E.ConditionalJump(testValueStorage.Storage, caseLabel);
            }

            //condition table defaults
            var hasDefaultBranch = defaultBranch != null;
            var tableDefaultLbl = hasDefaultBranch ? defaultLbl : endLbl;
            E.Jump(tableDefaultLbl);


            //generate branche statements
            for (var caseIndex = 0; caseIndex < caseBranches.Count; ++caseIndex)
            {
                var caseBranch = caseBranches[caseIndex];
                var caseLabel = caseLabels[caseIndex];

                startGroup(caseBranch);
                E.SetLabel(caseLabel);
                generateSubsequence(caseBranch);
                endGroup();
            }

            //generate default branch if any
            if (hasDefaultBranch)
            {
                E.SetLabel(defaultLbl);
                generateSubsequence(defaultBranch);
            }

            E.SetLabel(endLbl);
            E.Nop();

            endGroup();
        }

        /// <summary>
        /// Generate instructions for for block
        /// </summary>
        /// <param name="forBlock">For block which instructions will be generated</param>
        private void generateFor(INodeAST forBlock)
        {
            startGroup(forBlock);
            startInfoBlock(forBlock);

            //block labels
            var conditionLbl = E.GetTemporaryLabel(ConditionLabelCaption);
            var loopLbl = E.GetTemporaryLabel(LoopLabelCaption);
            var endLbl = E.GetTemporaryLabel(EndLabelCaption);
            var continueLbl = E.GetTemporaryLabel(ContinueLabelCaption);

            //push block to register continue and end labels
            Context.PushBlock(forBlock,
                    continueLbl,
                    endLbl
              );

            //for loop initializer
            var initializer = forBlock.Arguments[0];
            generateStatement(initializer);

            //for loop condition
            var condition = getRValue(forBlock.Arguments[1]);
            var increment = forBlock.Arguments[2];
            var loop = forBlock.Child;

            //conditional jump table
            E.SetLabel(conditionLbl);
            if (condition != null)
                E.ConditionalJump(condition.GenerateStorage(), loopLbl);
            E.Jump(endLbl);
            E.SetLabel(loopLbl);
            endGroup();

            startGroup(loop);
            //body of the loop
            generateSubsequence(loop);

            //for loop increment
            E.SetLabel(continueLbl);
            startInfoBlock(increment, "=increment: " + getStatementText(increment));
            generateStatement(increment);

            //repeat
            E.Jump(conditionLbl);
            E.SetLabel(endLbl);
            E.Nop();

            Context.PopBlock();
            endGroup();
        }

        /// <summary>
        /// Generate instructions for foreach block
        /// </summary>
        /// <param name="foreachBlock">Foreach block which instructions will be generated</param>
        private void generateForeach(INodeAST foreachBlock)
        {
            startGroup(foreachBlock);
            startInfoBlock(foreachBlock);

            //block labels
            var conditionLbl = E.GetTemporaryLabel(ConditionLabelCaption);
            var loopLbl = E.GetTemporaryLabel(LoopLabelCaption);
            var endLbl = E.GetTemporaryLabel(EndLabelCaption);

            //push block to register continue and end labels
            Context.PushBlock(foreachBlock,
                    conditionLbl,
                    endLbl
              );

            //get enumerator
            var enumeratedNode = foreachBlock.Arguments[1];
            var enumeratedValue = getRValue(enumeratedNode);

            var getEnumeratorCall = createCall(enumeratedValue, "GetEnumerator", enumeratedNode);
            var enumeratorStorage = new TemporaryRVariableValue(Context, getEnumeratorCall.GenerateStorage());
            var moveNextCall = createCall(enumeratorStorage, "MoveNext", enumeratedNode);

            //foreach loop condition
            E.SetLabel(conditionLbl);
            E.ConditionalJump(moveNextCall.GenerateStorage(), loopLbl);
            E.Jump(endLbl);
            endGroup();

            var loop = foreachBlock.Child;
            startGroup(loop);
            //body of the loop
            E.SetLabel(loopLbl);

            //iterated variable
            var getCurrent = createCall(enumeratorStorage, "get_Current", enumeratedNode);
            //for loop variable declaration
            var variable = getLValue(foreachBlock.Arguments[0], getCurrent.MethodInfo.ReturnType);
            getCurrent.GenerateAssignInto(variable);

            //body of the loop
            generateSubsequence(loop);

            //repeat
            E.Jump(conditionLbl);
            E.SetLabel(endLbl);
            E.Nop();

            Context.PopBlock();
            endGroup();
        }

        private CallValue createCall(RValueProvider calledObject, string callName, INodeAST calledObjectNode)
        {
            var searcher = Context.CreateSearcher();
            searcher.SetCalledObject(calledObject.Type);
            searcher.Dispatch(callName);
            if (!searcher.HasResults)
                throw parsingException(calledObjectNode, "Missing {0} method", callName);

            var selector = new MethodSelector(searcher.FoundResult, Context);
            var activation = createCallActivation(selector, calledObject, calledObjectNode, new Argument[0]);
            if (activation == null)
            {
                throw parsingException(calledObjectNode, "{0} there is no matching overload for given arguments");
            }

            var call = new CallValue(activation, Context);
            return call;
        }

        /// <summary>
        /// Generate instructions for while block
        /// </summary>
        /// <param name="whileBlock">While block which instructions will be generated</param>
        private void generateWhile(INodeAST whileBlock)
        {
            startGroup(whileBlock);
            startInfoBlock(whileBlock);

            //block labels
            var conditionLbl = E.GetTemporaryLabel(ConditionLabelCaption);
            var loopLbl = E.GetTemporaryLabel(LoopLabelCaption);
            var endLbl = E.GetTemporaryLabel(EndLabelCaption);

            Context.PushBlock(whileBlock,
                    conditionLbl,
                    endLbl
                );

            //loop primitives
            var condition = getRValue(whileBlock.Arguments[0]);
            var loop = whileBlock.Arguments[1];

            //conditional jump table
            E.SetLabel(conditionLbl);
            E.ConditionalJump(condition.GenerateStorage(), loopLbl);
            E.Jump(endLbl);

            endGroup();
            startGroup(loop);

            //loop body
            E.SetLabel(loopLbl);
            generateSubsequence(loop);

            //repeat
            E.Jump(conditionLbl);
            E.SetLabel(endLbl);
            E.Nop();

            Context.PopBlock();
            endGroup();
        }

        /// <summary>
        /// Generate instructions for do block
        /// </summary>
        /// <param name="block">Do block which instructions will be generated</param>
        private void generateDo(INodeAST doBlock)
        {
            startGroup(doBlock);
            startInfoBlock(doBlock);

            //block labels
            var conditionLbl = E.GetTemporaryLabel(ConditionLabelCaption);
            var loopLbl = E.GetTemporaryLabel(LoopLabelCaption);
            var endLbl = E.GetTemporaryLabel(EndLabelCaption);

            Context.PushBlock(doBlock,
                    conditionLbl,
                    endLbl
                );

            //loop primitives
            var loop = doBlock.Arguments[0];
            var condition = getRValue(doBlock.Arguments[1]);

            //conditional jump table
            E.Jump(loopLbl); //for first loop we skip condition
            E.SetLabel(conditionLbl);
            E.ConditionalJump(condition.GenerateStorage(), loopLbl);
            E.Jump(endLbl);

            endGroup();
            startGroup(loop);

            //loop body
            E.SetLabel(loopLbl);
            generateSubsequence(loop);

            //repeat
            E.Jump(conditionLbl);
            E.SetLabel(endLbl);
            E.Nop();

            Context.PopBlock();
            endGroup();
        }


        /// <summary>
        /// Generate instructions for if block
        /// </summary>
        /// <param name="ifBlock">If block which instructions will be generated</param>
        private void generateIf(INodeAST ifBlock)
        {
            startGroup(ifBlock);
            startInfoBlock(ifBlock);

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
            endGroup();

            startGroup(ifBranch);
            //generate if branch
            E.SetLabel(trueLbl);
            generateSubsequence(ifBranch);

            if (elseBranch != null)
            {
                //if there is else branch generate it
                //firstly protect falling into else branch from ifbranch
                endGroup();
                startGroup(elseBranch);
                E.Jump(endLbl);
                E.SetLabel(falseLbl);
                generateSubsequence(elseBranch);
            }

            E.SetLabel(endLbl);
            //because of editing can proceed smoothly (detecting borders)
            E.Nop();
            endGroup();
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
                case NodeTypes.keyword:
                    generateKeyWord(statement);
                    break;
                case NodeTypes.declaration:
                    //force declaration
                    getLValue(statement);
                    break;
                case NodeTypes.empty:
                    //there is nothing to do
                    E.Nop();
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
        /// Generate instructions for given keyword
        /// </summary>
        /// <param name="keywordNode">Node representing keyword</param>
        private void generateKeyWord(INodeAST keywordNode)
        {
            var keyword = keywordNode.Value;

            switch (keyword)
            {
                case CSharpSyntax.BreakKeyword:
                    var breakLabel = getBreakLabel(keywordNode);
                    E.Jump(breakLabel);
                    break;

                case CSharpSyntax.ContinueKeyword:
                    var continueLabel = getContinueLabel(keywordNode);
                    E.Jump(continueLabel);
                    break;

                default:
                    throw parsingException(keywordNode, "Unsupported keyword {0}", keyword);
            }
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
        private LValueProvider getLValue(INodeAST lValue, TypeDescriptor typeHint = null)
        {
            var value = lValue.Value;
            switch (lValue.NodeType)
            {
                case NodeTypes.declaration:
                    //declaration of new variable
                    var variable = resolveDeclaration(lValue, typeHint);
                    declareVariable(variable);
                    return new VariableLValue(variable, lValue, Context);

                case NodeTypes.hierarchy:
                    return resolveLHierarchy(lValue);

                default:
                    throw parsingException(lValue, "LValue {0} is not supported", value);
            }
        }

        /// <summary>
        /// Get currently registered continue label
        /// </summary>
        /// <param name="continueNode">Node resolved as continue</param>
        /// <returns>Label that is currently registered for continue statements</returns>
        private Label getContinueLabel(INodeAST continueNode)
        {
            var block = Context.CurrentBlock;
            if (block == null)
                throw parsingException(continueNode, "Continue statement is not allowed here");

            return block.ContinueLabel;
        }

        /// <summary>
        /// Get currently registered break label
        /// </summary>
        /// <param name="breakNode">Node resolved as break</param>
        /// <returns>Label that is currently registered for break statements</returns>
        private Label getBreakLabel(INodeAST breakNode)
        {
            var block = Context.CurrentBlock;
            if (block == null)
                throw parsingException(breakNode, "Break statement is not allowed here");

            return block.BreakLabel;
        }

        /// <summary>
        /// Get provider representing rvalue
        /// </summary>
        /// <param name="rValue">Node which rvalue provider is needed</param>
        /// <returns><see cref="RValueProvider"/> represented by given node</returns>
        private RValueProvider getRValue(INodeAST rValue)
        {
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
                case NodeTypes.empty:
                    //for empty there is no rvalue
                    return null;
                default:
                    throw parsingException(rValue, "Unexpected {0} token", rValue.NodeType);
            }

            CompilationInfo.RegisterNodeType(rValue, result.Type);
            return result;
        }

        /// <summary>
        /// Resolve hierarchy node providing begining of lvalue expression
        /// </summary>
        /// <param name="hierarchy">Node where hierarchy of lvalue expression starts</param>
        /// <returns><see cref="LValueProvider"/> representing lvalue provided by hierarchy</returns>
        private LValueProvider resolveLHierarchy(INodeAST hierarchy)
        {
            //hirarchy could looks like [this.]setter or rvalue.setter
            LValueProvider result;

            var hasBaseObject = tryGetLVariable(hierarchy, out result);
            var isIndexerCall = hierarchy.Indexer != null && hierarchy.NodeType == NodeTypes.hierarchy;
            var hasCallExtending = hierarchy.Child != null || hierarchy.Indexer != null;

            if (hasBaseObject && hasCallExtending)
            {
                //setter on explicit object
                RValueProvider baseObject;
                tryGetRVariable(hierarchy, out baseObject);

                if (!tryGetSetter(hierarchy, out result, baseObject))
                {
                    throw parsingException(hierarchy, "Unknown setter on object hierarchy construction", hierarchy);
                }
            }
            else if (!hasBaseObject)
            {
                //there can only be unbased call hierarchy (note: static method with namespaces, etc. is whole call hierarchy) 
                //Note that implicit this for setters is handled within CallHierarchyProcessor
                if (!tryGetSetter(hierarchy, out result))
                {
                    throw parsingException(hierarchy, "Unknown hierarchy construction", hierarchy);
                }
            }

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

            var hasBaseObject = tryGetLiteral(hierarchy, out result) || tryGetRVariable(hierarchy, out result);
            var isIndexerCall = hierarchy.Indexer != null && hierarchy.NodeType == NodeTypes.hierarchy;
            var hasCallExtending = hierarchy.Child != null || hierarchy.Indexer != null;

            if (hasBaseObject && hasCallExtending)
            {
                //object based call
                var baseObject = result;

                if (!tryGetCall(hierarchy, out result, baseObject))
                {
                    throw parsingException(hierarchy, "Unknown object call hierarchy construction", hierarchy);
                }
            }
            else if (!hasBaseObject)
            {
                //there can only be unbased call hierarchy (note: static method with namespaces, etc. is whole call hierarchy) 
                //Note that implicit this for getter is resolved in CreateCallActivation
                if (!tryGetCall(hierarchy, out result))
                {
                    throw parsingException(hierarchy, "Unknown hierarchy construction", hierarchy);
                }
            }

            return result;
        }



        private bool tryGetSetter(INodeAST callNode, out LValueProvider result, RValueProvider baseObject = null)
        {
            var processor = new CallHierarchyProcessor(callNode, this);

            return processor.TryGetSetter(out result, baseObject);
        }

        /// <summary>
        /// Try to get call hierarchy (chained calls, properties, indexes, namespaces and statit classes)
        /// </summary>
        /// <param name="callHierarchy">Node where call hierarchy starts</param>
        /// <param name="call">Result representation of call hierarchy</param>
        /// <param name="calledObject">Object on which call hierarchy starts if any</param>
        /// <returns><c>true</c> if call hierarchy is recognized, <c>false</c> otherwise</returns>
        internal bool tryGetCall(INodeAST callNode, out RValueProvider call, RValueProvider calledObject = null)
        {
            var processor = new CallHierarchyProcessor(callNode, this);

            return processor.TryGetCall(out call, calledObject);
        }


        /// <summary>
        /// Try to get variable providing assign support for given node
        /// </summary>
        /// <param name="variableNode">Node representing needed variable</param>
        /// <param name="variable">LValue provider of variable if available, <c>null</c> otherwise</param>
        /// <returns><c>true</c> if variable is available, <c>false</c> otherwise</returns>
        private bool tryGetLVariable(INodeAST variableNode, out LValueProvider variable)
        {
            var variableName = variableNode.Value;
            var varInfo = getVariableInfo(variableName);
            if (varInfo != null)
            {
                variable = new VariableLValue(varInfo, variableNode, Context);
                return true;
            }

            variable = null;
            return false;
        }

        /// <summary>
        /// Try to get variable providing value for given node
        /// </summary>
        /// <param name="variableNode">Node representing needed variable</param>
        /// <param name="variable">Value provider of variable if available, <c>null</c> otherwise</param>
        /// <returns><c>true</c> if variable is available, <c>false</c> otherwise</returns>
        private bool tryGetRVariable(INodeAST variableNode, out RValueProvider variable)
        {
            var variableName = variableNode.Value;
            var varInfo = getVariableInfo(variableName);
            if (varInfo != null)
            {
                variable = new VariableRValue(varInfo, variableNode, Context);
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
                    var incrementBase = getRValue(operandNode);
                    return resolveLValueAdd(incrementedLValue, incrementBase, 1, true, prefixOperator);

                case CSharpSyntax.DecrementOperator:
                    var decrementedLValue = getLValue(operandNode);
                    var decrementBase = getRValue(operandNode);
                    return resolveLValueAdd(decrementedLValue, decrementBase, -1, true, prefixOperator);

                default:
                    var result = tryGetUnary(operatorNotation, operandNode);
                    if (result == null)
                        throw parsingException(prefixOperator, "Prefix operation '{0}' is not supported", operatorNotation);

                    return result;
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

            LValueProvider lValue;
            RValueProvider source;

            switch (operatorNotation)
            {
                case CSharpSyntax.IncrementOperator:
                    lValue = getLValue(operandNode);
                    source = getRValue(operandNode);
                    return resolveLValueAdd(lValue, source, 1, false, postfixOperator);

                case CSharpSyntax.DecrementOperator:
                    lValue = getLValue(operandNode);
                    source = getRValue(operandNode);
                    return resolveLValueAdd(lValue, source, -1, false, postfixOperator);

                default:
                    var result = tryGetUnary(operatorNotation, operandNode);
                    if (result == null)
                        throw parsingException(postfixOperator, "Postfix operation '{0}' is not supported", operatorNotation);

                    return result;
            }
        }

        /// <summary>
        /// Resolve unary operation on given operand
        /// </summary>
        /// <param name="operandNode">Operand of unary operation</param>
        /// <returns>Representation of negate operation result</returns>
        private RValueProvider tryGetUnary(string operatorNotation, INodeAST operandNode)
        {
            string operatorMethod;
            if (!_mathOperatorMethods.TryGetValue(operatorNotation, out operatorMethod))
                return null;
            
            var operandValue=getRValue(operandNode);
            var activation = createUnaryOperatorActivation(operandValue, operatorMethod, operandNode.Parent);
            if (activation == null)
                return null;

            return new CallValue(activation, Context);
        }

        /// <summary>
        /// Find method representation of unary operator for given nodes
        /// </summary>
        /// <param name="leftOperandType">Operand</param>
        /// <param name="operatorMethod">Method belonging to operator</param>
        /// <returns>Found operator call</returns>
        private CallActivation createUnaryOperatorActivation(RValueProvider operand, string operatorMethod, INodeAST operatorNode)
        {
            //translate method according to operators table
            var searcher = Context.CreateSearcher();
            searcher.SetCalledObject(operand.Type);
            searcher.Dispatch(operatorMethod);

            if (!searcher.HasResults)
                throw parsingException(operatorNode, "Method implementation for operator {0} cannot be found", operatorMethod);

            var hasStaticMethod = false;
            foreach (var method in searcher.FoundResult)
            {
                if (method.IsStatic)
                {
                    hasStaticMethod = true;
                    break;
                }
            }


            Argument[] arguments;
            if (hasStaticMethod)
            {
                //static methods needs to pass both operands
                arguments = new[]{
                    new Argument(operand),
                };
            }
            else
            {
                //non static methods passes second argument as this object
                arguments = new Argument[]{
                };
            }


            var selector = new MethodSelector(searcher.FoundResult, Context);
            var activation = selector.CreateCallActivation(arguments);
            activation.CallNode = operatorNode;

            if (!hasStaticMethod)
                //static methods doesnt have called object
                activation.CalledObject = operand;

            if (activation == null)
                throw parsingException(operatorNode, "Cannot select method overload for operator {0}", operatorMethod);

            return activation;
        }

        /// <summary>
        /// Resolve adding specified number to given target
        /// </summary>
        /// <param name="target">Lvalue where value will be added</param>
        /// <param name="toAdd">Value that is added to lvalue</param>
        /// <param name="prefixReturn">Determine that result of operation is value after or before adding</param>
        /// <returns>Representation fo value adding</returns>
        private RValueProvider resolveLValueAdd(LValueProvider target, RValueProvider source, int toAdd, bool prefixReturn, INodeAST operatorNode)
        {
            //determine type because of possible implicit typing
            var resultType = target.Type == null ? source.Type : target.Type;

            var addRepresentation = new ComputedValue(resultType, (e, storage) =>
            {
                var lTypeInfo = target.Type;
                var rTypeInfo = TypeDescriptor.Create<int>();

                //value for adding
                var addedValue = new TemporaryRVariableValue(Context);
                var addedValueStorage = addedValue.Storage;

                E.AssignLiteral(addedValueStorage, toAdd);

                var operatorActivation = createBinaryOperatorActivation(source, "+", addedValue, operatorNode);

                var callProvider = new CallValue(operatorActivation, Context);
                callProvider.Generate();

                //assign added value 

                var targetStorageProvider = target as IStorageReadProvider;

                var needToPreserveReturn = false;
                TemporaryRVariableValue preservedReturn = null;

                if (storage != null)
                {
                    var sourceReturnCorruptive = !(source is IStorageReadProvider);
                    var storageReturnCorruptive = !(source is IStorageReadProvider);

                    needToPreserveReturn = (!prefixReturn && sourceReturnCorruptive) || (prefixReturn && storageReturnCorruptive);
                    if (needToPreserveReturn)
                    {
                        preservedReturn = new TemporaryRVariableValue(Context);
                        E.AssignReturnValue(preservedReturn.Storage, lTypeInfo);
                    }

                    //save copy to storage
                    if (prefixReturn)
                    {
                        storage.AssignReturnValue(lTypeInfo, operatorNode);
                    }
                    else
                    {
                        source.GenerateAssignInto(storage);
                    }
                }

                if (needToPreserveReturn)
                {
                    preservedReturn.GenerateAssignInto(target);
                }
                else
                {
                    target.AssignReturnValue(lTypeInfo, operatorNode);
                }


            }, Context);

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

                    //determine type because of possible implicit typing of variables
                    var type = lValue.Type == null ? rValue.Type : lValue.Type;

                    var assignComputation = new ComputedValue(type, (e, storage) =>
                    {
                        rValue.GenerateAssignInto(lValue);

                        if (storage != null)
                        {
                            //assign chaining

                            //this is needed because of getters/setters
                            var chainedRValue = getRValue(lNode);
                            chainedRValue.GenerateAssignInto(storage);
                        }
                    }, Context);

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

            var operatorActivation = createBinaryOperatorActivation(lOperandProvider, op, rOperandProvider, lNode.Parent);

            var call = new CallValue(operatorActivation, Context);
            return call;
        }

        /// <summary>
        /// Find method representation of binary operator for given nodes
        /// </summary>
        /// <param name="leftOperandType">Left operand</param>
        /// <param name="op">Operator notation</param>
        /// <param name="rightOperand">Right operand</param>
        /// <param name="leftOperand">Node available for operator</param>
        /// <returns>Found operator</returns>
        private CallActivation createBinaryOperatorActivation(RValueProvider leftOperand, string op, RValueProvider rightOperand, INodeAST operatorNode)
        {
            //translate method according to operators table
            var searcher = findOperatorMethod(leftOperand, op);

            if (!searcher.HasResults)
                throw parsingException(operatorNode, "Method implementation for operator {0} cannot be found", op);

            var hasStaticMethod = false;
            foreach (var method in searcher.FoundResult)
            {
                if (method.IsStatic)
                {
                    hasStaticMethod = true;
                    break;
                }
            }


            Argument[] arguments;
            if (hasStaticMethod)
            {
                //static methods needs to pass both operands
                arguments = new[]{
                    new Argument(leftOperand),
                    new Argument(rightOperand)
                };
            }
            else
            {
                //non static methods passes second argument as this object
                arguments = new[]{
                    new Argument(rightOperand)
                };
            }


            var selector = new MethodSelector(searcher.FoundResult, Context);
            var activation = selector.CreateCallActivation(arguments);
            activation.CallNode = operatorNode;

            if (!hasStaticMethod)
                //static methods doesnt have called object
                activation.CalledObject = leftOperand;

            if (activation == null)
                throw parsingException(operatorNode, "Cannot select method overload for operator {0}", op);

            return activation;
        }

        /// <summary>
        /// Find method representation of given operator
        /// </summary>
        /// <param name="leftOperand">Left operand</param>
        /// <param name="op">Operator notation</param>
        /// <returns>Searcher with found operator</returns>
        private MethodSearcher findOperatorMethod(RValueProvider leftOperand, string op)
        {
            var searcher = Context.CreateSearcher();

            if (op == "+" && leftOperand.Type.Equals(StringDescriptor))
            {
                //exception for handling string concatenation
                searcher.SetCalledObject(StringDescriptor);
                searcher.Dispatch("Concat");
            }
            else
            {
                var method = _mathOperatorMethods[op];
                var leftOperandType = leftOperand.Type;

                searcher.SetCalledObject(leftOperandType);
                searcher.Dispatch(method);
            }

            return searcher;
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

            if (literalToken == "null")
            {
                literal = new LiteralValue(new Null(), literalNode, Context);
                return true;
            }

            if (literalToken.Contains('"'))
            {
                //string literal

                //TODO correct string escaping
                literalToken = literalToken.Replace("\"", "");
                literal = new LiteralValue(literalToken, literalNode, Context);
                return true;
            }

            int num;
            if (int.TryParse(literalToken, out num))
            {
                //int literal

                literal = new LiteralValue(num, literalNode, Context);
                return true;
            }

            bool bl;
            if (bool.TryParse(literalToken, out bl))
            {
                //bool literal

                literal = new LiteralValue(bl, literalNode, Context);
                return true;
            }

            if (literalNode.Value == CSharpSyntax.TypeOfOperator)
            {
                //typeof(TypeLiteral) expression
                if (literalNode.Arguments.Length == 0)
                {
                    throw parsingException(literalNode, "Operator typeof doesn't have type specified");
                }

                var typeArgumentNode = literalNode.Arguments[0];
                var type = resolveTypeofArgument(typeArgumentNode);
                if (type == null)
                {
                    throw parsingException(typeArgumentNode, "Type argument of typeof cannot be resolved");
                }

                literal = new LiteralValue(type, literalNode, Context);
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
            var info = resolveTypeDescriptor(typeofArgument);
            if (info == null)
                return null;

            return new LiteralType(info);
        }

        /// <summary>
        /// Resolve suffix name of type described by root of type hierarchy
        /// </summary>
        /// <param name="typeHierarchyRoot">Hierarchy of partly namespaced type</param>
        /// <returns>Resolved suffix of type name</returns>
        private static string resolveSuffixTypeName(INodeAST typeHierarchyRoot)
        {
            var resultType = new StringBuilder();
            var currentNode = typeHierarchyRoot;

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

            var suffixTypeName = resultType.ToString();
            return suffixTypeName;
        }

        /// <summary>
        /// Resolve typename according to available namespaces
        /// </summary>
        /// <param name="typeNameSuffix">Type name which is resolved to descriptor</param>
        /// <returns><see cref="TypeDescriptor"/> resolved from given type name and namespace if available, <c>null</c> otherwise</returns>
        private TypeDescriptor resolveTypeDescriptor(string typeNameSuffix)
        {
            var translatedTypeName = Context.MapGeneric(typeNameSuffix);

            return Context.DescriptorFromSuffix(translatedTypeName);
        }

        /// <summary>
        /// Resolve <see cref="TypeDescriptor"/> of type described by root of type hierarchy
        /// </summary>
        /// <param name="typeHierarchyRoot">Hierarchy of partly namespaced type</param>
        /// <returns>Resolved <see cref="TypeDescriptor"/> if available, <c>null</c> otherwise</returns>
        private TypeDescriptor resolveTypeDescriptor(INodeAST typeHierarchyRoot)
        {
            var suffixTypeName = resolveSuffixTypeName(typeHierarchyRoot);
            return resolveTypeDescriptor(suffixTypeName);
        }

        /// <summary>
        /// Resolve declaration of variable specified by given declaration node
        /// </summary>
        /// <param name="declarationNode">Node where is variable declared</param>
        /// <returns>Declared variable<returns>
        private VariableInfo resolveDeclaration(INodeAST declarationNode, TypeDescriptor typeHint)
        {
            Debug.Assert(declarationNode.NodeType == NodeTypes.declaration);

            var typeNode = declarationNode.Arguments[0];
            var isImplicitlyTyped = typeNode.Value == LanguageDefinitions.CSharpSyntax.ImplicitVariableType;

            //resolve type if needed
            TypeDescriptor declaredType = typeHint;
            if (!isImplicitlyTyped)
            {
                declaredType = resolveTypeDescriptor(typeNode);
                if (declaredType == null)
                    throw parsingException(declarationNode, "Cannot resolve type of variable declaration");
            }

            return new VariableInfo(declarationNode, declaredType, _source.CompilationInfo);
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

            var typeName = Context.MapGeneric(name.ToString());
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
            //resolve type
            INodeAST callNode;
            var objectType = resolveObjectType(newOperand, out callNode);

            //resolve arguments and repair type accordingly
            List<RValueProvider> initializerArguments;
            List<Argument> constructorArguments;
            resolveArguments(newOperand, callNode, ref objectType, out initializerArguments, out constructorArguments);

            //create and construct object
            var overloads = getConstructorOverloads(callNode, objectType);
            var nObject = new NewObjectValue(objectType, newOperand.Parent, Context);
            var activation = createConstructorActivation(nObject, callNode, overloads, constructorArguments);
            if (activation == null)
            {
                throw parsingException(callNode, "Constructor call doesn't match to any available definition");
            }
            var ctorCall = new CallValue(activation, Context);

            //set object initialization

            nObject.SetCtor(ctorCall);
            nObject.SetInitializerArguments(initializerArguments);
            return nObject;
        }

        private IEnumerable<TypeMethodInfo> getConstructorOverloads(INodeAST callNode, TypeDescriptor objectType)
        {
            var searcher = Context.CreateSearcher();
            searcher.SetCalledObject(objectType);
            searcher.Dispatch(Naming.CtorName);

            if (!searcher.FoundResult.Any())
            {
                throw parsingException(callNode, "Constructor wasn't found");
            }

            return searcher.FoundResult;
        }


        private CallActivation createConstructorActivation(NewObjectValue nObject, INodeAST callNode, IEnumerable<TypeMethodInfo> overloads, List<Argument> constructorArguments)
        {
            var selector = new MethodSelector(overloads, Context);

            return createCallActivation(selector, nObject, callNode, constructorArguments.ToArray());
        }

        private void resolveArguments(INodeAST newOperand, INodeAST callNode, ref TypeDescriptor objectType, out List<RValueProvider> initializerArguments, out List<Argument> constructorArguments)
        {
            var isArray = objectType.TypeName.StartsWith("Array<");
            var isImplicitlyTypedArray = newOperand.Value == "[";
            var hasInitializer = callNode.Subsequence != null;

            initializerArguments = new List<RValueProvider>();
            constructorArguments = new List<Argument>();
            if (hasInitializer)
            {
                foreach (var arg in callNode.Subsequence.Lines)
                {
                    var initializerValue = getRValue(arg);
                    initializerArguments.Add(initializerValue);
                }

                if (isImplicitlyTypedArray && initializerArguments.Count > 0)
                    //we can do implicit type specification
                    objectType = TypeDescriptor.Create("Array<" + initializerArguments[0].Type.TypeName + ",1>");

                if (isArray)
                {
                    //array has to have implicit constructor parameter
                    var lengthArgument = new Argument(new LiteralValue(initializerArguments.Count, callNode, Context));
                    constructorArguments.Add(lengthArgument);
                }
            }
            else
            {
                var arguments = GetArguments(callNode, isArray);
                constructorArguments.AddRange(arguments);
            }
        }


        private TypeDescriptor resolveObjectType(INodeAST newOperand, out INodeAST callNode)
        {
            var isImplicitTypedArray = newOperand.Value == "[";

            var typeSuffix = resolveCtorSuffix(newOperand, out callNode);
            var objectType = isImplicitTypedArray ? TypeDescriptor.Create("Array<System.Object,1>") : resolveTypeDescriptor(typeSuffix);

            if (objectType == null)
                throw parsingException(callNode, "Cannot find type");

            return objectType;
        }

        /// <summary>
        /// Select <see cref="TypeMethodInfo"/> according to callNode arguments and creates
        /// <see cref="CallActivation"/>.
        /// </summary>
        /// <param name="calledObject">Object which method is called. Is passed only if it is available</param>
        /// <param name="callNode">Node determining call</param>
        /// <param name="methods">Methods used for right overloading selection</param>
        /// <returns>Created call activation</returns>
        internal CallActivation CreateCallActivation(RValueProvider calledObject, INodeAST callNode, IEnumerable<TypeMethodInfo> methods, bool forceIndexer = false)
        {
            //prepare arguments
            var selector = new MethodSelector(methods, Context);
            var arguments = GetArguments(callNode, selector.IsIndexer || forceIndexer);

            //create activation
            return createCallActivation(selector, calledObject, callNode, arguments);
        }

        private CallActivation createCallActivation(MethodSelector selector, RValueProvider calledObject, INodeAST callNode, Argument[] arguments)
        {
            var callActivation = selector.CreateCallActivation(arguments);

            if (callActivation != null)
            {
                callActivation.CallNode = callNode;

                if (calledObject == null && !callActivation.MethodInfo.IsStatic)
                {
                    //if there is no explicit calledObject, and method call is not static
                    //implicit this object has to be used
                    calledObject = CreateImplicitThis(callNode);
                }

                callActivation.CalledObject = calledObject;
            }

            return callActivation;
        }


        /// <summary>
        /// Create value provider with implicit this variable
        /// </summary>
        /// <param name="contextNode">Context node that enforces implicit this creation</param>
        /// <returns>Created provider</returns>
        internal RValueProvider CreateImplicitThis(INodeAST contextNode)
        {
            if (!MethodInfo.HasThis)
                throw parsingException(contextNode, "Implicit this is not available in static method");

            return new TemporaryRVariableValue(Context, CSharpSyntax.ThisVariable);
        }

        /// <summary>
        /// Get arguments representation that is available on given node
        /// </summary>
        /// <param name="node">Node which arguments are needed</param>
        /// <returns>Arguments available for given node</returns>
        internal Argument[] GetArguments(INodeAST node, bool isIndexer)
        {
            var argNodes = node.Arguments;
            if (isIndexer && node.Indexer != null)
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
        /// Start new block of isntructions that can be attached by additional
        /// information
        /// </summary>
        /// <param name="blockDescription">Textual description of started block</param>
        /// <returns>Info object created for the block</returns>
        private InstructionInfo startInfoBlock(INodeAST node, string blockDescription = null)
        {
            if (blockDescription == null)
                blockDescription = getConditionalBlockText(node);

            var info = E.StartNewInfoBlock();
            info.Comment = InstructionCommentStart + blockDescription + InstructionCommentEnd;
            info.BlockTransformProvider = new Transformations.BlockProvider(node, _method.Source);
            return info;
        }


        private void startGroup(INodeAST node)
        {
            _blocks.Push(node);
            E.SetCurrentGroup(node);
        }

        private void endGroup()
        {
            var popped = _blocks.Pop();

            var current = _blocks.Peek();
            E.SetCurrentGroup(current);
        }

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
        private string getConditionalBlockText(INodeAST block)
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

                Context.RegisterGenericArgument(genericParam, genericArg);
            }
        }

        #endregion

    }
}
