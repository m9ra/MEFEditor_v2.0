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
        readonly private MultiDictionary<string, TypeMethodInfo> _methodPaths = new MultiDictionary<string, TypeMethodInfo>();
        readonly private Dictionary<MethodID, MethodItem> _methodIds = new Dictionary<MethodID, MethodItem>();

        public void AddItem(MethodItem item)
        {
            //TODO better id's to avoid loosing methods
            if (!_methodIds.ContainsKey(item.Info.MethodID))
                _methodIds.Add(item.Info.MethodID, item);

            _methodPaths.Add(item.Info.Path, item.Info);
        }

        public IEnumerable<TypeMethodInfo> AccordingPath(string path)
        {
            return _methodPaths.GetExports(path);
        }

        public GeneratorBase AccordingId(MethodID method)
        {
            MethodItem item;
            if (_methodIds.TryGetValue(method, out item))
            {
                return item.Generator;
            }

            return null;
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
