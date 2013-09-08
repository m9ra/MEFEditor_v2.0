using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing
{
    delegate bool RemoveHandler(Instance instance, TransformationServices services);

    class InstanceRemoveProvider
    {
        readonly CallContext _context;

        internal InstanceRemoveProvider(CallContext context)
        {
            _context = context;
        }

        internal bool Remove(Instance instance, TransformationServices services)
        {
            var currBlock = _context.EntryBlock;

            while (currBlock != null)
            {
                var scopeStarts = currBlock.ScopeStarts(instance);
                if (scopeStarts.Any())
                {
                    //here instance scope starts - begin removing from this block
                    return remove(instance, currBlock,services);
                }

                currBlock = currBlock.NextBlock;
            }


            //instance hasn't been found in scope where removing is allowed/possible
            return false;
        }

        private bool remove(Instance instance, ExecutedBlock creationBlock,TransformationServices services)
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
                    services.Apply(transform);
                }
                currBlock = currBlock.NextBlock;
            }

         //   throw new NotImplementedException();

            return true;
        }
    }
}
