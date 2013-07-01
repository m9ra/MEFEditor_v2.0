using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing
{
    public interface IEmitter
    {
        /// <summary>
        /// TODO: think about have non internal instructions
        /// </summary>
        /// <param name="_testInstructions"></param>
        //void Emit(IInstruction[] _testInstructions);

        void AssignLiteral(string target, object literal);

        void Assign(string targetVar, string sourceVar);

        /// <summary>
        /// Assigning last call return value into specified target variable
        /// </summary>
        /// <param name="targetVar">Variable where returned value will be assigned</param>
        void AssignReturnValue(string targetVar);

        void StaticCall(string typeFullname, string methodName, params string[] inputVariables);

        void Return(string sourceVar);

        void Call(string methodName,string thisObjVariable, params string[] inputVariables);
    }
}
