using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem.Core;

namespace TypeSystem.Reflection.Definitions
{

    delegate Instance Invokable(Instance thisObj,params Instance[] args);

    class BodyDefinition
    {
        public readonly BodyModifier Modifiers;
        public readonly Invokable Instructions;

        public BodyDefinition(Invokable instructions)
        {
            //TODO implement
            Instructions = instructions;
        }
    }
}
