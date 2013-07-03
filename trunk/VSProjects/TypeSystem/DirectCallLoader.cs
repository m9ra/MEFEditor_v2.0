using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Analyzing;

namespace TypeSystem
{
    public class DirectCallLoader:IInstructionLoader
    {
        /// <summary>
        /// Types that will be represented directly in .NET representation
        /// </summary>
        readonly Type[] _directTypes;

        /// <summary>
        /// Direct types that has already been registered
        /// </summary>
        HashSet<string> _registeredDirectTypes = new HashSet<string>();

        /// <summary>
        /// Registered direct calls
        /// </summary>
        Dictionary<VersionedName, DirectGenerator> _directCalls = new Dictionary<VersionedName, DirectGenerator>();
        /// <summary>
        /// Currently wrapped instruction loader
        /// </summary>
        IInstructionLoader _wrappedLoader;
        public DirectCallLoader(IInstructionLoader wrappedLoader,Settings settings)
        {
            _wrappedLoader = wrappedLoader;
            _directTypes = settings.DirectTypes;

            foreach (var directType in _directTypes)
            {
                registerDirectType(directType);
            }

            foreach (var methodPair in settings.DirectMethods)
            {
                addDirectMethod(methodPair.Key, methodPair.Value);
            }
        }

        public IInstructionGenerator<MethodID,InstanceInfo> EntryPoint
        {
            get { return _wrappedLoader.EntryPoint; }
        }

        public VersionedName ResolveCallName(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            //TODO better method resolving
            return Name.From(method,staticArgumentInfo);
            throw new NotImplementedException();
        }

        public IInstructionGenerator<MethodID, InstanceInfo> GetGenerator(VersionedName methodName, MethodID method, InstanceInfo[] instanceArgumentInfo)
        {
            throw new NotImplementedException();
        }


        #region Direct types registration

        /// <summary>
        /// Register given type to be directly stored
        /// </summary>
        /// <param name="type">Registered type</param>
        private void registerDirectType(Type type)
        {
            //TODO generic types
            _registeredDirectTypes.Add(type.FullName);
            generateDirectCalls(type);
        }


        private void generateDirectCalls(Type type)
        {
            //TODO generic types
            foreach (var method in type.GetMethods())
            {
                if (isInDirectCover(method))
                {
                    var directMethod = generateDirectMethod(method);
                    var name = Name.From(method);
                    addDirectMethod(name, directMethod);
                }
            }
        }

        private void addDirectMethod(VersionedName name, DirectMethod<MethodID, InstanceInfo> method)
        {
            _directCalls.Add(name,new DirectGenerator(method));
        }
        
        private DirectMethod<MethodID, InstanceInfo> generateDirectMethod(MethodInfo method)
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
            return _directTypes.Contains(type);
        }
        #endregion

        private bool isCached(VersionedName name)
        {
            return _directCalls.ContainsKey(name);
        }


        public IInstructionGenerator<MethodID,InstanceInfo> GetGenerator(VersionedName methodName)
        {
            DirectGenerator generator;
            if(_directCalls.TryGetValue(methodName,out generator)){
                return generator;
            }

            return _wrappedLoader.GetGenerator(methodName);
        }


        public VersionedName ResolveStaticInitializer(InstanceInfo info)
        {
            return _wrappedLoader.ResolveStaticInitializer(info);
        }
    }
}
