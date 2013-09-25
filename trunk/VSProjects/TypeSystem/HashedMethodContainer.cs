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

        public void AddItem(MethodItem item, IEnumerable<InstanceInfo> implementedTypes)
        {
            foreach (var implementedType in implementedTypes)
            {
                var implementedMethod = Naming.ChangeDeclaringType(implementedType, item.Info.MethodID, true);
                var implementsEntry = Tuple.Create(item.Info.DeclaringType, implementedMethod);
                _explicitImplementations.Add(implementsEntry, item.Info.MethodID);
            }

            //TODO better id's to avoid loosing methods
            if (!_methodIds.ContainsKey(item.Info.MethodID))
                _methodIds.Add(item.Info.MethodID, item);

            _methodPaths.Add(item.Info.Path.Signature, item);
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
            MethodID implementation;

            var implementationEntry = Tuple.Create(dynamicInfo, method);
            _explicitImplementations.TryGetValue(implementationEntry, out implementation);

            return implementation;
        }

        public MethodID GetGenericImplementation(MethodID method, PathInfo searchPath, InstanceInfo dynamicInfo)
        {
            //TODO: throw new NotImplementedException();
            return null;
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
