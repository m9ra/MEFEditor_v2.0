using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics.SymbolStore;


using System.Threading;
using ReflectionNET = System.Reflection;
using EmitNET = System.Reflection.Emit;


using System.Reflection.Emit;
using System.Reflection;
//using Mono.Cecil;

using TypeSystem.Core;
using TypeSystem.Reflection;
using TypeSystem.Reflection.Definitions;
using TypeSystem.Reflection.ILAnalyzer;


namespace TypeSystem.TypeBuilding
{
    class MethodBuilder
    {
        public readonly MethodName Name;

        EmitNET.DynamicMethod _dynamicMethod;        
        EmitNET.ILGenerator _g;

        internal MethodBuilder(MethodName name)
        {
            Name = name;
            
            
            var invokeInfo = typeof(Invokable).GetMethod("Invoke");            
            var parameterTypes = (from param in invokeInfo.GetParameters() select param.ParameterType).ToArray();

            _dynamicMethod = new DynamicMethod(name.Simple, invokeInfo.ReturnType, parameterTypes,this.GetType());
            _g = _dynamicMethod.GetILGenerator();
        }

        public static MethodDefinition Wrap(ReflectionNET.MethodInfo methodInfo)
        {   
            var methodName = new MethodName(methodInfo.Name);
            var builder = new MethodBuilder(methodName);

            var generator = builder.GetGenerator();

            foreach (var local in methodInfo.GetMethodBody().LocalVariables)
            {
                generator.DeclareLocal(local.LocalType);
            }

            var instructions = new ILReader(methodInfo).Instructions;
            var modifyWriter = new StringModifyWriter(generator);
            foreach (var instr in instructions)
            {
                modifyWriter.Write(instr);
            }
            //ILInstructionWriter.WriteIL(instructions, generator);      
            return builder.CreateMethodDefinition();
        }

        private MethodDefinition CreateMethodDefinition()
        {            
            var deleg = _dynamicMethod.CreateDelegate(typeof(Invokable),null) as Invokable;           

            var resultBody = new BodyDefinition(deleg);
            var methodDefinition= new MethodDefinition(resultBody);
            return methodDefinition;
        }



        /// <summary>
        /// Emit return wrap (according to InternalType calling convention)
        /// </summary>
        private void EmitRetWrap(Type returnType)
        {
            if (returnType == typeof(void))
            {
                EmitVoidValue();
            }
            else
            {
                EmitWrap(returnType);
            }
            _g.Emit(OpCodes.Ret);            
        }

        private void EmitVoidValue()
        {
            _g.Emit(OpCodes.Ldnull);
        }

        private void EmitWrap(Type type)
        {
            if (type.IsValueType)
            {
                _g.Emit(OpCodes.Box);
            }

            
            //TODO check if there is wrapping in type system for given type
            EmitInternalTypeCall("_wrap");
        }

        /// <summary>
        /// Emit given method call in common CLR. (Expects arguments and this object on stack)
        /// </summary>
        /// <param name="methodInfo"></param>
        private void EmitCLRCall(MethodInfo methodInfo)
        {
            _g.EmitCall(OpCodes.Call, methodInfo, null);
        }

        /// <summary>
        /// Emit argument unwrap (according to InternalType calling convention)
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="type"></param>
        private void EmitArgUnWrap(int argIndex, Type type)
        {
            //TODO check if there is an unwrap
            _g.Emit(OpCodes.Ldnull);
            EmitInternalTypeCall("_unwrap",type);
        }

        private void EmitThis()
        {
            _g.Emit(OpCodes.Ldarg_0);   
        }

        /// <summary>
        /// Emit this object with appropriate method name
        /// </summary>
        /// <param name="methodName"></param>
        private void EmitInternalTypeCall(string methodName,params Type[] genericParams)
        {           
            var info = getInternalTypeMethod(methodName);
            if (info.IsGenericMethod)
            {
                info = info.MakeGenericMethod(genericParams);    
            }

            _g.EmitCall(OpCodes.Call, info, null);            
        }

        public ILGenerator GetGenerator()
        {
            throwIfBuilded();            
            return _g;
        }

        private void throwIfBuilded()
        {
            //TODO
        }
                       
        private static MethodInfo getInternalTypeMethod(string name)
        {
            return typeof(InternalType).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic);
        }
    }
}
