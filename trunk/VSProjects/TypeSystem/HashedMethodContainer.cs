using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using Utilities;

namespace TypeSystem
{
    public class HashedMethodContainer
    {
        readonly private MultiDictionary<string, MethodItem> _methodPaths = new MultiDictionary<string, MethodItem>();
        readonly private Dictionary<MethodID, MethodItem> _methodIds = new Dictionary<MethodID, MethodItem>();

        readonly private Dictionary<Tuple<InstanceInfo, MethodID>, MethodID> _explicitImplementations = new Dictionary<Tuple<InstanceInfo, MethodID>, MethodID>();
        readonly private Dictionary<Tuple<string, string>, MethodItem> _genericImplementations = new Dictionary<Tuple<string, string>, MethodItem>();

        public void AddItem(MethodItem item, IEnumerable<InstanceInfo> implementedTypes)
        {
            registerImplementedMethods(item, implementedTypes);

            //TODO better id's to avoid loosing methods
            if (!_methodIds.ContainsKey(item.Info.MethodID))
                _methodIds.Add(item.Info.MethodID, item);

            _methodPaths.Add(item.Info.Path.Signature, item);
        }

        private void registerImplementedMethods(MethodItem item, IEnumerable<InstanceInfo> implementedTypes)
        {
            foreach (var implementedType in implementedTypes)
            {
                var implementingType = item.Info.DeclaringType;
                var implementingMethodID = item.Info.MethodID;

                if (!item.Info.HasGenericParameters)
                {
                    var implementedMethod = Naming.ChangeDeclaringType(implementedType.TypeName, implementingMethodID, true);
                    var implementsEntry = Tuple.Create(implementingType, implementedMethod);
                    _explicitImplementations.Add(implementsEntry, implementingMethodID);
                }
                else
                {
                    //TODO parse out only real generic parameters
                    var implementingPath = new PathInfo(implementingType.TypeName);                    
                    var genericImplementedMethod = Naming.ChangeDeclaringType(implementedType.TypeName, implementingMethodID, true);
                    var implementedMethodPath = Naming.GetMethodPath(genericImplementedMethod);


                    var genericImplementsEntry = Tuple.Create(implementingPath.Signature, implementedMethodPath.Signature);
                    _genericImplementations.Add(genericImplementsEntry, item);
                }
            }
        }

        public IEnumerable<TypeMethodInfo> AccordingPath(PathInfo path)
        {
            var overloads = from overload in accordingPath(path) select overload.Info;
            return overloads;
        }

        public GeneratorBase AccordingId(MethodID method)
        {
            MethodItem item;
            if (_methodIds.TryGetValue(method, out item))
            {
                if (item.Info.HasGenericParameters)
                {
                    throw new NotSupportedException("Cannot get method with generic parameters");
                }

                return item.Generator;
            }

            //method hasn't been found
            return null;
        }

        public GeneratorBase AccordingGenericId(MethodID method, PathInfo searchPath)
        {
            var overloads = accordingPath(searchPath);
            foreach (var overload in overloads)
            {
                if (overload.Info.MethodID.Equals(method))
                {
                    return overload.Generator;
                }
            }

            return null;
        }

        public MethodID GetImplementation(MethodID method, InstanceInfo dynamicInfo)
        {
            var implementationEntry = Tuple.Create(dynamicInfo, method);

            MethodID implementation;
            _explicitImplementations.TryGetValue(implementationEntry, out implementation);

            return implementation;
        }

        public MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath)
        {
            //TODO: throw new NotImplementedException();
            var implementationEntry = Tuple.Create(implementingTypePath.Signature, methodSearchPath.Signature);

            MethodItem implementation;
            if (!_genericImplementations.TryGetValue(implementationEntry, out implementation))
                //implementation not found
                return null;

            var implementingMethod = Naming.ChangeDeclaringType(implementingTypePath.Name, methodID,false);
            var implementingMethodPath = Naming.GetMethodPath(implementingMethod);
            var genericImplementation = implementation.MethodProvider(implementingMethodPath, implementation.Info);

            return genericImplementation.Info.MethodID;
        }

        private IEnumerable<MethodItem> accordingPath(PathInfo path)
        {
            var methods = _methodPaths.Get(path.Signature);
            foreach (var methodItem in methods)
            {
                if (methodItem.Info.HasGenericParameters)
                {
                    yield return methodItem.MethodProvider(path, methodItem.Info);
                }
                else
                {
                    yield return methodItem;
                }
            }
        }

        public KeyValuePair<MethodID, MethodItem>[] MethodItems
        {
            get
            {
                return _methodIds.ToArray();
            }
        }
    }
}
