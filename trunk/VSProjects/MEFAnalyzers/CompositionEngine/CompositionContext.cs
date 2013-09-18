using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
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

        private readonly HashSet<ComponentRef> _componentRefs = new HashSet<ComponentRef>();

        private readonly Dictionary<InstanceRef, string> _instanceStorages = new Dictionary<InstanceRef, string>();

        internal readonly CompositionGenerator Generator=new CompositionGenerator();

        internal IEnumerable<ComponentRef> Components { get { return _componentRefs; } }

        internal CompositionContext(TypeServices services)
        {
            _services = services;
        }

        internal void AddConstructedComponents(params Instance[] components)
        {
            for (int i = 0; i < components.Length; ++i)
            {
                var component = components[i];
                var info = _services.GetComponentInfo(component);
                var componentRef = new ComponentRef(this, component, true, info);

                _componentRefs.Add(componentRef);
                addArgumentComponent(i, component, componentRef);
            }
        }

        /// <summary>
        /// Add component which will be available at given argument index in composition call
        /// </summary>
        /// <param name="argumentIndex"></param>
        /// <param name="component"></param>
        /// <param name="componentRef"></param>
        private void addArgumentComponent(int argumentIndex, Instance component, ComponentRef componentRef)
        {
            var storage = string.Format("arg_{0}", argumentIndex);
            emit((e) => e.AssignArgument(storage, component.Info, (uint)argumentIndex));
            _instanceStorages.Add(componentRef, storage);
        }

        /// <summary>
        /// Determine that testedType is type(C# is operator type is testedType)
        /// </summary>
        /// <param name="testedType"></param>
        /// <param name="testedType"></param>
        /// <returns></returns>
        internal bool IsOfType(InstanceInfo testedType, string type)
        {
            if (testedType.TypeName == type)
                return true;

            throw new NotImplementedException();
        }

        internal bool IsOfType(InstanceInfo testedtype, InstanceInfo type)
        {
            return IsOfType(testedtype, type.TypeName);
        }

        internal InstanceRef CreateArray(InstanceInfo instanceInfo, IEnumerable<InstanceRef> instances)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<TypeMethodInfo> GetMethods(InstanceInfo metadataType)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<TypeMethodInfo> GetMethods(InstanceInfo instType, string getterName)
        {
            throw new NotImplementedException();
        }

        internal void Call(InstanceRef calledInstance, MethodID methodID, InstanceRef[] arguments)
        {
            checkMethodID(methodID);
            var inst = getStorage(calledInstance);
            var args = getArgumentStorages(arguments);

            emit((e) => e.Call(methodID, inst, args));
        }

        internal InstanceRef CallWithReturn(InstanceRef calledInstance, MethodID methodID, InstanceRef[] arguments)
        {
            checkMethodID(methodID);
            var inst = getStorage(calledInstance);
            var args = getArgumentStorages(arguments);


            var resultStorage = string.Format("inst_{0}", _instanceStorages.Count);
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

        private string getStorage(InstanceRef instance)
        {
            return _instanceStorages[instance];
        }

        private string[] getArgumentStorages(InstanceRef[] arguments)
        {
            return (from arg in arguments select getStorage(arg)).ToArray();
        }

        private void checkMethodID(MethodID methodID)
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
    }

}
