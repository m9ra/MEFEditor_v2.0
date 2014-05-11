using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;
using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Transformations
{
    class AssignRemove : RemoveTransformProvider
    {
        private INodeAST _assignedValue;

        public AssignRemove(INodeAST assignedValue)
        {
            _assignedValue = assignedValue;
        }
        public override Transformation Remove()
        {
            //remove whole statement;
            return new SourceTransformation((view, source) =>
            {
                source.RemoveNode(view, _assignedValue);
            }, _assignedValue.Source);
        }

        public override NavigationAction GetNavigation()
        {
            return () =>
            {
                _assignedValue.StartingToken.Position.Navigate();
            };
        }
    }
}
