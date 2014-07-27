using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Execution;

namespace MEFEditor.Analyzing.Editing
{
    delegate bool RemoveHandler(Instance instance, ExecutionView services);

    class InstanceRemoveProvider
    {
        readonly CallContext _context;

        internal InstanceRemoveProvider(CallContext context)
        {
            _context = context;
        }

        internal bool Remove(Instance instance, ExecutionView view)
        {
            var currentBlock = instance.CreationBlock;
            if (currentBlock == null)
            {
                return false;
            }

            while (currentBlock != null)
            {
                var removeProviders = currentBlock.RemoveProviders(instance);
                foreach (var removeProvider in removeProviders)
                {
                    if (removeProvider == null)
                    {
                        //removing is not possible
                        view.Abort("Cannot remove because of missing RemoveProvider");
                        return false;
                    }

                    var transform = removeProvider.Remove();
                    view.Apply(transform);
                    if (view.IsAborted)
                        return false;
                }
                currentBlock = view.NextBlock(currentBlock);
            }

            return true;
        }

        /// <summary>
        /// Light over approximation test for possibility of instance removing in given view
        /// </summary>
        /// <param name="instance">Instance that is tested for removing possibility</param>
        /// <param name="view">View where removing will be processed</param>
        /// <returns><c>true</c> if instance can be removed, <c>false</c> otherwise</returns>
        internal bool CanRemove(Instance instance, ExecutionView view)
        {
            var currentBlock = instance.CreationBlock;
            if (currentBlock == null)
            {
                return false;
            }

            var hasRemoveProvider = false;
            while (currentBlock != null)
            {
                var removeProviders = currentBlock.RemoveProviders(instance);
                foreach (var removeProvider in removeProviders)
                {
                    if (removeProvider == null)
                    {
                        //removing is not possible
                        return false;
                    }

                    hasRemoveProvider = true;
                }

                currentBlock = view.NextBlock(currentBlock);
            }

            return hasRemoveProvider;
        }
    }
}
