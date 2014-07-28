using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Globalization;

using MEFEditor.Analyzing;

using MEFEditor.TypeSystem.Runtime.Building;

namespace MEFEditor.TypeSystem.Runtime
{
    /// <summary>
    /// Runtime definition of type for <see cref="DataInstance" />.
    /// </summary>
    public class DataTypeDefinition : RuntimeTypeDefinition
    {
        /// <summary>
        /// Fields defined in type definition.
        /// </summary>
        private readonly List<Field> _fields = new List<Field>();

        /// <summary>
        /// Children of represented <see cref="Instance" />.
        /// </summary>
        private readonly Field<HashSet<Instance>> _children;

        /// <summary>
        /// The represented type.
        /// </summary>
        private Type _simulatedType;

        /// <summary>
        /// Fullname of defined type.
        /// </summary>
        /// <value>The full name.</value>
        internal protected string FullName { get; protected set; }

        /// <summary>
        /// Create info for defined type, by its FullName.
        /// </summary>
        /// <value>The type information.</value>
        public override TypeDescriptor TypeInfo
        {
            get { return TypeDescriptor.Create(FullName); }
        }

        /// <summary>
        /// All reported children for current instance.
        /// </summary>
        /// <value>The children.</value>
        protected IEnumerable<Instance> Children
        {
            get
            {
                var children = _children.Get();
                if (children == null)
                    return new Instance[0];

                return children;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeDefinition" /> class.
        /// </summary>
        protected DataTypeDefinition()
        {
            _children = new Field<HashSet<Instance>>(this, "_children");

            initializeFieldHandlers();
        }
        
        /// <summary>
        /// Registers the field.
        /// </summary>
        /// <param name="field">The registered field.</param>
        /// <param name="storage">The storage of field.</param>
        internal void RegisterField(Field field, out string storage)
        {
            _fields.Add(field);
            storage = string.Format("@prop_{0}_{1}_{2}", _fields.Count, field, GetType());
        }

        /// <summary>
        /// Gets methods defined by current type definition.
        /// </summary>
        /// <returns>Defined methods</returns>
        internal override IEnumerable<RuntimeMethodGenerator> GetMethods()
        {
            return ContainingAssembly.GetMethodGenerators(this);
        }

        /// <summary>
        /// Get subchains defining current type definition's inheritance.
        /// </summary>
        /// <returns>Subchains.</returns>
        internal override IEnumerable<InheritanceChain> GetSubChains()
        {
            if (_simulatedType == null)
            {                
                return new InheritanceChain[] { ContainingAssembly.GetChain(typeof(object)) };
            }
            else
            {
                return GetSubChains(_simulatedType);
            }
        }


        /// <summary>
        /// Simulates type T.
        /// </summary>
        /// <typeparam name="T">Simulated type</typeparam>
        protected void Simulate<T>()
        {
            _simulatedType = typeof(T);
            FullName = _simulatedType.FullName;
        }

        /// <summary>
        /// Reports that child has been added.
        /// </summary>
        /// <param name="childArgIndex">Index of the child argument.</param>
        /// <param name="childDescription">The child description.</param>
        /// <param name="isOptional">if set to <c>true</c> can be removed from call.</param>
        protected void ReportChildAdd(int childArgIndex, string childDescription, bool isOptional = false)
        {
            ReportChildAdd(This, childArgIndex, childDescription, isOptional);
        }

        /// <summary>
        /// Reports that child has been added in variable arguments parameter.
        /// </summary>
        /// <param name="childParamArgIndex">Index of the child parameter argument.</param>
        /// <param name="child">The child.</param>
        /// <param name="childDescription">The child description.</param>
        /// <param name="isOptional">if set to <c>true</c> can be removed from call.</param>
        protected void ReportParamChildAdd(int childParamArgIndex, Instance child, string childDescription, bool isOptional = false)
        {
            ++childParamArgIndex;
            var attachedInstance = This;

            addChild(attachedInstance, child);

            if (isOptional)
                Edits.SetOptional(childParamArgIndex);

            var editName = UserInteraction.ExcludeName;
            Edits.AttachRemoveArgument(attachedInstance, child, childParamArgIndex, editName);
        }

        /// <summary>
        /// Reports that child has been added.
        /// </summary>
        /// <param name="attachedInstance">The attached instance.</param>
        /// <param name="childArgIndex">Index of the child argument.</param>
        /// <param name="childDescription">The child description.</param>
        /// <param name="isOptional">if set to <c>true</c> can be removed from call.</param>
        protected void ReportChildAdd(Instance attachedInstance, int childArgIndex, string childDescription, bool isOptional = false)
        {
            var child = CurrentArguments[childArgIndex];
            addChild(attachedInstance, child);

            if (isOptional)
                Edits.SetOptional(childArgIndex);

            var editName = UserInteraction.ExcludeName;
            Edits.AttachRemoveArgument(attachedInstance, child, childArgIndex, editName);
        }

        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="child">The child.</param>
        private void addChild(Instance parent, Instance child)
        {
            RunInContextOf(parent, () =>
            {
                //needs context change because of reading correct property
                var children = _children.Get();
                if (children == null)
                {
                    children = new HashSet<Instance>();
                    _children.Set(children);
                }
                children.Add(child);
            });
        }

        /// <summary>
        /// Initializes the field handlers.
        /// </summary>
        private void initializeFieldHandlers()
        {
            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                if (!typeof(Field).IsAssignableFrom(fieldType))
                {
                    //wrong type of field
                    continue;
                }

                //create field handler
                CultureInfo culture = null;
                var fieldHandler = Activator.CreateInstance(fieldType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { this }, culture);

                //assign created handler
                field.SetValue(this, fieldHandler);
            }
        }

    }
}
