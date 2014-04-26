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

    /// <summary>
    /// When implemented provides assembly services that are used in TypeSystem. Represents assembly form
    /// independant layer that could handle assemblies from different sources in the same way.
    /// </summary>
    public abstract class AssemblyProvider
    {
        /// <summary>
        /// Type services of current provider
        /// </summary>
        private TypeServices _services;

        /// <summary>
        /// Mapping that is used for current provider
        /// </summary>
        private string _fullPathMapping;

        /// <summary>
        /// Key of represented assembly
        /// </summary>
        private object _key;

        /// <summary>
        /// Event fired when type system is properly initialized for current provider.
        /// <see cref="TypeServices"/> are not available before initialization
        /// </summary>
        protected event Action OnTypeSystemInitialized;

        /// <summary>
        /// Event fired whenever new component is added into assembly
        /// </summary>
        internal event ComponentEvent ComponentAdded;

        /// <summary>
        /// Event fired whenever component is removed from assembly
        /// </summary>
        internal event ComponentEvent ComponentRemoved;

        /// <summary>
        /// References of represented assembly
        /// </summary>
        internal ReferencedAssemblies References { get { return _services.References; } }

        /// <summary>
        /// Event fired whenever path mapping for assembly is changed
        /// </summary>
        public AssemblyEvent MappingChanged;

        /// <summary>
        /// Name of provided assembly
        /// </summary>
        public string Name { get { return getAssemblyName(); } }

        /// <summary>
        /// Fullpath representing provided assembly 
        /// </summary>
        public string FullPath { get { return getAssemblyFullPath(); } }

        /// <summary>
        /// Key that was used for assembly loading
        /// </summary>
        public object Key
        {
            get
            {
                if (_key == null)
                    return this;

                return _key;
            }

            internal set
            {
                _key = value;
            }
        }

        /// <summary>
        /// Mapping of fullpath used for provided assembly. Path mapping
        /// is necessary for analzing development configuration: Extending of an Existing Application.
        /// </summary>
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

        /// <summary>
        /// Services exposed by type system for provided assembly. Services also handle assembly references.
        /// In terms of limiting access to not referenced assemblies.
        /// </summary>
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

        /// <summary>
        /// Unload provided assembly
        /// </summary>
        internal void Unload()
        {
            //TODO what to unload
        }

        /// <summary>
        /// Force to load components - suppose that no other components from this assembly are registered.
        /// <remarks>Can be called multiple times when changes in references are registered</remarks>
        /// </summary>
        internal void LoadComponents()
        {
            StartTransaction("Loading components");
            try
            {
                loadComponents();
            }
            finally
            {
                CommitTransaction();
            }
        }

        #region Template method API

        protected abstract string getAssemblyFullPath();

        protected abstract string getAssemblyName();

        /// <summary>
        /// Force to load components - suppose that no other components from this assembly are registered.
        /// <remarks>Can be called multiple times when changes in references are registered</remarks>
        /// </summary>
        protected abstract void loadComponents();

        public abstract GeneratorBase GetMethodGenerator(MethodID method);

        public abstract GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath);

        public abstract SearchIterator CreateRootIterator();

        public abstract MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo);

        public abstract MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath);

        public abstract InheritanceChain GetInheritanceChain(PathInfo typePath);

        #endregion

        #region Reference API

        /// <summary>
        /// Add reference for provided assembly. Referenced assembly is load if needed
        /// </summary>
        /// <param name="reference">Reference representation used for assembly loading</param>
        protected void AddReference(object reference)
        {
            _services.AddReference(reference);
        }

        /// <summary>
        /// Remove reference from provided assembly. Referenced assembly may be unloaded
        /// </summary>
        /// <param name="reference">Reference representation used for assembly unloading</param>
        protected void RemoveReference(object reference)
        {
            _services.RemoveReference(reference);
        }

        #endregion

        #region Component API

        /// <summary>
        /// Report that component has been discovered within provided assembly
        /// </summary>
        /// <param name="component">Discovered component</param>
        protected void ComponentDiscovered(ComponentInfo component)
        {
            var alreadyExists = _services.GetComponentInfo(component.ComponentType)!=null;
            if (alreadyExists)
                return;

            if (ComponentAdded != null)
                ComponentAdded(component);
        }

        /// <summary>
        /// Report that component has been removed from provided assembly
        /// </summary>
        protected void ComponentRemoveDiscovered(string fullname)
        {
            var components = _services.GetComponents(this);
            foreach (var component in components)
            {
                if (component.ComponentType.TypeName == fullname)
                {
                    if (ComponentRemoved != null)
                        ComponentRemoved(component);

                    //only single component could be removed
                    return;
                }
            }
        }

        #endregion

        #region Transaction API

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

        /// <summary>
        /// Commit started transaction. 
        /// </summary>
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

        /// <summary>
        /// TODO Form of invalidation
        /// </summary>
        /// <param name="name"></param>
        protected void ReportInvalidation(string invalidatedNamePrefix)
        {
            //TODO methods invalidation
        }


        #endregion


    }
}
