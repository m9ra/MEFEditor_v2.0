using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp.Interfaces;

using Analyzing.Editing;

namespace AssemblyProviders.CSharp.Transformations
{
    class SourceRemoveProvider : RemoveTransformProvider
    {
        readonly TransformAction _action;
        readonly INodeAST _contextNode;
        internal SourceRemoveProvider(TransformAction action, INodeAST contextNode)
        {
            if (action == null)
                throw new ArgumentNullException();
            _action = action;
            _contextNode = contextNode;
        }
        public override Transformation Remove()
        {
            return new SourceTransformation(_action, _contextNode.Source);
        }

        public override NavigationAction GetNavigation()
        {
            return () =>
            {
                _contextNode.StartingToken.Position.Navigate();
            };
        }
    }
}
