using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;
using EnvDTE80;

using TypeSystem;

using AssemblyProviders.ProjectAssembly.Traversing;

namespace AssemblyProviders.ProjectAssembly.MethodBuilding
{
    class MethodBuilder : CodeElementVisitor
    {
        private MethodItem _result;

        public override void VisitFunction(CodeFunction2 e)
        {
            throw new NotImplementedException("Build method from code function");
        }

        private void throwNotSupportedElement()
        {
            throw new NotSupportedException("Given element is not supported to be used as method definition");
        }

        internal MethodItem Build(CodeElement child)
        {
            _result = null;
            VisitElement(child);

            return _result;
        }
    }
}
