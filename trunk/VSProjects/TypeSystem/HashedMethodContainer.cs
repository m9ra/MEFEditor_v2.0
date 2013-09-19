﻿using System;
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

        public void AddItem(MethodItem item)
        {
            //TODO better id's to avoid loosing methods
            if (!_methodIds.ContainsKey(item.Info.MethodID))
                _methodIds.Add(item.Info.MethodID, item);

            _methodPaths.Add(item.Info.Path.Signature, item);
        }

        public IEnumerable<TypeMethodInfo> AccordingPath(PathInfo path)
        {
            return from overload in accordingPath(path) select overload.Info;
        }

        public GeneratorBase AccordingId(MethodID method)
        {
            MethodItem item;
            if (_methodIds.TryGetValue(method, out item) || tryGetGeneric(method,out item))
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

        private bool tryGetGeneric(MethodID method, out MethodItem result)
        {
            string path, paramDescr;
            Naming.GetParts(method,out path, out paramDescr);

            var searchPath = new PathInfo(path);
            var overloads=accordingPath(searchPath);
            foreach (var overload in overloads)
            {
                if (overload.Info.MethodID.Equals(method))
                {
                    result = overload;
                    return true;
                }
            }

            result = null;
            return false;
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
