using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

using Analyzing;
using Analyzing.Execution;

using TypeSystem.Runtime.Building;

namespace TypeSystem.Runtime
{
    /// <summary>
    /// Represents definitions of types that are stored directly in DirectInstance    
    /// </summary>
    /// <typeparam name="DirectType">DirectType represented by thid definition</typeparam>
    public class DirectTypeDefinition<DirectType> : RuntimeTypeDefinition
    {
        /// <summary>
        /// Method generators added explicitly to direct type
        /// <remarks>They can replace direct type methods</remarks>
        /// </summary>
        private readonly List<RuntimeMethodGenerator> _explicitGenerators = new List<RuntimeMethodGenerator>();

        /// <summary>
        /// Type representation of direct type
        /// </summary>
        private readonly Type _directType = typeof(DirectType);

        protected DirectType This { get; private set; }

        /// <summary>
        /// Type info of current DirectType (or generic definition if TypeDefinition is marked with IsGeneric)
        /// </summary>
        internal override InstanceInfo TypeInfo
        {
            get
            {
                var definingType = _directType;
                if (IsGeneric)
                {
                    definingType = _directType.GetGenericTypeDefinition();
                }

                return new InstanceInfo(definingType);
            }
        }

        /// <summary>
        /// Add explicit method to direct type
        /// <remarks>Added method can replace existing method in direct type, if it has same method signature</remarks>        
        /// </summary>
        /// <param name="method">Added direct method which is invoked on method call</param>
        /// <param name="methodInfo">Info of added method</param>
        protected void AddMethod(DirectMethod method, TypeMethodInfo methodInfo)
        {
            _explicitGenerators.Add(new RuntimeMethodGenerator(method, methodInfo));
        }

        /// <summary>
        /// Get all methods defined for direct type (including explicit methods)
        /// </summary>
        /// <returns>Defined methods</returns>
        internal override IEnumerable<RuntimeMethodGenerator> GetMethods()
        {
            //TODO resolve method replacing
            return generateDirectMethods(_directType).Union(_explicitGenerators);
        }

        #region Direct methods generation

        /// <summary>
        /// Generate direct method for given type
        /// <remarks>Only direct methods which are in direct cover are generated</remarks>
        /// </summary>
        /// <param name="type">Type which methods will be generated</param>
        /// <returns>Generated methods</returns>
        private IEnumerable<RuntimeMethodGenerator> generateDirectMethods(Type type)
        {
            foreach (var method in generatePublicMethods(type))
            {
                yield return method;
            }

            foreach (var method in generateConstructorMethods(type))
            {
                yield return method;
            }
        }

        /// <summary>
        /// Generate constructor methods for given type
        /// <remarks>Only instance constructors are generated (because static constructors cannot be wrapped)</remarks>
        /// </summary>
        /// <param name="type">Type which constructors will be generated</param>
        /// <returns>Generated constructor methods</returns>
        private IEnumerable<RuntimeMethodGenerator> generateConstructorMethods(Type type)
        {
            foreach (var ctor in type.GetConstructors())
            {
                if (!areParamsInDirectCover(ctor))
                    continue;

                var directMethod = generateDirectCtor(ctor);
                var paramsInfo = getParametersInfo(ctor);

                var returnInfo = InstanceInfo.Void;
                var info = new TypeMethodInfo(
                    TypeInfo, "#ctor",
                    returnInfo, paramsInfo.ToArray(),
                    false, IsGeneric);
                yield return new RuntimeMethodGenerator(directMethod, info);
            }
        }

        /// <summary>
        /// Generate public static/instance methods for given type
        /// </summary>
        /// <param name="type">Type which methods will be generated</param>
        /// <returns>Generated methods</returns>
        private IEnumerable<RuntimeMethodGenerator> generatePublicMethods(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                if (!isInDirectCover(method))
                    continue;

                var methodDefinition = method;
                if (method.IsGenericMethod)
                    methodDefinition = method.GetGenericMethodDefinition();

                var directMethod = generateDirectMethod(methodDefinition);
                var paramsInfo = getParametersInfo(methodDefinition);

                var returnInfo = new InstanceInfo(methodDefinition.ReturnType);
                var info = new TypeMethodInfo(
                    TypeInfo, methodDefinition.Name,
                    returnInfo, paramsInfo.ToArray(),
                    methodDefinition.IsStatic, IsGeneric);

                yield return new RuntimeMethodGenerator(directMethod, info);
            }
        }

        /// <summary>
        /// Get parameters info for given method base
        /// </summary>
        /// <param name="method">Base method which parameters will be created</param>
        /// <returns>Created parameters info</returns>
        private static IEnumerable<ParameterTypeInfo> getParametersInfo(MethodBase method)
        {
            var paramsInfo = new List<ParameterTypeInfo>();
            foreach (var param in method.GetParameters())
            {
                var paramInfo = ParameterTypeInfo.From(param);
                paramsInfo.Add(paramInfo);
            }
            return paramsInfo;
        }

        /// <summary>
        /// Generate direct method for constructor
        /// </summary>
        /// <param name="constructor">Constructor which method is generated</param>
        /// <returns>Generated method</returns>
        private DirectMethod generateDirectCtor(ConstructorInfo constructor)
        {
            var contextType = typeof(AnalyzingContext);
            var contextParameter = Expression.Parameter(contextType, "context");
            var inputParameters = new ParameterExpression[] { contextParameter };

            var args = getArgumentExpressions(constructor, contextParameter);
            var thisInstance = getArgumentInstance(0, contextParameter);
            var constructed = Expression.New(constructor, args);

            var ctorMethod = Expression.Call(contextParameter, contextType.GetMethod("Initialize"), thisInstance, constructed);
            return Expression.Lambda<DirectMethod>(ctorMethod, inputParameters).Compile();
        }

