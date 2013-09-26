using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem.Core;

namespace TypeSystem
{
    internal delegate void ChangeEvent(MethodID name);

    internal delegate void ComponentAdded(InstanceInfo type, ComponentInfo component);

    public abstract class AssemblyProvider
    {
        internal event ComponentAdded OnComponentAdded;

        #region Template method API

        protected TypeServices TypeServices { get; private set; }

        public abstract GeneratorBase GetMethodGenerator(MethodID method);

        public abstract GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath);

        public abstract SearchIterator CreateRootIterator();

        public abstract MethodID GetImplementation(MethodID method, InstanceInfo dynamicInfo);

        public abstract MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath);

        public abstract InheritanceChain GetInheritanceChain(PathInfo typePath);

        #endregion

        protected void ReportInvalidation(MethodID name)
        {
            throw new NotImplementedException();
        }

        protected void AddComponent(InstanceInfo type, ComponentInfo component)
        {
            if (OnComponentAdded != null)
                OnComponentAdded(type, component);
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
