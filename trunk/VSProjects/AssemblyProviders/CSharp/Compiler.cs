using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;
using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

using AssemblyProviders.CSharp.Compiling;

namespace AssemblyProviders.CSharp
{

    /// <summary>
    /// Resovolve methods are for getting result of operation
    /// Get methods are for getting value representation of operation
    /// </summary>
    public class Compiler
    {
        private readonly CodeNode _method;
        private readonly EmitterBase<MethodID, InstanceInfo> E;
        private readonly Context _context;
        private readonly TypeMethodInfo _methodInfo;
        private readonly CompilationInfo _info;

        private readonly Dictionary<string, VariableInfo> _declaredVariables = new Dictionary<string, VariableInfo>();

        private static Dictionary<string, string> _mathOperatorMethods = new Dictionary<string, string>(){
              {"+","add_operator"},
              {"-","sub_operator"},
              {"*","mul_operator"},
              {"/","div_operator"},
              {"<","lesser_operator"},
              {">","greater_operator"},
              
              {"==","Equals"}
        };

        public static void GenerateInstructions(CodeNode method, TypeMethodInfo info, EmitterBase<MethodID, InstanceInfo> emitter, TypeServices services)
        {
            var compiler = new Compiler(method, info, emitter, services);

            compiler.generateInstructions();
        }

        private Compiler(CodeNode method, TypeMethodInfo methodInfo, EmitterBase<MethodID, InstanceInfo> emitter, TypeServices services)
        {
            _method = method;
            _info=_method.SourceToken.Position.Source.CompilationInfo;
            _methodInfo = methodInfo;
            E = emitter;
            _context = new Context(emitter, services);
        }

        #region Info utilities
        private string statementText(INodeAST node)
        {
            return node.StartingToken.Position.GetStrip(node.EndingToken.Position);
        }
        #endregion

        #region Instruction generating
        private void generateInstructions()
        {
            E.StartNewInfoBlock().Comment = "===Compiler initialization===";
            
            if (_methodInfo.HasThis)
            {
                E.AssignArgument("this", _methodInfo.ThisType, 0);
            }

            //generate argument assigns
            for (uint i = 0; i < _methodInfo.Arguments.Length; ++i)
            {
                var arg = _methodInfo.Arguments[i];
                E.AssignArgument(arg.Name,arg.StaticInfo, i + 1); //argument 0 is always this object

                var variable = new VariableInfo(arg.Name, arg.StaticInfo);
                declareVariable(variable);                
            }

            //generate method body
            generateSubsequence(_method);
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
            var info = E.StartNewInfoBlock();
            info.Comment = "\n---" + block.Value + " block---";
            var condition = getRValue(block.Arguments[0]);
     
            var trueLbl = E.GetTemporaryLabel("_true");
            var falseLbl = E.GetTemporaryLabel("_false");
            var endLbl = E.GetTemporaryLabel("_end");

            E.ConditionalJump(condition.GetStorage(), trueLbl);
            E.Jump(falseLbl);

            E.SetLabel(trueLbl);
            generateSubsequence(block.Arguments[1]);

            E.Jump(endLbl);
            E.SetLabel(falseLbl);

            generateSubsequence(block.Arguments[2]);
            E.SetLabel(endLbl);
            //because of editation can proceed smoothly
            E.Nop();
        }

