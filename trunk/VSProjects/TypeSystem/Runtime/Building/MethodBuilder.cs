using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

using Analyzing;
using Analyzing.Execution;

namespace TypeSystem.Runtime.Building
{
    /// <summary>
    /// Builder of runtime method generators
    /// </summary>
    class MethodBuilder
    {
        readonly List<Expression> _parameterValues = new List<Expression>();
        readonly List<ParameterInfo> _parameters = new List<ParameterInfo>();
        readonly Expression _argumentsArray;
        readonly Expression _runtimeTypeDefinition;

        readonly RuntimeTypeDefinition _declaringType;
        readonly string _methodName;
        readonly MethodInfo _methodInfo;

        internal MethodBuilder(RuntimeTypeDefinition declaringType, MethodInfo methodInfo, string methodName)
        {
            _declaringType = declaringType;
            _methodName = methodName;
            _methodInfo = methodInfo;

            _runtimeTypeDefinition = Expression.Constant(declaringType);
            _argumentsArray = Expression.Property(_runtimeTypeDefinition, "CurrentArguments");
        }

        #region Internal API of builder

        internal void AddRawParam(ParameterInfo param, string typeName)
        {
            //TODO: throw new NotImplementedException("Find conversion type in attribute");

            var rawArg = getRawArg();
            addParam(rawArg, param);
        }

        internal void AddUnwrappedParam(ParameterInfo param)
        {
            var rawArg = getRawArg();
            var unwrapped = Expression.Call(this.GetType(), "Unwrap", new Type[] { param.ParameterType }, rawArg);

            addParam(unwrapped, param);
        }

        internal RuntimeMethodGenerator CreateGenerator()
        {
            var directMethod = buildDirectMethod();
            return new RuntimeMethodGenerator(directMethod, createMethodInfo());
        }

        #endregion

        #region Methods for wrap handling

        /// <summary>
        /// Unwrap given instance into type T
        /// <remarks>Is called from code emitted by expression tree</remarks>
        /// </summary>
        /// <typeparam name="T">Type to which instance will be unwrapped</typeparam>
        /// <param name="instance">Unwrapped instance</param>
        /// <returns>Unwrapped data</returns>
        internal static T Unwrap<T>(Instance instance)
        {
            return (T)instance.DirectValue;
        }

        /// <summary>
        /// Wrap given data of type T into instance
        /// <remarks>Is called from code emitted by expression tree</remarks>
        /// </summary>
        /// <typeparam name="T">Type from which instance will be wrapped</typeparam>
        /// <param name="context">Data to be wrapped</param>
        /// <returns>Instance wrapping given data</returns>
        internal static Instance Wrap<T>(AnalyzingContext context, T data)
        {
            return context.Machine.CreateDirectInstance(data);
        }

        #endregion

        private TypeMethodInfo createMethodInfo()
        {
            var returnType = new InstanceInfo(_methodInfo.ReturnType);
            var parameters = createParameters(_parameters);

            var result = new TypeMethodInfo(
                _declaringType.TypeInfo, _methodName,
                returnType, parameters,
                _methodInfo.IsStatic);
            return result;
        }

        private ParameterTypeInfo[] createParameters(IEnumerable<ParameterInfo> parameters)
        {
            var result = new List<ParameterTypeInfo>();
            foreach (var param in parameters)
            {
                result.Add(ParameterTypeInfo.From(param));
            }


            return result.ToArray();
        }

        private Expression getRawArg()
        {
            var argIndex = _parameters.Count + 1;
            var argIndexExpr = Expression.Constant(argIndex);
            return Expression.ArrayIndex(_argumentsArray, argIndexExpr);
        }

        private void addParam(Expression expression, ParameterInfo info)
        {
            _parameters.Add(info);
            _parameterValues.Add(expression);
        }

        private DirectMethod buildDirectMethod()
        {
            var wrappedMethod = getWrappedMethod();

            return (c) =>
            {
                _declaringType.Invoke(c, wrappedMethod);
            };
        }

        private DirectMethod getWrappedMethod()
        {
            //input values
            var contextParam = Expression.Parameter(typeof(AnalyzingContext));
            var declaringTypeExpr = Expression.Constant(_declaringType);

            //call to method obtained from runtime type definition
            var callResult = Expression.Call(declaringTypeExpr, _methodInfo, _parameterValues);

            var returnType = _methodInfo.ReturnType;

            if (returnType == typeof(void))
            {
                //there is no return value                
                return Expression.Lambda<DirectMethod>(callResult, contextParam).Compile();
            }


            if (!returnType.IsSubclassOf(typeof(Instance)))
            {
                //wrapping is needed
                callResult = Expression.Call(this.GetType(), "Wrap", new Type[] { returnType }, contextParam, callResult);
            }

            var returnCall = Expression.Call(contextParam, "Return", new Type[0], callResult);
            return Expression.Lambda<DirectMethod>(returnCall, contextParam).Compile();
        }
    }
}
