using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;
using EnvDTE80;

using MEFEditor.TypeSystem;

namespace RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.Traversing
{
    /// <summary>
    /// Visitor implementation for resolving possible names of <see cref="CodeElements"/>
    /// </summary>
    public class CodeElementNamesProvider : CodeElementVisitor
    {
        /// <summary>
        /// Names that has been reported
        /// </summary>
        public readonly HashSet<string> ReportedNames = new HashSet<string>();

        /// <summary>
        /// Initialize new instance of visitor implementation
        /// of element names provider
        /// </summary>
        public CodeElementNamesProvider()
        {
            RecursiveVisit = false;
        }

        /// <summary>
        /// Reports possible name of given element
        /// </summary>
        /// <param name="name">Reported name</param>
        protected void ReportName(string name)
        {
            ReportedNames.Add(name);
        }

        /// <summary>
        /// Reports getter and setter with given name
        /// </summary>
        /// <param name="name">Name to report</param>
        protected void ReportGetterSetter(string name)
        {
            ReportName(Naming.GetterPrefix + name);
            ReportName(Naming.SetterPrefix + name);
        }

        /// <inheritdoc />
        public override void VisitUnhandled(CodeElement e)
        {
            var simpleName = e.Name();
            ReportName(simpleName);
        }

        /// <inheritdoc />
        public override void VisitProperty(CodeProperty e)
        {
            //TODO resolve auto property
            ReportGetterSetter(e.Name);
        }

        /// <inheritdoc />
        public override void VisitVariable(CodeVariable e)
        {
            ReportGetterSetter(e.Name);
        }

        public override void VisitFunction(CodeFunction2 e)
        {
            if (e.FunctionKind == vsCMFunction.vsCMFunctionConstructor)
            {
                if (e.IsShared)
                    ReportName(Naming.ClassCtorName);
                else
                    ReportName(Naming.CtorName);
            }
            else
            {
                base.VisitFunction(e);
            }
        }
    }
}
