using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Reflection;

using TypeSystem.Core;

namespace TypeSystem.TypeBuilding.TypeWrapping
{
    /// <summary>
    /// Represents .NET type wrapped into InternalType
    /// </summary>
    /// <typeparam name="Wrapped">Type which wrap we are creating</typeparam>
    class WrappedType<Wrapped>:InternalType
    {
        Type _wrappedType = typeof(Wrapped);

        public WrappedType(InternalAssembly originAssembly)
            : base(TypeName.From(typeof(Wrapped)),originAssembly)
        {
        }

        internal override bool TryUnwrap<T>(Instance instance, out T obj)
        {
            var wrappedObj = instance as WrappedObject<Wrapped>;
            if (wrappedObj != null){
                obj = (T)(object)wrappedObj.WrappedData;
                return true;
            }
            return base.TryUnwrap<T>(instance, out obj);
        }

        internal override Instance Wrap(object obj)
        {
            var wrappedObj = new WrappedObject<Wrapped>(this);
            wrappedObj.WrappedData = (Wrapped)obj;
            return wrappedObj;
        }

        internal override Instance ConstructInstance(params object[] args)
        {
            var argTypesEnum=from arg in args select arg.GetType();
            var argTypes = argTypesEnum.ToArray();

            var isTypePrimitive = _wrappedType.IsPrimitive || _wrappedType==typeof(string);
            var hasMatchingArg=args.Length == 1 && args[0].GetType() == _wrappedType;

            Wrapped obj;
            if (isTypePrimitive && hasMatchingArg)
            {
                //use given argument as object
                obj = (Wrapped)args[0];
            }
            else
            {
                //create new instance of an object
                obj = (Wrapped)Activator.CreateInstance(typeof(Wrapped), argTypes);
            }
            
            return Wrap(obj);
        }
        
        protected override Instance _invoke(Instance thisInstance, string methodName, params Instance[] args)
        {
            Wrapped thisObj;
            if (!TryUnwrap<Wrapped>(thisInstance, out thisObj))
            {
                throw new NotSupportedException("Unwrapping is not supported");
            }
            
            var rawArgs=getUnwrappedArgs(args);

            var info=getMethodInfo(methodName,thisInstance.IsShared,rawArgs);


            if (thisInstance.IsShared)
                throw new NotImplementedException("Static call");

            var result=info.Invoke(thisObj, rawArgs);

            if (info.ReturnType == typeof(void))
                throw new NotImplementedException("Void return");

            var returnType = Assembly.ResolveType(TypeName.From(info.ReturnType));
            return returnType.Wrap(result);
        }

        private MethodInfo getMethodInfo(string methodName,bool isStatic, object[] instances)
        {
            var flags=BindingFlags.Public | BindingFlags.NonPublic;
            if (isStatic)
            {
                flags |= BindingFlags.Static;
            }
            else
            {
                flags |= BindingFlags.Instance;
            }
                        
            var methodInfo=_wrappedType.GetMethod(methodName, flags);

            return methodInfo;
        }

        private object[] getUnwrappedArgs(Instance[] args)
        {
            object[] unwrapped = new object[args.Length];
            for (int i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                var argType = arg.GetInternalType();
                if (!argType.TryUnwrap(arg, out unwrapped[i]))
                    throw new NotSupportedException("Cannot unwrap given instance");
            }
            return unwrapped;
        }
    }
}
