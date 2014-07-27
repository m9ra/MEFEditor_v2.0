using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;
using EnvDTE80;

using MEFEditor.TypeSystem;

using RecommendedExtensions.Core.Languages.CSharp;
using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.Traversing;

namespace RecommendedExtensions.Core.AssemblyProviders.CSharpAssembly
{
    /// <summary>
    /// Concrete implementation of C# naming conventions in Code model
    /// </summary>
    public class CSharpNames : CodeElementNamesProvider
    {
        /// <summary>
        /// Reports member as belonging to initializer
        /// </summary>
        /// <param name="isShared"><c>true</c> if initializer is shared, <c>false</c> otherwise</param>
        protected void ReportInitializer(bool isShared)
        {
            if (isShared)
            {
                ReportName(CSharpSyntax.MemberStaticInitializer);
            }
            else
            {
                ReportName(CSharpSyntax.MemberInitializer);
            }
        }

        /// <summary>
        /// Report indexer's getter and setter
        /// </summary>
        protected void ReportIndexer()
        {
            ReportName(Naming.IndexerGetter);
            ReportName(Naming.IndexerSetter);
        }

        /// <inheritdoc />
        public override void VisitClass(CodeClass2 e)
        {
            base.VisitClass(e);

            ReportInitializer(true);
            ReportInitializer(false);

            //in csharp there can be implicit ctors
            var hasCCtor = false;
            var hasCtor = false;

            foreach (CodeElement member in e.Members)
            {
                var fn = member as CodeFunction;
                if (fn == null)
                    continue;

                if (fn.FunctionKind == vsCMFunction.vsCMFunctionConstructor)
                {
                    if (fn.IsShared)
                        hasCCtor = true;
                    else
                        hasCtor = true;

                    if(hasCtor&& hasCCtor)
                        //speed optimization
                        break;
                }
            }

            if (!hasCtor)
                //can have implicit ctor
                ReportName(Naming.CtorName);

            if (!hasCCtor)
                //can have implicit cctor
                ReportName(Naming.ClassCtorName);
        }

        /// <inheritdoc />
        public override void VisitProperty(CodeProperty e)
        {
            var name = e.Name;
            var isIndexer = name == CSharpSyntax.ThisVariable;
            if (isIndexer)
            {
                //indexer is special name
                ReportIndexer();
                return;
            }

            base.VisitProperty(e);
        }
    }
}
