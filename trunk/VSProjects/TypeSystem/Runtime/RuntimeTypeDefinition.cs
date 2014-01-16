using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

using Analyzing.Editing;
using Analyzing.Editing.Transformations;

using TypeSystem.DrawingServices;
using TypeSystem.Runtime.Building;

using Drawing;

namespace TypeSystem.Runtime
{

    public delegate string NameProvider(RuntimeTypeDefinition definition, CallContext context);

    public delegate object[] ArgumentsProvider(ExecutionView view);

    /// <summary>
    /// Base class for runtime type definitions
    /// <remarks>Its used for defining analyzing types</remarks>
    /// </summary>
    public abstract class RuntimeTypeDefinition
    {
        /// <summary>
        /// Determine that defined type is generic
        /// </summary>
        public bool IsGeneric;

        /// <summary>
        /// Determine that defined type is interface
        /// </summary>
        public bool IsInterface;

        internal IEnumerable<Edit> StaticEdits { get { return _staticEdits; } }

        /// <summary>
        /// Available type services
        /// </summary>
        protected TypeServices Services { get; private set; }

        protected EditsProvider Edits { get { return Context.Edits; } }

        /// <summary>
        /// Component info of type (null if type is not a component)
        /// </summary>
        internal protected ComponentInfo ComponentInfo { get; protected set; }

        /// <summary>
        /// Assembly where type builded from this definition is present
        /// </summary>
        internal protected RuntimeAssembly ContainingAssembly { get; private set; }

        /// <summary>
        /// Context available for currently invoked call (Null, when no call is invoked)
        /// </summary>
        internal protected AnalyzingContext Context { get; private set; }

        /// <summary>
        /// Arguments available for currently invoked call
        /// </summary>
        internal protected Instance[] CurrentArguments { get { return Context.CurrentArguments; } }

        internal protected Instance This { get; private set; }

        abstract public  TypeDescriptor TypeInfo { get; }

        private List<Edit> _staticEdits = new List<Edit>();

        abstract internal IEnumerable<RuntimeMethodGenerator> GetMethods();

        abstract internal IEnumerable<InheritanceChain> GetSubChains();

        protected TypeDescriptor GetTypeInfo()
        {
            return TypeInfo;
        }

        protected virtual void draw(InstanceDrawer drawer)
        {
            //by default there is nothing to do
        }

        internal void Initialize(RuntimeAssembly containingAssembly, TypeServices typeServices)
        {
            if (containingAssembly == null)
                throw new ArgumentNullException("runtimeAssembly");

            if (typeServices == null)
                throw new ArgumentNullException("typeServices");

            Services = typeServices;
            ContainingAssembly = containingAssembly;
        }

        /// <summary>
        /// Unwrap given instance into type T
        /// <remarks>Is called from code emitted by expression tree</remarks>
        /// </summary>
        /// <typeparam name="T">Type to which instance will be unwrapped</typeparam>
        /// <param name="instance">Unwrapped instance</param>
        /// <returns>Unwrapped data</returns>
        internal protected virtual T Unwrap<T>(Instance instance)
        {
            if (typeof(T).IsArray)
            {
                var arrayDef = instance.DirectValue as Array<InstanceWrap>;
                return arrayDef.Unwrap<T>();
            }
            else
            {
                return (T)instance.DirectValue;
            }
        }

        /// <summary>
        /// Wrap given data of type T into instance
        /// <remarks>Is called from code emitted by expression tree</remarks>
        /// </summary>
        /// <typeparam name="T">Type from which instance will be wrapped</typeparam>
        /// <param name="context">Data to be wrapped</param>
        /// <returns>Instance wrapping given data</returns>
        internal protected virtual Instance Wrap<T>(AnalyzingContext context, T data)
        {
            var machine = context.Machine;
            if (typeof(T).IsArray)
            {
                var array = new Array<InstanceWrap>((System.Collections.IEnumerable)data, context);
                return machine.CreateDirectInstance(array, TypeDescriptor.Create<T>());
            }
            else
            {
                return machine.CreateDirectInstance(data);
            }
        }

        internal void Invoke(AnalyzingContext context, DirectMethod methodToInvoke)
        {
            Context = context;

            try
            {
                This = CurrentArguments[0];
                methodToInvoke(context);
            }
            finally
            {
                This = null;
                Context = null;
            }
        }

        internal void RunInContextOf(Instance contextInstance, Action runnedAction)
        {
            var thisSwp = This;

            try
            {
                This = contextInstance;
                runnedAction();
            }
            finally
            {
                This = thisSwp;
            }
        }

