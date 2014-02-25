using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;
using EnvDTE80;
using EnvDTE90;
using EnvDTE100;

namespace AssemblyProviders.ProjectAssembly.Traversing
{
    delegate void NamespaceEntered(CodeNamespace e);

    class CodeElementVisitor
    {
        public event NamespaceEntered OnNamespaceEntered;

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
                default:
                    throw new NotImplementedException();
            }
        }

        public virtual void VisitFunction(CodeFunction2 e)
        {
            foreach (CodeElement child in e.Children)
            {
                VisitElement(child);
            }
        }

        public virtual void VisitClass(CodeClass2 e)
        {
            foreach (CodeElement child in e.Children)
            {
                VisitElement(child);
            }
        }

        public virtual void VisitAttribute(CodeAttribute2 e)
        {
            //There is no default deeper traversing
        }

        public virtual void VisitInterface(CodeInterface2 e)
        {
            foreach (CodeElement child in e.Children)
            {
                VisitElement(child);
            }
        }

        public virtual void VisitNamespace(CodeNamespace e)
        {
            if (OnNamespaceEntered != null)
                OnNamespaceEntered(e);

            foreach (CodeElement child in e.Children)
            {
                VisitElement(child);
            }
        }

        public virtual void VisitProject(Project e)
        {
            if (e.ProjectItems != null)
            {
                foreach (ProjectItem item in e.ProjectItems)
                {
                    VisitProjectItem(item);
                }
            }
        }

        public virtual void VisitProjectItem(ProjectItem e)
        {
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
