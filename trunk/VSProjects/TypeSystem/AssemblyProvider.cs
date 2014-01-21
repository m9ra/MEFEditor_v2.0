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

    internal delegate void ComponentAdded(ComponentInfo component);

    public abstract class AssemblyProvider
    {
        private TypeServices _services;

        protected event Action OnInitialized;

        internal event ComponentAdded OnComponentAdded;


        internal protected TypeServices TypeServices
        {
            protected get
            {
                if (_services == null)
                    throw new InvalidOperationException("Cannot request services before theire initiliazed");
                return _services;
            }

            set
            {
                if (_services != null)
                    throw new InvalidOperationException("Cannot reset already initialized services");

                _services = value;

                if (OnInitialized != null)
                    OnInitialized();
            }
        }

        #region Template method API

        public abstract GeneratorBase GetMethodGenerator(MethodID method);

        public abstract GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath);

        public abstract SearchIterator CreateRootIterator();

        public abstract MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo);

        public abstract MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath);

        public abstract InheritanceChain GetInheritanceChain(PathInfo typePath);

        #endregion

        protected void ReportInvalidation(MethodID name)
        {
            throw new NotImplementedException();
        }

        protected void AddComponent(ComponentInfo component)
        {
            if (OnComponentAdded != null)
                OnComponentAdded(component);
        }

        protected void AddReference(object obj)
        {
            throw new NotImplementedException();
        }

        protected void RemoveReference(object obj)
        {
            throw new NotImplementedException();
        }

        protected void RemoveComponent()
        {
            throw new NotImplementedException();
        }

        protected void StartTransaction(string transactionDescription)
        {
            // throw new NotImplementedException();
        }
        
        protected void CommitTransaction()
        {
            //  throw new NotImplementedException();
        }

        /// <summary>
        /// Report progress of transaction
        /// </summary>
        /// <param name="progressDescription">Description of transaction progress that can be displayed to user</param>
        protected void ReportProgress(string progressDescription)
        {
        }

        internal void HookChange(ChangeEvent changeEvent)
        {
            throw new NotImplementedException();
        }

        internal void UnloadServices()
        {
            TypeServices = null;
        }
    }
}
