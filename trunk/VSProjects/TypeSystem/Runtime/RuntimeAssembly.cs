using System;
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
        private readonly Dictionary<string, MethodItem> _runtimeMethods = new Dictionary<string, MethodItem>();

        private readonly Dictionary<string, GeneratorProvider> _providers;


        public RuntimeAssembly()
        {
            _providers = new Dictionary<string, GeneratorProvider>()
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
        public void AddDefinition(RuntimeTypeDefinition definition)
        {
            if (this.TypeServices == null)
            {
                throw new NotSupportedException("Cannot add definitions until type services are initialized");
            }

            registerDefinition(definition);
        }

        #region Assembly provider implementation


        protected override string resolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            //TODO better method resolving
            var name = Name.From(method, staticArgumentInfo);

            if (_runtimeMethods.ContainsKey(name.Name))
            {
                return name.Name;
            }

            //there is no such a method
            return null;
        }

        protected override GeneratorBase getGenerator(string methodName)
        {
            if (_runtimeMethods.ContainsKey(methodName))
            {
                return _runtimeMethods[methodName].Generator;
            }
            return null;
        }

        public override SearchIterator CreateRootIterator()
        {
            return new HashIterator(_runtimeMethods);
        }

        #endregion

        #region TypeDefinition building

        /// <summary>
        /// Register given definition into runtime assembly
        /// </summary>
        /// <param name="definition">Registered definition</param>
        private void registerDefinition(RuntimeTypeDefinition definition)
        {
            //every definition needs initialization
            definition.Initialize(this, this.TypeServices);

            //TODO inheritance resolving
            //get all methods defined by definition
            var methodGenerators = getMethodGenerators(definition);
            foreach (var generator in methodGenerators)
            {
                var item = new MethodItem(generator, generator.MethodInfo);
                _runtimeMethods.Add(item.Info.Path, item);
            }
        }

        /// <summary>
        /// Get method generators defined in given runtime type definition
        /// </summary>
        /// <param name="definition">Definition which methods are resolved</param>
        /// <returns>Method generators for defined methods</returns>
        private IEnumerable<RuntimeMethodGenerator> getMethodGenerators(RuntimeTypeDefinition definition)
        {
            var result = new List<RuntimeMethodGenerator>();
            foreach (var method in definition.GetType().GetMethods())
            {
                var name = method.Name;

                //create method definition according to it's prefix
                foreach (var provider in _providers)
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
            return !par.GetType().IsSubclassOf(typeof(Instance));
        }

        #endregion

        #region Method generator providers

        private RuntimeMethodGenerator _createMethod(RuntimeTypeDefinition definition, System.Reflection.MethodInfo method, string name)
        {
            if (name == "ctor")
            {
                var nameParts = definition.FullName.Split('.');
                name = nameParts.Last();
            }
            var builder = buildMethod(definition, method, name);

            return builder.CreateGenerator();
        }

        private RuntimeMethodGenerator _createProperty(RuntimeTypeDefinition definition, System.Reflection.MethodInfo method, string name)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
