using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;

using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Editing.Transformations;

using MEFEditor.TypeSystem.DrawingServices;
using MEFEditor.TypeSystem.Runtime.Building;

using MEFEditor.Drawing;

namespace MEFEditor.TypeSystem.Runtime
{

    /// <summary>
    /// Delegate for providing name dialog.
    /// </summary>
    /// <param name="definition">The definition which requests name.</param>
    /// <param name="context">The context where names is requested.</param>
    /// <returns>Selected name.</returns>
    public delegate string NameProvider(RuntimeTypeDefinition definition, CallContext context);

    /// <summary>
    /// Delegate for providing argument dialogs.
    /// </summary>
    /// <param name="view">The view where arguments are requested.</param>
    /// <returns>Created arguments.</returns>
    public delegate object[] ArgumentsProvider(ExecutionView view);

    /// <summary>
    /// Base class for runtime type definitions
    /// <remarks>Its used for defining analyzing types</remarks>.
    /// </summary>
    public abstract class RuntimeTypeDefinition
    {
        /// <summary>
        /// Determine that defined type is interface.
        /// </summary>
        public bool IsInterface;

        /// <summary>
        /// Gets or sets the forced sub types.
        /// </summary>
        /// <value>The forced sub types.</value>
        public Type[] ForcedSubTypes { get; set; }
        
        /// <summary>
        /// Gets the static edits.
        /// </summary>
        /// <value>The static edits.</value>
        internal IEnumerable<Edit> StaticEdits { get { return _globalEdits; } }

        /// <summary>
        /// Services exposed for containing assembly by <see cref="MEFEditor.TypeSystem"/>.
        /// </summary>
        /// <value>The services.</value>
        protected TypeServices Services { get; private set; }

        /// <summary>
        /// Gets type services of assembly that is calling
        /// method on current <see cref="RuntimeTypeDefinition"/>.
        /// </summary>
        /// <value>The calling assembly type services.</value>
        protected TypeServices CallingAssemblyServices
        {
            get
            {
                var caller = GetCallerAssembly();
                if (caller == null)
                    return Services;

                return caller.Assembly.TypeServices;
            }
        }

        /// <summary>
        /// Gets the edits.
        /// </summary>
        /// <value>The edits.</value>
        protected EditsProvider Edits { get; private set; }

        /// <summary>
        /// Component info of type (null if type is not a component).
        /// </summary>
        /// <value>The component information.</value>
        internal protected ComponentInfo ComponentInfo { get; protected set; }

        /// <summary>
        /// Assembly where type built from this definition is present.
        /// </summary>
        /// <value>The containing assembly.</value>
        internal protected RuntimeAssembly ContainingAssembly { get; private set; }

        /// <summary>
        /// Context available for currently invoked call (Null, when no call is invoked).
        /// </summary>
        /// <value>The context.</value>
        internal protected AnalyzingContext Context { get; private set; }

        /// <summary>
        /// Arguments available for currently invoked call.
        /// </summary>
        /// <value>The current arguments.</value>
        internal protected Instance[] CurrentArguments { get { return Context.CurrentArguments; } }

        /// <summary>
        /// Gets this object representation.
        /// </summary>
        /// <value>The this.</value>
        internal protected Instance This { get; private set; }
        
        /// <summary>
        /// Gets the type information.
        /// </summary>
        /// <value>The type information.</value>
        abstract public TypeDescriptor TypeInfo { get; }

        /// <summary>
        /// The global edits defined for definition.
        /// </summary>
        private List<Edit> _globalEdits = new List<Edit>();

        /// <summary>
        /// Gets the methods.
        /// </summary>
        /// <returns>IEnumerable&lt;RuntimeMethodGenerator&gt;.</returns>
        abstract internal IEnumerable<RuntimeMethodGenerator> GetMethods();

        /// <summary>
        /// Gets the sub chains.
        /// </summary>
        /// <returns>IEnumerable&lt;InheritanceChain&gt;.</returns>
        abstract internal IEnumerable<InheritanceChain> GetSubChains();

        /// <summary>
        /// Gets the type information.
        /// </summary>
        /// <returns>TypeDescriptor.</returns>
        protected TypeDescriptor GetTypeInfo()
        {
            return TypeInfo;
        }
        
        /// <summary>
        /// Export data from represented <see cref="Instance"/> by using given drawer.
        /// <remarks>Note that only instances which are forced to display are displayed in root of <see cref="DiagramCanvas"/></remarks>
        /// </summary>
        /// <param name="drawer">The drawer.</param>
        protected virtual void draw(InstanceDrawer drawer)
        {
            //by default there is nothing to do
        }

