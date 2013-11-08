using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;
using Analyzing.Editing.Transformations;

namespace Analyzing.Editing
{
    /// <summary>
    /// Provides value for edit on editedInstance
    /// </summary>
    /// <returns>Value that will be pasted to transformation provider. The transformation provider decide, that it understand given value.</returns>
    public delegate object ValueProvider(TransformationServices services);

    public class EditsProvider
    {
        internal readonly CallTransformProvider TransformProvider;

        readonly ExecutedBlock _block;

        internal EditsProvider(CallTransformProvider callProvider, ExecutedBlock block)
        {
            if (callProvider == null)
            {
                throw new ArgumentNullException("callProvider");
            }
            TransformProvider = callProvider;
            _block = block;
        }

        /// <summary>
        /// Get variable for given instance in block associated with current
        /// edits provider call. Minimal sub transformation for getting common scope
        /// is used.
        /// </summary>
        /// <param name="instance">Instance which variable is searched.</param>
        /// <param name="services"></param>
        /// <returns></returns>
        public VariableName GetVariableFor(Instance instance, TransformationServices services)
        {
            var transformation = new ScopeBlockTransformation(_block, instance);
            services.Apply(transformation);

            return transformation.ScopeVariable;
        }

        public void AppendArgument(Instance editProvider, string editName, ValueProvider valueProvider)
        {
            var transformation = TransformProvider.AppendArgument(valueProvider);
            addEdit(editProvider, editName, transformation);
        }

        public void AddCall(Instance editProvider, string editName, CallProvider callProvider)
        {
            var transformation = new AddCallTransformation(callProvider);
            addEdit(editProvider, editName, transformation);
        }

        public void RemoveArgument(Instance editProvider, int argumentIndex, string editName)
        {
            var transformation = TransformProvider.RemoveArgument(argumentIndex, true).Remove();
            addEdit(editProvider, editName, transformation);
        }

        public void AttachRemoveArgument(Instance attachingInstance, Instance editProvider, int argumentIndex, string editName)
        {
            var transformation = TransformProvider.RemoveArgument(argumentIndex, true).Remove();
            attachEdit(attachingInstance, editProvider, editName, transformation);
        }

        public void RemoveCall(Instance editProvider, string editName)
        {
            var transform = TransformProvider.Remove().Remove();
            addEdit(editProvider, editName, transform);
        }

        public void AttachRemoveCall(Instance attachingInstance, Instance editProvider, string editName)
        {
            var transform = TransformProvider.Remove().Remove();
            attachEdit(attachingInstance, editProvider, editName, transform);
        }

        public void ChangeArgument(Instance editProvider, int argumentIndex, string editName, ValueProvider valueProvider)
        {
            var transformation = TransformProvider.RewriteArgument(argumentIndex, valueProvider);
            addEdit(editProvider, editName, transformation);
        }

        public void SetOptional(int index)
        {
            TransformProvider.SetOptionalArgument(index);
        }

        private void addEdit(Instance editProvider, string editName, Transformation transformation)
        {
            var edit = new Edit(editName, transformation);
            editProvider.AddEdit(edit);
        }

        private void attachEdit(Instance attachingInstance, Instance editProvider, string editName, Transformation transformation)
        {
            var edit = new Edit(editName, transformation);
            editProvider.AttachEdit(attachingInstance, edit);
        }


    }
}
