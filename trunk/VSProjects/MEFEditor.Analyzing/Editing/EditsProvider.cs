using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Execution;
using MEFEditor.Analyzing.Editing.Transformations;

namespace MEFEditor.Analyzing.Editing
{
    /// <summary>
    /// Provides value for edit on editedInstance
    /// </summary>
    /// <returns>Value that will be pasted to transformation provider. The transformation provider decide, that it understand given value.</returns>
    public delegate object ValueProvider(ExecutionView services);

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
        /// <param name="view"></param>
        /// <returns></returns>
        public VariableName GetVariableFor(Instance instance, ExecutionView view)
        {
            var transformation = new ScopeBlockTransformation(_block, instance);
            view.Apply(transformation);

            return transformation.ScopeVariable;
        }

        public void AppendArgument(Instance editProvider, int argIndex, string editName, ValueProvider valueProvider)
        {
            var transformation = TransformProvider.AppendArgument(argIndex, valueProvider);
            AddEdit(editProvider, editName, transformation);
        }

        public Edit AddCall(Instance editProvider, string editName, CallProvider callProvider)
        {
            var transformation = new AddCallTransformation(callProvider);
            return AddEdit(editProvider, editName, transformation);
        }

        public void RemoveArgument(Instance editProvider, int argumentIndex, string editName)
        {
            var transformation = TransformProvider.RemoveArgument(argumentIndex, true).Remove();
            AddEdit(editProvider, editName, transformation);
        }

        public void AttachRemoveArgument(Instance attachingInstance, Instance editProvider, int argumentIndex, string editName)
        {
            var transformation = TransformProvider.RemoveArgument(argumentIndex, true).Remove();
            AttachEdit(attachingInstance, editProvider, editName, transformation);
        }

        public void RemoveCall(Instance editProvider, string editName)
        {
            var transform = TransformProvider.Remove().Remove();
            AddEdit(editProvider, editName, transform);
        }

        public void AttachRemoveCall(Instance attachingInstance, Instance editProvider, string editName)
        {
            var transform = TransformProvider.Remove().Remove();
            AttachEdit(attachingInstance, editProvider, editName, transform);
        }

        public void ChangeArgument(Instance editProvider, int argumentIndex, string editName, ValueProvider valueProvider)
        {
            var transformation = TransformProvider.RewriteArgument(argumentIndex, valueProvider);
            AddEdit(editProvider, editName, transformation);
        }

        public void SetOptional(int index)
        {
            TransformProvider.SetOptionalArgument(index);
        }

        public void Remove(Edit edit)
        {
            if (edit == null)
                return;

            edit.Provider.RemoveEdit(edit);
        }

        public Edit AddEdit(Instance editProvider, string editName, Transformation transformation)
        {
            if (transformation == null)
                return null;

            var edit = new Edit(editProvider, editProvider, this, editName, transformation);
            editProvider.AddEdit(edit);
            return edit;
        }

        public Edit AttachEdit(Instance attachingInstance, Instance editProvider, string editName, Transformation transformation)
        {
            if (transformation == null)
                return null;

            var edit = new Edit(attachingInstance, editProvider, this, editName, transformation);
            editProvider.AttachEdit(attachingInstance, edit);
            return edit;
        }


    }
}