        /// <summary>
        /// Initializes the specified containing assembly.
        /// </summary>
        /// <param name="containingAssembly">The containing assembly.</param>
        /// <param name="typeServices">The type services.</param>
        /// <exception cref="System.ArgumentNullException">
        /// runtimeAssembly
        /// or
        /// typeServices
        /// </exception>
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
        /// <remarks>Is called from code emitted by expression tree</remarks>.
        /// </summary>
        /// <typeparam name="T">Type to which instance will be unwrapped.</typeparam>
        /// <param name="instance">Unwrapped instance.</param>
        /// <returns>Unwrapped data.</returns>
        internal protected virtual T Unwrap<T>(Instance instance)
        {
            var type = typeof(T);
            if (type.IsArray)
            {
                var arrayDef = instance.DirectValue as Array<InstanceWrap>;
                return arrayDef.Unwrap<T>();
            }
            else
            {
                var value = instance.DirectValue;

                if (value != null && type.IsAssignableFrom(value.GetType()))
                    return (T)instance.DirectValue;
                else
                    return default(T);
            }
        }

        /// <summary>
        /// Wrap given data of type T into instance.
        /// <remarks>Is called from code emitted by expression tree</remarks>.
        /// </summary>
        /// <typeparam name="T">Type from which instance will be wrapped.</typeparam>
        /// <param name="context">Data to be wrapped.</param>
        /// <param name="data">The data.</param>
        /// <returns>Instance wrapping given data.</returns>
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

        /// <summary>
        /// Invokes the specified method in given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="methodToInvoke">The method to invoke.</param>
        internal void Invoke(AnalyzingContext context, DirectMethod methodToInvoke)
        {
            Context = context;
            Edits = context.Edits;

            try
            {
                This = CurrentArguments[0];
                methodToInvoke(context);
            }
            finally
            {
                This = null;
                Context = null;
                Edits = null;
            }
        }

        /// <summary>
        /// Runs given action in specified context.
        /// </summary>
        /// <param name="contextInstance">The context instance.</param>
        /// <param name="runnedAction">The runned action.</param>
        /// <param name="editContext">The edit context.</param>
        public void RunInContextOf(Instance contextInstance, Action runnedAction, EditsProvider editContext = null)
        {
            var editsSwp = Edits;
            var thisSwp = This;

            Edits = editContext;
            This = contextInstance;

            try
            {
                runnedAction();
            }
            finally
            {
                Edits = editsSwp;
                This = thisSwp;
            }
        }

        /// <summary>
        /// Export data from represented <see cref="Instance" /> by using given drawer.
        /// <remarks>Note that only instances which are forced to display are displayed in root of <see cref="DiagramCanvas" /></remarks>.
        /// </summary>
        /// <param name="toDraw">Instance to be drawn.</param>
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

        /// <summary>
        /// Gets type descriptor from given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type descriptor.</returns>
        internal virtual InstanceInfo GetInstanceInfo(Type type)
        {
            return TypeDescriptor.Create(type);
        }

        /// <summary>
        /// Gets the caller assembly.
        /// </summary>
        /// <returns>TypeAssembly.</returns>
        protected TypeAssembly GetCallerAssembly()
        {
            var callerContext = Context.CurrentCall.Caller;
            if (callerContext == null)
                return null;

            var callerId = callerContext.Name;
            var callerAssembly = Services.GetDefiningAssembly(callerId);
            return callerAssembly;
        }


        /// <summary>
        /// Gets the sub chains.
        /// </summary>
        /// <param name="type">The type which subchains are requested.</param>
        /// <returns>IEnumerable&lt;InheritanceChain&gt;.</returns>
        protected IEnumerable<InheritanceChain> GetSubChains(Type type)
        {
            if (ForcedSubTypes == null)
            {
                yield return ContainingAssembly.GetChain(type.BaseType);

                foreach (var subType in type.GetInterfaces())
                {
                    yield return ContainingAssembly.GetChain(subType);
                }
            }
            else
            {
                foreach (var forcedSubType in ForcedSubTypes)
                {
                    yield return ContainingAssembly.GetChain(forcedSubType);
                }
            }
        }

