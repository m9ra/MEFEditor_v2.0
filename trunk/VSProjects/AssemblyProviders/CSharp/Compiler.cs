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
    public class Compiler
    {
        private readonly CodeNode _method;
        private readonly IEmitter<MethodID, InstanceInfo> _emitter;
        private readonly Context _context;

        public static void GenerateInstructions(CodeNode method, IEmitter<MethodID, InstanceInfo> emitter)
        {
            var compiler = new Compiler(method, emitter);

            compiler.generateInstructions();
        }

        private Compiler(CodeNode method, IEmitter<MethodID, InstanceInfo> emitter)
        {
            _method = method;
            _emitter = emitter;
            _context = new Context(emitter);
        }


        private void generateInstructions()
        {
            foreach (var line in _method.Subsequence.Lines)
            {
                generateLine(line);
            }
        }

        private void generateLine(INodeAST line)
        {
            //TODO add line info

            generateStatement(line);
        }

        private void generateStatement(INodeAST statement)
        {
            switch (statement.NodeType)
            {
                case NodeTypes.binaryOperator:
                    generateBinary(statement);
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
        
        private LValueProvider getLValue(INodeAST lValue)
        {
            switch (lValue.NodeType)
            {
                case NodeTypes.declaration:
                    return new VariableValue(lValue.Arguments[1].Value, _context);

                case NodeTypes.hierarchy:
                    //TODO resolve hierarchy
                    return new VariableValue(lValue.Value, _context);

                default:
                    throw new NotImplementedException();
            }
        }


        private RValueProvider getRValue(INodeAST valueNode)
        {
            switch (valueNode.NodeType)
            {
                case NodeTypes.hierarchy:
                    //TODO resolve hierarchy
                    var value = valueNode.Value;
                    if (value.Contains('"'))
                    {
                        value=value.Replace("\"","");
                        return new LiteralValue(value, _context);
                    }

                    return new VariableRValue(value, _context);

                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }
}