        private void generateLine(INodeAST line)
        {
            var info = E.StartNewInfoBlock();
            info.Comment = "\n---" + statementText(line) + "---";
            info.ShiftingProvider = new Transformations.ShiftingProvider(line);
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
            var binary= resolveBinary(statement);            
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
                    var variable = new VariableInfo(lValue, new InstanceInfo(typeNode.Value));
                    declareVariable(variable);
                    return new VariableValue(variable,lValue, _context);

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

            switch (valueNode.NodeType)
            {
                case NodeTypes.call:
                case NodeTypes.hierarchy:
                    return resolveRHierarchy(valueNode);
                case NodeTypes.binaryOperator:
                    return resolveBinary(valueNode);
                case  NodeTypes.prefixOperator:
                    return resolveUnary(valueNode);
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }


        private RValueProvider resolveUnary(INodeAST unary)
        {
            var operand=unary.Arguments[0];
            switch (unary.Value)
            {
                case "new":                    
                    var objectType=resolveCtorType(operand);
                    var nObject=new NewObjectValue(objectType,_context);

                    RValueProvider ctorCall;
                    if (!tryGetCall(operand, out ctorCall, nObject))
                    {
                        throw new NotSupportedException("Cannot construct object");
                    }

                    nObject.SetCtor(ctorCall);
                    return nObject;
                default:
                    throw new NotImplementedException();
            }
        }

        private RValueProvider resolveBinary(INodeAST binary)
        {
            var lNode = binary.Arguments[0];
            var rNode = binary.Arguments[1];
            
            if(_mathOperatorMethods.ContainsKey(binary.Value)){
                return resolveMathOperator(lNode,binary.Value,rNode);
            }else{
                return resolveAssignOperator(lNode,binary.Value,rNode);
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
                    return new VariableRValue(lVar,lNode,_context);
                default:
                    throw new NotImplementedException();
            }
        }

        private RValueProvider resolveMathOperator(INodeAST lNode, string op, INodeAST rNode)
        {
            //translate method according to operators table
            var method = _mathOperatorMethods[op];
            
            var lOperandProvider = getRValue(lNode);
            var rOperandProvider = getRValue(rNode);
                        
            var lTypeInfo= lOperandProvider.GetResultInfo();
            method = lTypeInfo.TypeName + "." + method;

            var lOperand = lOperandProvider.GetStorage();
            var rOperand = rOperandProvider.GetStorage();

            //TODO properly determine which call is needed (MethodSearcher)
            E.Call(new MethodID(method), lOperand, rOperand);
            var result = E.GetTemporaryVariable();

            E.AssignReturnValue(result, lTypeInfo);

            return new TemporaryRVariableValue(_context, result);
        }

        private RValueProvider[] getArguments(INodeAST node)
        {
            var args = new List<RValueProvider>();
            foreach (var arg in node.Arguments)
            {
                args.Add(getRValue(arg));
            }

            return args.ToArray();
        }
      
        private RValueProvider resolveRHierarchy(INodeAST node)
        {
            //first token can be literal/variable/call
            //other hierarchy tokens can only be calls (fields are resolved as calls to property getters)

            RValueProvider result;


            var hasBaseObject = tryGetLiteral(node, out result) || tryGetVariable(node, out result);
            var hasCallExtending = node.Child != null;

            if (hasBaseObject && hasCallExtending)
            {
                //object based call
                var baseObject = result;
                if (!tryGetCall(node.Child, out result, baseObject))
                {
                    throw new NotSupportedException("Unknown object call hierarchy construction on " + node.Child);
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
            var literalToken=literalNode.Value;
            if (literalToken.Contains('"'))
            {
                literalToken = literalToken.Replace("\"", "");
                literal = new LiteralValue(literalToken,literalNode, _context);
                return true;
            }

            int num;
            if (int.TryParse(literalToken, out num))
            {
                literal = new LiteralValue(num,literalNode, _context);
                return true;
            }

            bool bl;
            if (bool.TryParse(literalToken, out bl))
            {
                literal = new LiteralValue(bl,literalNode, _context);
                return true;
            }


            literal = null;
            return false;
        }

        private bool tryGetVariable(INodeAST variableNode, out RValueProvider variable)
        {
            var variableName = variableNode.Value;
            VariableInfo varInfo;
            if (_declaredVariables.TryGetValue(variableName,out varInfo))
            {
                variable = new VariableRValue(varInfo,variableNode, _context);
                return true;
            }

            variable = null;
            return false;
        }

        private InstanceInfo resolveCtorType(INodeAST ctorCall)
        {
            var name = new StringBuilder();

            while (ctorCall != null)
            {
                if (name.Length > 0)
                {
                    name.Append('.');
                }
                name.Append(ctorCall.Value);
                ctorCall = ctorCall.Child;
            }

            return new InstanceInfo(name.ToString());
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

            if (calledObject != null)
            {
                searcher.SetCalledObject(calledObject.GetResultInfo());
            }

            while (currNode != null)
            {
                var nextNode = currNode.Child;
                //TODO add namespaces

                switch (currNode.NodeType)
                {
                    case NodeTypes.hierarchy:
                        searcher.Dispatch("get_" + currNode.Value);
                        break;
                    case NodeTypes.call:
                        searcher.Dispatch(currNode.Value);
                        break;
                    default:
                        throw new NotSupportedException("Cannot resolve given node type inside hierarchy");
                }

                if (searcher.HasResults)
                {
                    //TODO method chaining
                    //TODO overloading
                    var methodInfo = searcher.FoundResult.First();
                    var arguments = getArguments(currNode);

                    //TODO this object resolution
                    if (calledObject == null && !methodInfo.IsStatic)
                    {
                        calledObject = new TemporaryRVariableValue(_context,"this");
                    }

                    call = new CallRValue(currNode,methodInfo,calledObject, arguments, _context);
                    return true;
                }

                if (currNode.NodeType == NodeTypes.hierarchy)
                {
                    //only hierarchy hasn't been resolved immediately(namespaces) -> shift to next node
                    searcher.ExtendName(currNode.Value);
                    currNode = nextNode;
                }
                else {
                    //call has to be found
                    break;   
                }
            }

            call = null;
            return false;
        }

        private void declareVariable(VariableInfo variable)
        {
            _info.AddVariable(variable);
            _declaredVariables.Add(variable.Name, variable);
        }

        private VariableInfo getVariableInfo(string variableName)
        {
            return _declaredVariables[variableName];
        }

        #endregion
    }
}
