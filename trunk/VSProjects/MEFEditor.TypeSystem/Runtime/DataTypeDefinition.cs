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
    public class DataTypeDefinition : RuntimeTypeDefinition
    {
        /// <summary>
        /// Fields defined in type definition
        /// </summary>
        private readonly List<Field> _fields = new List<Field>();

        private readonly Field<HashSet<Instance>> _children;

        private Type _simulatedType;

        /// <summary>
        /// Fullname of defined type
        /// </summary>
        internal protected string FullName { get; protected set; }

        /// <summary>
        /// Create info for defined type by its FullName
        /// </summary>
        public override TypeDescriptor TypeInfo
        {
            get { return TypeDescriptor.Create(FullName); }
        }

        /// <summary>
        /// All reported children for current instance
        /// </summary>
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


        protected DataTypeDefinition()
        {
            _children = new Field<HashSet<Instance>>(this, "_children");

            initializeFieldHandlers();
        }



        internal void RegisterField(Field directProperty, out string storage)
        {
            _fields.Add(directProperty);
            storage = string.Format("@prop_{0}_{1}_{2}", _fields.Count, directProperty, GetType());
        }

        internal override IEnumerable<RuntimeMethodGenerator> GetMethods()
        {
            //TODO resolve inheritance
            return ContainingAssembly.GetMethodGenerators(this);
        }

        internal override IEnumerable<InheritanceChain> GetSubChains()
        {
            if (_simulatedType == null)
            {
                //TODO allow overwrite base type
                return new InheritanceChain[] { ContainingAssembly.GetChain(typeof(object)) };
            }
            else
            {
                return GetSubChains(_simulatedType);
            }
        }


        protected void Simulate<T>()
        {
            _simulatedType = typeof(T);
            FullName = _simulatedType.FullName;
        }

        protected void ReportChildAdd(int childArgIndex, string childDescription, bool isOptional = false)
        {
            ReportChildAdd(This, childArgIndex, childDescription, isOptional);
        }

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

        protected void ReportChildAdd(Instance attachedInstance, int childArgIndex, string childDescription, bool isOptional = false)
        {
            var child = CurrentArguments[childArgIndex];
            addChild(attachedInstance, child);

            if (isOptional)
                Edits.SetOptional(childArgIndex);

            var editName = UserInteraction.ExcludeName;
            Edits.AttachRemoveArgument(attachedInstance, child, childArgIndex, editName);
        }

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
