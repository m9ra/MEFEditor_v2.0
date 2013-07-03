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
    }
}
