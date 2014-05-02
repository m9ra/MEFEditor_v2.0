using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TypeSystem;

namespace AssemblyProviders.CILAssembly
{
    /// <summary>
    /// Class used for storing available namespaces of types.
    /// It can be used for determining that any namespace is present within assembly or not
    /// </summary>
    class NamespaceStorage
    {
        HashSet<string> _firstLevelNs = new HashSet<string>();

        public void Insert(string type)
        {
             getDistinguishedPart(type, true);
        }

        public bool CanContains(string path)
        {
            var ns = getDistinguishedPart(path);

            return _firstLevelNs.Contains(ns);
        }

        private string getDistinguishedPart(string path, bool withAdd = false)
        {
            var namespaces = path.Split(new[] { Naming.PathDelimiter }, 3);

            var result = namespaces[0];
            if (namespaces.Length > 1)
            {
                if (withAdd)
                {
                    _firstLevelNs.Add(namespaces[0]);
                }

                result = namespaces[0] + "." + namespaces[1];
            }

            if (withAdd)
            {
                _firstLevelNs.Add(result);
            }

            return result;
        }
    }
}
