using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem.Core;

namespace TypeSystem
{

    /// <summary>
    /// Event fired for assembly action
    /// </summary>
    /// <param name="assembly">Assembly causing event firing</param>
    public delegate void AssemblyEvent(AssemblyProvider assembly);

    internal delegate void MethodEvent(MethodID name);


    public abstract class AssemblyProvider
    {
        private TypeServices _services;

        private string _fullPathMapping;

        protected event Action OnTypeSystemInitialized;

        internal event ComponentEvent ComponentAdded;

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

                //default path mapping
                FullPathMapping = FullPath;

                if (OnTypeSystemInitialized != null)
                    OnTypeSystemInitialized();
            }
        }

        public AssemblyEvent MappingChanged;

        public string Name { get { return getAssemblyName(); } }

        public string FullPath { get { return getAssemblyFullPath(); } }

        public string FullPathMapping
        {
            get
            {
                return _fullPathMapping;
            }
            set
            {
                if (_fullPathMapping == value)
                    return;

                _fullPathMapping = value;

                if (MappingChanged != null)
                    MappingChanged(this);
            }
        }

        #region Template method API

        protected abstract string getAssemblyFullPath();

        protected abstract string getAssemblyName();

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
            if (ComponentAdded != null)
                ComponentAdded(component);
        }

        protected void AddReference(object obj)
        {
            //throw new NotImplementedException();
        }

        protected void RemoveReference(object obj)
        {
            throw new NotImplementedException();
        }

        protected void RemoveComponent()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Start transaction with given description. Transaction can be reported about progress.
        /// Transaction should end with CommitTransaction. If explicit CommitTransaction is 
        /// not placed, auto commit is processed with parent transactions commit.
        /// </summary>
        /// <param name="transactionDescription">Description of started transaction</param>
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

        internal void HookChange(MethodEvent changeEvent)
        {
            throw new NotImplementedException();
        }

        internal void UnloadServices()
        {
            //TODO what unload
        }
    }
}
