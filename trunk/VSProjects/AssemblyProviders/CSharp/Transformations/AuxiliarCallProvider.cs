using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Analyzing.Editing;
using Analyzing.Editing.Transformations;

namespace AssemblyProviders.CSharp.Transformations
{
    /// <summary>
    /// Implementation of <see cref="CallTransformProvider"/> used by compiler for auxiliary calls
    /// </summary>
    class AuxiliarCallProvider:CallTransformProvider
    {
        public override NavigationAction GetNavigation()
        {
            return null;
        }

        public override RemoveTransformProvider RemoveArgument(int argumentIndex, bool keepSideEffect)
        {
            return new NoRemoveProvider();
        }

        public override Transformation RewriteArgument(int argumentIndex, ValueProvider valueProvider)
        {
            return new IdentityTransformation();
        }

        public override Transformation AppendArgument(int argumentIndex, ValueProvider valueProvider)
        {
            return new IdentityTransformation();
        }

        public override bool IsOptionalArgument(int argumentIndex)
        {
            return true;
        }

        public override void SetOptionalArgument(int index)
        {
            //all arguments could behave like optional - auxiliary
            //provider is used for masking non-presence of real transformation
        }

        public override RemoveTransformProvider Remove()
        {
            throw new NotImplementedException();
        }
    }
}
