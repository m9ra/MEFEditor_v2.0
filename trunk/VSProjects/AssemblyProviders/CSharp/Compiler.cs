﻿using System;
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
    public class Compiler
    {
        private readonly CodeNode _method;
        private readonly EmitterBase<MethodID, InstanceInfo> _emitter;
        private readonly Context _context;
        private readonly ParameterInfo[] _arguments;

        private readonly Dictionary<string, string> _declaredVariables = new Dictionary<string, string>();

        public static void GenerateInstructions(CodeNode method, ParameterInfo[] arguments, EmitterBase<MethodID, InstanceInfo> emitter, TypeServices services)
        {
            var compiler = new Compiler(method, arguments, emitter, services);

            compiler.generateInstructions();
        }

        private Compiler(CodeNode method, ParameterInfo[] arguments, EmitterBase<MethodID, InstanceInfo> emitter, TypeServices services)
        {            
            _method = method;
            _arguments = arguments; 
            _emitter = emitter;
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
            _emitter.StartNewInfoBlock().Comment = "===Compiler initialization===";
            //TODO Debug only
            _emitter.AssignLiteral("this", "Fake Value of this object");

            //generate argument assigns
            for (uint i = 0; i < _arguments.Length; ++i)
            {
                var arg = _arguments[i];
                _emitter.AssignArgument(arg.Name, i+1); //argument 0 is always this object
                _declaredVariables.Add(arg.Name, arg.StaticInfo.TypeName);
            }

            //generate method body
            foreach (var line in _method.Subsequence.Lines)
            {
                generateLine(line);
            }
        }

        private void generateLine(INodeAST line)
        {

            var info = _emitter.StartNewInfoBlock();
            info.Comment = "\n---" + statementText(line) + "---";
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
                default:
                    throw new NotImplementedException();
            }
        }

        private void generateBinary(INodeAST statement)
        {
            switch (statement.Value)
            {
                case "=":
                    var lValue = getLValue(statement.Arguments[0]);
                    var rValue = getRValue(statement.Arguments[1]);

                    rValue.AssignInto(lValue);

                    break;
            }
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
        #endregion

        #region Value providing
        private LValueProvider getLValue(INodeAST lValue)
        {
            switch (lValue.NodeType)
            {
                case NodeTypes.declaration:
                    var varType = lValue.Arguments[0].Value;
                    var varName = lValue.Arguments[1].Value;
                    _declaredVariables.Add(varName, varType);
                    return new VariableValue(varName, _context);

                case NodeTypes.hierarchy:
                    //TODO resolve hierarchy
                    return new VariableValue(lValue.Value, _context);

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

                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
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
            var value = node.Value;

            //first token can be literal/variable/call
            //other hierarchy tokens can only be calls (fields are resolved as calls to property getters)

            RValueProvider result;


            var hasBaseObject = tryGetLiteral(value, out result) || tryGetVariable(value, out result);
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


        private bool tryGetLiteral(string literalToken, out RValueProvider literal)
        {
            if (literalToken.Contains('"'))
            {
                literalToken = literalToken.Replace("\"", "");
                literal = new LiteralValue(literalToken, _context);
                return true;
            }

            int num;
            if (int.TryParse(literalToken, out num))
            {
                literal = new LiteralValue(num, _context);
                return true;
            }

            literal = null;
            return false;
        }

        private bool tryGetVariable(string variableName, out RValueProvider variable)
        {

            if (_declaredVariables.ContainsKey(variableName))
            {
                variable = new VariableRValue(variableName, _context);
                return true;
            }

            variable = null;
            return false;
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
                    call = new CallRValue(methodInfo, arguments, _context);
                    return true;
                }

                //shift to next node
                searcher.ExtendName(currNode.Value);
                currNode = nextNode;
            }

            call = null;
            return false;
        }
        #endregion
    }
}
