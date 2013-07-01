using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution.Instructions;

namespace Analyzing.Execution
{
    class CallEmitter : IEmitter
    {
        /// <summary>
        /// Owning loader - is used for resolving
        /// </summary>
        readonly IInstructionLoader _loader;
        /// <summary>
        /// Instructions emitted by this emitter
        /// </summary>
        readonly List<IInstruction> _instructions = new List<IInstruction>();
        /// <summary>
        /// Types resolved for variables
        /// </summary>
        readonly Dictionary<VariableName, TypeDescription> _variableTypes = new Dictionary<VariableName, TypeDescription>();

        internal CallEmitter(IInstructionLoader loader)
        {
            _loader = loader;
        }

        #region Emittor API implementation
        public void AssignLiteral(string targetVar, object literal)
        {
            var target = getVariable(targetVar, literal.GetType());
            var literalInstance = new Instance(literal);

            _instructions.Add(new AssignLiteral(target, literalInstance));
        }

        public void Assign(string targetVar, string sourceVar)
        {
            var source = getVariable(sourceVar);
            var target = getVariable(targetVar, variableType(source));

            _instructions.Add(new Assign(target, source));
        }

        public void AssignReturnValue(string targetVar)
        {
            //TODO resolve return value of previous call
            var target = getVariable(targetVar);

            _instructions.Add(new AssignReturnValue(target));
        }

        public void StaticCall(string typeFullname, string methodName, params string[] inputVariables)
        {
            var inputArgumentVars = translateVariables(inputVariables);
            var methodDescription = createMethodDescription(typeFullname, methodName, inputArgumentVars.ToArray(), true);
            var initializerDescription = createMethodDescription(typeFullname, ".initializer", new VariableName[0], true);

            var sharedThisVar = getSharedVar(typeFullname);
            var callArgVars = new VariableName[] { sharedThisVar }.Concat(inputArgumentVars);

            var generatorName = _loader.ResolveCallName(methodDescription);
            var initializatorName = _loader.ResolveCallName(initializerDescription);

            var ensureInitialization = new EnsureInitialized(sharedThisVar, initializatorName);
            var lateInitialization = new LateReturnInitialization(sharedThisVar);
            var preCall = new PreCall(callArgVars);
            var call = new Call(generatorName);

            _instructions.Add(ensureInitialization);
            _instructions.Add(lateInitialization);
            _instructions.Add(preCall);
            _instructions.Add(call);
        }

        public void Call(string methodName, string thisObjVariable, params string[] inputVariables)
        {
            var thisVar = getVariable(thisObjVariable);
            var thisType = variableType(thisVar);

            var inputArgumentVars = translateVariables(inputVariables);
            var callArgVars = new VariableName[] { thisVar }.Concat(inputArgumentVars);
                        
            var methodDesription = createMethodDescription(thisType, methodName, inputArgumentVars.ToArray());
            var generatorName = _loader.ResolveCallName(methodDesription);

            var preCall = new PreCall(callArgVars);
            var call = new Call(generatorName);

            _instructions.Add(preCall);
            _instructions.Add(call);
        }

        public void Return(string sourceVar)
        {
            var sourceVariable = getVariable(sourceVar);
            _instructions.Add(new Return(sourceVariable));
        }
        #endregion

        #region Internal emittor services

        /// <summary>
        /// Get emitted program
        /// </summary>
        /// <returns>Program that has been emitted</returns>
        internal IInstruction[] GetEmittedInstructions()
        {
            return _instructions.ToArray();
        }

        /// <summary>
        /// Directly emits given instructions
        /// NOTE:
        ///     Is used for emitting from cached calls
        /// </summary>
        /// <param name="instructions">Instractions that will be emitted</param>
        internal void DirectEmit(IEnumerable<IInstruction> instructions)
        {
            _instructions.AddRange(instructions);
        }
        #endregion

        #region Private variable services

        private VariableName getSharedVar(string typeFullname)
        {
            return new VariableName("shared_" + typeFullname);
        }

        private VariableName getVariable(string variable, Type type = null)
        {
            TypeDescription typeDescription = null;
            if (type != null)
            {
                //TODO proper type resolving
                typeDescription = new TypeDescription(type.FullName);
            }
            return getVariable(variable, typeDescription);
        }

        private VariableName getVariable(string variable, TypeDescription type)
        {
            var variableName = new VariableName(variable);
            if (type != null && !_variableTypes.ContainsKey(variableName))
            {
                //firstly determined variable type
                _variableTypes[variableName] = type;
            }

            return variableName;
        }

        private TypeDescription variableType(VariableName name)
        {
            TypeDescription varType;
            _variableTypes.TryGetValue(name, out varType);
            return varType;
        }

        private IEnumerable<VariableName> translateVariables(string[] inputArguments)
        {
            foreach (var arg in inputArguments)
            {
                yield return new VariableName(arg);
            }
        }

        #endregion

        #region Private method services

        private MethodDescription createMethodDescription(string thisObjType, string methodName, VariableName[] inputVariables, bool isStatic = false)
        {
            var thisType=_loader.ResolveDescription(thisObjType);
            return createMethodDescription(thisType, methodName, inputVariables, isStatic);
        }

        private MethodDescription createMethodDescription(TypeDescription thisType, string methodName, VariableName[] inputVariables, bool isStatic = false)
        {            
            return new MethodDescription(thisType, methodName, new ParamDescription[inputVariables.Length], isStatic);
        }

        #endregion
    }
}
