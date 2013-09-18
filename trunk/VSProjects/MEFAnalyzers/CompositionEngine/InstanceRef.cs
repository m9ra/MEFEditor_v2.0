using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using Analyzing;

namespace MEFAnalyzers.CompositionEngine
{
    class InstanceRef
    {
        protected readonly CompositionContext _context;

        internal readonly InstanceInfo Type;

        internal bool IsConstructed { get; private set; }
        
        internal InstanceRef(CompositionContext context, InstanceInfo type, bool isConstructed)
        {
            _context = context;
            Type = type;
            IsConstructed = isConstructed;
        }

        internal void Construct(MethodID constructor, params InstanceRef[] arguments)
        {
            Debug.Assert(!IsConstructed, "Cant construct instance twice");
            IsConstructed = true;
            Call(constructor, arguments);
        }

        internal void Call(MethodID methodID, params InstanceRef[] arguments)
        {
            _context.Call(this, methodID, arguments);
        }

        internal InstanceRef CallWithReturn(MethodID methodID, params InstanceRef[] arguments)
        {
            return _context.CallWithReturn(this, methodID, arguments);
        }
    }
}
