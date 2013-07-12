using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp.Interfaces
{
    /// <summary>
    /// Describes semantic node type.
    /// </summary>
    enum NodeKind
    {
        /// <summary>
        /// Variable declaration.
        /// </summary>
        declaration,
        /// <summary>
        /// Variable assign.
        /// </summary>
        assign,
        /// <summary>
        /// Function return statement.
        /// </summary>
        fReturn,
        /// <summary>
        /// Prefix operator.
        /// </summary>
        prefixOp,
        /// <summary>
        /// Binary operator.
        /// </summary>
        binOp,
        /// <summary>
        /// Post operator.
        /// </summary>
        postOp,
        /// <summary>
        /// Function call.
        /// </summary>
        fCall,
        /// <summary>
        /// Variable occurence.
        /// </summary>
        variable,
        /// <summary>
        /// Argument occurence.
        /// </summary>
        argument,
        /// <summary>
        /// Static class instance.
        /// </summary>
        staticClass,
        /// <summary>
        /// Primitive value type.
        /// </summary>
        value,
        /// <summary>
        /// This object occurence.
        /// </summary>
        thisObj,
        /// <summary>
        /// Constructor call.
        /// </summary>
        cCall,
        /// <summary>
        /// Conditional block. (if,while,...)
        /// </summary>
        condBlock,
        /// <summary>
        /// For block.
        /// </summary>
        forBlock,
        /// <summary>
        /// Switch block.
        /// </summary>
        switchBlock,
        /// <summary>
        /// Keyword occurence.
        /// </summary>
        keyword, 
        /// <summary>
        /// Explicit conversion.
        /// </summary>
        conversion
    }

    /// <summary>
    /// Method node representation.
    /// </summary>
    interface ICodeMethod
    {
        /// <summary>
        /// All variables declared in method.
        /// </summary>
        IVariableInfo[] Locals { get; }
        /// <summary>
        /// First instruction of method.
        /// </summary>
        ICodeInstruction FirstInstruction { get; }
    }

    /// <summary>
    /// Representation of single instruction.
    /// </summary>
    interface ICodeInstruction
    {
        /// <summary>
        /// Next instruction which should be executed, if no jumps occur.
        /// </summary>
        ICodeInstruction Next { get; }
        /// <summary>
        /// Statement semantic node represented by this instruction.
        /// </summary>
        ICodeStatement Statement { get; }
    }

    /// <summary>
    /// Semantic node prototype.
    /// </summary>
    interface ICodeNode
    {
        /// <summary>
        /// Kind of node.
        /// </summary>
        NodeKind Kind { get; }
        /// <summary>
        /// End position of node.
        /// </summary>
        IPosition End { get; }
        /// <summary>
        /// Start position of node.
        /// </summary>
        IPosition Start { get; }
    }

    /// <summary>
    /// Semantic node representation of statement.
    /// </summary>
    interface ICodeStatement : ICodeNode { }

    /// <summary>
    /// Semantic node representation of value providing expression.
    /// </summary>
    interface ICodeValueProvider : ICodeNode
    {
        /// <summary>
        /// Determines if object is type (can provide static class)
        /// </summary>
        bool IsStatic { get; }        
    }

    /// <summary>
    /// Semantic node representation of keyword. (break, continue,..)
    /// </summary>
    interface ICodeKeyword : ICodeStatement
    {
        /// <summary>
        /// Represented keyword.
        /// </summary>
        string Keyword { get; }
    }

    /// <summary>
    /// Semantic node representation of condition block. (if, while,...)
    /// </summary>
    interface ICodeConditionBlock : ICodeStatement
    {
        /// <summary>
        /// Determine if/while/do/... command
        /// </summary>
        string Command { get; }
        /// <summary>
        /// Condition, determine if MainBranch should be executed
        /// </summary>
        ICodeValueProvider Condition { get; }
        /// <summary>
        /// Branch which should be executed if condition is true
        /// </summary>
        ICodeInstruction MainBranch { get; }
        /// <summary>
        /// branch which should be executed if condition is false
        /// </summary>
        /// <remarks>Else branch may not be available, because of type of command. (e.g. While commands doesnt have else branch.)
        /// On if command also may not be available, because is not defined.
        /// </remarks>
        ICodeInstruction ElseBranch { get; }
        /// <summary>
        /// All variables declared inside this block
        /// </summary>
        IVariableInfo[] DeclaredVariables { get; }
    }

    /// <summary>
    /// Semantic node representation of for block.
    /// </summary>
    interface IForBlock : ICodeStatement
    {
        /// <summary>
        ///For command initialization
        /// </summary>
        ICodeStatement Init { get; }
        /// <summary>
        /// Condition in for command
        /// </summary>
        ICodeValueProvider Condition { get; }
        /// <summary>
        /// Increment statement in for command
        /// </summary>
        ICodeStatement Increment { get; }
        /// <summary>
        /// First instruction in for command
        /// </summary>
        ICodeInstruction FirstInstruction { get; }
    }

    /// <summary>
    /// Semantic node representation of swithc block.
    /// </summary>
    interface ISwitchBlock : ICodeStatement
    {
        /// <summary>
        /// Value which is used for selecting appropriate CaseBlock
        /// </summary>
        ICodeValueProvider SwitchedValue { get; }
        /// <summary>
        /// Case blocks of switch statement
        /// </summary>
        CaseBlock[] CaseBlocks { get; }
        /// <summary>
        /// Default block of switch statement. If no default is available, is null.
        /// </summary>
        CaseBlock DefaultBlock { get; }
    }

    /// <summary>
    /// Semantic node representation of LValue expression.
    /// </summary>
    interface ICodeLValue : ICodeNode
    {
    }

    /// <summary>
    /// Semantic node representation of function call.
    /// </summary>
    interface ICodeCall : ICodeValueProvider, ICodeStatement, ICodeArgumented
    {
        /// <summary>
        /// Called object.
        /// </summary>
        ICodeValueProvider CalledObject { get; }
    }

    /// <summary>
    /// Semantic node representation of constructor call.
    /// </summary>
    interface ICodeConstructorCall : ICodeValueProvider, ICodeStatement, ICodeArgumented
    {
    }

    /// <summary>
    /// Semantic node representation of argumented call.
    /// </summary>
    interface ICodeArgumented
    {
        /// <summary>
        /// Arguments of call.
        /// </summary>
        ICodeValueProvider[] Arguments { get; }
        /// <summary>
        /// Position where place next argument.
        /// </summary>
        IPosition NextArgument { get; }
    }

    /// <summary>
    /// Semantic node representation of this occurence.
    /// </summary>
    interface ICodeThis : ICodeValueProvider { }

    /// <summary>
    /// Information stored about variable.
    /// </summary>
    interface IVariableInfo
    {
        /// <summary>
        /// Variable name.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Determine if variable is constant.
        /// </summary>
        bool IsConstant { get; }
        /// <summary>
        /// All occurences of this VariableInfo
        /// </summary>
        ICodeVariable[] Variables { get; }
    }

    /// <summary>
    /// Semantic node representation of variable occurence.
    /// </summary>
    interface ICodeVariable : ICodeValueProvider, ICodeLValue
    {
        /// <summary>
        /// Variable info of occured variable.
        /// </summary>
        IVariableInfo VariableInfo { get; }
    }

    /// <summary>
    /// Semantic node representation of LValue assign.
    /// </summary>
    interface ICodeAssign : ICodeValueProvider, ICodeStatement
    {
        /// <summary>
        /// Assigned LValue.
        /// </summary>
        ICodeLValue LValue { get; }
        /// <summary>
        /// Assigned value.
        /// </summary>
        ICodeValueProvider ValueProvider { get; }
    }

    /// <summary>
    /// Semantic node representation of argument occurence.
    /// </summary>
    interface ICodeArgument : ICodeValueProvider, ICodeLValue
    {
        /// <summary>
        /// Name of argument.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Position of argument in function call.
        /// </summary>
        int ArgPos { get; }
    }

    /// <summary>
    /// Semantic node representation of primitive value.
    /// </summary>
    interface ICodeValue : ICodeValueProvider
    {
    }

    /// <summary>
    /// Semantic node representation of explicit conversion.
    /// </summary>
    interface ICodeConversion : ICodeValueProvider
    {
        /// <summary>
        /// Value provider which has to be converted
        /// </summary>
        ICodeValueProvider ToConvert { get; }
    }

    /// <summary>
    /// Semantic node representation of variable declaration.
    /// </summary>
    interface ICodeDeclaration : ICodeStatement, ICodeValueProvider, ICodeLValue
    {
        /// <summary>
        /// Declared variable.
        /// </summary>
        IVariableInfo VariableInfo { get; }
        /// <summary>
        /// Occurence of declared variable.
        /// </summary>
        ICodeVariable Variable { get; }
    }

    /// <summary>
    /// Semantic node representation of return statement.
    /// </summary>
    interface ICodeReturn : ICodeStatement, ICodeValueProvider
    {
        /// <summary>
        /// Return value of statement.
        /// </summary>
        ICodeValueProvider ReturnValue { get; }
    }
}
