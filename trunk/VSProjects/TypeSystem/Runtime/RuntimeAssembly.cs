﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

using Analyzing;

using TypeSystem.Runtime.Building;

namespace TypeSystem.Runtime
{
    delegate RuntimeMethodGenerator GeneratorProvider(RuntimeTypeDefinition definition, System.Reflection.MethodInfo method, string name);

    public class RuntimeAssembly : AssemblyProvider
    {
        /// <summary>
        /// Registered runtime methods
        /// </summary>
        private readonly HashedMethodContainer _runtimeMethods = new HashedMethodContainer();

        /// <summary>
        /// Registered direct types
        /// </summary>
        private readonly Dictionary<Type, RuntimeTypeDefinition> _directTypes = new Dictionary<Type, RuntimeTypeDefinition>();

        private readonly HashSet<InstanceInfo> _directTypeInfos = new HashSet<InstanceInfo>();

        /// <summary>
        /// Registered data types
        /// </summary>
        private readonly List<DataTypeDefinition> _dataTypes = new List<DataTypeDefinition>();

        private readonly Dictionary<string, GeneratorProvider> _methodGeneratorProviders;

        internal RuntimeAssembly()
        {
            _methodGeneratorProviders = new Dictionary<string, GeneratorProvider>()
            {
                {"_method_",_createMethod},
                {"_get_",_createProperty},
                {"_set_",_createProperty},
            };
        }

        /// <summary>
        /// Add runtime type definition into runtime assembly
        /// </summary>
        /// <param name="definition">Added type definition</param>
        public void AddDefinition(DataTypeDefinition definition)
        {
            _dataTypes.Add(definition);
        }

        public void AddDirectDefinition<T>(DirectTypeDefinition<T> definition)
        {
            _directTypes[typeof(T)] = definition;
            _directTypeInfos.Add(InstanceInfo.Create<T>());
        }

        public void BuildAssembly()
        {
            if (this.TypeServices == null)
            {
                throw new NotSupportedException("Cannot add definitions until type services are initialized");
            }

            foreach (var directType in _directTypes.Values)
            {
                buildDefinition(directType);
            }

            foreach (var dataType in _dataTypes)
            {
                buildDefinition(dataType);
                if (dataType.ComponentInfo != null)
                {
                    AddComponent(dataType.TypeInfo, dataType.ComponentInfo);
                }
            }
        }

        internal bool IsInDirectCover(Type type)
        {
            return _directTypes.ContainsKey(type);
        }

        internal bool IsDirectType(InstanceInfo typeInfo)
        {
            return _directTypeInfos.Contains(typeInfo);
        }

        #region Assembly provider implementation

        public override SearchIterator CreateRootIterator()
        {
            return new HashIterator(_runtimeMethods);
        }

        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            return _runtimeMethods.AccordingId(method);
        }

        #endregion

        #region TypeDefinition building

        /// <summary>
        /// Register given definition into runtime assembly
        /// </summary>
        /// <param name="definition">Registered definition</param>
        private void buildDefinition(RuntimeTypeDefinition definition)
        {
            //every definition needs initialization
            definition.Initialize(this, this.TypeServices);
                        
            //get all methods defined by definition
            var methodGenerators = definition.GetMethods();
            foreach (var generator in methodGenerators)
            {
                var item = new MethodItem(generator, generator.MethodInfo);
                _runtimeMethods.AddItem(item);
            }
        }

        /// <summary>
        /// Get method generators defined in given runtime type definition
        /// </summary>
        /// <param name="definition">Definition which methods are resolved</param>
        /// <returns>Method generators for defined methods</returns>
        internal IEnumerable<RuntimeMethodGenerator> GetMethodGenerators(RuntimeTypeDefinition definition)
        {
            var result = new List<RuntimeMethodGenerator>();
            foreach (var method in definition.GetType().GetMethods())
            {
                var name = method.Name;

                //create method definition according to it's prefix
                foreach (var provider in _methodGeneratorProviders)
                {
                    if (name.StartsWith(provider.Key))
                    {
                        var notPrefixedName = name.Substring(provider.Key.Length);
                        var generator=provider.Value(definition, method, notPrefixedName);
                        result.Add(generator);
                        break;
                    }
                }
            }

            return result;
        }

        #endregion

        #region Runtime method building
        
        /// <summary>
        /// Build method generator from given method info
        /// </summary>
        /// <param name="definition">Type where method is defined</param>
        /// <param name="method">Method info defining method</param>
        /// <param name="methodName">Name of defined method</param>
        /// <returns>Builder where method is builded</returns>
        private MethodBuilder buildMethod(RuntimeTypeDefinition definition, MethodInfo method, string methodName)
        {
            var wrapping = new MethodBuilder(definition, method, methodName);
            foreach (var param in method.GetParameters())
            {
                if (needWrapping(param))
                {
                    wrapping.AddUnwrappedParam(param);
                }
                else
                {
                    wrapping.AddRawParam(param, "TypeName.NotImplemented");
                }
            }

            return wrapping;
        }

        /// <summary>
        /// Determine, that given parameter needs wrapping
        /// </summary>
        /// <param name="par">Parameter which wrapping is determined</param>
        /// <returns>True if parameter needs wrapping, false otherwise</returns>
        private bool needWrapping(System.Reflection.ParameterInfo par)
        {
            var parType = par.ParameterType;
            var instType = typeof(Instance);

            var isInstance=instType == parType;
            var hasInstanceParent=instType.IsSubclassOf(parType);


            return !isInstance && !hasInstanceParent;
        }

        #endregion

        #region Method generator providers

        private RuntimeMethodGenerator _createMethod(RuntimeTypeDefinition definition, System.Reflection.MethodInfo method, string name)
        {
            if (name == "ctor")
            {
                var nameParts = definition.TypeInfo.TypeName.Split('.');
                name = nameParts.Last();
            }
            var builder = buildMethod(definition, method, name);

            return builder.CreateGenerator();
        }

        private RuntimeMethodGenerator _createProperty(RuntimeTypeDefinition definition, System.Reflection.MethodInfo method, string name)
        {
            var builder = buildMethod(definition, method, method.Name.Substring(1));

            return builder.CreateGenerator();
        }

        #endregion

    }
}