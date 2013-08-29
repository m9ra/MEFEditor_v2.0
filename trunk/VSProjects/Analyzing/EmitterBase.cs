﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing
{
    public abstract class EmitterBase<MethodID,InstanceInfo>
    {
        /// <summary>
        /// Create new instruction info for block starting with next emitted instruction
        /// </summary>
        /// <returns>Created instruction info</returns>
        public abstract InstructionInfo StartNewInfoBlock();

        /// <summary>
        /// Get variable, which is not used yet in emitted code
        /// </summary>        
        /// <param name="description">Description of varible, used in name</param>
        /// <returns>Name of variable</returns>
        public abstract string GetTemporaryVariable(string description = "");

        /// <summary>
        /// Get label, which is not used yet in emitted code
        /// </summary>
        /// <param name="description">Description of label, used in name</param>
        /// <returns>Created label</returns>
        public abstract Label GetTemporaryLabel(string description = "");

        /// <summary>
        /// Get batch with instructions that has been emitted. Can be used for caching emitted instructions.
        /// </summary>
        /// <returns>Emitted instructions</returns>
        public abstract InstructionBatch<MethodID, InstanceInfo> GetEmittedInstructions();

        /// <summary>
        /// Insert given batch of instructions (as they were emitted)
        /// </summary>
        /// <param name="instructions">Inserted instructions</param>
        public abstract void InsertInstructions(InstructionBatch<MethodID, InstanceInfo> instructions);

        public abstract void AssignLiteral(string target, object literal);

        public abstract void Assign(string targetVar, string sourceVar);

        public abstract void AssignArgument(string targetVar,InstanceInfo staticInfo, uint argumentPosition);

        /// <summary>
        /// Assigning last call return value into specified target variable
        /// </summary>
        /// <param name="targetVar">Variable where returned value will be assigned</param>
        public abstract void AssignReturnValue(string targetVar,InstanceInfo staticInfo);

        public abstract CallBuilder<MethodID, InstanceInfo> StaticCall(string typeFullname, MethodID method, params string[] inputVariables);

        public abstract CallBuilder<MethodID,InstanceInfo> Call(MethodID method, string thisObjVariable, params string[] inputVariables);

        public abstract void Return(string sourceVar);

        public abstract void DirectInvoke(DirectMethod<MethodID, InstanceInfo> method);

        /// <summary>
        /// Creates label
        /// NOTE:
        ///     Every label has to be initialized by SetLabel
        /// </summary>
        /// <param name="identifier">Label identifier</param>
        /// <returns>Created label</returns>
        public abstract Label CreateLabel(string identifier);
        
        /// <summary>
        /// Jumps at given target if instance under conditionVariable is resolved as true
        /// </summary>
        /// <param name="conditionVariable">Variable where condition is stored</param>
        /// <param name="target">Target label</param>
        public abstract void ConditionalJump(string conditionVariable, Label target);

        /// <summary>
        /// Jumps at given target
        /// </summary>
        /// <param name="target">Target label</param>
        public abstract void Jump(Label target);
        /// <summary>
        /// Set label pointing to next instruction that will be generated
        /// </summary>
        /// <param name="label">Label that will be set</param>
        public abstract void SetLabel(Label label);

        public abstract void Nop();

        /// <summary>
        /// Returns instance info stored for given variable
        /// </summary>
        /// <param name="variable">Variable which info is resolved</param>
        /// <returns>Stored info</returns>
        public abstract InstanceInfo VariableInfo(string variable);

        
    }
}