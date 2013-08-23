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
    /// <param name="editedInstance">Instance that is edited</param>
    /// <returns>Value that will be pasted to transformation provider. The transformation provider decide, that it understand given value.</returns>
    public delegate object ValueProvider(Instance editedInstance);

    public class EditsProvider
    {
        CallTransformProvider _callProvider;

        internal void SetProvider(CallTransformProvider callProvider)
        {
            if (callProvider == null)
            {
                throw new ArgumentNullException("callProvider");
            }
            _callProvider = callProvider;
        }

        public void AppendArgument(Instance editProvider, string editName)
        {
            throw new NotImplementedException();
        }

        public void RemoveArgument(Instance editProvider, int argumentIndex, string editName)
        {
            var transformation = _callProvider.RemoveArgument(argumentIndex);
            addEdit(editProvider, editName, transformation);
        }

        public void ChangeArgument(Instance editProvider, int argumentIndex, string editName, ValueProvider valueProvider)
        {
            throw new NotImplementedException();
        }

        private void addEdit(Instance editProvider, string editName, Transformation transformation)
        {
            var edit=new Edit(editName,transformation);
            editProvider.AddEdit(edit);
        }

    }
}
