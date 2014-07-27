using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;
using EnvDTE80;

namespace RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.Traversing
{
    /// <summary>
    /// Delegate called whenever any <see cref="CodeNamespace"/> is entered
    /// </summary>
    /// <param name="e">Entered <see cref="CodeNamespace"/></param>
    public delegate void NamespaceEntered(CodeNamespace e);

    public class CodeElementVisitor
    {
        public event NamespaceEntered OnNamespaceEntered;

        /// <summary>
        /// Determine that recursive visiting of elements will be proceeded
        /// </summary>
        protected bool RecursiveVisit = true;

        public virtual void VisitElement(CodeElement e)
        {
            switch (e.Kind)
            {
                case vsCMElement.vsCMElementClass:
                    VisitClass(e as CodeClass2);
                    break;
                case vsCMElement.vsCMElementNamespace:
                    VisitNamespace(e as CodeNamespace);
                    break;
                case vsCMElement.vsCMElementInterface:
                    VisitInterface(e as CodeInterface2);
                    break;
                case vsCMElement.vsCMElementFunction:
                    VisitFunction(e as CodeFunction2);
                    break;
                case vsCMElement.vsCMElementAttribute:
                    VisitAttribute(e as CodeAttribute2);
                    break;
                case vsCMElement.vsCMElementImportStmt:
                    VisitImport(e as CodeImport);
                    break;
                case vsCMElement.vsCMElementVariable:
                    VisitVariable(e as CodeVariable);
                    break;
                case vsCMElement.vsCMElementProperty:
                    VisitProperty(e as CodeProperty);
                    break;
                default:
                    VisitUnknown(e);
                    break;
            }
        }


        /// <summary>
        /// <see cref="CodeElement"/> casting helper
        /// </summary>
        /// <param name="e">Element thats visiting hasn't been handled</param>
        private void visitUnhandled(object e)
        {
            VisitUnhandled(e as CodeElement);
        }

        /// <summary>
        /// Handler for elements which visiting has no action 
        /// (is not reimplemented or is not recursive)
        /// </summary>
        /// <param name="e">Element thats visiting hasn't been handled</param>
        public virtual void VisitUnhandled(CodeElement e)
        {
            //nothing to do
        }

        /// <summary>
        /// Visit given element
        /// </summary>
        /// <param name="e">Element to visit</param>
        public virtual void VisitUnknown(CodeElement e)
        {
            VisitUnhandled(e);
        }

        /// <summary>
        /// Visit given element
        /// </summary>
        /// <param name="e">Element to visit</param>
        private void VisitImport(CodeImport e)
        {
            VisitUnhandled(e);
        }

        /// <summary>
        /// Visit given element
        /// </summary>
        /// <param name="e">Element to visit</param>
        public virtual void VisitProperty(CodeProperty e)
        {
            if (!RecursiveVisit)
            {
                //stop recursion
                visitUnhandled(e);
                return;
            }

            foreach (CodeElement child in e.Children)
            {
                VisitElement(child);
            }
        }

        /// <summary>
        /// Visit given element
        /// </summary>
        /// <param name="e">Element to visit</param>
        public virtual void VisitVariable(CodeVariable e)
        {
            if (!RecursiveVisit)
            {
                //stop recursion
                visitUnhandled(e);
                return;
            }

            foreach (CodeElement child in e.Children)
            {
                VisitElement(child);
            }
        }

        /// <summary>
        /// Visit given element
        /// </summary>
        /// <param name="e">Element to visit</param>
        public virtual void VisitFunction(CodeFunction2 e)
        {
            if (!RecursiveVisit)
            {
                //stop recursion
                visitUnhandled(e);
                return;
            }

            foreach (CodeElement child in e.Children)
            {
                VisitElement(child);
            }
        }

        /// <summary>
        /// Visit given element
        /// </summary>
        /// <param name="e">Element to visit</param>
        public virtual void VisitClass(CodeClass2 e)
        {
            if (!RecursiveVisit)
            {
                //stop recursion
                visitUnhandled(e);
                return;
            }

            foreach (CodeElement child in e.Children)
            {
                VisitElement(child);
            }
        }

        /// <summary>
        /// Visit given element
        /// </summary>
        /// <param name="e">Element to visit</param>
        public virtual void VisitAttribute(CodeAttribute2 e)
        {
            if (!RecursiveVisit)
            {
                //stop recursion
                visitUnhandled(e);
                return;
            }
            //There is no default deeper traversing
        }

        /// <summary>
        /// Visit given element
        /// </summary>
        /// <param name="e">Element to visit</param>
        public virtual void VisitInterface(CodeInterface2 e)
        {
            if (!RecursiveVisit)
            {
                //stop recursion
                visitUnhandled(e);
                return;
            }

            foreach (CodeElement child in e.Children)
            {
                VisitElement(child);
            }
        }

        /// <summary>
        /// Visit given element
        /// </summary>
        /// <param name="e">Element to visit</param>
        public virtual void VisitNamespace(CodeNamespace e)
        {
            if (OnNamespaceEntered != null)
                OnNamespaceEntered(e);

            if (!RecursiveVisit)
            {
                //stop recursion
                visitUnhandled(e);
                return;
            }

            foreach (CodeElement child in e.Members)
            {
                VisitElement(child);
            }
        }

        /// <summary>
        /// Visit given element
        /// </summary>
        /// <param name="e">Element to visit</param>
        public virtual void VisitProject(Project e)
        {
            if (!RecursiveVisit)
            {
                //stop recursion
                return;
            }

            if (e.ProjectItems != null)
            {
                foreach (ProjectItem item in e.ProjectItems)
                {
                    VisitProjectItem(item);
                }
            }
        }

        /// <summary>
        /// Visit given element
        /// </summary>
        /// <param name="e">Element to visit</param>
        public virtual void VisitProjectItem(ProjectItem e)
        {
            if (!RecursiveVisit)
            {
                //stop recursion
                return;
            }

            if (e.SubProject != null)
            {
                VisitProject(e.SubProject);
            }

            if (e.ProjectItems != null)
            {
                foreach (ProjectItem item in e.ProjectItems)
                {
                    VisitProjectItem(item);
                }
            }

            if (e.FileCodeModel != null)
            {
                foreach (CodeElement element in e.FileCodeModel.CodeElements)
                {
                    VisitElement(element);
                }
            }
        }
    }
}
