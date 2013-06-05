using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution.Instructions;

namespace Analyzing.Execution
{
    class CallEmitter:IEmitter
    {
        readonly IInstructionLoader _loader;
        internal CallEmitter(IInstructionLoader loader)
        {
            _loader = loader;
        }

        /// <summary>
        /// Instructions emitted by this emitter
        /// </summary>
        List<IInstruction> _instructions = new List<IInstruction>();
        public void AssignLiteral(string targetVar, object literal)
        {
            var target = new VariableName(targetVar);
            var literlInstance = new Instance(literal);

            _instructions.Add(new AssignLiteral(target, literlInstance));
        }

        public void Assign(string targetVar, string sourceVar)
        {
            var target = new VariableName(targetVar);
            var source = new VariableName(sourceVar);

            _instructions.Add(new Assign(target, source));
        }

        public void AssignReturnValue(string targetVar)
        {
            var target = new VariableName(targetVar);

            _instructions.Add(new AssignReturnValue(target));
        }

        public void StaticCall(string typeFullname, string methodName, params string[] inputArguments)
        {
            var inputArgumentVars=translateVariables(inputArguments);

            var sharedThisVar=getSharedVar(typeFullname);
            var callArgVars=new VariableName[]{sharedThisVar}.Concat(inputArgumentVars);
            
            var typeDescription=_loader.ResolveDescription(typeFullname);
            var generatorName=_loader.ResolveCallName(typeDescription,methodName);
            var initializatorName=_loader.ResolveCallName(typeDescription,".initializer");
            
            var ensureInitialization = new EnsureInitialized(sharedThisVar, initializatorName);
            var lateInitialization = new LateReturnInitialization(sharedThisVar);
            var call=new Call(generatorName,callArgVars);
            

            _instructions.Add(ensureInitialization);
            _instructions.Add(lateInitialization);
            _instructions.Add(call);
        }


        public void Return(string sourceVar)
        {
            var sourceVariable = new VariableName(sourceVar);
            _instructions.Add(new Return(sourceVariable));
        }
        /// <summary>
        /// Get emitted program
        /// </summary>
        /// <returns>Program that has been emitted</returns>
        internal IInstruction[] GetEmittedInstructions()
        {
            return _instructions.ToArray();
        }

        private VariableName getSharedVar(string typeFullname)
        {
            return new VariableName("shared_"+typeFullname);
        }

        private IEnumerable<VariableName> translateVariables(string[] inputArguments)
        {
            foreach (var arg in inputArguments)
            {
                yield return new VariableName(arg);
            }
        }


    }
}
