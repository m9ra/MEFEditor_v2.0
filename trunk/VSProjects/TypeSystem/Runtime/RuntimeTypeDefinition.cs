using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

using TypeSystem.Runtime.Building;

namespace TypeSystem.Runtime
{
    /// <summary>
    /// Value provider for getting objects for edits
    /// </summary>
    /// <returns></returns>
    public delegate object ValueProvider();

    /// <summary>
    /// Base class for runtime type definitions
    /// <remarks>Its used for defining analyzing types</remarks>
    /// </summary>
    public abstract class RuntimeTypeDefinition
    {
        /// <summary>
        /// Determine that runtime type definition is generic type
        /// </summary>
        public bool IsGeneric;

        /// <summary>
        /// Available type services
        /// </summary>
        protected TypeServices Services { get; private set; }

        /// <summary>
        /// Component info of type (null if type is not a component)
        /// </summary>
        internal protected ComponentInfo ComponentInfo { get; protected set; }

        /// <summary>
        /// Assembly where type builded from this definition is present
        /// </summary>
        protected RuntimeAssembly ContainingAssembly { get; private set; }
         
        /// <summary>
        /// Context available for currently invoked call (Null, when no call is invoked)
        /// </summary>
        internal protected AnalyzingContext Context { get; private set; }

        /// <summary>
        /// Arguments available for currently invoked call
        /// </summary>
        internal protected Instance[] CurrentArguments { get { return Context.CurrentArguments; } }


        abstract internal InstanceInfo TypeInfo{get;}

        protected InstanceInfo GetTypeInfo()
        {
            return TypeInfo;
        }

        abstract internal IEnumerable<RuntimeMethodGenerator> GetMethods();
        
        internal void Initialize(RuntimeAssembly containingAssembly, TypeServices typeServices)
        {
            if (containingAssembly == null)
                throw new ArgumentNullException("runtimeAssembly");

            if (typeServices == null)
                throw new ArgumentNullException("typeServices");

            Services = typeServices;
            ContainingAssembly = containingAssembly;
        }
        
        internal void Invoke(AnalyzingContext context,DirectMethod methodToInvoke)
        {
            Context = context;

            try
            {
                methodToInvoke(context);
            }
            finally
            {
                Context = null;
            }
        }
        
        protected void RewriteArg(int argIndex, string editName, ValueProvider valueProvider)
        {
            throw new NotImplementedException();
        }

        protected void AddArg(int argIndex, string editName, ValueProvider valueProvider)
        {
            throw new NotImplementedException();
        }
    }
}
