using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem.Runtime.Building;

namespace TypeSystem.Runtime
{
    public class DataTypeDefinition : RuntimeTypeDefinition
    {
        /// <summary>
        /// Fields defined in type definition
        /// </summary>
        private readonly List<Field> _fields = new List<Field>();

        private readonly Field<HashSet<Instance>> _children;

        /// <summary>
        /// Fullname of defined type
        /// </summary>
        internal protected string FullName { get; protected set; }

        /// <summary>
        /// Create info for defined type by its FullName
        /// </summary>
        internal override InstanceInfo TypeInfo
        {
            get { return new InstanceInfo(FullName); }
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
        }

        protected void Simulate<T>()
        {
            FullName = typeof(T).FullName;
        }

        protected void ReportChildAdd(int childArgIndex, string childDescription, bool isOptional = false)
        {
            ReportChildAdd(This, childArgIndex, childDescription, isOptional);
        }

        protected void ReportParamChildAdd(int childParamArgIndex, Instance child, string childDescription, bool isOptional = false)
        {
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
    }
}
