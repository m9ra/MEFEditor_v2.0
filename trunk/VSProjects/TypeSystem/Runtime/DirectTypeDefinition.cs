using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Analyzing;

using TypeSystem.Runtime.Building;

namespace TypeSystem.Runtime
{
    /// <summary>
    /// Represents definitions of types that are stored directly in DirectInstance    
    /// </summary>
    /// <typeparam name="DirectType">DirectType represented by thid definition</typeparam>
    public class DirectTypeDefinition<DirectType> : RuntimeTypeDefinition
    {
        protected DirectType This { get; private set; }

        private readonly List<RuntimeMethodGenerator> _explicitGenerators = new List<RuntimeMethodGenerator>();

        protected void AddMethod(DirectMethod method, TypeMethodInfo methodInfo)
        {
            _explicitGenerators.Add(new RuntimeMethodGenerator(method, methodInfo));
        }

        internal override InstanceInfo TypeInfo
        {
            get { return InstanceInfo.Create<DirectType>(); }
        }

        internal override IEnumerable<RuntimeMethodGenerator> GetMethods()
        {
            //TODO resolve method replacing
            return generateDirectMethods(typeof(DirectType)).Union(_explicitGenerators);
        }

        #region Direct methods generation

        private IEnumerable<RuntimeMethodGenerator> generateDirectMethods(Type type)
        {
            //TODO generic types
            foreach (var method in publicMethods(type))
            {
                yield return method;
            }

            foreach (var method in constructorMethods(type))
            {
                yield return method;
            }
        }

        private IEnumerable<RuntimeMethodGenerator> constructorMethods(Type type)
        {
            var ctorName = type.Name;
            foreach (var ctor in type.GetConstructors())
            {
                if (!areParamsInDirectCover(ctor))
                    continue;

                var directMethod = generateDirectMethod(ctor);
                var paramsInfo = getParametersInfo(ctor);

                var returnInfo = InstanceInfo.Void;
                var info = new TypeMethodInfo(
                    TypeInfo, ctorName,
                    returnInfo, paramsInfo.ToArray(),
                    false);
                yield return new RuntimeMethodGenerator(directMethod, info);
            }
        }

        private IEnumerable<RuntimeMethodGenerator> publicMethods(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                if (!isInDirectCover(method))
                    continue;

                var directMethod = generateDirectMethod(method);
                var paramsInfo = getParametersInfo(method);

                var returnInfo = new InstanceInfo(method.ReturnType);
                var info = new TypeMethodInfo(
                    TypeInfo, method.Name, 
                    returnInfo, paramsInfo.ToArray(),
                    method.IsStatic);

                yield return new RuntimeMethodGenerator(directMethod, info);
            }
        }

        private static List<ParameterTypeInfo> getParametersInfo(MethodBase method)
        {
            var paramsInfo = new List<ParameterTypeInfo>();
            foreach (var param in method.GetParameters())
            {
                var paramInfo = ParameterTypeInfo.From(param);
                paramsInfo.Add(paramInfo);
            }
            return paramsInfo;
        }

        private DirectMethod generateDirectMethod(MethodBase method)
        {
            return (context) =>
            {
                var args = context.CurrentArguments;
                object[] directArgs;
                object thisObj;

                if (method.IsStatic)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    directArgs = (from arg in args.Skip(1) select arg.DirectValue).ToArray();
                    thisObj = args[0].DirectValue;
                }

                if (method.IsConstructor)
                {
                    var ctor = method as ConstructorInfo;
                    var constructedInstance = ctor.Invoke(directArgs);
                    context.Initialize(args[0], constructedInstance);
                }
                else
                {
                    var returnValue = method.Invoke(thisObj, directArgs);
                    var returnInstance = context.Machine.CreateDirectInstance(returnValue);
                    context.Return(returnInstance);
                }
            };
        }


        #endregion

        #region Direct type services


        private bool isInDirectCover(MethodInfo method)
        {
            //TODO void
            return areParamsInDirectCover(method) && isInDirectCover(method.ReturnType);
        }

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

        private bool isInDirectCover(Type type)
        {
            return ContainingAssembly.IsInDirectCover(type);
        }
        #endregion
    }
}
