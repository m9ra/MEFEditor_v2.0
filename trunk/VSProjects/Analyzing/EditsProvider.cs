using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;
using Analyzing.Editing;

namespace Analyzing
{
    /// <summary>
    /// Provides value for edit on editedInstance
    /// </summary>
    /// <returns>Value that will be pasted to transformation provider. The transformation provider decide, that it understand given value.</returns>
    public delegate object ValueProvider(TransformationServices services);

    public class EditsProvider<MethodID,InstanceInfo>
    {
        readonly CallTransformProvider _callProvider;

        readonly ExecutedBlock<MethodID, InstanceInfo> _block;

        internal EditsProvider(CallTransformProvider callProvider,ExecutedBlock<MethodID,InstanceInfo> block)
        {
            if (callProvider == null)
            {
                throw new ArgumentNullException("callProvider");
            }
            _callProvider = callProvider;
            _block = block;
        }

        public VariableName GetVariableFor(Instance instance, TransformationServices services)
        {
            //TODO this is not correct

            foreach (var variable in _block.Call.Variables)
            {
                var value = _block.Call.GetValue(variable);
                if (value == instance)
                    return variable;
            }

            return null;
        }

        public void AppendArgument(Instance editProvider, string editName, ValueProvider valueProvider)
        {
            var transformation = _callProvider.AppendArgument(valueProvider);
            addEdit(editProvider, editName, transformation);
        }

        public void RemoveArgument(Instance editProvider, int argumentIndex, string editName)
        {
            var transformation = _callProvider.RemoveArgument(argumentIndex);
            addEdit(editProvider, editName, transformation);
        }

        public void ChangeArgument(Instance editProvider, int argumentIndex, string editName, ValueProvider valueProvider)
        {
            var transformation = _callProvider.RewriteArgument(argumentIndex, valueProvider);
            addEdit(editProvider, editName, transformation);
        }

        private void addEdit(Instance editProvider, string editName, Transformation transformation)
        {
            var edit = new Edit(editName, transformation);
            editProvider.AddEdit(edit);
        }

    }
}