        internal void Draw(DrawedInstance toDraw)
        {
            This = toDraw.WrappedInstance;

            try
            {
                draw(toDraw.InstanceDrawer);
            }
            finally
            {
                This = null;
            }
        }

        internal virtual InstanceInfo GetInstanceInfo(Type type)
        {
            //TODO consider generic params
            return TypeDescriptor.Create(type);
        }

        protected TypeAssembly GetCallerAssembly()
        {
            var callerContext = Context.CurrentCall.Caller;
            if (callerContext == null)
                return null;

            var callerId = callerContext.Name;
            var callerAssembly = Services.DefiningAssembly(callerId);
            return callerAssembly;
        }


        protected IEnumerable<InheritanceChain> GetSubChains(Type type)
        {
            yield return ContainingAssembly.GetChain(type.BaseType);

            foreach (var subType in type.GetInterfaces())
            {
                yield return ContainingAssembly.GetChain(subType);
            }
        }

        protected void AsyncCall<TResult>(Instance calledObject, string callName, Action<TResult> callback = null, params Instance[] passedArgs)
        {
            var searcher = Services.CreateSearcher();
            searcher.ExtendName(calledObject.Info.TypeName);

            searcher.Dispatch(callName);

            if (!searcher.HasResults)
                throw new KeyNotFoundException("Cannot found method: " + callName + ", on " + calledObject);

            var foundMethods = searcher.FoundResult;
            var matchingMethods = (from method in foundMethods where method.Parameters.Length == passedArgs.Length select method).ToArray();

            if (matchingMethods.Length > 1)
                throw new NotSupportedException("Cannot process async call on ambiguous method: " + callName + ", on" + calledObject);


            var edits = Edits;

            var callGenerator = new DirectedGenerator((e) =>
            {
                var thisArg = e.GetTemporaryVariable();

                var argVars = new List<string>();
                foreach (var passedArg in passedArgs)
                {
                    var argVar = e.GetTemporaryVariable();

                    e.AssignInstance(argVar, passedArg, passedArg.Info);
                    argVars.Add(argVar);
                }

                e.AssignArgument(thisArg, calledObject.Info, 1);
                e.Call(matchingMethods[0].MethodID, thisArg, Arguments.Values(argVars));

                if (callback != null)
                {
                    var callReturn = e.GetTemporaryVariable();
                    e.AssignReturnValue(callReturn, TypeDescriptor.Create<object>());

                    e.DirectInvoke((context) =>
                    {
                        var callValue = context.GetValue(new VariableName(callReturn));
                        var unwrapped = Unwrap<TResult>(callValue);

                        context.ShareEdits(edits);
                        Invoke(context, (c) => callback(unwrapped));
                    });
                }
            });


            Context.DynamicCall(callName, callGenerator, This, calledObject);
        }

        protected void ContinuationCall(DirectMethod callback)
        {
            var callGenerator = new DirectedGenerator((e) =>
            {
                e.DirectInvoke((c) =>
                    Invoke(c, callback)
                    );
            });

            Context.DynamicCall("DirectAsyncCall", callGenerator, This);
        }

        protected Edit AddCreationEdit(string editName, NameProvider variableNameProvider, ArgumentsProvider argumentsProvider = null)
        {
            var creationTransformation = new AddCallTransformation((v) =>
            {
                var entryContext = v.EntryBlock.Call;
                var name = variableNameProvider(this, entryContext);

                var args = argumentsProvider == null ? null : argumentsProvider(v);

                if (v.IsAborted)
                {
                    return null;
                }

                if (args == null)
                {
                    args = new object[0];
                }

                if (name == null)
                {
                    v.Abort("Name hasn't been selected");
                    return null;
                }

                var call = new CallEditInfo(TypeInfo.TypeName, Naming.CtorName, args);
                call.ReturnName = name;
                return call;
            });

            var edit = new Edit(null, editName, creationTransformation);
            _staticEdits.Add(edit);

            return edit;
        }

        protected Edit AddCallEdit(string name, CallProvider accepter)
        {
            return Edits.AddCall(This, name, accepter);
        }

        protected void RewriteArg(int argIndex, string editName, ValueProvider valueProvider)
        {
            Edits.ChangeArgument(This, argIndex, editName, valueProvider);
        }

        protected void AppendArg(int argIndex, string editName, ValueProvider valueProvider)
        {
            Edits.AppendArgument(CurrentArguments[0], argIndex, editName, valueProvider);
        }

        internal EditViewBase RunEdit(Instance thisObj, Edit edit, EditView view)
        {
            try
            {
                This = thisObj;
                return view.Apply(edit.Transformation);
            }
            finally
            {
                This = null;
            }
        }
    }
}
