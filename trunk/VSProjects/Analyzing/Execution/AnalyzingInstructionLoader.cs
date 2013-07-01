using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;
using System.Reflection;

namespace Analyzing.Execution
{
    delegate Instance InstanceFunction(Instance[] arguments);

    class AnalyzingInstructionLoader : IInstructionLoader
    {
        /// <summary>
        /// Types that will be represented directly in .NET representation
        /// </summary>
        Type[] _directTypes = new Type[]{
            typeof(int),       
            typeof(string)
        };

        /// <summary>
        /// Direct types that has already been registered
        /// </summary>
        HashSet<string> _registeredDirectTypes = new HashSet<string>();
        /// <summary>
        /// Registered direct calls
        /// </summary>
        Dictionary<VersionedName, CachedGenerator> _directCalls = new Dictionary<VersionedName, CachedGenerator>();
        /// <summary>
        /// Currently wrapped instruction loader
        /// </summary>
        IInstructionLoader _currentLoader;

        public AnalyzingInstructionLoader()
        {
            foreach (var directType in _directTypes)
            {
                registerDirectType(directType);
            }            
        }
        
        #region Internal API for Virtual Machine

        /// <summary>
        /// Set wrapped instruction loader
        /// NOTE:
        ///     Wrapping provides caching services and direct calls handling
        /// </summary>
        /// <param name="loader">Wrapped loader</param>
        internal void SetLoader(IInstructionLoader loader)
        {
            _currentLoader = loader;
        }

        #endregion
                
        #region Instruction Loader interface implementation - Wrapping of _currentLoader
        
        public IInstructionGenerator EntryPoint
        {
            get { return _currentLoader.EntryPoint; }
        }

        public TypeDescription ResolveDescription(string typeFullname)
        {
            //TODO caching services
            return _currentLoader.ResolveDescription(typeFullname);
        }

        public VersionedName ResolveCallName(MethodDescription description)
        {
            //TODO caching services
            var standardName = Name.FromMethod(description);

            if (isCached(standardName))
            {
                //is cached, we don't want to process description in hosted loader
                return standardName;
            }

            return _currentLoader.ResolveCallName(description);
        }

        public IInstructionGenerator GetGenerator(VersionedName methodName)
        {
            //TODO caching services

            CachedGenerator generator;
            if (_directCalls.TryGetValue(methodName, out generator))
            {
                return generator;
            }

            return _currentLoader.GetGenerator(methodName);
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
                    var call = generateDirectCall(method);
                    var methodName = Name.FromMethod(method);
                    _directCalls.Add(methodName, new CachedGenerator(call));
                }
            }
        }

        private TypeDescription resolveDirectType(Type type)
        {
            //TODO 
            return new TypeDescription(type.FullName);
        }

        private InstanceFunction generateDirectCall(MethodInfo method)
        {
            var returnTypeDescription = resolveDirectType(method.ReturnType);
            return (args) =>
            {
                
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
                return createDirectInstance(returnTypeDescription, returnValue);
            };
        }

        #endregion

        #region Direct type services

        private Instance createDirectInstance(TypeDescription type, object directValue)
        {
            //TODO proper type resolving
            return new Instance(directValue);
        }

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

    }
}
