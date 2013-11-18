using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing
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
            var currBlock = _context.EntryBlock;

            while (currBlock != null)
            {
                var scopeStarts = currBlock.ScopeStarts(instance);
                if (scopeStarts.Any())
                {
                    //here instance scope starts - begin removing from this block
                    return remove(instance, currBlock,view);
                }

                currBlock = view.NextBlock(currBlock);
            }


            //instance hasn't been found in scope where removing is allowed/possible
            return false;
        }

        private bool remove(Instance instance, ExecutedBlock creationBlock,ExecutionView view)
        {
            
            var currBlock=creationBlock;
            while (currBlock != null)
            {
                var removeProviders = currBlock.RemoveProviders(instance);

                foreach (var removeProvider in removeProviders)
                {
                    if (removeProvider == null)
                    {
                        //TODO removing cannot be proceeded
                        continue;
                    }
                    var transform = removeProvider.Remove();
                    view.Apply(transform);
                }
                currBlock = view.NextBlock(currBlock);
            }

         //   throw new NotImplementedException();

            return true;
        }
    }
}