        /// <summary>
        /// Make asynchronous call in <see cref="Machine"/> context.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="calledObject">The called object.</param>
        /// <param name="callName">Name of the call.</param>
        /// <param name="callback">The callback called after call is finished.</param>
        /// <param name="passedArgs">The passed arguments.</param>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Cannot found method:  + callName + , on  + calledObject</exception>
        /// <exception cref="System.NotSupportedException">Cannot process async call on ambiguous method:  + callName + , on + calledObject</exception>
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

                        context.InjectEdits(edits);
                        Invoke(context, (c) => callback(unwrapped));
                    });
                }
            });


            Context.DynamicCall(callName, callGenerator, This, calledObject);
        }

        /// <summary>
        /// Push call on stack of <see cref="Machine"/> that is invoked
        /// after all async methods are finished.
        /// </summary>
        /// <param name="callback">The callback called after async methods are finished.</param>
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

        /// <summary>
        /// Adds global edit that will create instance of current type definition.
        /// </summary>
        /// <param name="editName">Name of the edit.</param>
        /// <param name="argumentsProvider">Provider of arguments used in constructor.</param>
        /// <returns>Edit.</returns>
        protected Edit AddCreationEdit(string editName, ArgumentsProvider argumentsProvider = null)
        {
            return AddCreationEdit(editName, Dialogs.VariableName.GetName, argumentsProvider);
        }

        /// <summary>
        /// Adds global edit that will create instance of current type definition.
        /// </summary>
        /// <param name="editName">Name of the edit.</param>
        /// <param name="variableNameProvider">Provider of name of created instance.</param>
        /// <param name="argumentsProvider">Provider of arguments used in constructor.</param>
        /// <returns>Edit.</returns>
        protected Edit AddCreationEdit(string editName, NameProvider variableNameProvider, ArgumentsProvider argumentsProvider = null)
        {
            var creationTransformation = new AddCallTransformation((v) =>
            {
                var entryContext = v.EntryBlock.Call;
                var name = variableNameProvider(this, entryContext);
                if (name == null)
                {
                    v.Abort("Name hasn't been selected");
                    return null;
                }

                var args = argumentsProvider == null ? null : argumentsProvider(v);
                if (args == null)
                    args = new object[0];

                if (v.IsAborted)
                    return null;

                var call = new CallEditInfo(TypeInfo, Naming.CtorName, args);
                call.ReturnName = name;
                return call;
            });

            var edit = new Edit(null, null, null, editName, creationTransformation);
            _globalEdits.Add(edit);

            return edit;
        }

        /// <summary>
        /// Create edit for call creation.
        /// </summary>
        /// <param name="name">The name of edit.</param>
        /// <param name="callProvider">The provider of created call.</param>
        /// <returns>Created edit.</returns>
        protected Edit AddCallEdit(string name, CallProvider callProvider)
        {
            return Edits.AddCall(This, name, callProvider);
        }

        /// <summary>
        /// Create edit that rewrite the argument.
        /// </summary>
        /// <param name="argIndex">Index of the argument.</param>
        /// <param name="editName">Name of the edit.</param>
        /// <param name="valueProvider">The value provider.</param>
        protected void RewriteArg(int argIndex, string editName, ValueProvider valueProvider)
        {
            Edits.ChangeArgument(This, argIndex, editName, valueProvider);
        }

        /// <summary>
        /// Create edit that appends the argument.
        /// </summary>
        /// <param name="argIndex">Index of the argument.</param>
        /// <param name="editName">Name of the edit.</param>
        /// <param name="valueProvider">The value provider.</param>
        protected void AppendArg(int argIndex, string editName, ValueProvider valueProvider)
        {
            Edits.AppendArgument(CurrentArguments[0], argIndex, editName, valueProvider);
        }

        /// <summary>
        /// Create edit that accepts value as last argument.
        /// </summary>
        /// <param name="valueProvider">The value provider.</param>
        protected void AcceptAsLastArgument(ValueProvider valueProvider)
        {
            var index = CurrentArguments.Length;
            if (CurrentArguments.Length > 0)
            {
                var paramArray = CurrentArguments.Last().DirectValue as Array<InstanceWrap>;
                if (paramArray != null)
                    index += paramArray.Length - 1;
            }

            AppendArg(index, UserInteraction.AcceptEditName, valueProvider);
        }

        /// <summary>
        /// Create edit that executes given action.
        /// </summary>
        /// <param name="editName">Name of the edit.</param>
        /// <param name="action">The action.</param>
        protected void AddActionEdit(string editName, Action action)
        {
            Edits.AddEdit(This, editName, new ActionTransformation(action));
        }

    }
}
