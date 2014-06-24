using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeExperiments.Core
{
    enum MethodModifier
    {
        Virtual,Abstract,New,Static
    }

    enum BodyModifier
    {
        Implemented,NotImplemented,Native
    }

    enum TypeModifier
    {
        Instance,Static,Abstract        
    }

    enum AccessModifier
    {
        Public,Private,Internal,Runtime
    }
}
