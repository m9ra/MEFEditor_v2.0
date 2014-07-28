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
    /// Provides value by user that can be used in edits.
    /// </summary>
    /// <param name="view">View where value will be used.</param>
    /// <returns>Value that will be pasted to transformation provider. The transformation provider decide, that it understand given value.</returns>
    public delegate object ValueProvider(ExecutionView view);

    /// <summary>
    /// Provider of edit services available from <see cref="MEFEditor.Analyzing" /> library.
    /// </summary>
    public class EditsProvider
    {
        /// <summary>
        /// The context block where edits are valid.
        /// </summary>
        private readonly ExecutedBlock _block;

        /// <summary>
        /// Transformation provider of context call.
        /// </summary>
        internal readonly CallTransformProvider TransformProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditsProvider" /> class.
        /// </summary>
        /// <param name="transformProvider">The call transformation provider.</param>
        /// <param name="block">The block.</param>
        /// <exception cref="System.ArgumentNullException">transformProvider</exception>
        internal EditsProvider(CallTransformProvider transformProvider, ExecutedBlock block)
        {
            if (transformProvider == null)
            {
                throw new ArgumentNullException("transformProvider");
            }
            TransformProvider = transformProvider;
            _block = block;
        }

        /// <summary>
        /// Get variable for given instance in block associated with current
        /// edits provider call. Minimal sub transformation for getting common scope
        /// is used.
        /// </summary>
        /// <param name="instance">Instance which variable is searched.</param>
        /// <param name="view">The view.</param>
        /// <returns>VariableName.</returns>
        public VariableName GetVariableFor(Instance instance, ExecutionView view)
        {
            var transformation = new ScopeBlockTransformation(_block, instance);
            view.Apply(transformation);

            return transformation.ScopeVariable;
        }

        /// <summary>
        /// Creates edit that appends the argument to current call.
        /// </summary>
        /// <param name="editProvider">The edit provider.</param>
        /// <param name="argIndex">Index of the argument.</param>
        /// <param name="editName">Name of the edit.</param>
        /// <param name="valueProvider">The value provider.</param>
        public void AppendArgument(Instance editProvider, int argIndex, string editName, ValueProvider valueProvider)
        {
            var transformation = TransformProvider.AppendArgument(argIndex, valueProvider);
            AddEdit(editProvider, editName, transformation);
        }

        /// <summary>
        /// Creates edit that creates specified call.
        /// </summary>
        /// <param name="editProvider">The edit provider.</param>
        /// <param name="editName">Name of the edit.</param>
        /// <param name="callProvider">The call provider.</param>
        /// <returns>Created edit.</returns>
        public Edit AddCall(Instance editProvider, string editName, CallProvider callProvider)
        {
            var transformation = new AddCallTransformation(callProvider);
            return AddEdit(editProvider, editName, transformation);
        }

        /// <summary>
        /// Creates edit that removes the specified argument.
        /// </summary>
        /// <param name="editProvider">The edit provider.</param>
        /// <param name="argumentIndex">Index of the argument.</param>
        /// <param name="editName">Name of the edit.</param>
        public void RemoveArgument(Instance editProvider, int argumentIndex, string editName)
        {
            var transformation = TransformProvider.RemoveArgument(argumentIndex, true).Remove();
            AddEdit(editProvider, editName, transformation);
        }

        /// <summary>
        /// Creates attached edit that remove specified argument.
        /// </summary>
        /// <param name="attachingInstance">The attaching instance.</param>
        /// <param name="editProvider">The edit provider.</param>
        /// <param name="argumentIndex">Index of the argument.</param>
        /// <param name="editName">Name of the edit.</param>
        public void AttachRemoveArgument(Instance attachingInstance, Instance editProvider, int argumentIndex, string editName)
        {
            var transformation = TransformProvider.RemoveArgument(argumentIndex, true).Remove();
            AttachEdit(attachingInstance, editProvider, editName, transformation);
        }

        /// <summary>
        /// Creates edit that removes current call.
        /// </summary>
        /// <param name="editProvider">The edit provider.</param>
        /// <param name="editName">Name of the edit.</param>
        public void RemoveCall(Instance editProvider, string editName)
        {
            var transform = TransformProvider.Remove().Remove();
            AddEdit(editProvider, editName, transform);
        }

        /// <summary>
        /// Creates attached edit that removes current call.
        /// </summary>
        /// <param name="attachingInstance">The attaching instance.</param>
        /// <param name="editProvider">The edit provider.</param>
        /// <param name="editName">Name of the edit.</param>
        public void AttachRemoveCall(Instance attachingInstance, Instance editProvider, string editName)
        {
            var transform = TransformProvider.Remove().Remove();
            AttachEdit(attachingInstance, editProvider, editName, transform);
        }

        /// <summary>
        /// Creates edit that changes the specified argument to value retrieved from given value provider.
        /// </summary>
        /// <param name="editProvider">The edit provider.</param>
        /// <param name="argumentIndex">Index of the argument.</param>
        /// <param name="editName">Name of the edit.</param>
        /// <param name="valueProvider">The value provider.</param>
        public void ChangeArgument(Instance editProvider, int argumentIndex, string editName, ValueProvider valueProvider)
        {
            var transformation = TransformProvider.RewriteArgument(argumentIndex, valueProvider);
            AddEdit(editProvider, editName, transformation);
        }

        /// <summary>
        /// Set specified argument of current call as optional.
        /// </summary>
        /// <param name="index">The index.</param>
        public void SetOptional(int index)
        {
            TransformProvider.SetOptionalArgument(index);
        }

        /// <summary>
        /// Removes the specified edit.
        /// </summary>
        /// <param name="edit">The edit.</param>
        public void Remove(Edit edit)
        {
            if (edit == null)
                return;

            edit.Provider.RemoveEdit(edit);
        }

        /// <summary>
        /// Adds specified edit to given editProvider.
        /// </summary>
        /// <param name="editProvider">The edit provider.</param>
        /// <param name="editName">Name of the edit.</param>
        /// <param name="transformation">Transformation describing edit.</param>
        /// <returns>Created added.</returns>
        public Edit AddEdit(Instance editProvider, string editName, Transformation transformation)
        {
            if (transformation == null)
                return null;

            var edit = new Edit(editProvider, editProvider, this, editName, transformation);
            editProvider.AddEdit(edit);
            return edit;
        }

        /// <summary>
        /// Attach specified edit to given editProvider.
        /// </summary>
        /// <param name="attachingInstance">The attaching instance.</param>
        /// <param name="editProvider">The edit provider.</param>
        /// <param name="editName">Name of the edit.</param>
        /// <param name="transformation">Transformation describing edit.</param>
        /// <returns>Created added.</returns>
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
