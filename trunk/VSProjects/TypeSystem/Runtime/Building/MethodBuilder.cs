﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Reflection;
using System.Linq.Expressions;

using Analyzing;
using Analyzing.Execution;

namespace TypeSystem.Runtime.Building
{
    /// <summary>
    /// Builder for runtime methods
    /// </summary>
    class MethodBuilder
    {
        /// <summary>
        /// Type of analyzing context
        /// TODO: Refactor method infos out of classes
        /// </summary>
        private static readonly Type _contextType = typeof(AnalyzingContext);

        /// <summary>
        /// Definition that is declaring builded method
        /// </summary>
        private readonly RuntimeTypeDefinition _declaringDefinition;
     
        /// <summary>
        /// Array where arguments are stored
        /// </summary>
        private readonly Expression _argumentsArray;

        /// <summary>
        /// Input parameter for direct method (contains Analyzing context)
        /// </summary>
        private readonly ParameterExpression _contextParam;

        /// <summary>
        /// Name of builded method
        /// </summary>
        private readonly string _methodName;

        /// <summary>
        /// Expression which can get declaring definition object
        /// </summary>
        internal readonly Expression DeclaringDefinitionConstant;

        /// <summary>
        /// Method info of builded method
        /// (Can be overriden before build)
        /// </summary>
        internal TypeMethodInfo BuildedMethodInfo;

        /// <summary>
        /// Method that is invoked in context of declaring definition
        /// (Can be overriden before build)
        /// </summary>
        internal DirectMethod Adapter;

        /// <summary>
        /// Object on which adapter is called
        /// (Can be overriden before build)
        /// </summary>
        internal Expression ThisObjectExpression;

        /// <summary>
        /// Types that are implemented byt builded method
        /// </summary>
        internal HashSet<Type> ImplementedTypes = new HashSet<Type>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="declaringDefinition"></param>
        /// <param name="methodName"></param>
        internal MethodBuilder(RuntimeTypeDefinition declaringDefinition, string methodName)
        {
            _declaringDefinition = declaringDefinition;
            DeclaringDefinitionConstant = Expression.Constant(_declaringDefinition);
            _argumentsArray = Expression.Property(DeclaringDefinitionConstant, "CurrentArguments");
            _contextParam = Expression.Parameter(typeof(AnalyzingContext), "context");
            _methodName = methodName;
        }

        internal Expression ArgumentInstanceExpression(int argumentIndex)
        {
            return Expression.ArrayIndex(_argumentsArray, Expression.Constant(argumentIndex));
        }

        internal void AdapterFor(MethodInfo method)
        {
            Adapter = generateAdapter(method);
            var paramsInfo = getParametersInfo(method);

            var returnInfo = getInstanceInfo(method.ReturnType);

            var isAbstract = method.IsAbstract || _declaringDefinition.IsInterface;
            BuildedMethodInfo= new TypeMethodInfo(
                _declaringDefinition.TypeInfo, _methodName,
                returnInfo, paramsInfo.ToArray(),
                method.IsStatic, _declaringDefinition.IsGeneric, isAbstract);
        }

        internal RuntimeMethodGenerator Build()
        {
            var implementedTypes = ImplementedTypes.ToArray();
            var methodInfo = BuildedMethodInfo;
            var adapter = Adapter;

            return new RuntimeMethodGenerator(
                (c) => invoke(c,methodInfo, Adapter, _declaringDefinition),
                methodInfo,
                implementedTypes);
        }

        private static void invoke(AnalyzingContext context,TypeMethodInfo method, DirectMethod adapter, RuntimeTypeDefinition definition)
        {
            definition.Invoke(context, adapter);
        }

        /// <summary>
        /// Get parameters info for given method base
        /// </summary>
        /// <param name="method">Base method which parameters will be created</param>
        /// <returns>Created parameters info</returns>
        private IEnumerable<ParameterTypeInfo> getParametersInfo(MethodBase method)
        {
            var paramsInfo = new List<ParameterTypeInfo>();
            foreach (var param in method.GetParameters())
            {
                var paramType = getInstanceInfo(param.ParameterType);
                var paramInfo = ParameterTypeInfo.From(param, paramType);
                paramsInfo.Add(paramInfo);
            }
            return paramsInfo;
        }

        private InstanceInfo getInstanceInfo(Type type)
        {
            return _declaringDefinition.GetInstanceInfo(type);
        }

        private DirectMethod generateAdapter(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var arguments = new Expression[parameters.Length];
            for (var i = 0; i < parameters.Length; ++i)
            {
                var argInstance = ArgumentInstanceExpression(i + 1);
                arguments[i] = convert(argInstance,parameters[i].ParameterType);
            }

            var thisExpression = convert(ThisObjectExpression, method.DeclaringType);
            var adapterCall = Expression.Call(thisExpression, method, arguments);
            var handledAdapter = handleReturn(adapterCall);

            return Expression.Lambda<DirectMethod>(handledAdapter, _contextParam).Compile();
        }

        private Expression handleReturn(MethodCallExpression adapterCall)
        {
            var returnType = adapterCall.Method.ReturnType;
            if (returnType == typeof(void))
                //there is no need for unwrapping logic
                return adapterCall;


            //TODO refactor conversion logic

            var needReturnUnWrapping = returnType == typeof(InstanceWrap);
            Expression returnValue = adapterCall;
            if (needReturnUnWrapping)
            {
                //Wrapped instance has been returned - unwrap it
                returnValue = Expression.PropertyOrField(returnValue, "Wrapped");
            }
            else
            {
                //Direct object has been returned - create its direct instance
                var machine = Expression.PropertyOrField(_contextParam, "Machine");
                
                if (returnType.IsArray)
                {
                    var arrayWrapType = typeof(Array<InstanceWrap>);
                    var arrayWrapCtor = arrayWrapType.GetConstructor(new Type[] { typeof(IEnumerable), _contextType });
                    returnValue = Expression.New(arrayWrapCtor, returnValue, _contextParam);
                }

                var instanceInfo = Expression.Constant(new InstanceInfo(returnType));
                returnValue = Expression.Convert(returnValue, typeof(object));
                returnValue = Expression.Call(machine, typeof(Machine).GetMethod("CreateDirectInstance"), returnValue, instanceInfo);
            }

            //return value is reported via Context.Return call
            return Expression.Call(_contextParam, _contextType.GetMethod("Return"), returnValue);
        }

        /// <summary>
        /// Get argument expression according to index
        /// </summary>
        /// <param name="index">Zero based index of arguments - zero arguments belongs to this instance</param>
        /// <param name="resultType">Expected type of result - wrapping, direct value obtaining is processed</param>
        /// <param name="contextParameter">Parameter with context object</param>
        /// <returns>Argument expression</returns>
        private Expression convert(Expression instanceExpression, Type resultType)
        {
            if (instanceExpression == null)
                return null;

            if (instanceExpression.Type == resultType)
                //there is no conversion needed
                return instanceExpression;

            var instanceWrapType = typeof(InstanceWrap);

            if (resultType == instanceWrapType)
            {
                //wrapp as InstanceWrap
                var ctor = instanceWrapType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
                return Expression.New(ctor, new Expression[] { instanceExpression });
            }
            else
            {
                //unwrap to direct value
                return Expression.Call(DeclaringDefinitionConstant, "Unwrap", new Type[] { resultType }, instanceExpression);
            }
        }
    }
}
