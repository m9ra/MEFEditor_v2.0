using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Analyzing;

namespace TypeSystem
{
    public class DirectAssembly : AssemblyProvider, IInstructionLoader
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
        Dictionary<string, MethodItem> _directMethods = new Dictionary<string, MethodItem>();
        /// <summary>
        /// Currently wrapped instruction loader
        /// </summary>
        IInstructionLoader _wrappedLoader;
        public DirectAssembly(IInstructionLoader wrappedLoader, Settings settings)
        {
            _wrappedLoader = wrappedLoader;
            _directTypes = settings.DirectTypes;

            foreach (var directType in _directTypes)
            {
                registerDirectType(directType);
            }

            foreach (var method in settings.DirectMethods.Values)
            {
                addDirectMethod(method);
            }
        }

        #region IInstructionLoader implementation
        public IInstructionGenerator<MethodID, InstanceInfo> EntryPoint
        {
            get { return _wrappedLoader.EntryPoint; }
        }

        public VersionedName ResolveCallName(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            //TODO better method resolving

            var methodName=staticArgumentInfo[0].TypeName+"."+ method.MethodName;
            if (!_directMethods.ContainsKey(methodName))
            {
                methodName = Name.From(method, staticArgumentInfo).Name;
                methodName=staticArgumentInfo[0].TypeName + "." + methodName;
            }

            var name = new VersionedName(methodName,733);
            return name;
        }
        public IInstructionGenerator<MethodID, InstanceInfo> GetGenerator(VersionedName methodName)
        {
            MethodItem method;
            
            if (_directMethods.TryGetValue(methodName.Name, out method))
            {
                return method.Generator;
            }

            return _wrappedLoader.GetGenerator(methodName);
        }

        public VersionedName ResolveStaticInitializer(InstanceInfo info)
        {
            return _wrappedLoader.ResolveStaticInitializer(info);
        }

        public IInstructionGenerator<MethodID, InstanceInfo> GetGenerator(VersionedName methodName, MethodID method, InstanceInfo[] instanceArgumentInfo)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region AssemblyProvider implementation

        protected override string resolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            //TODO better method resolving
            var name = Name.From(method, staticArgumentInfo);

            if (_directMethods.ContainsKey(name.Name))
            {
                return name.Name;
            }

            //there is no such a method
            return null;
        }

        protected override IInstructionGenerator getGenerator(string methodName)
        {
            if (_directMethods.ContainsKey(methodName))
            {
                return _directMethods[methodName].Generator;
            }
            return null;
        }

        public override SearchIterator CreateRootIterator()
        {

            return new HashIterator(_directMethods);
        }

        #endregion

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

                    //TODO create proper info
                    var info = new TypeMethodInfo(type.FullName, name.Name, null, false);
                    var item = new MethodItem(new DirectGenerator(directMethod), info);
                    addDirectMethod(item);
                }
            }
        }

        private void addDirectMethod(MethodItem method)
        {            
            _directMethods.Add(method.Info.Path, method);
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
            return _directMethods.ContainsKey(name.Name);
        }



    }
}
