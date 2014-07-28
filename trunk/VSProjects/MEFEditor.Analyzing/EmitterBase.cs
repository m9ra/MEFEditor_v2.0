using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Execution;

namespace MEFEditor.Analyzing
{
    /// <summary>
    /// Base class of emitter implementation. It provides ability to emit
    /// Instruction Analyzing Language (IAL) instructions.
    /// </summary>
    public abstract class EmitterBase
    {
        /// <summary>
        /// The current analyzing context.
        /// </summary>
        internal readonly AnalyzingContext Context;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmitterBase"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        internal EmitterBase(AnalyzingContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Create new instruction info for block starting with next emitted instruction.
        /// </summary>
        /// <returns>Created instruction info.</returns>
        public abstract InstructionInfo StartNewInfoBlock();

        /// <summary>
        /// Sets the current group.
        /// </summary>
        /// <param name="groupID">The group identifier.</param>
        public abstract void SetCurrentGroup(object groupID);

        /// <summary>
        /// Get variable, which is not used yet in emitted code.
        /// </summary>
        /// <param name="description">Description of variable, used in name.</param>
        /// <returns>Name of variable.</returns>
        public abstract string GetTemporaryVariable(string description = "");

        /// <summary>
        /// Get label, which is not used yet in emitted code.
        /// </summary>
        /// <param name="description">Description of label, used in name.</param>
        /// <returns>Created label.</returns>
        public abstract Label GetTemporaryLabel(string description = "");

        /// <summary>
        /// Get batch with instructions that has been emitted. Can be used for caching emitted instructions.
        /// </summary>
        /// <returns>Emitted instructions.</returns>
        public abstract InstructionBatch GetEmittedInstructions();

        /// <summary>
        /// Insert given batch of instructions (as they were emitted).
        /// </summary>
        /// <param name="instructions">Inserted instructions.</param>
        public abstract void InsertInstructions(InstructionBatch instructions);

        /// <summary>
        /// Assigns the literal to target variable.
        /// </summary>
        /// <param name="targetVar">The target variable.</param>
        /// <param name="literal">The literal.</param>
        /// <param name="literalInfo">The literal information.</param>
        /// <returns>AssignBuilder.</returns>
        public abstract AssignBuilder AssignLiteral(string targetVar, object literal, InstanceInfo literalInfo = null);

        /// <summary>
        /// Assigns the instance to target variable.
        /// </summary>
        /// <param name="targetVar">The target variable.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="instanceInfo">The instance information.</param>
        /// <returns>AssignBuilder.</returns>
        public abstract AssignBuilder AssignInstance(string targetVar, Instance instance, InstanceInfo instanceInfo = null);

        /// <summary>
        /// Assigns the new object to target variable.
        /// </summary>
        /// <param name="targetVar">The target variable.</param>
        /// <param name="objectInfo">The object information.</param>
        /// <returns>AssignBuilder.</returns>
        public abstract AssignBuilder AssignNewObject(string targetVar, InstanceInfo objectInfo);

        /// <summary>
        /// Assigns the specified target variable by value from source variable.
        /// </summary>
        /// <param name="targetVar">The target variable.</param>
        /// <param name="sourceVar">The source variable.</param>
        /// <returns>AssignBuilder.</returns>
        public abstract AssignBuilder Assign(string targetVar, string sourceVar);

        /// <summary>
        /// Assigns the argument at specified position to target variable.
        /// </summary>
        /// <param name="targetVar">The target variable.</param>
        /// <param name="staticInfo">The static information.</param>
        /// <param name="argumentPosition">The argument position.</param>
        /// <returns>AssignBuilder.</returns>
        public abstract AssignBuilder AssignArgument(string targetVar, InstanceInfo staticInfo, uint argumentPosition);

        /// <summary>
        /// Assigning last call return value into specified target variable.
        /// </summary>
        /// <param name="targetVar">Variable where returned value will be assigned.</param>
        /// <param name="staticInfo">The static information.</param>
        /// <returns>AssignBuilder.</returns>
        public abstract AssignBuilder AssignReturnValue(string targetVar, InstanceInfo staticInfo);

        /// <summary>
        /// Emit static call on shared instance with given information.
        /// </summary>
        /// <param name="sharedInstanceInfo">The shared instance information.</param>
        /// <param name="method">The method.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>CallBuilder.</returns>
        public abstract CallBuilder StaticCall(InstanceInfo sharedInstanceInfo, MethodID method, Arguments arguments);

        /// <summary>
        /// Emit call on given object with given arguments.
        /// <remarks>Notice that thisObject is only syntax sugar. It is passed as method argument 0.</remarks>
        /// </summary>
        /// <param name="method">The called method.</param>
        /// <param name="thisObjVariable">Variable with called object.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>CallBuilder.</returns>
        public abstract CallBuilder Call(MethodID method, string thisObjVariable, Arguments arguments);

        /// <summary>
        /// Emit return which finishes call and set return value stored in specified variable.
        /// </summary>
        /// <param name="sourceVar">The variable with return value.</param>
        public abstract void Return(string sourceVar);

        /// <summary>
        /// Emit direct invokation of given native method.
        /// </summary>
        /// <param name="method">The method.</param>
        public abstract void DirectInvoke(DirectMethod method);

        /// <summary>
        /// Creates label.
        /// NOTE:
        /// Every label has to be initialized by SetLabel.
        /// </summary>
        /// <param name="identifier">Label identifier.</param>
        /// <returns>Created label.</returns>
        public abstract Label CreateLabel(string identifier);

        /// <summary>
        /// Jumps at given target if instance under conditionVariable is resolved as true.
        /// </summary>
        /// <param name="conditionVariable">Variable where condition is stored.</param>
        /// <param name="target">Target label.</param>
        public abstract void ConditionalJump(string conditionVariable, Label target);

        /// <summary>
        /// Jumps at given target.
        /// </summary>
        /// <param name="target">Target label.</param>
        public abstract void Jump(Label target);

        /// <summary>
        /// Set label pointing to next instruction that will be generated.
        /// </summary>
        /// <param name="label">Label that will be set.</param>
        public abstract void SetLabel(Label label);

        /// <summary>
        /// Emit no-operation instruction (nop).
        /// </summary>
        public abstract void Nop();

        /// <summary>
        /// Returns instance info stored for given variable.
        /// </summary>
        /// <param name="variable">Variable which info is resolved.</param>
        /// <returns>Stored info.</returns>
        public abstract InstanceInfo VariableInfo(string variable);
    }
}
