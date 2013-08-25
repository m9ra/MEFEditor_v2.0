using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    public class ExecutedBlock<MethodID, InstanceInfo>
    {
        private LinkedList<CallContext<MethodID, InstanceInfo>> _calls;

        private ExecutedBlock<MethodID, InstanceInfo> _nextBlock;

        public readonly InstructionInfo Info;

        /// <summary>
        /// Call where this block has been executed
        /// </summary>
        public readonly CallContext<MethodID, InstanceInfo> Call;

        public ExecutedBlock<MethodID, InstanceInfo> PreviousBlock { get; private set; }

        public ExecutedBlock<MethodID, InstanceInfo> NextBlock
        {
            get
            {
                return _nextBlock;
            }
            internal set
            {
                _nextBlock = value;
                _nextBlock.PreviousBlock = this;
            }
        }



        public IEnumerable<CallContext<MethodID, InstanceInfo>> Calls
        {
            get
            {
                if (_calls == null)
                {
                    return new CallContext<MethodID, InstanceInfo>[0];
                }
                else
                {
                    return _calls;
                }
            }
        }

        internal ExecutedBlock(InstructionInfo info,CallContext<MethodID, InstanceInfo> call)
        {
            Info = info;
            Call = call;
        }

        internal void RegisterCall(CallContext<MethodID, InstanceInfo> callContext)
        {
            if (_calls == null)
            {
                _calls = new LinkedList<CallContext<MethodID, InstanceInfo>>();
            }
            _calls.AddLast(callContext);
        }

    }
}
