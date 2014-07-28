using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utilities;
using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;
using MEFEditor.TypeSystem;

namespace RecommendedExtensions.Core.TypeDefinitions.CompositionEngine
{

    /// <summary>
    /// Delegate for direct methods used during composition.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="arguments">The arguments method arguments.</param>
    /// <returns>Return value of direct method.</returns>
    internal delegate Instance DirectCompositionMethod(AnalyzingContext context, Instance[] arguments);
    
    /// <summary>
    /// Delegate for direct methods used during composition.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="arguments">The arguments method arguments.</param>
    internal delegate void DirectContextMethod(AnalyzingContext context, Instance[] arguments);

    /// <summary>
    /// Context of composition provides access to type and emitting services.
    /// </summary>
    public class CompositionContext
    {
        /// <summary>
        /// The services from <see cref="MEFEditor.TypeSystem"/>.
        /// </summary>
        private readonly TypeServices _services;

        /// <summary>
        /// The analysis context.
        /// </summary>
        private readonly AnalyzingContext _context;

        /// <summary>
        /// The component references.
        /// </summary>
        private readonly HashSet<ComponentRef> _componentRefs = new HashSet<ComponentRef>();

        /// <summary>
        /// The input instances.
        /// </summary>
        private readonly List<Instance> _inputInstances = new List<Instance>();

        /// <summary>
        /// The instance reference storages.
        /// </summary>
        private readonly Dictionary<InstanceRef, string> _instanceStorages = new Dictionary<InstanceRef, string>();

        /// <summary>
        /// The generator of composition method.
        /// </summary>
        internal readonly CompositionGenerator Generator = new CompositionGenerator();

        /// <summary>
        /// Gets the components.
        /// </summary>
        /// <value>The components.</value>
        internal IEnumerable<ComponentRef> Components { get { return _componentRefs; } }

