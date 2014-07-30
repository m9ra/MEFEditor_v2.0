using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

using MEFEditor.TypeSystem.Core;
using MEFEditor.TypeSystem.Transactions;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Event fired for assembly action.
    /// </summary>
    /// <param name="assembly">Assembly causing event firing</param>
    public delegate void AssemblyEvent(AssemblyProvider assembly);

    /// <summary>
    /// Event fired for method action.
    /// </summary>
    /// <param name="name">The method name.</param>
    internal delegate void MethodEvent(MethodID name);

    /// <summary>
    /// When implemented provides assembly services that are used in <see cref="MEFEditor.TypeSystem" />. It is supposed
    /// to represent assembly to be usable by type system.
    /// </summary>
    public abstract class AssemblyProvider
    {
        /// <summary>
        /// Type services of current provider.
        /// </summary>
        private TypeServices _services;

        /// <summary>
        /// Mapping that is used for current provider.
        /// </summary>
        private string _fullPathMapping;

        /// <summary>
        /// Cache for fullpath of assembly.
        /// </summary>
        private string _fullPath;

        /// <summary>
        /// Cache for name of assembly.
        /// </summary>
        private string _name;

        /// <summary>
        /// Key of represented assembly.
        /// </summary>
        private object _key;

        /// <summary>
        /// Determine that assembly has been initialized.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Stack of running transactions.
        /// </summary>
        private Stack<Transaction> _transactions = new Stack<Transaction>();

        /// <summary>
        /// Event fired when type system is properly initialized for current provider.
        /// <see cref="TypeServices" /> are not available before initialization
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
        /// References of represented assembly.
        /// </summary>
        /// <value>The references.</value>
        internal ReferencedAssemblies References { get { return _services.References; } }

        /// <summary>
        /// Event fired whenever path mapping for assembly is changed.
        /// </summary>
        public AssemblyEvent MappingChanged;

        /// <summary>
        /// Name of provided assembly.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get { return _name == null ? _name = getAssemblyName() : _name; } }

        /// <summary>
        /// Fullpath representing provided assembly.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath { get { return _fullPath == null ? _fullPath = getAssemblyFullPath() : _fullPath; } }

        /// <summary>
        /// Key that was used for assembly loading.
        /// </summary>
        /// <value>The key.</value>
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
        /// is necessary for analyzing development configuration: Extending of an Existing Application.
        /// </summary>
        /// <value>The full path mapping.</value>
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
        /// In terms of limiting access to only referenced assemblies.
        /// </summary>
        /// <value>The type services.</value>
        /// <exception cref="System.InvalidOperationException">
        /// Cannot request services before they are initialized
        /// or
        /// Cannot reset already initialized services
        /// </exception>
        internal protected TypeServices TypeServices
        {
            get
            {
                if (_services == null)
                    throw new InvalidOperationException("Cannot request services before they are initialized");

                return _services;
            }

            internal set
            {
                if (_services != null)
                    throw new InvalidOperationException("Cannot reset already initialized services");

                _services = value;

                //default path mapping
                FullPathMapping = FullPath;
            }
        }

        /// <summary>
        /// Here are managed all <see cref="Transaction" /> objects.
        /// </summary>
        /// <value>The transactions.</value>
        protected TransactionManager Transactions { get { return _services.Transactions; } }

        /// <summary>
        /// Unload provided assembly.
        /// </summary>
        internal protected virtual void Unload()
        {
            //by default there is nothing to do
        }

        internal void InitializeAssembly()
        {
            if (_isInitialized)
                //assembly is already initialized
                return;

            if (TypeServices == null)
                throw new NotSupportedException("Cannot initialize assembly where TypeServices are not available");

            _isInitialized = true;
            if (OnTypeSystemInitialized != null)
                OnTypeSystemInitialized();
        }

        /// <summary>
        /// Force to load components - suppose that no other components from this assembly are registered.
        /// <remarks>Can be called multiple times when changes in references are registered</remarks>.
        /// </summary>
        internal void LoadComponents()
        {
            StartTransaction(Name + " is loading components");
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

        /// <summary>
        /// Gets the assembly full path.
        /// </summary>
        /// <returns>Assembly full path.</returns>
        protected abstract string getAssemblyFullPath();

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <returns>Assembly name.</returns>
        protected abstract string getAssemblyName();

        /// <summary>
        /// Force to load components - suppose that no other components from this assembly are registered.
        /// <remarks>Can be called multiple times when changes in references are registered</remarks>.
        /// </summary>
        protected abstract void loadComponents();

        /// <summary>
        /// Gets the method generator for given method identifier.
        /// For performance purposes no generic search has to be done.
        /// </summary>
        /// <param name="method">The method identifier.</param>
        /// <returns>GeneratorBase.</returns>
        public abstract GeneratorBase GetMethodGenerator(MethodID method);

        /// <summary>
        /// Gets the generic method generator for given method identifier.
        /// Generic has to be resolved according to given search path.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="searchPath">The search path.</param>
        /// <returns>GeneratorBase.</returns>
        public abstract GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath);

        /// <summary>
        /// Creates the root iterator. That is used for
        /// searching method definitions.
        /// </summary>
        /// <returns>SearchIterator.</returns>
        public abstract SearchIterator CreateRootIterator();

        /// <summary>
        /// Gets identifier of implementing method for given abstract method.
        /// </summary>
        /// <param name="method">The abstract method identifier.</param>
        /// <param name="dynamicInfo">The dynamic information.</param>
        /// <param name="alternativeImplementer">The alternative implementer which can define requested method.</param>
        /// <returns>Identifier of implementing method.</returns>
        public abstract MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo, out TypeDescriptor alternativeImplementer);

        /// <summary>
        /// Gets identifier of implementing method for given abstract method.
        /// </summary>
        /// <param name="methodID">The abstract method identifier.</param>
        /// <param name="methodSearchPath">The method search path.</param>
        /// <param name="implementingTypePath">The implementing type path.</param>
        /// <param name="alternativeImplementer">The alternative implementer which can define requested method.</param>
        /// <returns>Identifier of implementing method.</returns>
        public abstract MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath, out PathInfo alternativeImplementer);

        /// <summary>
        /// Gets inheritance chain for type described by given path.
        /// </summary>
        /// <param name="typePath">The type path.</param>
        /// <returns>InheritanceChain.</returns>
        public abstract InheritanceChain GetInheritanceChain(PathInfo typePath);

        #endregion

        #region Reference API

        /// <summary>
        /// Add reference for provided assembly. Referenced assembly is load if needed.
        /// </summary>
        /// <param name="reference">Reference representation used for assembly loading.</param>
        protected void AddReference(object reference)
        {
            _services.AddReference(reference);
        }

        /// <summary>
        /// Remove reference from provided assembly. Referenced assembly may be unloaded.
        /// </summary>
        /// <param name="reference">Reference representation used for assembly unloading.</param>
        protected void RemoveReference(object reference)
        {
            _services.RemoveReference(reference);
        }

        #endregion

        #region Component API

        /// <summary>
        /// Report that component has been discovered within provided assembly.
        /// </summary>
        /// <param name="component">Discovered component.</param>
        protected void ComponentDiscovered(ComponentInfo component)
        {
            if (ComponentAdded != null)
                ComponentAdded(component);
        }

        /// <summary>
        /// Report that component has been removed from provided assembly.
        /// </summary>
        /// <param name="fullname">The fullname.</param>
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
        /// <param name="transactionDescription">Description of started transaction.</param>
        protected void StartTransaction(string transactionDescription)
        {
            var transaction = _services.Transactions.StartNew(transactionDescription);
            _transactions.Push(transaction);
        }

        /// <summary>
        /// Commit started transaction.
        /// </summary>
        protected void CommitTransaction()
        {
            if (_transactions.Count > 0)
                _transactions.Pop().Commit();
        }

        /// <summary>
        /// Report progress of transaction.
        /// </summary>
        /// <param name="progressDescription">Description of transaction progress that can be displayed to user.</param>
        protected void ReportProgress(string progressDescription)
        {
            if (Transactions.CurrentTransaction != null)
                Transactions.CurrentTransaction.ReportProgress(progressDescription);
        }

        /// <summary>
        /// Invalidate all methods/types/begining with given prefix from cache.
        /// </summary>
        /// <param name="invalidatedNamePrefix">Prefix used for method invalidation.</param>
        protected void ReportInvalidation(string invalidatedNamePrefix)
        {
            _services.Invalidate(invalidatedNamePrefix);
        }

        /// <summary>
        /// Report that whole assembly is invalid.
        /// </summary>
        protected void ReportAssemblyInvalidation()
        {
            _services.InvalidateAssembly(this);
        }

        #endregion
    }
}
