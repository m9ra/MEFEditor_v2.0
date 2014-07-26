using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeExperiments.Core;

namespace TypeExperiments.Reflection.Definitions
{
    class MethodDefinition
    {
      //  public readonly MethodName Name;
        public readonly BodyDefinition Body;
        public MethodDefinition(BodyDefinition body)
        {
            //TODO implement
            Body = body;
        }
    }
}
