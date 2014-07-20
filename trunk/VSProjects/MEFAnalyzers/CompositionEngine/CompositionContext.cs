using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;
using TypeSystem;
using Utilities;


namespace MEFAnalyzers.CompositionEngine
{

    /// <summary>
    /// Context of composition provides access to type and emitting services
    /// </summary>
    public class CompositionContext
    {
        private readonly TypeServices _services;

        private readonly AnalyzingContext _context;

        private readonly HashSet<ComponentRef> _componentRefs = new HashSet<ComponentRef>();

        private readonly List<Instance> _inputInstances = new List<Instance>();

        private readonly Dictionary<InstanceRef, string> _instanceStorages = new Dictionary<InstanceRef, string>();

        internal readonly CompositionGenerator Generator = new CompositionGenerator();

        internal IEnumerable<ComponentRef> Components { get { return _componentRefs; } }

        public Instance[] InputInstances { get { return _inputInstances.ToArray(); } }

        internal CompositionContext(TypeServices services, AnalyzingContext context)
        {
            _services = services;
            _context = context;
        }

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
        /// Add component which will be available at given argument index in composition call
        /// </summary>
        /// <param name="argumentIndex"></param>
        /// <param name="component"></param>
        /// <param name="componentRef"></param>
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
        /// Determine that testedType is type(C# is operator analogy 'type is testedType')
        /// </summary>
        /// <param name="testedType"></param>
        /// <param name="testedType"></param>
        /// <returns></returns>
        internal bool IsOfType(InstanceInfo testedType, string type)
        {
            return _services.IsAssignable(type, testedType.TypeName);
        }

        internal bool IsOfType(InstanceInfo testedtype, InstanceInfo type)
        {
            return IsOfType(testedtype, type.TypeName);
        }

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
                //array constrution
                e.AssignNewObject(arrayStorage, arrayInfo);
                var lengthVar = e.GetTemporaryVariable("len");
                e.AssignLiteral(lengthVar, instArray.Length);
                e.Call(ctorID, arrayStorage, Arguments.Values(lengthVar));

                //set instances to appropriate indexes
                var arrIndex = e.GetTemporaryVariable("set");
                for (int i = 0; i < instArray.Length; ++i)
                {
                    var instStorage = GetStorage(instArray[i]);
                    e.AssignLiteral(arrIndex, i);
                    e.Call(setID, arrayStorage, Arguments.Values(arrIndex, instStorage));
                }
            });

            var array = new InstanceRef(this, arrayInfo, true);
            _instanceStorages[array] = arrayStorage;

            return array;
        }


        internal IEnumerable<TypeMethodInfo> GetOverloads(InstanceInfo type, string methodName = null)
        {
            var searcher = _services.CreateSearcher();
            searcher.SetCalledObject(type);

            if (methodName != null)
                searcher.Dispatch(methodName);

            return searcher.FoundResult;
        }

        internal TypeMethodInfo GetMethod(InstanceInfo type, string methodName)
        {
            var method = TryGetMethod(type, methodName);
            if (method == null)
                throw new NotSupportedException("Cannot get " + methodName + " method for " + type.TypeName);

            return method;
        }

        internal TypeMethodInfo TryGetMethod(InstanceInfo type, string methodName)
        {
            var overloads = GetOverloads(type, methodName);
            if (overloads.Count() != 1)
            {
                return null;
            }

            return overloads.First();
        }

        internal void Call(InstanceRef calledInstance, MethodID methodID, InstanceRef[] arguments)
        {
            checkNull(methodID);
            var inst = GetStorage(calledInstance);
            var args = getArgumentStorages(arguments);

            emit((e) => e.Call(methodID, inst, args));
        }

        internal InstanceRef CallWithReturn(InstanceRef calledInstance, MethodID methodID, InstanceRef[] arguments)
        {
            checkNull(methodID);
            var inst = GetStorage(calledInstance);
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


        internal InstanceRef CallDirectWithReturn(DirectMethod method)
        {
            var resultStorage = getFreeStorage("ret");
            //TODO determine result type
            var resultInstance = new InstanceRef(this, null, true);
            _instanceStorages.Add(resultInstance, resultStorage);

            emit((e) =>
            {
                e.DirectInvoke(method);
                e.AssignReturnValue(resultStorage, resultInstance.Type);
            });

            return resultInstance;
        }


        internal string GetStorage(InstanceRef instance)
        {
            return _instanceStorages[instance];
        }

        private Arguments getArgumentStorages(InstanceRef[] arguments)
        {
            var argVars = (from arg in arguments select GetStorage(arg)).ToArray();
            return Arguments.Values(argVars);
        }

        private void checkNull(MethodID methodID)
        {
            if (methodID == null)
            {
                throw new ArgumentNullException("methodID");
            }
            //everything is OK
        }

        private void emit(EmitAction action)
        {
            Generator.EmitAction(action);
        }

        private string getFreeStorage(string nameHint)
        {
            //TODO check for presence
            return string.Format("{0}_{1}", nameHint, _instanceStorages.Count);
        }

        internal MethodID TryGetImplementation(TypeDescriptor type, MethodID abstractMethod)
        {
            return _services.TryGetImplementation(type, abstractMethod);
        }
    }
}