        /// <summary>
        /// Gets the input instances.
        /// </summary>
        /// <value>The input instances.</value>
        public Instance[] InputInstances { get { return _inputInstances.ToArray(); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionContext" /> class.
        /// </summary>
        /// <param name="services">The services from <see cref="MEFEditor.TypeSystem"/>.</param>
        /// <param name="context">The context of analysis.</param>
        internal CompositionContext(TypeServices services, AnalyzingContext context)
        {
            _services = services;
            _context = context;
        }

        /// <summary>
        /// Adds the constructed components to the context.
        /// </summary>
        /// <param name="components">The constructed components.</param>
        internal void AddConstructedComponents(IEnumerable<Instance> components)
        {
            if (components == null)
                return;

            foreach (var component in components)
            {
                var info = _services.GetComponentInfo(component.Info);
                var componentRef = new ComponentRef(this, true, info, component);

                addInputComponent(component, componentRef);
            }
        }

        /// <summary>
        /// Adds the not constructed components to context.
        /// </summary>
        /// <param name="components">The not constructed components.</param>
        internal void AddNotConstructedComponents(IEnumerable<Instance> components)
        {
            if (components == null)
                return;

            foreach (var component in components)
            {
                var info = _services.GetComponentInfo(component.Info);
                var componentRef = new ComponentRef(this, false, info, component);

                addInputComponent(component, componentRef);
            }
        }

        /// <summary>
        /// Add component which will be available at given component reference.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="componentRef">The component reference.</param>
        private void addInputComponent(Instance component, ComponentRef componentRef)
        {
            var argumentIndex = _inputInstances.Count;

            _inputInstances.Add(component);

            var storage = string.Format("$arg_{0}", argumentIndex);
            emit((e) => e.AssignArgument(storage, component.Info, (uint)argumentIndex));

            _instanceStorages.Add(componentRef, storage);
            _componentRefs.Add(componentRef);
        }

        /// <summary>
        /// Determine that testedType is type(C# is operator analogy 'type is testedType').
        /// </summary>
        /// <param name="testedType">Type of the tested.</param>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if [is of type] [the specified tested type]; otherwise, <c>false</c>.</returns>
        internal bool IsOfType(InstanceInfo testedType, string type)
        {
            return _services.IsAssignable(type, testedType.TypeName);
        }

        /// <summary>
        /// Determines whether tested type is of the given type.
        /// </summary>
        /// <param name="testedtype">The tested type.</param>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if tested type is of the given type; otherwise, <c>false</c>.</returns>
        internal bool IsOfType(InstanceInfo testedtype, InstanceInfo type)
        {
            return IsOfType(testedtype, type.TypeName);
        }

        /// <summary>
        /// Creates the array that will contains given instances.
        /// </summary>
        /// <param name="itemType">Type of the array item.</param>
        /// <param name="instances">The instances.</param>
        /// <returns>Instance referene with created array.</returns>
        internal InstanceRef CreateArray(TypeDescriptor itemType, IEnumerable<InstanceRef> instances)
        {
            var instArray = instances.ToArray();

            var arrayInfo = TypeDescriptor.Create(string.Format("Array<{0},1>", itemType.TypeName));
            var intParam = ParameterTypeInfo.Create("p", TypeDescriptor.Create<int>());
            var ctorID = Naming.Method(arrayInfo, Naming.CtorName, false, intParam);
            var setID = Naming.Method(arrayInfo, "set_Item", false, intParam, ParameterTypeInfo.Create("p2", itemType));

            var arrayStorage = getFreeStorage("arr");

            emit((e) =>
            {
                //array construction
                e.AssignNewObject(arrayStorage, arrayInfo);
                var lengthVar = e.GetTemporaryVariable("len");
                e.AssignLiteral(lengthVar, instArray.Length);
                e.Call(ctorID, arrayStorage, Arguments.Values(lengthVar));

                //set instances to appropriate indexes
                var arrIndex = e.GetTemporaryVariable("set");
                for (int i = 0; i < instArray.Length; ++i)
                {
                    var instStorage = getStorage(instArray[i]);
                    e.AssignLiteral(arrIndex, i);
                    e.Call(setID, arrayStorage, Arguments.Values(arrIndex, instStorage));
                }
            });

            var array = new InstanceRef(this, arrayInfo, true);
            _instanceStorages[array] = arrayStorage;

            return array;
        }


        /// <summary>
        /// Gets the method overloads on given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>IEnumerable&lt;TypeMethodInfo&gt;.</returns>
        internal IEnumerable<TypeMethodInfo> GetOverloads(InstanceInfo type, string methodName = null)
        {
            var searcher = _services.CreateSearcher();
            searcher.SetCalledObject(type);

            //for getting all methods we use null constraint
            searcher.Dispatch(methodName);

            return searcher.FoundResult;
        }

        /// <summary>
        /// Gets the method on given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>TypeMethodInfo.</returns>
        /// <exception cref="System.NotSupportedException">Cannot get  method</exception>
        internal TypeMethodInfo GetMethod(InstanceInfo type, string methodName)
        {
            var method = TryGetMethod(type, methodName);
            if (method == null)
                throw new NotSupportedException("Cannot get " + methodName + " method for " + type.TypeName);

            return method;
        }

        /// <summary>
        /// Tries the get method.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>TypeMethodInfo.</returns>
        internal TypeMethodInfo TryGetMethod(InstanceInfo type, string methodName)
        {
            var overloads = GetOverloads(type, methodName);
            if (overloads.Count() != 1)
            {
                return null;
            }

            return overloads.First();
        }

        /// <summary>
        /// Calls the specified called instance.
        /// </summary>
        /// <param name="calledInstance">The called instance.</param>
        /// <param name="methodID">The method identifier.</param>
        /// <param name="arguments">The arguments.</param>
        internal void Call(InstanceRef calledInstance, MethodID methodID, InstanceRef[] arguments)
        {
            checkNull(methodID);
            var inst = getStorage(calledInstance);
            var args = getArgumentStorages(arguments);

            emit((e) => e.Call(methodID, inst, args));
        }

        /// <summary>
        /// Calls the with return.
        /// </summary>
        /// <param name="calledInstance">The called instance.</param>
        /// <param name="methodID">The method identifier.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>InstanceRef.</returns>
        internal InstanceRef CallWithReturn(InstanceRef calledInstance, MethodID methodID, InstanceRef[] arguments)
        {
            checkNull(methodID);
            var inst = getStorage(calledInstance);
            var args = getArgumentStorages(arguments);


            var resultStorage = getFreeStorage("ret");
            //TODO determine result type
            var resultInstance = new InstanceRef(this, null, true);

            _instanceStorages.Add(resultInstance, resultStorage);

            emit((e) =>
            {
                e.Call(methodID, inst, args);
                e.AssignReturnValue(resultStorage, resultInstance.Type);
            });

            return resultInstance;
        }


        /// <summary>
        /// Calls the direct.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="argumentInstances">The argument instances.</param>
        internal void CallDirect(DirectContextMethod method, params InstanceRef[] argumentInstances)
        {
            var argumentStorages = from argumentInstance in argumentInstances select new VariableName(getStorage(argumentInstance));

            //emitting routine
            emit((e) =>
            {
                //wrap method into direct invoke instruction
                e.DirectInvoke((c) =>
                {
                    //fetch arguments
                    var argumentValues = new List<Instance>();
                    foreach (var argumentStorage in argumentStorages)
                    {
                        var argumentValue = c.GetValue(argumentStorage);
                        argumentValues.Add(argumentValue);
                    }

                    method(c, argumentValues.ToArray());
                });
            });
        }


        /// <summary>
        /// Calls the direct with return.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="argumentInstances">The argument instances.</param>
        /// <returns>InstanceRef.</returns>
        internal InstanceRef CallDirectWithReturn(DirectCompositionMethod method, params InstanceRef[] argumentInstances)
        {
            var resultStorage = getFreeStorage("ret");
            //TODO determine result type
            var resultInstance = new InstanceRef(this, null, true);
            _instanceStorages.Add(resultInstance, resultStorage);

            var argumentStorages = from argumentInstance in argumentInstances select new VariableName(getStorage(argumentInstance));

            //emitting routine
            emit((e) =>
            {
                //wrap method into direct invoke instruction
                e.DirectInvoke((c) =>
                {
                    //fetch arguments
                    var argumentValues = new List<Instance>();
                    foreach (var argumentStorage in argumentStorages)
                    {
                        var argumentValue = c.GetValue(argumentStorage);
                        argumentValues.Add(argumentValue);
                    }

                    var result = method(c, argumentValues.ToArray());
                    c.SetValue(new VariableName(resultStorage), result);
                });
            });

            return resultInstance;
        }

        /// <summary>
        /// Registers the call handler.
        /// </summary>
        /// <param name="registeredInstance">The registered instance.</param>
        /// <param name="registeredMethod">The registered method.</param>
        internal void RegisterCallHandler(Instance registeredInstance, DirectMethod registeredMethod)
        {
            _services.RegisterCallHandler(registeredInstance, registeredMethod);
        }


        /// <summary>
        /// Gets the storage.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>System.String.</returns>
        private string getStorage(InstanceRef instance)
        {
            return _instanceStorages[instance];
        }

        /// <summary>
        /// Gets the argument storages.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>Arguments.</returns>
        private Arguments getArgumentStorages(InstanceRef[] arguments)
        {
            var argVars = (from arg in arguments select getStorage(arg)).ToArray();
            return Arguments.Values(argVars);
        }

        /// <summary>
        /// Checks the null.
        /// </summary>
        /// <param name="methodID">The method identifier.</param>
        /// <exception cref="System.ArgumentNullException">methodID</exception>
        private void checkNull(MethodID methodID)
        {
            if (methodID == null)
            {
                throw new ArgumentNullException("methodID");
            }
            //everything is OK
        }

        /// <summary>
        /// Emits the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        private void emit(EmitAction action)
        {
            Generator.EmitAction(action);
        }

        /// <summary>
        /// Gets the free storage.
        /// </summary>
        /// <param name="nameHint">The name hint.</param>
        /// <returns>System.String.</returns>
        private string getFreeStorage(string nameHint)
        {
            //TODO check for presence
            return string.Format("{0}_{1}", nameHint, _instanceStorages.Count);
        }

        /// <summary>
        /// Tries the get implementation.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="abstractMethod">The abstract method.</param>
        /// <returns>MethodID.</returns>
        internal MethodID TryGetImplementation(TypeDescriptor type, MethodID abstractMethod)
        {
            return _services.TryGetImplementation(type, abstractMethod);
        }

        /// <summary>
        /// Determines whether the specified import collection is null.
        /// </summary>
        /// <param name="importCollection">The import collection.</param>
        /// <returns><c>true</c> if the specified import collection is null; otherwise, <c>false</c>.</returns>
        internal bool IsNull(Instance importCollection)
        {
            return _services.IsNull(importCollection);
        }
    }
}
