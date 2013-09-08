using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem.Core;

namespace TypeSystem
{
    internal delegate void ChangeEvent(VersionedName name);

    public abstract class AssemblyProvider
    {
        #region Template method API

        protected TypeServices TypeServices { get; private set; }

        protected abstract string resolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo);

        protected abstract GeneratorBase getGenerator(string methodName);

        public abstract SearchIterator CreateRootIterator();

        #endregion

        protected void ReportInvalidation(VersionedName name)
        {
            throw new NotImplementedException();
        }

        protected void AddComponent()
        {
            throw new NotImplementedException();
        }

        protected void RemoveComponent()
        {
            throw new NotImplementedException();
        }

        protected void StartTransaction()
        {
            throw new NotImplementedException();
        }

        protected void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        internal void HookChange(ChangeEvent changeEvent)
        {
            throw new NotImplementedException();
        }

        internal string ResolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            return resolveMethod(method, staticArgumentInfo);
        }

        internal GeneratorBase GetGenerator(VersionedName methodName)
        {
            return getGenerator(methodName.Name);
        }

        internal void SetServices(TypeServices services)
        {
            if (TypeServices != null)
            {
                throw new NotSupportedException("Cannot set services twice");
            }

            TypeServices = services;
        }

        internal void UnloadServices()
        {
            TypeServices = null;
        }

        
    }
}
