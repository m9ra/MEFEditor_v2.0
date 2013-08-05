using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing
{
    public interface IEmitter<MethodID,InstanceInfo>
    {
        void AssignLiteral(string target, object literal);

        void Assign(string targetVar, string sourceVar);

        /// <summary>
        /// Assigning last call return value into specified target variable
        /// </summary>
        /// <param name="targetVar">Variable where returned value will be assigned</param>
        void AssignReturnValue(string targetVar);

        void StaticCall(string typeFullname,MethodID method, params string[] inputVariables);

        void Call(MethodID method, string thisObjVariable, params string[] inputVariables);

        void Return(string sourceVar);

        void DirectInvoke(DirectMethod<MethodID,InstanceInfo> method);

        /// <summary>
        /// Creates label
        /// NOTE:
        ///     Every label has to be initialized by SetLabel
        /// </summary>
        /// <param name="identifier">Label identifier</param>
        /// <returns>Created label</returns>
        Label CreateLabel(string identifier);
        
        /// <summary>
        /// Jumps at given target if instance under conditionVariable is resolved as true
        /// </summary>
        /// <param name="conditionVariable">Variable where condition is stored</param>
        /// <param name="target">Target label</param>
        void ConditionalJump(string conditionVariable, Label target);

        /// <summary>
        /// Jumps at given target
        /// </summary>
        /// <param name="target">Target label</param>
        void Jump(Label target);
        /// <summary>
        /// Set label pointing to next instruction that will be generated
        /// </summary>
        /// <param name="label">Label that will be set</param>
        void SetLabel(Label label);

        /// <summary>
        /// Returns instance info stored for given variable
        /// </summary>
        /// <param name="variable">Variable which info is resolved</param>
        /// <returns>Stored info</returns>
        InstanceInfo VariableInfo(string variable);
    }
}
