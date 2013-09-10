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
            foreach (var method in type.GetMethods())
            {
                if (isInDirectCover(method))
                {
                    var directMethod = generateDirectMethod(method);

                    var paramsInfo = new List<TypeParameterInfo>();
                    foreach (var param in method.GetParameters())
                    {
                        var paramInfo = TypeParameterInfo.From(param);
                        paramsInfo.Add(paramInfo);
                    }

                    //TODO create proper info
                    var typeInfo = new InstanceInfo(type);
                    var returnInfo = new InstanceInfo(method.ReturnType);
                    var info = new TypeMethodInfo(typeInfo, method.Name, returnInfo, paramsInfo.ToArray(), method.IsStatic);

                    yield return new RuntimeMethodGenerator(directMethod, info);
                }
            }
        }

        private DirectMethod generateDirectMethod(MethodInfo method)
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

                var returnValue = method.Invoke(thisObj, directArgs);
                var returnInstance = context.CreateDirectInstance(returnValue);

                context.Return(returnInstance);
            };
        }


        #endregion

        #region Direct type services


        private bool isInDirectCover(MethodInfo method)
        {
            foreach (var parameter in method.GetParameters())
            {
                if (!isInDirectCover(parameter.ParameterType))
                {
                    return false;
                }
            }

            //TODO void
            return isInDirectCover(method.ReturnType);
        }

        private bool isInDirectCover(Type type)
        {
            return ContainingAssembly.IsInDirectCover(type);
        }
        #endregion
    }
}