        /// <summary>
        /// Generate direct method for given method info
        /// </summary>
        /// <param name="method">Method info which direct method is generated</param>
        /// <returns>Generated method</returns>
        private DirectMethod generateDirectMethod(MethodInfo method)
        {
            if (method.IsStatic)
                return (c) => { throw new NotImplementedException(); };

            var hasReturnValue = method.ReturnType != typeof(void);
            var needReturnUnWrapping = hasReturnValue && method.ReturnType == typeof(InstanceWrap);

            var contextType = typeof(AnalyzingContext);
            var contextParameter = Expression.Parameter(contextType, "context");
            var inputParameters = new ParameterExpression[] { contextParameter };

            var argumentExpressions = getArgumentExpressions(method, contextParameter);
            var thisExpression = getArgument(0, _directType, contextParameter);
            var methodCall = Expression.Call(thisExpression, method, argumentExpressions);

            if (hasReturnValue)
            {
                //if there is return value, resolve wrapping of result

                Expression returnValue = methodCall;
                if (needReturnUnWrapping)
                {
                    //Wrapped instance has been returned - unwrap it
                    returnValue = Expression.PropertyOrField(returnValue, "Wrapped");
                }
                else
                {
                    //Direct object has been returned - create its direct instance
                    var machine = Expression.PropertyOrField(contextParameter, "Machine");
                    var instanceInfo = Expression.Constant(new InstanceInfo(method.ReturnType));
                    returnValue = Expression.Convert(returnValue, typeof(object));
                    returnValue = Expression.Call(machine, typeof(Machine).GetMethod("CreateDirectInstance"), returnValue, instanceInfo);
                }

                //return value is reported via Context.Return call
                var returnCall = Expression.Call(contextParameter, contextType.GetMethod("Return"), returnValue);
                return Expression.Lambda<DirectMethod>(returnCall, inputParameters).Compile();
            }
            else
            {
                //there is no return value
                return Expression.Lambda<DirectMethod>(methodCall, inputParameters).Compile();
            }
        }

        /// <summary>
        /// Get argument expressions for given method. Argument expression get value from context.CurrentArguments
        /// </summary>
        /// <param name="method">Method which arguments</param>
        /// <param name="contextParameter">Parameter with context object</param>
        /// <returns>Argument expressions</returns>
        private IEnumerable<Expression> getArgumentExpressions(MethodBase method, ParameterExpression contextParameter)
        {
            var parameters = method.GetParameters();
            var argumentExpressions = new List<Expression>();
            for (int i = 0; i < parameters.Length; ++i)
            {
                var parameter = parameters[i];
                var argument = getArgument(i + 1, parameter.ParameterType, contextParameter);
                argumentExpressions.Add(argument);
            }
            return argumentExpressions;
        }

        /// <summary>
        /// Get argument instance according to index
        /// <remarks>No conversions nor wrapping is made</remarks>
        /// </summary>
        /// <param name="index">Zero based index of arguments - zero arguments belongs to this instance</param>
        /// <param name="contextParameter">Parameter with context object</param>
        /// <returns>Argument instance</returns>
        private Expression getArgumentInstance(int index, ParameterExpression contextParameter)
        {
            var contextType = typeof(AnalyzingContext);
            var argsArray = Expression.Property(contextParameter, contextType.GetProperty("CurrentArguments"));
            return Expression.ArrayAccess(argsArray, Expression.Constant(index));
        }

        /// <summary>
        /// Get argument expression according to index
        /// </summary>
        /// <param name="index">Zero based index of arguments - zero arguments belongs to this instance</param>
        /// <param name="resultType">Expected type of result - wrapping, direct value obtaining is processed</param>
        /// <param name="contextParameter">Parameter with context object</param>
        /// <returns>Argument expression</returns>
        private Expression getArgument(int index, Type resultType, ParameterExpression contextParameter)
        {
            var argumentInstance = getArgumentInstance(index, contextParameter);
            var instanceWrapType = typeof(InstanceWrap);

            if (resultType == instanceWrapType)
            {
                //wrapp as InstanceWrap
                var ctor = instanceWrapType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
                return Expression.New(ctor, new Expression[] { argumentInstance });
            }
            else
            {
                //unwrap to direct instance
                return Expression.Convert(Expression.PropertyOrField(argumentInstance, "DirectValue"), resultType);
            }
        }

        #endregion

        #region Direct type services

        /// <summary>
        /// Determine that method is in direct cover
        /// </summary>
        /// <param name="method">Tested method</param>
        /// <returns>True if method is in direct cover, false otherwise</returns>
        private bool isInDirectCover(MethodInfo method)
        {
            //TODO void
            return areParamsInDirectCover(method) && isInDirectCover(method.ReturnType);
        }

        /// <summary>
        /// Determine that parameters of method are in direct cover
        /// </summary>
        /// <param name="method">Method whic parameters will be tested</param>
        /// <returns>True if method parameters are in direct cover, false otherwise</returns>
        private bool areParamsInDirectCover(MethodBase method)
        {
            foreach (var parameter in method.GetParameters())
            {
                if (!isInDirectCover(parameter.ParameterType))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determine that type is in direct cover
        /// </summary>
        /// <param name="type">Tested type</param>
        /// <returns>True if type is in direct cover, false otherwise</returns>
        private bool isInDirectCover(Type type)
        {
            return ContainingAssembly.IsInDirectCover(type);
        }

        #endregion
    }
}
